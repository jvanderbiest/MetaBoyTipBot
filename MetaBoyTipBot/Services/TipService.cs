﻿using System;
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
        private readonly IWithdrawalRepository _withdrawalRepository;

        public TipService(IUserBalanceRepository userBalanceRepository, IUserBalanceHistoryRepository userBalanceHistoryRepository, IWithdrawalRepository withdrawalRepository)
        {
            _userBalanceRepository = userBalanceRepository ?? throw new ArgumentNullException(nameof(userBalanceRepository));
            _userBalanceHistoryRepository = userBalanceHistoryRepository ?? throw new ArgumentNullException(nameof(userBalanceHistoryRepository));
            _withdrawalRepository = withdrawalRepository ?? throw new ArgumentNullException(nameof(withdrawalRepository));
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
        public async Task<double> TryTip(string messageText, int senderUserId, int receiverUserId)
        {
            var amount = CalculateTipTextAmount(messageText);
            if (amount <= 0) { return amount; }

            if (IsWithdrawalInProgress(senderUserId))
            {
                return 0;
            }

            var settledAmount = await SettleTip(amount, senderUserId, receiverUserId);
           
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
            if (userWithdrawal?.StartDate < DateTime.UtcNow.AddMinutes(1))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Deducts the tip from the sender and adds it to the receivers tip balance
        /// </summary>
        /// <param name="amount">The original tip amount</param>
        /// <param name="senderUserId">The id of the tip sender</param>
        /// <param name="receiverUserId">The id of the tip receiver</param>
        /// <returns>The amount that was tipped which includes the user tip default multiplier</returns>
        private async Task<double> SettleTip(double amount, int senderUserId, int receiverUserId)
        {
            var senderProfile = await _userBalanceRepository.Get(senderUserId);
            var totalAmount = senderProfile.DefaultTipAmount * amount;

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
        private double CalculateTipTextAmount(string messageText)
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
                    var tipAmountDouble = (double)tipAmount;
                    tipAmountDouble = Math.Round(tipAmountDouble, 6);
                    return tipAmountDouble;
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
    }

    public interface ITipService
    {
        Task<double> TryTip(string messageText, int senderUserId, int receiverUserId);
    }
}