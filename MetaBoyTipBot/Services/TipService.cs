using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MetaBoyTipBot.Extensions;
using MetaBoyTipBot.Repositories;
using MetaBoyTipBot.TableEntities;
using Microsoft.Extensions.Logging;

namespace MetaBoyTipBot.Services
{
    public class TipService : ITipService
    {
        private readonly ILogger<ITipService> _logger;
        private readonly IUserBalanceRepository _userBalanceRepository;
        private readonly IUserBalanceHistoryRepository _userBalanceHistoryRepository;
        private readonly IWithdrawalRepository _withdrawalRepository;

        public TipService(ILogger<ITipService> logger, IUserBalanceRepository userBalanceRepository, IUserBalanceHistoryRepository userBalanceHistoryRepository, IWithdrawalRepository withdrawalRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userBalanceRepository = userBalanceRepository ?? throw new ArgumentNullException(nameof(userBalanceRepository));
            _userBalanceHistoryRepository = userBalanceHistoryRepository ?? throw new ArgumentNullException(nameof(userBalanceHistoryRepository));
            _withdrawalRepository = withdrawalRepository ?? throw new ArgumentNullException(nameof(withdrawalRepository));
        }

        /// <summary>
        /// Sends a tip to the user
        /// </summary>
        /// <param name="messageText">The text message of the user</param>
        /// <param name="senderUserId">The user sending the tip</param>
        /// <param name="receiverUserId">The user receiving the tip</param>
        /// <returns>The tip amount if successful</returns>
        public async Task<double> TryTip(string messageText, int senderUserId, int receiverUserId)
        {
            var tipResult = CalculateTipTextAmount(messageText);
            if (tipResult == null || tipResult?.Amount <= 0) { return 0; }

            if (IsWithdrawalInProgress(senderUserId))
            {
                return 0;
            }

            var settledAmount = await SettleTip(tipResult, senderUserId, receiverUserId);
           
            return settledAmount;
        }

        /// <summary>
        /// Block the user giving tips if withdrawal is in progress
        /// </summary>
        /// <param name="senderUserId"></param>
        /// <returns></returns>
        private bool IsWithdrawalInProgress(int senderUserId)
        {
            var userWithdrawal = _withdrawalRepository.GetByUserId(senderUserId);
            if (userWithdrawal != null && userWithdrawal.State == WithdrawalState.Failed && userWithdrawal?.StartDate > DateTime.UtcNow.AddMinutes(-15))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Deducts the tip from the sender and adds it to the receivers tip balance
        /// </summary>
        /// <param name="tipAmountResult"></param>
        /// <param name="senderUserId">The id of the tip sender</param>
        /// <param name="receiverUserId">The id of the tip receiver</param>
        /// <returns>The amount that was tipped which includes the user tip default multiplier</returns>
        private async Task<double> SettleTip(TipAmountResult tipAmountResult, int senderUserId, int receiverUserId)
        {
            if (tipAmountResult == null || tipAmountResult.Amount <= 0) { throw new Exception("Tip amount cannot be null or 0"); }

            var senderProfile = await _userBalanceRepository.Get(senderUserId);

            double totalAmount = 0;
            if (tipAmountResult.RequiredMultiplier) {
                totalAmount = senderProfile.DefaultTipAmount * tipAmountResult.Amount;
            }
            else
            {
                totalAmount = tipAmountResult.Amount;
            }

            if (senderProfile.Balance < totalAmount)
            {
                return 0;
            }

            var receiverProfile = await _userBalanceRepository.Get(receiverUserId);
            senderProfile.GiveTip(totalAmount);
            receiverProfile.ReceiveTip(totalAmount);

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
        private TipAmountResult CalculateTipTextAmount(string messageText)
        {
            var hasText = !string.IsNullOrWhiteSpace(messageText);
            if (hasText)
            {
                var tipAmount = GetTipAmount(messageText);

                if (tipAmount == null)
                {
                    var tipMultiplier = GetTipMultiplier(messageText);
                    return new TipAmountResult { Amount = tipMultiplier, RequiredMultiplier = true };
                }

                if (tipAmount > 0)
                {
                    var tipAmountDouble = (double)tipAmount;
                    tipAmountDouble = tipAmountDouble.RoundMetahashHash();
                    return new TipAmountResult {Amount = tipAmountDouble, RequiredMultiplier = false};
                }
            }

            return null;
        }

        private int GetTipMultiplier(string messageText)
        {
            var plusMultiplier = messageText.Count(x => x == '+');

            if (plusMultiplier > 0)
            {
                return plusMultiplier;
            }

            var thumbMultiplier = Regex.Matches(messageText, "\ud83d\udc4d").Count;
            return thumbMultiplier;
        }

        private double? GetTipAmount(string messageText)
        {
            var tipTextMatch = GetTipTextMatch("!tip ", messageText);
            if (tipTextMatch != null)
            {
                return tipTextMatch;
            }

            tipTextMatch = GetTipTextMatch("\\+", messageText);
            return tipTextMatch;
        }

        private double? GetTipTextMatch(string prefix, string messageText)
        {
            var tipRegex = Regex.Match(messageText, $"{prefix}(\\d+[\\.\\,]?\\d*)", RegexOptions.IgnoreCase);
            if (!tipRegex.Success || tipRegex.Groups.Count != 2) { return null; }

            if (double.TryParse(tipRegex.Groups[1].Value, out double tipAmount))
            {
                return tipAmount;
            }

            return null;
        }

        private bool IsTipText(string messageText)
        {
            var tipAmount = GetTipTextMatch("!tip ", messageText);
            if (tipAmount != null)
            {
                return true;
            }

            return false;
        }
    }

    public interface ITipService
    {
        Task<double> TryTip(string messageText, int senderUserId, int receiverUserId);
    }

    public class TipAmountResult
    {
        private double _amount;
        public bool RequiredMultiplier { get; set; }

        public double Amount
        {
            get => _amount;
            set => _amount = value.RoundMetahashHash();
        }
    }
}