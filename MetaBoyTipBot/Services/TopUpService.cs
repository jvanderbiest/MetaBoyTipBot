using System;
using System.Threading.Tasks;
using MetaBoyTipBot.Configuration;
using MetaBoyTipBot.Constants;
using MetaBoyTipBot.Repositories;
using MetaBoyTipBot.TableEntities;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace MetaBoyTipBot.Services
{
    public class TopUpService : ITopUpService
    {
        private readonly IOptions<BotConfiguration> _botConfiguration;
        private readonly IWalletUserRepository _walletUserRepository;
        private readonly IBotService _botService;

        public TopUpService(IOptions<BotConfiguration> botConfiguration, IWalletUserRepository walletUserRepository, IBotService botService)
        {
            _botConfiguration = botConfiguration ?? throw new ArgumentNullException(nameof(botConfiguration));
            _walletUserRepository = walletUserRepository ?? throw new ArgumentNullException(nameof(walletUserRepository));
            _botService = botService ?? throw new ArgumentNullException(nameof(botService));
        }

        private WalletUser GetWallet(int userId)
        {
            var walletUser = _walletUserRepository.GetByUserId(userId);
            return walletUser;
        }

        public async Task Handle(Chat chat, int chatUserId)
        {
            var walletUser = GetWallet(chatUserId);
            if (walletUser != null)
            {
                var walletAddress = walletUser.PartitionKey;
                await _botService.SendTextMessage(chat.Id, string.Format(ReplyConstants.CurrentWallet, walletAddress));
                await _botService.SendTextMessage(chat.Id, _botConfiguration.Value.TipWalletAddress);

                await _botService.ShowMainButtonMenu(chat.Id, null);
            }
            else
            {
                await _botService.SendTextMessage(chat.Id, ReplyConstants.EnterTopUpMetahashWallet, new ForceReplyMarkup { Selective = false });
            }
        }
    }

    public interface ITopUpService
    {
        Task Handle(Chat chat, int chatUserId);
    }
}
