using Telegram.Bot.Types;

namespace MetaBoyTipBot.Extensions
{
    public static class MessageExtensions
    {
        /// <summary>
        /// Checks if the message is a originating from a private conversation
        /// </summary>
        /// <param name="message"></param>
        /// <returns>True or false</returns>
        public static bool IsPrivateMessage(this Message message)
        {
            if (!IsValidMessage(message))
            {
                return false;
            }

            return message.Chat.Id == message.From.Id;
        }

        /// <summary>
        /// Checks if the message is a originating from a group conversation
        /// </summary>
        /// <param name="message"></param>
        /// <returns>True or false</returns>
        public static bool IsGroupMessage(this Message message)
        {
            if (!IsValidMessage(message)) {
                return false;
            }

            return message.Chat.Id != message.From.Id;
        }

        private static bool IsValidMessage(Message message)
        {
            if (message?.Chat == null || message.From == null)
            {
                return false;
            }

            return true;
        }
    }
}
