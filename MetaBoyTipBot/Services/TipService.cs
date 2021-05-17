using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MetaBoyTipBot.Repositories;
using MetaBoyTipBot.TableEntities;

namespace MetaBoyTipBot.Services
{
    public class TipService : ITipService
    {
        private readonly IUserBalanceRepository _userBalanceRepository;
        private readonly IUserBalanceHistoryRepository _userBalanceHistoryRepository;

        public TipService(IUserBalanceRepository userBalanceRepository, IUserBalanceHistoryRepository userBalanceHistoryRepository)
        {
            _userBalanceRepository = userBalanceRepository ?? throw new ArgumentNullException(nameof(userBalanceRepository));
            _userBalanceHistoryRepository = userBalanceHistoryRepository ?? throw new ArgumentNullException(nameof(userBalanceHistoryRepository));
        }

        public int GetTipDefault()
        {
            return 1;
        }

        /// <summary>
        /// Sends a tip to the user
        /// </summary>
        /// <param name="messageText">The text message of the user</param>
        /// <param name="senderUserId">The user sending the tip</param>
        /// <param name="receiverUserId">The user receiving the tip</param>
        /// <returns>The tip amount if successful</returns>
        public async Task<int> TryTip(string messageText, int senderUserId, int receiverUserId)
        {
            var amount = CalculateTipTextAmount(messageText);
            if (amount <= 0) { return amount; }

            var settledAmount = await SettleTip(amount, senderUserId, receiverUserId);
           
            return settledAmount;
        }

        /// <summary>
        /// Deducts the tip from the sender and adds it to the receivers tip balance
        /// </summary>
        /// <param name="amount">The original tip amount</param>
        /// <param name="senderUserId">The id of the tip sender</param>
        /// <param name="receiverUserId">The id of the tip receiver</param>
        /// <returns>The amount that was tipped which includes the user tip default multiplier</returns>
        private async Task<int> SettleTip(int amount, int senderUserId, int receiverUserId)
        {
            var senderProfile = await _userBalanceRepository.Get(senderUserId);
            var totalAmount = senderProfile.DefaultTipAmount * amount;

            if (senderProfile.Balance < totalAmount)
            {
                return 0;
            }

            var receiverProfile = await _userBalanceRepository.Get(receiverUserId);
            senderProfile.Balance -= totalAmount;
            receiverProfile.Balance += totalAmount;

            await _userBalanceRepository.Update(senderProfile);
            await _userBalanceRepository.Update(receiverProfile);

            await _userBalanceHistoryRepository.Update(new UserBalanceHistory(senderUserId, DateTime.UtcNow.Ticks) { Out = totalAmount, ToUserId = senderUserId });
            await _userBalanceHistoryRepository.Update(new UserBalanceHistory(receiverUserId, DateTime.UtcNow.Ticks) { In = totalAmount, FromUserId = senderUserId });
            return totalAmount;
        }

        /// <summary>
        /// Extract the tip from the user's input. The user can send multiple tips in a single message by repeating the tip trigger (like +, thumbs up, ...)
        /// </summary>
        /// <param name="messageText">The text that the user sends in the chat</param>
        /// <returns></returns>
        private int CalculateTipTextAmount(string messageText)
        {
            var hasText = !string.IsNullOrWhiteSpace(messageText);
            if (hasText)
            {
                var tipAmount = GetTipAmount(messageText);

                if (tipAmount == null)
                {
                    var tipMultiplier = GetTipMultiplier(messageText);
                    var tipDefault = GetTipDefault();
                    tipAmount = tipDefault * tipMultiplier;
                }

                if (tipAmount > 0)
                {
                    return (int)tipAmount;
                }
            }

            return 0;
        }

        private int GetTipMultiplier(string messageText)
        {
            var plusMultiplier = messageText.Count(x => x == '+');

            if (plusMultiplier > 0)
            {
                return plusMultiplier;
            }

            var thumbMultiplier = messageText.Count(x => x == '\ud83d');
            return thumbMultiplier;
        }

        private int? GetTipAmount(string messageText)
        {
            var tipTextMatch = GetTipTextMatch("!tip ", messageText);
            if (tipTextMatch != null)
            {
                return tipTextMatch;
            }

            tipTextMatch = GetTipTextMatch("\\+", messageText);
            return tipTextMatch;
        }

        private int? GetTipTextMatch(string prefix, string messageText)
        {
            var tipRegex = Regex.Match(messageText, $"{prefix}(\\d+)", RegexOptions.IgnoreCase);
            if (!tipRegex.Success || tipRegex.Groups.Count != 2) { return null; }

            if (int.TryParse(tipRegex.Groups[1].Value, out int tipAmount))
            {
                return tipAmount;
            }

            return null;
        }
    }

    public interface ITipService
    {
        Task<int> TryTip(string messageText, int senderUserId, int receiverUserId);
    }
}