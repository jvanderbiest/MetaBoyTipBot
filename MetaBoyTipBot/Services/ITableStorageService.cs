using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace MetaBoyTipBot.Services
{
    public interface ITableStorageService
    {
        Task<T> InsertOrMergeEntity<T>(string tableName, T entity) where T : ITableEntity;
        Task<T> Retrieve<T>(string tableName, string partitionKey, string rowKey) where T : ITableEntity;
        Task<CloudTable> CreateTableAsync(string tableName);
        IEnumerable<T> RetrieveByPartitionKey<T>(string tableName, string partitionKey) where T : ITableEntity, new();
        T RetrieveFirst<T>(string tableName, string partitionKey) where T : ITableEntity, new();
        Task DeleteTableAsync(string tableName);
        Task Delete<T>(string tableName, T entity) where T : ITableEntity;
        IEnumerable<T> RetrieveByRowKey<T>(string tableName, string rowKey) where T : ITableEntity, new();
    }
}
