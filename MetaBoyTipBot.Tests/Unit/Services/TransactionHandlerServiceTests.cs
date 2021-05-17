using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using MetaBoyTipBot.Configuration;
using MetaBoyTipBot.Repositories;
using MetaBoyTipBot.Responses;
using MetaBoyTipBot.Services;
using MetaBoyTipBot.TableEntities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace MetaBoyTipBot.Tests.Unit.Services
{
    [TestFixture]
    public class TransactionHandlerServiceTests
    {
        private TransactionHandlerService _sut;
        private Mock<IMhcHttpClient> _mhcHttpClientMock;
        private BotConfiguration _botConfiguration;
        private Mock<ITransactionHistoryRepository> _transactionHistoryRepositoryMock;
        private Mock<IWalletUserRepository> _walletUserRepositoryMock;
        private Mock<IUserBalanceRepository> _userBalanceRepositoryMock;
        private Mock<IUserBalanceHistoryRepository> _userBalanceHistoryRepository;
        private Mock<ITransactionCheckHistoryRepository> _transactionCheckHistoryRepositoryMock;
        private Mock<IBotService> _botServiceMock;

        [SetUp]
        public void BeforeEachTest()
        {
            var loggerMock = new Mock<ILogger<ITransactionHandlerService>>();
            _botConfiguration = new BotConfiguration();
            var botConfigurationMock = Options.Create(_botConfiguration);
            _mhcHttpClientMock = new Mock<IMhcHttpClient>();
            _transactionHistoryRepositoryMock = new Mock<ITransactionHistoryRepository>();
            _transactionCheckHistoryRepositoryMock = new Mock<ITransactionCheckHistoryRepository>();
            _walletUserRepositoryMock = new Mock<IWalletUserRepository>();
            _userBalanceRepositoryMock = new Mock<IUserBalanceRepository>();
            _userBalanceHistoryRepository = new Mock<IUserBalanceHistoryRepository>();
            _botServiceMock = new Mock<IBotService>();

            _sut = new TransactionHandlerService(loggerMock.Object, botConfigurationMock, _mhcHttpClientMock.Object,
                _transactionHistoryRepositoryMock.Object, _transactionCheckHistoryRepositoryMock.Object, _walletUserRepositoryMock.Object, _userBalanceRepositoryMock.Object,
                _userBalanceHistoryRepository.Object, _botServiceMock.Object);
        }

        [Test]
        public async Task ShouldNotImportIfAlreadyTransactionHistory()
        {
            var walletAddress = "0x002d0dd81812c0e4072a284e0b03dbf7d5d242ac70de0a916a";
            _botConfiguration.TipWalletAddress = walletAddress;
            var responseJson = ReadSampleFile("Transactions1.json");
            var fetchHistoryResponse = JsonConvert.DeserializeObject<FetchHistoryFilterResponse>(responseJson);

            _mhcHttpClientMock.Setup(x => x.FetchHistory(walletAddress)).ReturnsAsync(fetchHistoryResponse);
            _transactionHistoryRepositoryMock.Setup(x => x.Get(walletAddress, It.IsAny<int>())).ReturnsAsync(new TransactionHistory(walletAddress, 123));

            await _sut.ImportTransactions(DateTime.Parse("2021/05/15 12:13:18"));

            _walletUserRepositoryMock.VerifyNoOtherCalls();
            _userBalanceRepositoryMock.VerifyNoOtherCalls();
            _userBalanceHistoryRepository.VerifyNoOtherCalls();
        }

        [Test]
        public async Task ShouldNotImportIfNoAssociatedUserWallet()
        {
            var walletAddress = "0x002d0dd81812c0e4072a284e0b03dbf7d5d242ac70de0a916a";
            _botConfiguration.TipWalletAddress = walletAddress;
            var responseJson = ReadSampleFile("Transactions1.json");
            var fetchHistoryResponse = JsonConvert.DeserializeObject<FetchHistoryFilterResponse>(responseJson);

            _mhcHttpClientMock.Setup(x => x.FetchHistory(walletAddress)).ReturnsAsync(fetchHistoryResponse);

            _walletUserRepositoryMock.Setup(x => x.GetByWalletId(walletAddress)).Returns((WalletUser)null);

            await _sut.ImportTransactions(DateTime.Parse("2021/05/15 12:13:18"));

            _userBalanceRepositoryMock.VerifyNoOtherCalls();
            _userBalanceHistoryRepository.VerifyNoOtherCalls();
        }

        [Test]
        public async Task ShouldImport1Transaction()
        {
            var walletAddress = "0x002d0dd81812c0e4072a284e0b03dbf7d5d242ac70de0a916a";
            _botConfiguration.TipWalletAddress = walletAddress;
            var responseJson = ReadSampleFile("Transactions1.json");
            var fetchHistoryResponse = JsonConvert.DeserializeObject<FetchHistoryFilterResponse>(responseJson);
            var userId = 1234;
            var userBalance = 10.5;
            var walletUser = new WalletUser { RowKey = userId.ToString() };

            _mhcHttpClientMock.Setup(x => x.FetchHistory(walletAddress)).ReturnsAsync(fetchHistoryResponse);
            _walletUserRepositoryMock.Setup(x => x.GetByWalletId(walletAddress)).Returns(walletUser);
            _userBalanceRepositoryMock.Setup(x => x.Get(userId)).ReturnsAsync(new UserBalance(userId) { Balance = userBalance });

            DateTimeOffset dateJustBeforeLastTransaction = DateTimeOffset.FromUnixTimeSeconds(1621080798 - 1);

            await _sut.ImportTransactions(dateJustBeforeLastTransaction.DateTime);

            _userBalanceRepositoryMock.Verify(x => x.Update(It.Is<UserBalance>(ub => ub.Balance.CompareTo(35.5) == 0)), Times.Once);
            _transactionHistoryRepositoryMock.Verify(x => x.AddOrUpdate(It.IsAny<TransactionHistory>()), Times.Exactly(2));

        }

        private string ReadSampleFile(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"MetaBoyTipBot.Tests.SampleFiles.{fileName}";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                return result;
            }
        }
    }
}