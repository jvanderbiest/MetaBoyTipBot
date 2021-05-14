using System.Threading.Tasks;
using MetaBoyTipBot.Repositories;
using MetaBoyTipBot.Services;
using Moq;
using NUnit.Framework;

namespace MetaBoyTipBot.Tests.Unit.Repositories
{
    [TestFixture]
    public class UserBalanceRepositoryTests
    {
        private UserBalanceRepository _sut;
        private Mock<ITableStorageService> _tableStorageServiceMock;

        [SetUp]
        public void BeforeEachTest()
        {
            _tableStorageServiceMock = new Mock<ITableStorageService>();
            _sut = new UserBalanceRepository(_tableStorageServiceMock.Object);
        }

        [Test]
        public async Task GetShouldReturnNewUserBalanceEntityIfNotExists()
        {
            var userId = "123";
            var userBalanceEntity = await _sut.Get(userId);

            Assert.NotNull(userBalanceEntity);
            Assert.AreEqual(userId, userBalanceEntity.RowKey);
        }
    }
}
