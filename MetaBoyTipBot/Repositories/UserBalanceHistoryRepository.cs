using System;
using System.Threading.Tasks;
using MetaBoyTipBot.Constants;
using MetaBoyTipBot.Services;
using MetaBoyTipBot.TableEntities;

namespace MetaBoyTipBot.Repositories
{
    public class UserBalanceHistoryRepository : IUserBalanceHistoryRepository
    {
        private const string TableName = AzureTableConstants.UserBalanceHistory.TableName;

        private readonly ITableStorageService _tableStorageService;

        public UserBalanceHistoryRepository(ITableStorageService tableStorageService)
        {
            _tableStorageService = tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
        }

        public async Task Update(UserBalanceHistory userBalanceHistory)
        {
            await _tableStorageService.InsertOrMergeEntity(TableName, userBalanceHistory);
        }
    }

    public interface IUserBalanceHistoryRepository
    {
        Task Update(UserBalanceHistory userBalance);
    }
}
