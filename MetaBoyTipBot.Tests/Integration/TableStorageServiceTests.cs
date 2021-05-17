using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MetaBoyTipBot.Configuration;
using MetaBoyTipBot.Constants;
using MetaBoyTipBot.Services;
using MetaBoyTipBot.TableEntities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace MetaBoyTipBot.Tests.Integration
{
    [TestFixture(Category = "Integration")]
    public class TableStorageServiceTests
    {
        private TableStorageService _sut;

        [SetUp]
        public void BeforeEachTest()
        {
            var logger = new Mock<ILogger<ITableStorageService>>();

            var botConfiguration = new BotConfiguration { TableStorageConnectionString = "UseDevelopmentStorage=true" };
            var botConfigurationOptions = Options.Create(botConfiguration);

            _sut = new TableStorageService(logger.Object, botConfigurationOptions);
        }

        [Test]
        public async Task ShouldCreateTable()
        {   
            await _sut.DeleteTableAsync("integration");
            var cloudTable = await _sut.CreateTableAsync("integration");
            Assert.NotNull(cloudTable);
        }

        [Test]
        public async Task ShouldRetrieveNullTableRecord()
        {
            var tableName = "integration";
            await _sut.DeleteTableAsync(tableName);
            await _sut.CreateTableAsync(tableName);
            var userId = "123";

            var userBalanceEntity = await _sut.Retrieve<UserBalance>(tableName, "partitionKey", userId);
            Assert.IsNull(userBalanceEntity);
        }

        [Test]
        public async Task ShouldRetrieveLastTableRecord()
        {
            var tableName = "integrationhistory";
            await _sut.DeleteTableAsync(tableName);
            await _sut.CreateTableAsync(tableName);
            var transactionCheckHistory1 = new TransactionCheckHistory();
            await _sut.InsertOrMergeEntity(tableName, transactionCheckHistory1);

            Thread.Sleep(TimeSpan.FromSeconds(2));
            var transactionCheckHistory2 = new TransactionCheckHistory();
            await _sut.InsertOrMergeEntity(tableName, transactionCheckHistory2);

            var tableRecord = _sut.RetrieveFirst<TransactionCheckHistory>(tableName,
                AzureTableConstants.TransactionCheckHistory.PartitionKey);
            Assert.NotNull(tableRecord);
            Assert.AreEqual(transactionCheckHistory2.RowKey, tableRecord.RowKey);

            await _sut.Delete(tableName, transactionCheckHistory2);

            tableRecord = _sut.RetrieveFirst<TransactionCheckHistory>(tableName,
                AzureTableConstants.TransactionCheckHistory.PartitionKey);
            Assert.NotNull(tableRecord);
            Assert.AreEqual(transactionCheckHistory1.RowKey, tableRecord.RowKey);
        }

        [Test]
        public async Task ShouldRetrieveByPartitionKey()
        {
            var tableName = "integrationhistory";
            await _sut.DeleteTableAsync(tableName);
            await _sut.CreateTableAsync(tableName);
            var partitionKey = "partition-key-1";
            var rowKey = 123;
            var walletUser = new WalletUser(partitionKey, rowKey);
            await _sut.InsertOrMergeEntity(tableName, walletUser);

            var partitionKey2 = "partition-key-2";
            var rowKey2 = 456;
            var walletUser2 = new WalletUser(partitionKey2, rowKey2);
            await _sut.InsertOrMergeEntity(tableName, walletUser2);

            var firstRecord = _sut.RetrieveByRowKey<WalletUser>(tableName, partitionKey).ToList();
            Assert.NotNull(firstRecord);
            Assert.AreEqual(1, firstRecord.Count);
            Assert.AreEqual(partitionKey, firstRecord.First().PartitionKey);
            Assert.AreEqual(rowKey.ToString(), firstRecord.First().RowKey);

            var secondRecord = _sut.RetrieveByRowKey<WalletUser>(tableName, partitionKey2).ToList();
            Assert.NotNull(secondRecord);
            Assert.AreEqual(1, secondRecord.Count);
            Assert.AreEqual(partitionKey2, secondRecord.First().PartitionKey);
            Assert.AreEqual(rowKey2.ToString(), secondRecord.First().RowKey);
        }

        [Test]
        public async Task ShouldRetrieveByRowKey()
        {
            var tableName = "integrationhistory";
            await _sut.DeleteTableAsync(tableName);
            await _sut.CreateTableAsync(tableName);
            var partitionKey = "partition-key-1";
            var rowKey = 123;
            var walletUser = new WalletUser(partitionKey, rowKey);
            await _sut.InsertOrMergeEntity(tableName, walletUser);

            var partitionKey2 = "partition-key-2";
            var rowKey2 = 456;
            var walletUser2 = new WalletUser(partitionKey2, rowKey2);
            await _sut.InsertOrMergeEntity(tableName, walletUser2);

            var firstRecord = _sut.RetrieveByRowKey<WalletUser>(tableName, rowKey.ToString()).ToList();
            Assert.NotNull(firstRecord);
            Assert.AreEqual(1, firstRecord.Count);
            Assert.AreEqual(partitionKey, firstRecord.First().PartitionKey);
            Assert.AreEqual(rowKey.ToString(), firstRecord.First().RowKey);

            var secondRecord = _sut.RetrieveByRowKey<WalletUser>(tableName, rowKey2.ToString()).ToList();
            Assert.NotNull(secondRecord);
            Assert.AreEqual(1, secondRecord.Count);
            Assert.AreEqual(partitionKey2, secondRecord.First().PartitionKey);
            Assert.AreEqual(rowKey2.ToString(), secondRecord.First().RowKey);
        }


        [Test]
        public async Task ShouldCreateAndRetrieveTableRecord()
        {
            var tableName = "integration";
            await _sut.DeleteTableAsync(tableName);
            await _sut.CreateTableAsync(tableName);
            var userId = 123;

            var userBalanceEntity = await _sut.Retrieve<UserBalance>(tableName, AzureTableConstants.UserBalance.PartitionKeyName, userId.ToString());
            Assert.IsNull(userBalanceEntity);

            userBalanceEntity = new UserBalance(userId) { Balance = 1000, Address = "address", DefaultTipAmount = 5 };

            userBalanceEntity = await _sut.InsertOrMergeEntity(tableName, userBalanceEntity);

            var savedUserBalanceEntity = await _sut.Retrieve<UserBalance>(tableName, AzureTableConstants.UserBalance.PartitionKeyName, userId.ToString());
            Assert.IsNotNull(savedUserBalanceEntity);

            Assert.AreEqual(userBalanceEntity.PartitionKey, savedUserBalanceEntity.PartitionKey);
            Assert.AreEqual(userBalanceEntity.RowKey, savedUserBalanceEntity.RowKey);
            Assert.AreEqual(userBalanceEntity.Balance, savedUserBalanceEntity.Balance);
            Assert.AreEqual(userBalanceEntity.Address, savedUserBalanceEntity.Address);
            Assert.AreEqual(userBalanceEntity.DefaultTipAmount, savedUserBalanceEntity.DefaultTipAmount);
        }
    }
}
