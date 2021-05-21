using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace MetaBoyTipBot.Services
{
    public interface IWithdrawalService
    {
        Task Handle(Chat chat, int chatUserId, double result);
        Task Prompt(Chat messageChat, int fromId);
    }
}
