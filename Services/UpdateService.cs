using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace MetaBoyTipBot.Services
{
    public interface IUpdateService
    {
        Task Update(Update update);
    }

    public class UpdateService : IUpdateService
    {
        private readonly IMessageFactory _messageFactory;

        public UpdateService(IMessageFactory messageFactory)
        {
            _messageFactory = messageFactory ?? throw new ArgumentNullException(nameof(messageFactory));
        }

        public async Task Update(Update update)
        {
            var messageService = _messageFactory.Create(update);

            if (messageService != null)
            {
                await messageService.Handle(update);
            }
        }
    }
}