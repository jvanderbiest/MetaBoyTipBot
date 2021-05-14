using System;
using System.Threading.Tasks;
using MetaBoyTipBot.Constants;
using MetaBoyTipBot.Services;
using MetaBoyTipBot.TableEntities;

namespace MetaBoyTipBot.Repositories
{
    public class UserBalanceRepository : IUserBalanceRepository
    {
        private const string TableName = AzureTableConstants.Balance.TableName;
        private const string PartitionKey = AzureTableConstants.Balance.PartitionKeyName;

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
        public async Task<UserBalanceEntity> Get(string userId)
        {
            var userBalanceEntity = await _tableStorageService.Retrieve<UserBalanceEntity>(TableName, PartitionKey, userId);
            return userBalanceEntity ?? new UserBalanceEntity(userId);
        }

        public async Task Update(UserBalanceEntity userBalanceEntity)
        {
            await _tableStorageService.InsertOrMergeEntity(TableName, userBalanceEntity);
        }
    }

    public interface IUserBalanceRepository
    {
        Task<UserBalanceEntity> Get(string userId);
        Task Update(UserBalanceEntity userBalanceEntity);
    }
}
