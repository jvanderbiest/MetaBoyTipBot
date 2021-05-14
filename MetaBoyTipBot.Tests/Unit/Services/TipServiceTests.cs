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

        [SetUp]
        public void BeforeEachTest()
        {
            _userBalanceRepository = new Mock<IUserBalanceRepository>();
            _sut = new TipService(_userBalanceRepository.Object);
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

            _userBalanceRepository.Setup(x => x.Get(senderUserId.ToString())).ReturnsAsync(new UserBalanceEntity { Balance = 1000 });
            _userBalanceRepository.Setup(x => x.Get(receiverUserId.ToString())).ReturnsAsync(new UserBalanceEntity());

            var amount = await _sut.TryTip(text, senderUserId, receiverUserId);
            Assert.AreEqual(tipAmount, amount);
        }

        [TestCase("👍👍", 0)]
        public async Task ShouldValidateToZeroWhenBalanceIsInsufficient(string text, int tipAmount)
        {
            var senderUserId = 1111;
            var receiverUserId = 9999;

            var senderBalance = new UserBalanceEntity { Balance = 1 };
            var receiverBalance = new UserBalanceEntity();

            _userBalanceRepository.Setup(x => x.Get(senderUserId.ToString())).ReturnsAsync(senderBalance);
            _userBalanceRepository.Setup(x => x.Get(receiverUserId.ToString())).ReturnsAsync(receiverBalance);

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

            var senderBalance = new UserBalanceEntity { Balance = 20, DefaultTipAmount = 10 };
            var receiverBalance = new UserBalanceEntity();

            _userBalanceRepository.Setup(x => x.Get(senderUserId.ToString())).ReturnsAsync(senderBalance);
            _userBalanceRepository.Setup(x => x.Get(receiverUserId.ToString())).ReturnsAsync(receiverBalance);

            var amount = await _sut.TryTip(text, senderUserId, receiverUserId);
            Assert.AreEqual(tipAmount * 2, amount);
            Assert.AreEqual(0, senderBalance.Balance);
            Assert.AreEqual(tipAmount * 2, receiverBalance.Balance);
        }
    }
}