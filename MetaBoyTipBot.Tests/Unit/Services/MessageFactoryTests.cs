using System;
using MetaBoyTipBot.Services;
using Moq;
using NUnit.Framework;
using Telegram.Bot.Types;

namespace MetaBoyTipBot.Tests.Unit.Services
{
    [TestFixture]
    public class MessageFactoryTests
    {
        private MessageFactory _sut;

        [SetUp]
        public void BeforeEachTest()
        {
            var serviceProviderMock = new Mock<IServiceProvider>();
            var botServiceMock = new Mock<IBotService>();
            var tipServiceMock = new Mock<ITipService>();
            serviceProviderMock.Setup(x => x.GetService(typeof(CallbackMessageService))).Returns(new CallbackMessageService(botServiceMock.Object));
            serviceProviderMock.Setup(x => x.GetService(typeof(PrivateMessageService))).Returns(new PrivateMessageService(botServiceMock.Object));
            serviceProviderMock.Setup(x => x.GetService(typeof(GroupMessageService))).Returns(new GroupMessageService(botServiceMock.Object, tipServiceMock.Object));
            _sut = new MessageFactory(serviceProviderMock.Object);
        }

        [Test]
        public void ShouldReturnPrivateMessageService()
        {
            var userId = 135135;
            var update = new Update
            {
                Message = new Message { Chat = new Chat { Id = userId }, From = new User { Id = userId } }
            };

            var messageService = _sut.Create(update);
            Assert.IsTrue(messageService is PrivateMessageService);
        }

        [Test]
        public void ShouldReturnGroupMessageService()
        {
            var userId = 135135;
            var chatId = 968484;

            var update = new Update
            {
                Message = new Message { Chat = new Chat { Id = chatId }, From = new User { Id = userId } }
            };

            var messageService = _sut.Create(update);
            Assert.IsTrue(messageService is GroupMessageService);
        }

        [Test]
        public void ShouldReturnCallbackMessageService()
        {
            var userId = 135135;
            var chatId = 968484;

            var update = new Update
            {
                CallbackQuery = new CallbackQuery { Data = "123" },
                Message = new Message { Chat = new Chat { Id = chatId }, From = new User { Id = userId } }
            };

            var messageService = _sut.Create(update);
            Assert.IsTrue(messageService is CallbackMessageService);
        }

        [Test]
        public void ShouldReturnNull()
        {
            var messageService = _sut.Create(new Update());
            Assert.IsNull(messageService);
        }
    }
}