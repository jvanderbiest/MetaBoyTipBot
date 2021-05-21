using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using MetaBoyTipBot.Extensions;
using Serilog;

namespace MetaBoyTipBot
{
    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("logs\\log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: null)
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                Log.Information("Application Starting.");
                var webHost = CreateHostBuilder(args).Build();
                webHost.CreateAzureTables();
                await webHost.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "The Application failed to start.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}