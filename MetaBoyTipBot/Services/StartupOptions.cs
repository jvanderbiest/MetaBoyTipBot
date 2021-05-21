using System;

namespace MetaBoyTipBot.Services
{
    public class StartupOptions : IStartupOptions
    {
        public StartupOptions()
        {
            StartDateTime = DateTime.UtcNow;
        }

        public DateTime StartDateTime { get; }
    }

    public interface IStartupOptions
    {
        DateTime StartDateTime { get; }
    }
}