using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace MetaBoyTipBot.Services
{
    public interface ITableStorageService
    {
        Task<T> InsertOrMergeEntity<T>(string tableName, T entity) where T : ITableEntity;
        Task<T> Retrieve<T>(string tableName, string partitionKey, string rowKey) where T : ITableEntity;
        Task<CloudTable> CreateTableAsync(string tableName);
    }
}
