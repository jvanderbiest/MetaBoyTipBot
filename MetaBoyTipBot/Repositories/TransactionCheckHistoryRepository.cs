using System;
using System.Threading.Tasks;
using MetaBoyTipBot.Constants;
using MetaBoyTipBot.Services;
using MetaBoyTipBot.TableEntities;

namespace MetaBoyTipBot.Repositories
{
    public class TransactionCheckHistoryRepository : ITransactionCheckHistoryRepository
    {
        private const string TableName = AzureTableConstants.TransactionCheckHistory.TableName;

        private readonly ITableStorageService _tableStorageService;

        public TransactionCheckHistoryRepository(ITableStorageService tableStorageService)
        {
            _tableStorageService = tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
        }

        public async Task AddOrUpdate(TransactionCheckHistory transaction)
        {
            await _tableStorageService.InsertOrMergeEntity(TableName, transaction);
        }

        public TransactionCheckHistory GetLatest()
        {
            var transactionHistory = _tableStorageService.RetrieveFirst< TransactionCheckHistory>(TableName, AzureTableConstants.TransactionCheckHistory.PartitionKey);
            return transactionHistory;
        }
    }

    public interface ITransactionCheckHistoryRepository
    {
        Task AddOrUpdate(TransactionCheckHistory transaction);
        TransactionCheckHistory GetLatest();
    }
}
