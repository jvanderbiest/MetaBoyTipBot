using System;
using System.Threading.Tasks;
using MetaBoyTipBot.Constants;
using MetaBoyTipBot.Repositories;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace MetaBoyTipBot.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly IBotService _botService;
        private readonly IUserBalanceRepository _userBalanceRepository;

        public SettingsService(IBotService botService, IUserBalanceRepository userBalanceRepository)
        {
            _botService = botService ?? throw new ArgumentNullException(nameof(botService));
            _userBalanceRepository = userBalanceRepository ?? throw new ArgumentNullException(nameof(userBalanceRepository));
        }

        public async Task ShowSettingsMenu(Chat chat, int chatUserId)
        {
            await _botService.ShowSettingsButtonMenu(chat.Id);
        }

        public async Task HandleDefaultTipAmountPrompt(Chat chat, int fromId)
        {
            await _botService.SendTextMessage(chat.Id, ReplyConstants.EnterDefaultTipAmount, new ForceReplyMarkup { Selective = false });
        }

        public async Task HandleChangeWalletAddress(Chat chat, int fromId)
        {
            await _botService.SendTextMessage(chat.Id, ReplyConstants.EnterTopUpMetahashWallet, new ForceReplyMarkup { Selective = false });
        }

        public async Task SetDefaultTipAmount(Chat chat, int fromId, double defaultTipAmount)
        {
            var userBalance = await _userBalanceRepository.Get(fromId);
            userBalance.DefaultTipAmount = defaultTipAmount;
            await _userBalanceRepository.Update(userBalance);

            await _botService.SendTextMessage(chat.Id, string.Format(ReplyConstants.DefaultTipAmountConfirmation, defaultTipAmount));

            await _botService.ShowMainButtonMenu(chat.Id, null);
        }
    }

    public interface ISettingsService
    {
        Task ShowSettingsMenu(Chat chat, int fromId);
        Task HandleDefaultTipAmountPrompt(Chat chat, int fromId);
        Task HandleChangeWalletAddress(Chat chat, int fromId);
        Task SetDefaultTipAmount(Chat chat, int fromId, double defaultTipAmount);
    }
}