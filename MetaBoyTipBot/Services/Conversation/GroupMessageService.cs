using System;
using System.Threading.Tasks;
using MetaBoyTipBot.Extensions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MetaBoyTipBot.Services.Conversation
{
    public class GroupMessageService : IMessageService
    {
        private readonly IBotService _botService;
        private readonly ITipService _tipService;

        public GroupMessageService(IBotService botService, ITipService tipService)
        {
            _botService = botService ?? throw new ArgumentNullException(nameof(botService));
            _tipService = tipService ?? throw new ArgumentNullException(nameof(tipService));
        }

        public async Task Handle(Update update)
        {
            var isReplyToMessage = update.Message.ReplyToMessage != null;

            if (isReplyToMessage)
            {
                var isReplyToBot = update.Message.ReplyToMessage?.From?.IsBot;
                var hasUserId = update.Message.From?.Id > 0;
                var isMessageFromBot = update.Message?.From?.IsBot;
                var isNotSelfTip = update.Message.ReplyToMessage.From?.Id != update.Message.From?.Id;

                if (isReplyToBot.HasValue && !isReplyToBot.Value && isMessageFromBot.HasValue && !isMessageFromBot.Value && hasUserId && isNotSelfTip)
                {
                    var senderUserId = update.Message.From.Id;
                    var receiverUserId = update.Message.ReplyToMessage.From.Id;
                    var tipAmount = await _tipService.TryTip(update.Message.Text, senderUserId, receiverUserId);

                    if (tipAmount > 0)
                    {
                        var tipFromUsername = update.Message.From.GetUserFriendlyName();

                        var tipText = $"You got tipped *{tipAmount} MHC*";
                        if (!string.IsNullOrWhiteSpace(tipFromUsername))
                        {
                            tipText += $" by *{tipFromUsername}*";
                        }

                        await _botService.SendTextMessageAsReply(
                            update.Message.Chat.Id,
                            tipText,
                            update.Message.ReplyToMessage.MessageId);
                    }
                }
            }
        }
    }
}