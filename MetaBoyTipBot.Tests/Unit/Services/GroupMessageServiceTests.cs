using System.Threading.Tasks;
using Castle.Core.Logging;
using MetaBoyTipBot.Services;
using MetaBoyTipBot.Services.Conversation;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Telegram.Bot.Types;

namespace MetaBoyTipBot.Tests.Unit.Services
{
    [TestFixture]
    public class GroupMessageServiceTests
    {
        private GroupMessageService _sut;
        private Mock<IBotService> _botServiceMock;
        private Mock<ITipService> _tipServiceMock;

        [SetUp]
        public void BeforeEachTest()
        {
            var loggerMock = new Mock<ILogger<IMessageService>>();
            _botServiceMock = new Mock<IBotService>();

            _tipServiceMock = new Mock<ITipService>();
            _tipServiceMock.Setup(x => x.TryTip(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(10);

            _sut = new GroupMessageService(loggerMock.Object, _botServiceMock.Object, _tipServiceMock.Object);
        }

        [Test]
        public async Task ShouldOnlyActOnReply()
        {
            var update = ValidUpdate;
            update.Message.ReplyToMessage = null;
            await _sut.Handle(update);

            _botServiceMock.VerifyNoOtherCalls();;
        }

        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        public async Task ShouldOnlyActOnUserMessages(bool isMessageBot, bool isReplyBot)
        {
            var update = ValidUpdate;
            update.Message.From.IsBot = isMessageBot;
            update.Message.ReplyToMessage.From.IsBot = isReplyBot;

            await _sut.Handle(update);
            _botServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task ShouldNotBeAbleToTipYourself()
        {
            var userId = 123;
            var update = ValidUpdate;
            update.Message.From.Id = userId;
            update.Message.ReplyToMessage.From.Id = userId;

            await _sut.Handle(update);
            _botServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task ShouldDoATip()
        {
            var update = ValidUpdate;
            var expectedTipText = "You got tipped *10 MHC*";

            await _sut.Handle(update);

            _botServiceMock.Verify(x => x.SendTextMessageAsReply(update.Message.Chat.Id, expectedTipText, update.Message.ReplyToMessage.MessageId, null), Times.Once);
            _botServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task ShouldDoATipWithName()
        {
            var update = ValidUpdate;
            update.Message.From.FirstName = "First";
            update.Message.From.LastName = "LastName";

            var expectedTipText = "You got tipped *10 MHC* by *First LastName*";

            await _sut.Handle(update);

            _botServiceMock.Verify(x => x.SendTextMessageAsReply(update.Message.Chat.Id, expectedTipText, update.Message.ReplyToMessage.MessageId, null), Times.Once);
            _botServiceMock.VerifyNoOtherCalls();
        }

        public Update ValidUpdate => new()
        {
            Message = new Message
            {
                Chat = new Chat { Id = 1 },
                Text = "thank you",
                From = new User { Id = 2
                },
                ReplyToMessage = new Message
                {
                    MessageId = 4,
                    From = new User { Id = 3 }
                }
            }
        };
    }
}