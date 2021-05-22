using System;
using System.Linq;
using System.Threading.Tasks;
using MetaBoyTipBot.Configuration;
using MetaBoyTipBot.Constants;
using MetaBoyTipBot.Extensions;
using MetaBoyTipBot.Repositories;
using MetaBoyTipBot.TableEntities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types.Enums;

namespace MetaBoyTipBot.Services
{
    public class TransactionHandlerService : ITransactionHandlerService
    {
        private readonly ILogger<ITransactionHandlerService> _logger;
        private readonly IOptions<BotConfiguration> _botConfiguration;
        private readonly IMhcHttpClient _mhcHttpClient;
        private readonly ITransactionHistoryRepository _transactionHistoryRepository;
        private readonly ITransactionCheckHistoryRepository _transactionCheckHistoryRepository;
        private readonly IWalletUserRepository _walletUserRepository;
        private readonly IUserBalanceRepository _userBalanceRepository;
        private readonly IUserBalanceHistoryRepository _userBalanceHistoryRepository;
        private readonly IBotService _botService;

        public TransactionHandlerService(ILogger<ITransactionHandlerService> logger, IOptions<BotConfiguration> botConfiguration,
            IMhcHttpClient mhcHttpClient, ITransactionHistoryRepository transactionHistoryRepository,
            ITransactionCheckHistoryRepository transactionCheckHistoryRepository,
            IWalletUserRepository walletUserRepository, IUserBalanceRepository userBalanceRepository,
            IUserBalanceHistoryRepository userBalanceHistoryRepository, IBotService botService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _botConfiguration = botConfiguration ?? throw new ArgumentNullException(nameof(botConfiguration));
            _mhcHttpClient = mhcHttpClient ?? throw new ArgumentNullException(nameof(mhcHttpClient));
            _transactionHistoryRepository = transactionHistoryRepository ?? throw new ArgumentNullException(nameof(transactionHistoryRepository));
            _transactionCheckHistoryRepository = transactionCheckHistoryRepository ?? throw new ArgumentNullException(nameof(transactionCheckHistoryRepository));
            _walletUserRepository = walletUserRepository ?? throw new ArgumentNullException(nameof(walletUserRepository));
            _userBalanceRepository = userBalanceRepository ?? throw new ArgumentNullException(nameof(userBalanceRepository));
            _userBalanceHistoryRepository = userBalanceHistoryRepository ?? throw new ArgumentNullException(nameof(userBalanceHistoryRepository));
            _botService = botService ?? throw new ArgumentNullException(nameof(botService));
        }

        public async Task ImportTransactions()
        {
            var latest = _transactionCheckHistoryRepository.GetLatest();
            if (latest != null && latest.LastTransactionTime.HasValue)
            {
                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(latest.LastTransactionTime.Value);
                await ImportTransactions(dateTimeOffset.DateTime);
            }
            else
            {
                await ImportTransactions(DateTime.UtcNow);
            }
        }

        public async Task ImportTransactions(DateTime startDateTime)
        {
            var allTransactionHistory = await _mhcHttpClient.FetchHistory(_botConfiguration.Value.TipWalletAddress);

            var startUnixTimeStamp = startDateTime.GetUnixEpochTimestamp();
            var newTransactionHistory = allTransactionHistory.Result.Where(x =>
                x.Timestamp > startUnixTimeStamp &&
                x.To == _botConfiguration.Value.TipWalletAddress &&
                x.Status == "ok" &&
                !x.IsDelegate);

            var handledTransactions = 0;
            foreach (var newTransaction in newTransactionHistory)
            {
                var transactionHandled = await HasHandledTransaction(newTransaction.From, newTransaction.Timestamp);
                if (transactionHandled != null)
                {
                    _logger.LogWarning($"Transaction '{newTransaction.Transaction}' was already handled (Status: '{transactionHandled.Status}'). Skipping.");
                    continue;
                }

                var walletUser = GetAssociatedWalletUserId(newTransaction.From);

                if (walletUser == null)
                {
                    _logger.LogInformation($"No associated user for wallet '{newTransaction.To} and transaction '{newTransaction.Transaction}'. Thanks for the donation of '{newTransaction.RealValue}' MHC!");
                }
                else
                {
                    var userId = walletUser.GetUserId().GetValueOrDefault();
                    var transactionHistory = new TransactionHistory(newTransaction.To, newTransaction.Timestamp)
                    {
                        BlockIndex = newTransaction.BlockIndex,
                        BlockNumber = newTransaction.BlockNumber,
                        Value = newTransaction.Value,
                        To = newTransaction.To,
                        From = newTransaction.From,
                        Status = "In Progress",
                        Transaction = newTransaction.Transaction,
                        ToChatUserId = userId
                    };

                    await _transactionHistoryRepository.AddOrUpdate(transactionHistory);

                    var user = await _userBalanceRepository.Get(userId);
                    user.Balance += newTransaction.RealValue;
                    await _userBalanceRepository.Update(user);

                    await _userBalanceHistoryRepository.Update(new UserBalanceHistory(userId, newTransaction.Timestamp)
                    { In = newTransaction.RealValue });

                    transactionHistory.Status = "Completed";
                    await _transactionHistoryRepository.AddOrUpdate(transactionHistory);

                    if (walletUser.PrivateChatId != null)
                    {
                        await _botService.Client.SendTextMessageAsync(
                            chatId: walletUser.PrivateChatId,
                            text: string.Format(ReplyConstants.TopUp, newTransaction.RealValue),
                            parseMode: ParseMode.Markdown,
                            disableNotification: true
                        );
                    }

                    handledTransactions++;
                }
            }

            int? lastTxTimestamp = null;
            var lastTransaction = allTransactionHistory.Result.OrderByDescending(x => x.Timestamp).FirstOrDefault();
            if (lastTransaction != null)
            {
                lastTxTimestamp = lastTransaction.Timestamp;
            }
            await _transactionCheckHistoryRepository.AddOrUpdate(new TransactionCheckHistory { HandledTransactions = handledTransactions, LastTransactionTime = lastTxTimestamp });
        }

        private WalletUser GetAssociatedWalletUserId(string walletAddress)
        {
            var walletUser = _walletUserRepository.GetByWalletId(walletAddress);
            return walletUser;
        }

        private async Task<TransactionHistory> HasHandledTransaction(string walletId, int txTimestamp)
        {
            var transactionHistory = await _transactionHistoryRepository.Get(walletId, txTimestamp);
            return transactionHistory;
        }
    }

    public interface ITransactionHandlerService
    {
        Task ImportTransactions(DateTime startDateTime);
        Task ImportTransactions();
    }
}
