using System.Threading.Tasks;
using MetaBoyTipBot.Configuration;
using MetaBoyTipBot.Constants;
using MetaBoyTipBot.Repositories;
using MetaBoyTipBot.Services;
using MetaBoyTipBot.TableEntities;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace MetaBoyTipBot.Tests.Unit.Services
{
    [TestFixture]
    public class BalanceServiceTests
    {
        private BalanceService _sut;
        private Mock<IBotService> _botServiceMock;
        private Mock<IUserBalanceRepository> _userBalanceRepositoryMock;

        [SetUp]
        public void BeforeEachTest()
        {
            _userBalanceRepositoryMock = new Mock<IUserBalanceRepository>();
            _botServiceMock = new Mock<IBotService>();

            _sut = new BalanceService(_botServiceMock.Object, _userBalanceRepositoryMock.Object);
        }

        [Test]
        public async Task ShouldReturnRoundedValueTo6Digits()
        {
            var userId = 123;
            var chat = new Chat { Id = 123546 };
            var balance = 0.1684685351843;
            var expectedBalance = 0.168469;

            _userBalanceRepositoryMock.Setup(x => x.Get(userId)).ReturnsAsync(new UserBalance(userId) { Balance = balance });

            await _sut.Handle(chat, userId);

            _botServiceMock.Verify(x => x.SendTextMessage(chat.Id, string.Format(ReplyConstants.Balance, expectedBalance), null), Times.Once);
            _userBalanceRepositoryMock.Verify(x => x.Get(userId), Times.Once);
            _botServiceMock.VerifyNoOtherCalls();
        }
    }
}