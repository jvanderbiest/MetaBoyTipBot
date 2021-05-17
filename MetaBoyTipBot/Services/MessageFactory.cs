using System;
using MetaBoyTipBot.Extensions;
using MetaBoyTipBot.Services.Conversation;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MetaBoyTipBot.Services
{
    public interface IMessageFactory
    {
        IMessageService Create(Update update);
    }

    public class MessageFactory : IMessageFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public MessageFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public IMessageService Create(Update update)
        {
            var isCallbackMessage = update.CallbackQuery?.Data != null;

            if (isCallbackMessage)
            {
                return (IMessageService)_serviceProvider.GetService(typeof(CallbackMessageService));
            }

            var isMessageType = update.Type == UpdateType.Message;

            if (isMessageType)
            {
                var isGroupMessage = update.Message.IsGroupMessage();

                if (isGroupMessage)
                {
                    return (IMessageService)_serviceProvider.GetService(typeof(GroupMessageService));
                }

                var isPrivateMessage = update.Message.IsPrivateMessage();

                if (isPrivateMessage)
                {
                    return (IMessageService)_serviceProvider.GetService(typeof(PrivateMessageService));
                }
            }

            return null;
        }
    }
}