using System;
using System.Threading.Tasks;
using MetaBoyTipBot.Constants;
using MetaBoyTipBot.Repositories;
using MetaBoyTipBot.TableEntities;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MetaBoyTipBot.Services
{
    public class WithdrawalService : IWithdrawalService
    {
        private readonly ILogger<IWithdrawalService> _logger;
        private readonly IWalletUserRepository _walletUserRepository;
        private readonly IBotService _botService;
        private readonly INodeExecutionService _nodeExecutionService;

        public WithdrawalService(ILogger<IWithdrawalService> logger, IWalletUserRepository walletUserRepository, IBotService botService,
            INodeExecutionService nodeExecutionService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _walletUserRepository =
                walletUserRepository ?? throw new ArgumentNullException(nameof(walletUserRepository));
            _botService = botService ?? throw new ArgumentNullException(nameof(botService));
            _nodeExecutionService =
                nodeExecutionService ?? throw new ArgumentNullException(nameof(nodeExecutionService));
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
                    // todo start wallet table storage
                    await _nodeExecutionService.Withdraw(walletAddress, amount);
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