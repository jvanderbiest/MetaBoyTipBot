using MetaBoyTipBot.Constants;
using MetaBoyTipBot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MetaBoyTipBot.Extensions
{
    public static class HostExtensions
    {
        public static void CreateAzureTables(this IHost host)
        {
            var tableStorageService = host.Services.GetRequiredService<ITableStorageService>();
            tableStorageService.CreateTableAsync(AzureTableConstants.Balance.TableName);
        }
    }

}
