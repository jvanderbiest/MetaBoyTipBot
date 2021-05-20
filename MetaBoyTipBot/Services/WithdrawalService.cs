using System;
using System.Threading.Tasks;
using MetaBoyTipBot.Configuration;
using MetaBoyTipBot.Constants;
using MetaBoyTipBot.Extensions;
using MetaBoyTipBot.Repositories;
using MetaBoyTipBot.TableEntities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MetaBoyTipBot.Services
{
    public class WithdrawalService : IWithdrawalService
    {
        private readonly ILogger<IWithdrawalService> _logger;
        private readonly IOptions<BotConfiguration> _botConfiguration;
        private readonly IWalletUserRepository _walletUserRepository;
        private readonly IBotService _botService;
        private readonly INodeExecutionService _nodeExecutionService;

        public WithdrawalService(ILogger<IWithdrawalService> logger, IOptions<BotConfiguration> botConfiguration,
            IWalletUserRepository walletUserRepository, IBotService botService,
            INodeExecutionService nodeExecutionService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _botConfiguration = botConfiguration ?? throw new ArgumentNullException(nameof(botConfiguration));
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

                    await _botService.Client.SendTextMessageAsync(
                        chatId: chat,
                        text: string.Format(ReplyConstants.CurrentWallet, walletAddress),
                        parseMode: ParseMode.Markdown,
                        disableNotification: true
                    );

                }

            }
        }
    }
}