using System.Globalization;
using System.Threading.Tasks;
using MetaBoyTipBot.Configuration;
using MetaBoyTipBot.Constants;
using MetaBoyTipBot.Repositories;
using MetaBoyTipBot.Services;
using MetaBoyTipBot.Services.Conversation;
using MetaBoyTipBot.TableEntities;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace MetaBoyTipBot.Tests.Unit.Scenarios
{
    [TestFixture]
    public class PrivateMessageScenarioTests
    {
        private PrivateMessageService _sut;
        private Mock<IBotService> _botServiceMock;
        private Mock<IWalletUserRepository> _walletUserRepositoryMock;
        private BotConfiguration _botConfigurationMock;
        private Mock<IWithdrawalService> _withdrawalService;
        private Mock<ISettingsService> _settingsServiceMock;

        [SetUp]
        public void BeforeEachTest()
        {
            _botServiceMock = new Mock<IBotService>();
            _withdrawalService = new Mock<IWithdrawalService>();
            _walletUserRepositoryMock = new Mock<IWalletUserRepository>();
            _settingsServiceMock = new Mock<ISettingsService>();
            _botConfigurationMock = new BotConfiguration();
            var botConfigurationOptions = Options.Create(_botConfigurationMock);

            _sut = new PrivateMessageService(botConfigurationOptions, _botServiceMock.Object,
                _walletUserRepositoryMock.Object, _withdrawalService.Object, _settingsServiceMock.Object);
        }

        /// <summary>
        /// The default scenario when the says something random to the bot using a private message
        /// </summary>
        [Test]
        public async Task ShouldShowMenuIfUserSaysSomethingOutOfContext()
        {
            var update = new Update
            {
                Message = new Message
                {
                    MessageId = 123456,
                    Chat = new Chat { Id = 13556 },
                    ReplyToMessage = new Message
                    {
                        Text = "something random"
                    }
                }
            };

            await _sut.Handle(update);
            _botServiceMock.Verify(x => x.ShowMainButtonMenu(update.Message.Chat.Id, update.Message.MessageId), Times.Once);
            _botServiceMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// When the user is prompted to set their wallet we should send invalid when the wallet is wrong
        /// </summary>
        [Test]
        public async Task ShouldSendInvalidMessageOnTopUpWalletMismatch()
        {
            var update = new Update
            {
                Message = new Message
                {
                    MessageId = 123456,
                    Text = "This id is not a wallet id",
                    Chat = new Chat { Id = 13556 },
                    ReplyToMessage = new Message
                    {
                        Text = ReplyConstants.EnterTopUpMetahashWallet
                    }
                }
            };

            await _sut.Handle(update);
            _botServiceMock.Verify(x => x.SendTextMessage(update.Message.Chat.Id, ReplyConstants.InvalidWalletAddress, null), Times.Once());
            _botServiceMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// When the user is prompted to set their wallet we should send an error when it's already in use by another user
        /// </summary>
        [Test]
        public async Task ShouldSendInvalidMessageOnDuplicateTopUpWallet()
        {
            var newWalletUser = 123;
            var existingWalletUser = 456;

            var update = new Update
            {
                Message = new Message
                {
                    From = new User { Id = newWalletUser },
                    MessageId = 123456,
                    Text = "0x007c05fb709c27b08d47d05f07c745901c00d9afce047f9050",
                    Chat = new Chat { Id = 13556 },
                    ReplyToMessage = new Message
                    {
                        Text = ReplyConstants.EnterTopUpMetahashWallet
                    }
                }
            };

            _walletUserRepositoryMock.Setup(x => x.GetByWalletId(update.Message.Text)).Returns(new WalletUser(update.Message.Text, existingWalletUser));

            await _sut.Handle(update);
            _botServiceMock.Verify(x => x.SendTextMessage(update.Message.Chat.Id, ReplyConstants.WalletAddressInUse, null), Times.Once());
            _botServiceMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// When the user is prompted to set their wallet he should be able to replace his existing wallet
        /// </summary>
        [Test]
        public async Task ShouldSendDuplicateMessageOnReplaceTopUpWallet()
        {
            var existingWalletUser = 456;
            _botConfigurationMock.TipWalletAddress = "0x006e9cda31edf1907d494d2a0c8050e9daace7c880258785b1";

            var update = new Update
            {
                Message = new Message
                {
                    From = new User { Id = existingWalletUser },
                    MessageId = 123456,
                    Text = "0x007c05fb709c27b08d47d05f07c745901c00d9afce047f9050",
                    Chat = new Chat { Id = 13556 },
                    ReplyToMessage = new Message
                    {
                        Text = ReplyConstants.EnterTopUpMetahashWallet
                    }
                }
            };

            _walletUserRepositoryMock.Setup(x => x.GetByWalletId(update.Message.Text)).Returns(new WalletUser(update.Message.Text, existingWalletUser));

            await _sut.Handle(update);

            _botServiceMock.Verify(x => x.SendTextMessage(update.Message.Chat.Id, ReplyConstants.WalletAddressInUse, null), Times.Once);
            _botServiceMock.VerifyNoOtherCalls();
        }


        /// <summary>
        /// When the user is prompted to set their wallet he should be able to replace his existing wallet
        /// </summary>
        [Test]
        public async Task ShouldSendReplacedMessageOnReplaceTopUpWallet()
        {
            var existingWalletUser = 456;
            _botConfigurationMock.TipWalletAddress = "0x006e9cda31edf1907d494d2a0c8050e9daace7c880258785b1";

            var update = new Update
            {
                Message = new Message
                {
                    From = new User { Id = existingWalletUser },
                    MessageId = 123456,
                    Text = "0x007c05fb709c27b08d47d05f07c745901c00d9afce047f9050",
                    Chat = new Chat { Id = 13556 },
                    ReplyToMessage = new Message
                    {
                        Text = ReplyConstants.EnterTopUpMetahashWallet
                    }
                }
            };

            _walletUserRepositoryMock.Setup(x => x.GetByWalletId(update.Message.Text)).Returns((WalletUser) null);

            await _sut.Handle(update);

            _botServiceMock.Verify(x => x.SendTextMessage(update.Message.Chat.Id, string.Format(ReplyConstants.TransferToDonationWallet, update.Message.Text), null), Times.Once);
            _botServiceMock.Verify(x => x.SendTextMessage(update.Message.Chat.Id, _botConfigurationMock.TipWalletAddress, null), Times.Once);
            _botServiceMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// When the user is prompted to set their wallet he should be able to add a new wallet
        /// </summary>
        [Test]
        public async Task ShouldSendWalletAddOnNewWallet()
        {
            var existingWalletUser = 456;
            _botConfigurationMock.TipWalletAddress = "0x006e9cda31edf1907d494d2a0c8050e9daace7c880258785b1";

            var update = new Update
            {
                Message = new Message
                {
                    From = new User { Id = existingWalletUser },
                    MessageId = 123456,
                    Text = "0x007c05fb709c27b08d47d05f07c745901c00d9afce047f9050",
                    Chat = new Chat { Id = 13556 },
                    ReplyToMessage = new Message
                    {
                        Text = ReplyConstants.EnterTopUpMetahashWallet
                    }
                }
            };

            _walletUserRepositoryMock.Setup(x => x.GetByWalletId(update.Message.Text)).Returns((WalletUser) null);

            await _sut.Handle(update);

            _botServiceMock.Verify(x => x.SendTextMessage(update.Message.Chat.Id, string.Format(ReplyConstants.TransferToDonationWallet, update.Message.Text), null), Times.Once);
            _botServiceMock.Verify(x => x.SendTextMessage(update.Message.Chat.Id, _botConfigurationMock.TipWalletAddress, null), Times.Once);
            _botServiceMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// When the user is prompted to set their wallet he should be able to add a new wallet and continue the withdrawal flow
        /// </summary>
        [Test]
        public async Task ShouldSetWalletAndContinueWithdrawalFlow()
        {
            var existingWalletUser = 456;
            _botConfigurationMock.TipWalletAddress = "0x006e9cda31edf1907d494d2a0c8050e9daace7c880258785b1";

            var update = new Update
            {
                Message = new Message
                {
                    From = new User { Id = existingWalletUser },
                    MessageId = 123456,
                    Text = "0x007c05fb709c27b08d47d05f07c745901c00d9afce047f9050",
                    Chat = new Chat { Id = 13556 },
                    ReplyToMessage = new Message
                    {
                        Text = ReplyConstants.EnterWithdrawalWallet
                    }
                }
            };

            _walletUserRepositoryMock.Setup(x => x.GetByWalletId(update.Message.Text)).Returns((WalletUser)null);

            await _sut.Handle(update);

            _botServiceMock.Verify(x => x.SendTextMessage(update.Message.Chat.Id, string.Format(ReplyConstants.WithdrawalWalletConfirmation, update.Message.Text), null), Times.Once);
            _botServiceMock.Verify(x => x.SendTextMessage(update.Message.Chat.Id, ReplyConstants.EnterWithdrawalAmount, It.Is<ForceReplyMarkup>(y => !y.Selective)), Times.Once);
            _botServiceMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// When the user is prompted to enter their withdrawal amount
        /// </summary>
        [Test]
        public async Task ShouldEnterWithdrawalAmount()
        {
            var amount = 15.2132;

            var update = new Update
            {
                Message = new Message
                {
                    From = new User { Id = 123 },
                    MessageId = 123456,
                    Text = amount.ToString(CultureInfo.InvariantCulture),
                    Chat = new Chat { Id = 13556 },
                    ReplyToMessage = new Message
                    {
                        Text = ReplyConstants.EnterWithdrawalAmount
                    }
                }
            };

            await _sut.Handle(update);

            _withdrawalService.Verify(x => x.Handle(update.Message.Chat, update.Message.From.Id, amount), Times.Once);
        }

        /// <summary>
        /// When the user is prompted to enter their withdrawal amount it should fail when amount is invalid
        /// </summary>
        [Test]
        public async Task ShouldEnterWithdrawalAmountButSendInvalid()
        {
            var update = new Update
            {
                Message = new Message
                {
                    From = new User { Id = 123 },
                    MessageId = 123456,
                    Text = "not a double",
                    Chat = new Chat { Id = 13556 },
                    ReplyToMessage = new Message
                    {
                        Text = ReplyConstants.EnterWithdrawalAmount
                    }
                }
            };

            await _sut.Handle(update);

            _botServiceMock.Verify(x => x.SendTextMessage(update.Message.Chat.Id, ReplyConstants.InvalidAmount, null), Times.Once);
            _botServiceMock.VerifyNoOtherCalls();
            _walletUserRepositoryMock.VerifyNoOtherCalls();
        }
    }
}