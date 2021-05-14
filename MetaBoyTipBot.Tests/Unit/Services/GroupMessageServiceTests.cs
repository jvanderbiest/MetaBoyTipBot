using System.Threading.Tasks;
using MetaBoyTipBot.Services;
using Moq;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MetaBoyTipBot.Tests.Unit.Services
{
    [TestFixture]
    public class GroupMessageServiceTests
    {
        private GroupMessageService _sut;
        private Mock<IBotService> _botServiceMock;
        private MockHttpMessageHandler _mockHttp;
        private Mock<ITipService> _tipServiceMock;

        public string Token => "1234567:4TT8bAc8GHUspu3ERYn-KGcvsvGB9u_n4ddy";

        [SetUp]
        public void BeforeEachTest()
        {
            _mockHttp = new MockHttpMessageHandler();

            _botServiceMock = new Mock<IBotService>();
            _botServiceMock.Setup(x => x.Client).Returns(new TelegramBotClient(Token, _mockHttp.ToHttpClient()));

            _tipServiceMock = new Mock<ITipService>();
            _tipServiceMock.Setup(x => x.TryTip(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(10);

            _sut = new GroupMessageService(_botServiceMock.Object, _tipServiceMock.Object);
        }

        [Test]
        public async Task ShouldOnlyActOnReply()
        {
            var update = ValidUpdate;
            update.Message.ReplyToMessage = null;
            await _sut.Handle(update);
            _mockHttp.VerifyNoOutstandingExpectation();
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
            _mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task ShouldNotBeAbleToTipYourself()
        {
            var userId = 123;
            var update = ValidUpdate;
            update.Message.From.Id = userId;
            update.Message.ReplyToMessage.From.Id = userId;

            await _sut.Handle(update);
            _mockHttp.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task ShouldDoATip()
        {
            _mockHttp.Expect($"https://api.telegram.org/bot{Token}/sendMessage")
                .Respond("application/json", "{'ok' : true }");

            var update = ValidUpdate;

            await _sut.Handle(update);
            _mockHttp.VerifyNoOutstandingExpectation();
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