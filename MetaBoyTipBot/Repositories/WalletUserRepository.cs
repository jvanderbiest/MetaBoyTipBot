using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MetaBoyTipBot.Constants;
using MetaBoyTipBot.Services;
using MetaBoyTipBot.TableEntities;

namespace MetaBoyTipBot.Repositories
{
    public class WalletUserRepository : IWalletUserRepository
    {
        private const string TableName = AzureTableConstants.WalletUser.TableName;

        private readonly ITableStorageService _tableStorageService;

        public WalletUserRepository(ITableStorageService tableStorageService)
        {
            _tableStorageService = tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
        }

        public async Task AddOrUpdate(WalletUser transaction)
        {
            await _tableStorageService.InsertOrMergeEntity(TableName, transaction);
        }

        public async Task Delete(WalletUser transaction)
        {
            await _tableStorageService.Delete(TableName, transaction);
        }

        public WalletUser GetByWalletId(string walletId)
        {
            var walletUsers = _tableStorageService.RetrieveByPartitionKey<WalletUser>(TableName, walletId);
            return walletUsers?.FirstOrDefault();
        }

        public WalletUser GetByUserId(int userId)
        {
            var walletUsers = _tableStorageService.RetrieveByRowKey<WalletUser>(TableName, userId.ToString());
            return walletUsers?.FirstOrDefault();
        }

        public IEnumerable<WalletUser> GetByUserIdDuplicates(int userId)
        {
            var walletUsers = _tableStorageService.RetrieveByRowKey<WalletUser>(TableName, userId.ToString());
            return walletUsers;
        }
    }

    public interface IWalletUserRepository
    {
        Task AddOrUpdate(WalletUser transaction);
        WalletUser GetByWalletId(string walletId);
        Task Delete(WalletUser transaction);
        WalletUser GetByUserId(int userId);
        IEnumerable<WalletUser> GetByUserIdDuplicates(int userId);
    }
}
