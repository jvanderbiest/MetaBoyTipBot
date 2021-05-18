using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;

namespace MetaBoyTipBot.Tests.Integration
{
    [TestFixture]
    public class MhcHttpClientTests
    {
        private MhcHttpClient _sut;

        [SetUp]
        public void BeforeEachTest()
        {
            _sut = new MhcHttpClient(new HttpClient());
        }

        [Test]
        public async Task GetIncomingTransactionHistory()
        {
            var testAddress = "0x002d0dd81812c0e4072a284e0b03dbf7d5d242ac70de0a916a";
            var allHistory = await _sut.FetchHistory(testAddress);
            Assert.NotNull(allHistory);

        }
    }
}
