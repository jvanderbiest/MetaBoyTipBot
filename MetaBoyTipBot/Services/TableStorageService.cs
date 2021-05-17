using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MetaBoyTipBot.Configuration;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Cosmos.Table.Queryable;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MetaBoyTipBot.Services
{
    public class TableStorageService : ITableStorageService
    {
        private readonly ILogger<ITableStorageService> _logger;
        private readonly IOptions<BotConfiguration> _botConfiguration;

        public TableStorageService(ILogger<ITableStorageService> logger, IOptions<BotConfiguration> botConfiguration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _botConfiguration = botConfiguration ?? throw new ArgumentNullException(nameof(botConfiguration));
        }

        public async Task<CloudTable> CreateTableAsync(string tableName)
        {
            var table = GetCloudTable(tableName);
            await table.CreateIfNotExistsAsync();
            return table;
        }

        public async Task DeleteTableAsync(string tableName)
        {
            var table = GetCloudTable(tableName);
            await table.DeleteIfExistsAsync();
        }

        public async Task Delete<T>(string tableName, T entity) where T : ITableEntity
        {
            try
            {
                var cloudTable = GetCloudTable(tableName);
                TableOperation deleteOperation = TableOperation.Delete(entity);
                await cloudTable.ExecuteAsync(deleteOperation);
            }
            catch (StorageException e)
            {
                _logger.LogError(e.ToString());
                throw;
            }
        }

        private CloudTable GetCloudTable(string tableName)
        {
            string storageConnectionString = _botConfiguration.Value.TableStorageConnectionString;
            CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString(storageConnectionString);

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());

            CloudTable cloudTable = tableClient.GetTableReference(tableName);

            return cloudTable;
        }

        public async Task<T> InsertOrMergeEntity<T>(string tableName, T entity) where T : ITableEntity
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            try
            {
                var cloudTable = GetCloudTable(tableName);

                // Create the InsertOrReplace table operation
                TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(entity);

                // Execute the operation.
                TableResult result = await cloudTable.ExecuteAsync(insertOrMergeOperation);
                var insertedCustomer = (T)result.Result;

                return insertedCustomer;
            }
            catch (StorageException e)
            {
                _logger.LogError(e.ToString());
                throw;
            }
        }

        public CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString)
        {
            CloudStorageAccount storageAccount;
            try
            {
                storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            }
            catch (FormatException)
            {
                _logger.LogError("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the application.");
                throw;
            }
            catch (ArgumentException)
            {
                _logger.LogError("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                throw;
            }

            return storageAccount;
        }

        public async Task<T> Retrieve<T>(string tableName, string partitionKey, string rowKey) where T : ITableEntity
        {
            try
            {
                var cloudTable = GetCloudTable(tableName);
                TableOperation retrieveOperation = TableOperation.Retrieve<T>(partitionKey, rowKey);
                TableResult result = await cloudTable.ExecuteAsync(retrieveOperation);
                return (T)result.Result;
            }
            catch (StorageException e)
            {
                _logger.LogError(e.ToString());
                throw;
            }
        }

        public T RetrieveFirst<T>(string tableName, string partitionKey) where T : ITableEntity, new()
        {
            try
            {
                var cloudTable = GetCloudTable(tableName);
                var tableQuery = new TableQuery<T>().Take(1).AsTableQuery();
                var result = cloudTable.ExecuteQuery(tableQuery).FirstOrDefault();
                return result;
            }
            catch (StorageException e)
            {
                _logger.LogError(e.ToString());
                throw;
            }
        }

        public IEnumerable<T> RetrieveByPartitionKey<T>(string tableName, string partitionKey) where T : ITableEntity, new()
        {
            try
            {
                var cloudTable = GetCloudTable(tableName);

                string partitionFilter = TableQuery.GenerateFilterCondition("PartitionKey",
                    QueryComparisons.Equal, partitionKey);

                var query = new TableQuery<T> { FilterString = partitionFilter };
                var result = cloudTable.ExecuteQuery(query);
                return result;
            }
            catch (StorageException e)
            {
                _logger.LogError(e.ToString());
                throw;
            }
        }

        public IEnumerable<T> RetrieveByRowKey<T>(string tableName, string rowKey) where T : ITableEntity, new()
        {
            try
            {
                var cloudTable = GetCloudTable(tableName);

                string partitionFilter = TableQuery.GenerateFilterCondition("RowKey",
                    QueryComparisons.Equal, rowKey);

                var query = new TableQuery<T> { FilterString = partitionFilter };
                var result = cloudTable.ExecuteQuery(query);
                return result;
            }
            catch (StorageException e)
            {
                _logger.LogError(e.ToString());
                throw;
            }
        }
    }
}