using System.Threading.Tasks;
using MetaBoyTipBot.Repositories;
using MetaBoyTipBot.Services;
using MetaBoyTipBot.TableEntities;
using Moq;
using NUnit.Framework;

namespace MetaBoyTipBot.Tests.Unit.Services
{
    [TestFixture]
    public class TipServiceTests
    {
        private TipService _sut;
        private Mock<IUserBalanceRepository> _userBalanceRepository;
        private Mock<IUserBalanceHistoryRepository> _userBalanceHistoryRepository;

        [SetUp]
        public void BeforeEachTest()
        {
            _userBalanceRepository = new Mock<IUserBalanceRepository>();
            _userBalanceHistoryRepository = new Mock<IUserBalanceHistoryRepository>();
            _sut = new TipService(_userBalanceRepository.Object, _userBalanceHistoryRepository.Object);
        }

        /// <summary>
        /// In case the user message is not tip related, we need to ignore the tip
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        [TestCase("just some text")]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("!tip0")]
        public async Task ShouldValidateToZeroTip(string text)
        {
            var amount = await _sut.TryTip(text, 123, 456);
            Assert.AreEqual(0, amount);
        }

        [TestCase("👍", 1)]
        [TestCase("👍👍👍", 3)]
        [TestCase("+", 1)]
        [TestCase("+1", 1)]
        [TestCase("+alot", 1)]
        [TestCase("+++++", 5)]
        [TestCase("+50", 50)]
        [TestCase("!tip 50", 50)]
        [TestCase("!tip 50 good job", 50)]
        [TestCase("+50ze64", 50)]
        public async Task ShouldValidateToTipAmount(string text, int tipAmount)
        {
            var senderUserId = 1111;
            var receiverUserId = 9999;

            _userBalanceRepository.Setup(x => x.Get(senderUserId)).ReturnsAsync(new UserBalance { Balance = 1000 });
            _userBalanceRepository.Setup(x => x.Get(receiverUserId)).ReturnsAsync(new UserBalance());

            var amount = await _sut.TryTip(text, senderUserId, receiverUserId);
            Assert.AreEqual(tipAmount, amount);
        }

        [Test]
        public async Task ShouldSettleTotalTipGivenAndReceived()
        {
            var senderUserId = 1111;
            var receiverUserId = 9999;
            var tipAmount = 12;

            var senderUserBalance = new UserBalance {Balance = 1000, TotalTipsGiven = 10.2, TotalTipsReceived = 4.6};
            var receiverUserBalance = new UserBalance {Balance = 30.20, TotalTipsGiven = 409.5, TotalTipsReceived = 300.984};

            _userBalanceRepository.Setup(x => x.Get(senderUserId)).ReturnsAsync(senderUserBalance);
            _userBalanceRepository.Setup(x => x.Get(receiverUserId)).ReturnsAsync(receiverUserBalance);

            var amount = await _sut.TryTip($"!tip {tipAmount}", senderUserId, receiverUserId);
            Assert.AreEqual(1000 - amount, senderUserBalance.Balance);
            Assert.AreEqual(10.2 + amount, senderUserBalance.TotalTipsGiven);
            Assert.AreEqual(4.6, senderUserBalance.TotalTipsReceived);

            Assert.AreEqual(30.20 + amount, receiverUserBalance.Balance);
            Assert.AreEqual(409.5, receiverUserBalance.TotalTipsGiven);
            Assert.AreEqual(300.984 + amount, receiverUserBalance.TotalTipsReceived);
        }

        /// <summary>
        /// Tip amount allows 6 decimal digits, any other decimals need to be rounded up to those 6 decimal digits.
        /// </summary>
        /// <returns></returns>
        [TestCase("!tip 95.12345678", 95.123457)]
        [TestCase("!tip 95.12", 95.12)]
        [TestCase("!tip 95.12222222", 95.122222)]
        public async Task ShouldRoundIfMoreDecimals(string tipText, double expectedTip)
        {
            var senderUserId = 1111;
            var receiverUserId = 9999;

            _userBalanceRepository.Setup(x => x.Get(senderUserId)).ReturnsAsync(new UserBalance { Balance = 1000 });
            _userBalanceRepository.Setup(x => x.Get(receiverUserId)).ReturnsAsync(new UserBalance());

            var amount = await _sut.TryTip(tipText, senderUserId, receiverUserId);
            Assert.AreEqual(expectedTip, amount);
        }

        [TestCase("👍👍", 0)]
        public async Task ShouldValidateToZeroWhenBalanceIsInsufficient(string text, int tipAmount)
        {
            var senderUserId = 1111;
            var receiverUserId = 9999;

            var senderBalance = new UserBalance { Balance = 1 };
            var receiverBalance = new UserBalance();

            _userBalanceRepository.Setup(x => x.Get(senderUserId)).ReturnsAsync(senderBalance);
            _userBalanceRepository.Setup(x => x.Get(receiverUserId)).ReturnsAsync(receiverBalance);

            var amount = await _sut.TryTip(text, senderUserId, receiverUserId);
            Assert.AreEqual(tipAmount, amount);
            Assert.AreEqual(1, senderBalance.Balance);
            Assert.AreEqual(0, receiverBalance.Balance);
        }

        [Test]
        public async Task ShouldMultiplyWithUserDefaultTipAmount()
        {
            var text = "👍👍";
            var tipAmount = 10;

            var senderUserId = 1111;
            var receiverUserId = 9999;

            var senderBalance = new UserBalance { Balance = 20, DefaultTipAmount = 10 };
            var receiverBalance = new UserBalance();

            _userBalanceRepository.Setup(x => x.Get(senderUserId)).ReturnsAsync(senderBalance);
            _userBalanceRepository.Setup(x => x.Get(receiverUserId)).ReturnsAsync(receiverBalance);

            var amount = await _sut.TryTip(text, senderUserId, receiverUserId);
            Assert.AreEqual(tipAmount * 2, amount);
            Assert.AreEqual(0, senderBalance.Balance);
            Assert.AreEqual(tipAmount * 2, receiverBalance.Balance);
        }
    }
}