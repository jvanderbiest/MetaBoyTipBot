using Telegram.Bot.Types;

namespace MetaBoyTipBot.Extensions
{
    public static class UserExtensions
    {
        public static string GetUserFriendlyName(this User user)
        {
            var hasFirstName = !string.IsNullOrWhiteSpace(user.FirstName);
            return hasFirstName ? $"{user.FirstName} {user.LastName}".TrimEnd() : null;
        }
    }
}