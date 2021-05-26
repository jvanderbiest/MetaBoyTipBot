using System;

namespace MetaBoyTipBot.Extensions
{
    public static class DoubleExtensions
    {
        /// <summary>
        /// Rounds an amount to the lowest HASH that is possible for MetaHash which is 6 digits
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static double RoundMetahashHash(this double amount)
        {
            return Math.Round(amount, 6);
        }
    }
}
