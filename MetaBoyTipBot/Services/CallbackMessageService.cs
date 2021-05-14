using System;
using System.Threading.Tasks;
using MetaBoyTipBot.Constants;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MetaBoyTipBot.Services
{
    public class CallbackMessageService : IMessageService
    {
        private readonly IBotService _botService;

        public CallbackMessageService(IBotService botService)
        {
            _botService = botService ?? throw new ArgumentNullException(nameof(botService));
        }

        public async Task Handle(Update update)
        {
            if (update.CallbackQuery.Data == CallBackConstants.TopUp)
            {
                // todo check if user has already an address, if yes, then show only the top up address

                Message message = await _botService.Client.SendTextMessageAsync(
                    chatId: update.CallbackQuery.Message.Chat,
                    text: ReplyConstants.EnterMetahashWallet,
                    parseMode: ParseMode.Markdown,
                    disableNotification: true,
                    replyMarkup: new ForceReplyMarkup { Selective = false }
                );
            }

            else if (update.CallbackQuery.Data == CallBackConstants.Balance)
            {
                // balance lookup
                // get user from blob with associated address, if address lookup, if not send error message and show menu

                Message message = await _botService.Client.SendTextMessageAsync(
                    chatId: update.CallbackQuery.Message.Chat,
                    text: ReplyConstants.EnterMetahashWallet,
                    parseMode: ParseMode.Markdown,
                    disableNotification: true,
                    replyMarkup: new ForceReplyMarkup { Selective = false }
                );
            }
        }
    }
}