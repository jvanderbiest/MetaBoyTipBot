using System.Threading.Tasks;
using MetaBoyTipBot.Responses;

namespace MetaBoyTipBot
{
    public interface IMhcHttpClient
    {
        Task<FetchHistoryFilterResponse> FetchHistory(string walletAddress);
        Task<FetchBalanceResponse> FetchBalance(string walletAddress);
        Task<GetTxResponse> GetTx(string txId);
    }
}
