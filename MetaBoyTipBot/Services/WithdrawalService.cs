using System;
using System.Threading.Tasks;
using MetaBoyTipBot.Constants;
using MetaBoyTipBot.Extensions;
using MetaBoyTipBot.Repositories;
using MetaBoyTipBot.TableEntities;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace MetaBoyTipBot.Services
{
    public class WithdrawalService : IWithdrawalService
    {
        private readonly ILogger<IWithdrawalService> _logger;
        private readonly IWalletUserRepository _walletUserRepository;
        private readonly IBotService _botService;
        private readonly INodeExecutionService _nodeExecutionService;
        private readonly IWithdrawalRepository _withdrawalRepository;
        private readonly IMhcHttpClient _mhcHttpClient;
        private readonly IUserBalanceRepository _userBalanceRepository;

        public WithdrawalService(ILogger<IWithdrawalService> logger, IWalletUserRepository walletUserRepository, IBotService botService,
            INodeExecutionService nodeExecutionService, IWithdrawalRepository withdrawalRepository, IMhcHttpClient mhcHttpClient, IUserBalanceRepository userBalanceRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _walletUserRepository =
                walletUserRepository ?? throw new ArgumentNullException(nameof(walletUserRepository));
            _botService = botService ?? throw new ArgumentNullException(nameof(botService));
            _nodeExecutionService =
                nodeExecutionService ?? throw new ArgumentNullException(nameof(nodeExecutionService));
            _withdrawalRepository = withdrawalRepository ?? throw new ArgumentNullException(nameof(withdrawalRepository));
            _mhcHttpClient = mhcHttpClient ?? throw new ArgumentNullException(nameof(mhcHttpClient));
            _userBalanceRepository = userBalanceRepository ?? throw new ArgumentNullException(nameof(userBalanceRepository));
        }

        private WalletUser GetWallet(int userId)
        {
            var walletUser = _walletUserRepository.GetByUserId(userId);
            return walletUser;
        }

        public async Task Handle(Chat chat, int chatUserId, double amount)
        {

            var walletUser = GetWallet(chatUserId);
            var userBalance = await _userBalanceRepository.Get(chatUserId);
            if (userBalance.Balance < amount)
            {
                await _botService.SendTextMessage(chat.Id,
                    string.Format(ReplyConstants.InsufficientBalance, userBalance.Balance.RoundMetahashHash(), amount.RoundMetahashHash()));
            }
            else
            {
                if (walletUser != null)
                {
                    var walletAddress = walletUser.PartitionKey;

                    try
                    {
                        // get previous withdrawals and see if they completed. We do this in case the network sends a failed transaction and would still complete after 10 minutes.
                        var lastWithdrawal = _withdrawalRepository.GetByUserId(walletUser.GetUserId().GetValueOrDefault());

                        if (lastWithdrawal != null && lastWithdrawal.StartDate > DateTime.UtcNow.AddMinutes(-15) && lastWithdrawal.State != WithdrawalState.Completed)
                        {
                            _logger.LogInformation($"Retrying to verify Tx {lastWithdrawal.TxId}");
                            var isSuccess = await VerifyTx(lastWithdrawal.TxId, lastWithdrawal);

                            _logger.LogInformation($"Verify Tx {lastWithdrawal.TxId} result is {isSuccess}");
                            await SetWithdrawalState(isSuccess, lastWithdrawal, chat.Id, chatUserId, amount, lastWithdrawal.TxId);
                        }
                        else
                        {
                            if (lastWithdrawal != null && lastWithdrawal.StartDate > DateTime.UtcNow.AddHours(-4))
                            {
                                _logger.LogInformation($"Withdrawal denied for user {chatUserId}. Only 1 withdrawal is allowed every 4h.");

                                await _botService.SendTextMessage(chat.Id, ReplyConstants.WithdrawLimit);
                            }
                            else
                            {

                                var userWithdrawal = new UserWithdrawal(chatUserId) { WalletAddress = walletAddress, Amount = amount, State = WithdrawalState.Created, StartDate = DateTime.UtcNow };
                                await _withdrawalRepository.AddOrUpdate(userWithdrawal);
                                var transactionId = await _nodeExecutionService.Withdraw(walletAddress, amount);

                                if (!string.IsNullOrEmpty(transactionId))
                                {
                                    userWithdrawal.TxId = transactionId;
                                    userWithdrawal.State = WithdrawalState.Verification;
                                    await _withdrawalRepository.AddOrUpdate(userWithdrawal);

                                    await _botService.SendTextMessage(chat.Id, ReplyConstants.WithdrawVerification);

                                    // make sure network is up to date
                                    System.Threading.Thread.Sleep(5000);

                                    if (!await VerifyTx(transactionId, userWithdrawal))
                                    {
                                        await _botService.SendTextMessage(chat.Id, ReplyConstants.WithdrawVerificationLonger);

                                        // make sure network is up to date
                                        System.Threading.Thread.Sleep(20000);

                                        var isSuccess = await VerifyTx(transactionId, userWithdrawal);
                                        await SetWithdrawalState(isSuccess, userWithdrawal, chat.Id, chatUserId, amount, transactionId);
                                    }
                                    else
                                    {
                                        await SetWithdrawalSuccess(userWithdrawal, chat.Id, chatUserId, amount, transactionId);
                                    }
                                }
                                else
                                {
                                    userWithdrawal.State = WithdrawalState.Failed;
                                    await _withdrawalRepository.AddOrUpdate(userWithdrawal);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogCritical(
                            $"Withdrawal failed for userId: {chatUserId} and wallet: {walletAddress} for amount: {amount}",
                            ex);

                        await _botService.SendTextMessage(chat.Id, ReplyConstants.UnableToWithdraw);
                    }
                }
            }
        }

        private async Task SetWithdrawalState(bool isSuccess, UserWithdrawal userWithdrawal, long chatId, int chatUserId, double amount, string transactionId)
        {
            if (!isSuccess)
            {
                userWithdrawal.State = WithdrawalState.Failed;
                await _withdrawalRepository.AddOrUpdate(userWithdrawal);
                await _botService.SendTextMessage(chatId, ReplyConstants.UnableToWithdraw);
            }
            else
            {
                await SetWithdrawalSuccess(userWithdrawal, chatId, chatUserId, amount, transactionId);
            }
        }

        private async Task SetWithdrawalSuccess(UserWithdrawal userWithdrawal, long chatId, int chatUserId, double amount, string transactionId)
        {
            var userBalance = await _userBalanceRepository.Get(chatUserId);
            userBalance.Balance -= amount;
            await _userBalanceRepository.Update(userBalance);
            if (userBalance.Balance < 0)
            {
                _logger.LogError($"Balance for {chatUserId} is below 0 after withdrawal transaction {transactionId}");
            }
            userWithdrawal.State = WithdrawalState.Completed;
            await _withdrawalRepository.AddOrUpdate(userWithdrawal);
            await _botService.SendTextMessage(chatId, ReplyConstants.WithdrawalSuccess);
        }

        private async Task<bool> VerifyTx(string transactionId, UserWithdrawal userWithdrawal)
        {
            try
            {
                var txResponse = await _mhcHttpClient.GetTx(transactionId);
                if (txResponse.TxResult?.Transaction?.Status == "ok")
                {
                    userWithdrawal.State = WithdrawalState.Completed;
                    await _withdrawalRepository.AddOrUpdate(userWithdrawal);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Could not verify transaction {transactionId}", ex);
            }

            return false;
        }

        public async Task Prompt(Chat chat, int chatUserId)
        {
            var walletUser = GetWallet(chatUserId);

            if (walletUser != null)
            {
                await _botService.SendTextMessage(chat.Id, ReplyConstants.EnterWithdrawalAmount, new ForceReplyMarkup { Selective = false });
            }
            else
            {
                await _botService.SendTextMessage(chat.Id, ReplyConstants.EnterWithdrawalWallet, new ForceReplyMarkup { Selective = false });
            }
        }
    }
}