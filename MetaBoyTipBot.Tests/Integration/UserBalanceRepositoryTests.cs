using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetaBoyTipBot.Configuration;
using MetaBoyTipBot.Constants;
using MetaBoyTipBot.Repositories;
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
            var cloudTable = await _sut.CreateTableAsync($"integration{new Random().Next(Int32.MaxValue)}");
            Assert.NotNull(cloudTable);
        }

        [Test]
        public async Task ShouldRetrieveNullTableRecord()
        {
            var tableName = "integration";
            await _sut.CreateTableAsync(tableName);
            var userId = "123";

            var userBalanceEntity = await _sut.Retrieve<UserBalanceEntity>(tableName, "partitionKey", userId);
            Assert.IsNull(userBalanceEntity);
        }

        [Test]
        public async Task ShouldCreateAndRetrieveTableRecord()
        {
            var tableName = "integration";
            await _sut.CreateTableAsync(tableName);
            var userId = Guid.NewGuid().ToString();

            var userBalanceEntity = await _sut.Retrieve<UserBalanceEntity>(tableName, AzureTableConstants.Balance.PartitionKeyName, userId);
            Assert.IsNull(userBalanceEntity);

            userBalanceEntity = new UserBalanceEntity(userId) {Balance = 1000, Address = "address", DefaultTipAmount = 5};

            userBalanceEntity = await _sut.InsertOrMergeEntity(tableName, userBalanceEntity);

            var savedUserBalanceEntity = await _sut.Retrieve<UserBalanceEntity>(tableName, AzureTableConstants.Balance.PartitionKeyName, userId);
            Assert.IsNotNull(savedUserBalanceEntity);

            Assert.AreEqual(userBalanceEntity.PartitionKey, savedUserBalanceEntity.PartitionKey);
            Assert.AreEqual(userBalanceEntity.RowKey, savedUserBalanceEntity.RowKey);
            Assert.AreEqual(userBalanceEntity.Balance, savedUserBalanceEntity.Balance);
            Assert.AreEqual(userBalanceEntity.Address, savedUserBalanceEntity.Address);
            Assert.AreEqual(userBalanceEntity.DefaultTipAmount, savedUserBalanceEntity.DefaultTipAmount);
        }
    }
}
