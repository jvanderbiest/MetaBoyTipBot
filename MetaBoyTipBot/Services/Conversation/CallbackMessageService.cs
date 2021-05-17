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

        public CallbackMessageService(IBalanceService balanceService, ITopUpService topUpService)
        {
            _balanceService = balanceService ?? throw new ArgumentNullException(nameof(balanceService));
            _topUpService = topUpService ?? throw new ArgumentNullException(nameof(topUpService));
        }

        public async Task Handle(Update update)
        {
            if (update.CallbackQuery.Data == CallBackConstants.TopUp)
            {
                await _topUpService.Handle(update.CallbackQuery.Message.Chat, update.CallbackQuery.From.Id);
            }

            else if (update.CallbackQuery.Data == CallBackConstants.Balance)
            {
                await _balanceService.Handle(update.CallbackQuery.Message.Chat, update.CallbackQuery.From.Id);
            }
        }
    }
}