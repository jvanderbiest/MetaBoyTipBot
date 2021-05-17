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
            using (var scope = host.Services.CreateScope())
            {
                var tableStorageService = scope.ServiceProvider.GetRequiredService<ITableStorageService>();
                tableStorageService.CreateTableAsync(AzureTableConstants.UserBalance.TableName);
                tableStorageService.CreateTableAsync(AzureTableConstants.WalletUser.TableName);
                tableStorageService.CreateTableAsync(AzureTableConstants.TransactionHistory.TableName);
                tableStorageService.CreateTableAsync(AzureTableConstants.TransactionCheckHistory.TableName);
                tableStorageService.CreateTableAsync(AzureTableConstants.UserBalanceHistory.TableName);
            }
        }
    }

}
