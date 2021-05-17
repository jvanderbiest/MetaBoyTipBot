using System;
using System.Threading.Tasks;
using MetaBoyTipBot.Constants;
using MetaBoyTipBot.Services;
using MetaBoyTipBot.TableEntities;

namespace MetaBoyTipBot.Repositories
{
    public class UserBalanceRepository : IUserBalanceRepository
    {
        private const string TableName = AzureTableConstants.UserBalance.TableName;
        private const string PartitionKey = AzureTableConstants.UserBalance.PartitionKeyName;

        private readonly ITableStorageService _tableStorageService;

        public UserBalanceRepository(ITableStorageService tableStorageService)
        {
            _tableStorageService = tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
        }

        /// <summary>
        /// Returns the balance record for a specific user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<UserBalance> Get(int userId)
        {
            var userBalanceEntity = await _tableStorageService.Retrieve<UserBalance>(TableName, PartitionKey, userId.ToString());
            return userBalanceEntity ?? new UserBalance(userId);
        }

        public async Task Update(UserBalance userBalance)
        {
            await _tableStorageService.InsertOrMergeEntity(TableName, userBalance);
        }
    }

    public interface IUserBalanceRepository
    {
        Task<UserBalance> Get(int userId);
        Task Update(UserBalance userBalance);
    }
}
