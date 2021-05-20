using System;
using System.Threading.Tasks;
using MetaBoyTipBot.Constants;
using MetaBoyTipBot.Repositories;
using Telegram.Bot.Types;

namespace MetaBoyTipBot.Services
{
    public class BalanceService : IBalanceService
    {
        private readonly IBotService _botService;
        private readonly IUserBalanceRepository _userBalanceRepository;

        public BalanceService(IBotService botService, IUserBalanceRepository userBalanceRepository)
        {
            _botService = botService ?? throw new ArgumentNullException(nameof(botService));
            _userBalanceRepository = userBalanceRepository ?? throw new ArgumentNullException(nameof(userBalanceRepository));
        }

        private async Task<double?> GetBalance(int userId)
        {
            var userBalance = await _userBalanceRepository.Get(userId);
            return userBalance?.Balance;
        }

        public async Task Handle(Chat chat, int chatUserId)
        {
            double balance = 0;
            var userBalance = await GetBalance(chatUserId);
            if (userBalance != null)
            {
                balance = userBalance.Value;
            }

            await _botService.SendTextMessage(chat.Id, string.Format(ReplyConstants.Balance, balance));
        }
    }

    public interface IBalanceService
    {
        Task Handle(Chat chat, int chatUserId);
    }
}