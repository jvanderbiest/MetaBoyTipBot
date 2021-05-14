using System.Threading.Tasks;
using MetaBoyTipBot.Services;
using Moq;
using NUnit.Framework;
using Telegram.Bot.Types;

namespace MetaBoyTipBot.Tests.Unit.Services
{
    [TestFixture]
    public class UpdateServiceTests
    {
        private UpdateService _sut;
        private Mock<IMessageFactory> _messageFactoryMock;

        [SetUp]
        public void BeforeEachTest()
        {
            _messageFactoryMock = new Mock<IMessageFactory>();
            _sut = new UpdateService(_messageFactoryMock.Object);
        }

        [Test]
        public async Task ShouldHandle()
        {
            var messageServiceMock = new Mock<IMessageService>();
            _messageFactoryMock.Setup(x => x.Create(It.IsAny<Update>())).Returns(messageServiceMock.Object);
            await _sut.Update(new Update());
            messageServiceMock.Verify(x => x.Handle(It.IsAny<Update>()), Times.Once);
        }

        [Test]
        public async Task ShouldIgnore()
        {
            var messageServiceMock = new Mock<IMessageService>();
            _messageFactoryMock.Setup(x => x.Create(It.IsAny<Update>())).Returns((IMessageService) null);
            await _sut.Update(new Update());
            messageServiceMock.Verify(x => x.Handle(It.IsAny<Update>()), Times.Never);
        }
    }
}