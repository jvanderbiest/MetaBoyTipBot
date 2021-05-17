using System;
using System.Threading.Tasks;
using MetaBoyTipBot.Constants;
using MetaBoyTipBot.Services;
using MetaBoyTipBot.TableEntities;

namespace MetaBoyTipBot.Repositories
{
    public class TransactionHistoryRepository : ITransactionHistoryRepository
    {
        private const string TableName = AzureTableConstants.TransactionHistory.TableName;

        private readonly ITableStorageService _tableStorageService;

        public TransactionHistoryRepository(ITableStorageService tableStorageService)
        {
            _tableStorageService = tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));
        }

        public async Task AddOrUpdate(TransactionHistory transaction)
        {
            await _tableStorageService.InsertOrMergeEntity(TableName, transaction);
        }

        public async Task<TransactionHistory> Get(string walletId, int transactionTimestamp)
        {
            var transactionHistory = await _tableStorageService.Retrieve<TransactionHistory>(TableName, walletId, transactionTimestamp.ToString());
            return transactionHistory;
        }
    }

    public interface ITransactionHistoryRepository
    {
        Task AddOrUpdate(TransactionHistory transaction);
        Task<TransactionHistory> Get(string newTransactionTo, int newTransactionTimestamp);
    }
}
