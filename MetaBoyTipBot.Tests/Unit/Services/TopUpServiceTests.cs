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
    public class TopUpServiceTests
    {
        private TopUpService _sut;
        private BotConfiguration _botConfigurationMock;
        private Mock<IBotService> _botServiceMock;
        private Mock<IWalletUserRepository> _walletUserRepositoryMock;

        [SetUp]
        public void BeforeEachTest()
        {
            _walletUserRepositoryMock = new Mock<IWalletUserRepository>();
            _botServiceMock = new Mock<IBotService>();
            _botConfigurationMock = new BotConfiguration();
            var botConfigurationOptions = Options.Create(_botConfigurationMock);

            _sut = new TopUpService(botConfigurationOptions, _walletUserRepositoryMock.Object, _botServiceMock.Object);
        }

        [Test]
        public async Task ShouldInformUserToTopUpWhenExistingWallet()
        {
            var userId = 123;
            var userWalletAddress = "0x123";
            var tipWalletAddress = "0x987";
            _botConfigurationMock.TipWalletAddress = tipWalletAddress;
            var chat = new Chat { Id = 123546 };

            _walletUserRepositoryMock.Setup(x => x.GetByUserId(userId)).Returns(new WalletUser(userWalletAddress, userId));

            await _sut.Handle(chat, userId);

            _botServiceMock.Verify(x => x.SendTextMessage(chat.Id, string.Format(ReplyConstants.CurrentWallet, userWalletAddress), null), Times.Once);
            _botServiceMock.Verify(x => x.SendTextMessage(chat.Id, tipWalletAddress, null), Times.Once);
            _botServiceMock.Verify(x => x.ShowMainButtonMenu(chat.Id, null), Times.Once);
            _botServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task ShouldAskUserForWalletWhenNoExistingWallet()
        {
            var userId = 123;
            var chat = new Chat { Id = 123546 };

            _walletUserRepositoryMock.Setup(x => x.GetByUserId(userId)).Returns((WalletUser) null);

            await _sut.Handle(chat, userId);

            _botServiceMock.Verify(x => x.SendTextMessage(chat.Id, ReplyConstants.EnterTopUpMetahashWallet, It.Is<ForceReplyMarkup>(x => !x.Selective)), Times.Once);
            _botServiceMock.VerifyNoOtherCalls();
        }
    }
}