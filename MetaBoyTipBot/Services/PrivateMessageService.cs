using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MetaBoyTipBot.Constants;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MetaBoyTipBot.Services
{
    public class PrivateMessageService : IMessageService
    {
        private readonly IBotService _botService;

        public PrivateMessageService(IBotService botService)
        {
            _botService = botService ?? throw new ArgumentNullException(nameof(botService));
        }

        public async Task Handle(Update update)
        {
            if (update.Message.ReplyToMessage?.Text == ReplyConstants.EnterMetahashWallet)
            {
                var match = Regex.Match(update.Message.Text, "0[xX][0-9a-fA-F]+");

                // todo check if wallet is already registered for user on blob

                if (!match.Success)
                {
                    await _botService.Client.SendTextMessageAsync(
                        chatId: update.Message.Chat,
                        text: ReplyConstants.InvalidWalletAddress,
                        parseMode: ParseMode.Markdown,
                        disableNotification: true
                    );
                }
                else
                {
                    await _botService.Client.SendTextMessageAsync(
                        chatId: update.Message.Chat,
                        text: string.Format(ReplyConstants.TransferToDonationWallet, match.Value),
                        parseMode: ParseMode.Markdown,
                        disableNotification: true
                    );

                    await _botService.Client.SendTextMessageAsync(
                        chatId: update.Message.Chat,
                        text: ReplyConstants.TransferToDonationWalletId,
                        parseMode: ParseMode.Markdown,
                        disableNotification: true
                    );
                }
            }
            else
            {
                // emoticons: https://charbase.com/1f4e5-unicode-inbox-tray

                var firstRow = new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData("\ud83d\udce4 Top up", CallBackConstants.TopUp),
                    InlineKeyboardButton.WithCallbackData("\ud83d\udce5 Withdraw", CallBackConstants.WithDraw)
                };
                var secondRow = new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData("\ud83d\udcb0 Balance", CallBackConstants.Balance),
                    InlineKeyboardButton.WithCallbackData("\u2699 Settings", CallBackConstants.Settings)
                };

                var replyMarkup = new InlineKeyboardMarkup(new List<IEnumerable<InlineKeyboardButton>> { firstRow, secondRow });

                await _botService.Client.SendTextMessageAsync(
                    chatId: update.Message.Chat,
                    text: "Welcome! MetaBoy can send MHC tips other telegram users.\r\n\r\nAny response to another user's message containing\"+\", \" \ud83d\udc4d \" or \"Thank you\" transfers the MHC from your balance to that user..",
                    parseMode: ParseMode.Markdown,
                    disableNotification: true,
                    replyToMessageId: update.Message.MessageId,
                    replyMarkup: replyMarkup
                );
            }
        }
    }
}