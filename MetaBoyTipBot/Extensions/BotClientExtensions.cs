using System.Collections.Generic;
using System.Threading.Tasks;
using MetaBoyTipBot.Constants;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MetaBoyTipBot.Extensions
{
    public static class BotClientExtensions
    {
        public static async Task ShowMenu(this ITelegramBotClient botClient, Chat chat, int? replyToMessageId)
        {
            // emoticons: https://charbase.com/1f4e5-unicode-inbox-tray

            var firstRow = new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData("\ud83d\udce4 Top up", CallBackConstants.TopUp),
                    InlineKeyboardButton.WithCallbackData("\ud83d\udcb0 Balance", CallBackConstants.Balance),

                };
            //var secondRow = new List<InlineKeyboardButton>
            //    {
            //        InlineKeyboardButton.WithCallbackData("\ud83d\udce5 Withdraw", CallBackConstants.WithDraw),
                    
            //        InlineKeyboardButton.WithCallbackData("\u2699 Settings", CallBackConstants.Settings)
            //    };

            var replyMarkup = new InlineKeyboardMarkup(new List<IEnumerable<InlineKeyboardButton>> {firstRow}); //, secondRow });

            if (replyToMessageId.HasValue)
            {
                var text = "Welcome! MetaBoy can send MHC tips other telegram users.\r\n\r\nAny response to another user's message containing\"+\", \" \ud83d\udc4d \" or \"Thank you\" transfers the MHC from your balance to that user..";
                await botClient.SendTextMessageAsync(
                    chatId: chat,
                    text: text,
                    parseMode: ParseMode.Markdown,
                    disableNotification: true,
                    replyToMessageId: replyToMessageId.Value,
                    replyMarkup: replyMarkup
                );
            }
            else
            {
                await botClient.SendTextMessageAsync(
                    chatId: chat,
                    text: "Choose an option:",
                    parseMode: ParseMode.Markdown,
                    disableNotification: true,
                    replyMarkup: replyMarkup
                );
            }
        }
    }
}