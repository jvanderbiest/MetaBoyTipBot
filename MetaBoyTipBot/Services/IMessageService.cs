using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace MetaBoyTipBot.Services
{
    public interface IMessageService
    {
        Task Handle(Update update);
    }
}