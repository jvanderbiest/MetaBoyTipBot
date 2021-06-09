using System.Threading.Tasks;
using MetaBoyTipBot.Constants;
using MetaBoyTipBot.Extensions;
using MetaBoyTipBot.Repositories;
using MetaBoyTipBot.Responses;
using MetaBoyTipBot.Services;
using MetaBoyTipBot.TableEntities;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace MetaBoyTipBot.Tests.Unit.Services
{
    [TestFixture]
    public class WithdrawalServiceTests
    {
        private WithdrawalService _sut;
        private Mock<IWithdrawalRepository> _withdrawalRepositoryMock;
        private Mock<IWalletUserRepository> _walletUserRepositoryMock;
        private Mock<IBotService> _botServiceMock;
        private Mock<INodeExecutionService> _nodeExecutionServiceMock;
        private Mock<IMhcHttpClient> _mhcHttpClientMock;
        private Mock<IUserBalanceRepository> _userBalanceRepositoryMock;

        [SetUp]
        public void BeforeEachTest()
        {
            var loggerMock = new Mock<ILogger<WithdrawalService>>();
            _userBalanceRepositoryMock = new Mock<IUserBalanceRepository>();
            _withdrawalRepositoryMock = new Mock<IWithdrawalRepository>();
            _walletUserRepositoryMock = new Mock<IWalletUserRepository>();
            _botServiceMock = new Mock<IBotService>();
            _nodeExecutionServiceMock = new Mock<INodeExecutionService>();
            _mhcHttpClientMock = new Mock<IMhcHttpClient>();

            _sut = new WithdrawalService(loggerMock.Object, _walletUserRepositoryMock.Object, _botServiceMock.Object, _nodeExecutionServiceMock.Object, _withdrawalRepositoryMock.Object, _mhcHttpClientMock.Object, _userBalanceRepositoryMock.Object);
        }

        [Test]
        public async Task ShouldNotBeAbleToWithdrawMoreThanBalance()
        {
            var walletAddress = "1234";
            var userId = 1111;

            double withdrawAmount = 6;

            var walletUser = new WalletUser(walletAddress, userId);
            var userBalance = new UserBalance { Balance = 5 };

            var chat = new Chat { Id = 123456789 };

            _walletUserRepositoryMock.Setup(x => x.GetByUserId(userId)).Returns(walletUser);
            _userBalanceRepositoryMock.Setup(x => x.Get(userId)).ReturnsAsync(userBalance);

            await _sut.Handle(chat, userId, withdrawAmount);

            _botServiceMock.Verify(x => x.SendTextMessage(chat.Id,
                string.Format(ReplyConstants.InsufficientBalance, userBalance.Balance.RoundMetahashHash(), withdrawAmount.RoundMetahashHash())
                , It.IsAny<IReplyMarkup>()));
            _botServiceMock.VerifyNoOtherCalls();
            _withdrawalRepositoryMock.VerifyNoOtherCalls();
            _mhcHttpClientMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task ShouldBeAbleToWithdrawExactBalance()
        {
            var walletAddress = "1234";
            var userId = 1111;

            double withdrawAmount = 6;

            var walletUser = new WalletUser(walletAddress, userId);
            var userBalance = new UserBalance { Balance = 6 };

            var chat = new Chat { Id = 123456789 };

            var txId = "76c93d17bed19ab03354c37668d54f29ef46bbeee3cbc4b0de52ec8f2f53171f";

            _walletUserRepositoryMock.Setup(x => x.GetByUserId(userId)).Returns(walletUser);
            _userBalanceRepositoryMock.Setup(x => x.Get(userId)).ReturnsAsync(userBalance);
            _nodeExecutionServiceMock.Setup(x => x.Withdraw(walletAddress, withdrawAmount)).ReturnsAsync(txId);
            _mhcHttpClientMock.Setup(x => x.GetTx(txId)).ReturnsAsync(new GetTxResponse
                {Id = 86448648653, TxResult = new TxResult {Transaction = new Transaction {Status = "ok"}}});

            await _sut.Handle(chat, userId, withdrawAmount);

            // todo could use some extra verify
            _userBalanceRepositoryMock.Verify(x => x.Update(It.Is<UserBalance>(x => x.Balance == 0)), Times.Once);
            _botServiceMock.Verify(x => x.SendTextMessage(chat.Id, ReplyConstants.WithdrawVerification, It.IsAny<IReplyMarkup>()));
            _botServiceMock.Verify(x => x.SendTextMessage(chat.Id, ReplyConstants.WithdrawalSuccess, It.IsAny<IReplyMarkup>()));
            _botServiceMock.VerifyNoOtherCalls();
            _mhcHttpClientMock.Verify(x => x.GetTx(txId), Times.Once);
            _mhcHttpClientMock.VerifyNoOtherCalls();
        }
    }
}