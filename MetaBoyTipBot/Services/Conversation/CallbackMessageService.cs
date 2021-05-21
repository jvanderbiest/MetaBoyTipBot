using System;
using System.Threading.Tasks;
using MetaBoyTipBot.Configuration;
using MetaBoyTipBot.Constants;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MetaBoyTipBot.Services.Conversation
{
    public class CallbackMessageService : IMessageService
    {
        private readonly IBalanceService _balanceService;
        private readonly ITopUpService _topUpService;
        private readonly IWithdrawalService _withdrawalService;
        private readonly ISettingsService _settingsService;

        public CallbackMessageService(IBalanceService balanceService, ITopUpService topUpService, IWithdrawalService withdrawalService, ISettingsService settingsService)
        {
            _balanceService = balanceService ?? throw new ArgumentNullException(nameof(balanceService));
            _topUpService = topUpService ?? throw new ArgumentNullException(nameof(topUpService));
            _withdrawalService = withdrawalService ?? throw new ArgumentNullException(nameof(withdrawalService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        }

        public async Task Handle(Update update)
        {
            switch (update.CallbackQuery.Data)
            {
                case CallBackConstants.TopUp:
                    await _topUpService.Handle(update.CallbackQuery.Message.Chat, update.CallbackQuery.From.Id);
                    break;
                case CallBackConstants.Balance:
                    await _balanceService.Handle(update.CallbackQuery.Message.Chat, update.CallbackQuery.From.Id);
                    break;
                case CallBackConstants.WithDraw:
                    await _withdrawalService.Prompt(update.CallbackQuery.Message.Chat, update.CallbackQuery.From.Id);
                    break;
                case CallBackConstants.Settings:
                    await _settingsService.ShowSettingsMenu(update.CallbackQuery.Message.Chat, update.CallbackQuery.From.Id);
                    break;
                case CallBackConstants.SettingsDefaultTipAmount:
                    await _settingsService.HandleDefaultTipAmountPrompt(update.CallbackQuery.Message.Chat, update.CallbackQuery.From.Id);
                    break;
                case CallBackConstants.SettingsChangeWalletAddress:
                    await _settingsService.HandleChangeWalletAddress(update.CallbackQuery.Message.Chat, update.CallbackQuery.From.Id);
                    break;
            }
        }
    }
   
}