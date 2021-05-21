using System.Collections.Generic;
using System.Threading.Tasks;
using MetaBoyTipBot.Configuration;
using MetaBoyTipBot.Constants;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MetaBoyTipBot.Services
{
    public interface IBotService
    {
        TelegramBotClient Client { get; }
        Task ShowMainButtonMenu(long chatId, int? replyToMessageId);
        Task ShowSettingsButtonMenu(long chatId);
        Task SendTextMessage(long chatId, string text, IReplyMarkup replyMarkup = null);
        Task SendTextMessageAsReply(long chatId, string text, int replyMessageId, IReplyMarkup replyMarkup = null);
    }

    public class BotService : IBotService
    {
        public BotService(IOptions<BotConfiguration> config)
        {
            var config1 = config.Value;
            Client = new TelegramBotClient(config1.BotToken);
        }

        public TelegramBotClient Client { get; }

        public async Task ShowMainButtonMenu(long chatId, int? replyToMessageId)
        {
            // emoticons: https://charbase.com/1f4e5-unicode-inbox-tray

            var firstRow = new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData("\ud83d\udce4 Top up", CallBackConstants.TopUp),
                InlineKeyboardButton.WithCallbackData("\ud83d\udcb0 Balance", CallBackConstants.Balance),

            };
            var secondRow = new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData("\ud83d\udce5 Withdraw", CallBackConstants.WithDraw),

                    InlineKeyboardButton.WithCallbackData("\u2699 Settings", CallBackConstants.Settings)
                };

            var replyMarkup = new InlineKeyboardMarkup(new List<IEnumerable<InlineKeyboardButton>> { firstRow, secondRow });

            if (replyToMessageId.HasValue)
            {
                var text = "Welcome! MetaBoy can send MHC tips other telegram users.\r\n\r\nAny response to another user's message containing\"+\", \" \ud83d\udc4d \" or \"Thank you\" or \"!tip [amount]\" transfers the MHC from your balance to that user...\n\rSee examples here: ";

                await SendTextMessageAsReply(chatId, text, replyToMessageId.Value, replyMarkup);
            }
            else
            {
                await SendTextMessage(chatId, "Choose an option:", replyMarkup);
            }
        }

        public async Task ShowSettingsButtonMenu(long chatId)
        {
            var firstRow = new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData("\ud83c\udf81 Set single tip amount", CallBackConstants.SettingsDefaultTipAmount),
                InlineKeyboardButton.WithCallbackData("\ud834\udf21 Change wallet address", CallBackConstants.SettingsChangeWalletAddress),
            };

            var replyMarkup = new InlineKeyboardMarkup(new List<IEnumerable<InlineKeyboardButton>> { firstRow });

            await SendTextMessage(chatId, "Choose a settings option:", replyMarkup);
        }

        public async Task SendTextMessage(long chatId, string text, IReplyMarkup replyMarkup = null)
        {
            await Client.SendTextMessageAsync(
                chatId: chatId,
                text: text,
                parseMode: ParseMode.Markdown,
                disableNotification: true,
                replyMarkup: replyMarkup
            );
        }

        public async Task SendTextMessageAsReply(long chatId, string text, int replyMessageId, IReplyMarkup replyMarkup = null)
        {
            await Client.SendTextMessageAsync(
                chatId: chatId,
                text: text,
                parseMode: ParseMode.Markdown,
                disableNotification: true,
                replyToMessageId: replyMessageId,
                replyMarkup: replyMarkup
            );
        }
    }
}
