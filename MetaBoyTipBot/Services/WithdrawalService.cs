using System;
using System.Threading.Tasks;
using MetaBoyTipBot.Constants;
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

        public WithdrawalService(ILogger<IWithdrawalService> logger, IWalletUserRepository walletUserRepository, IBotService botService,
            INodeExecutionService nodeExecutionService, IWithdrawalRepository withdrawalRepository, IMhcHttpClient mhcHttpClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _walletUserRepository =
                walletUserRepository ?? throw new ArgumentNullException(nameof(walletUserRepository));
            _botService = botService ?? throw new ArgumentNullException(nameof(botService));
            _nodeExecutionService =
                nodeExecutionService ?? throw new ArgumentNullException(nameof(nodeExecutionService));
            _withdrawalRepository = withdrawalRepository ?? throw new ArgumentNullException(nameof(withdrawalRepository));
            _mhcHttpClient = mhcHttpClient ?? throw new ArgumentNullException(nameof(mhcHttpClient));
        }

        private WalletUser GetWallet(int userId)
        {
            var walletUser = _walletUserRepository.GetByUserId(userId);
            return walletUser;
        }

        public async Task Handle(Chat chat, int chatUserId, double amount)
        {
            var walletUser = GetWallet(chatUserId);

            if (walletUser != null)
            {
                var walletAddress = walletUser.PartitionKey;

                try
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
                            System.Threading.Thread.Sleep(10000);

                            if (!await VerifyTx(transactionId, userWithdrawal))
                            {
                                userWithdrawal.State = WithdrawalState.Failed;
                                await _withdrawalRepository.AddOrUpdate(userWithdrawal);
                                await _botService.SendTextMessage(chat.Id, ReplyConstants.UnableToWithdraw);
                            }
                        }
                        else
                        {
                            userWithdrawal.State = WithdrawalState.Completed;
                            await _withdrawalRepository.AddOrUpdate(userWithdrawal);
                            await _botService.SendTextMessage(chat.Id, ReplyConstants.WithdrawalSuccess);
                        }
                    }
                    else
                    {
                        userWithdrawal.State = WithdrawalState.Failed;
                        await _withdrawalRepository.AddOrUpdate(userWithdrawal);
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