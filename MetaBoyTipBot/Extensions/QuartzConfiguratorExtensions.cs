using System;
using Microsoft.Extensions.Configuration;
using Quartz;

namespace MetaBoyTipBot.Extensions
{
    public static class QuartzConfiguratorExtensions
    {
        public static void AddJobAndTrigger<T>(this IServiceCollectionQuartzConfigurator quartz, IConfiguration config) where T : IJob
        {
            string jobName = typeof(T).Name;
            var jobKey = new JobKey(jobName);
            quartz.AddJob<T>(opts => opts.WithIdentity(jobKey));

            quartz.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity(jobName + "-trigger")
                .WithSchedule(SimpleScheduleBuilder.RepeatMinutelyForever()));
        }
    }
}
