using System;

namespace MetaBoyTipBot.Extensions
{
    public static class DateTimeExtensions
    {
        public static int GetUnixEpochTimestamp(this DateTime startDateTime)
        {
            return (int) startDateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }
    }
}
