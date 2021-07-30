using System;
using System.Linq;
using System.Threading.Tasks;
using MetaBoyTipBot.Constants;
using MetaBoyTipBot.Services;
using MetaBoyTipBot.TableEntities;

namespace MetaBoyTipBot.Repositories
{
    public class WithdrawalRepository : IWithdrawalRepository
    {
        private const string TableName = AzureTableConstants.Withdrawal.TableName;

        private readonly ITableStorageService _tableStorageService;

        public WithdrawalRepository(ITableStorageService tableStorageService)
        {
            _tableStorageService = tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
        }

        public async Task AddOrUpdate(UserWithdrawal transaction)
        {
            await _tableStorageService.InsertOrMergeEntity(TableName, transaction);
        }

        public UserWithdrawal GetByUserId(int userId)
        {
            var userWithdrawals = _tableStorageService.RetrieveByPartitionKey<UserWithdrawal>(TableName, userId.ToString());
            return userWithdrawals?.FirstOrDefault();
        }
    }

    public interface IWithdrawalRepository
    {
        Task AddOrUpdate(UserWithdrawal transaction);
        UserWithdrawal GetByUserId(int userId);
    }
}
