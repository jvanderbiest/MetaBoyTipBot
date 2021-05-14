using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MetaBoyTipBot.Constants;
using MetaBoyTipBot.Requests.Torrent;
using MetaBoyTipBot.Responses;
using Newtonsoft.Json;

namespace MetaBoyTipBot
{
    public class MhcHttpClient : IMhcHttpClient
    {
        private readonly HttpClient _httpClient;

        public const string TorUrl = "http://tor.net-main.metahashnetwork.com:5795/";
        public const string ProxyUrl = "http://proxy.net-main.metahashnetwork.com:9999/";

        public MhcHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<FetchHistoryResponse> Validate(string walletAddress)
        {
            var validateRequest = new ValidateRequest
            {
                Address = walletAddress
            };

            var torrentRequest = new TorrentRequest { Params = validateRequest, Method = "validate" };
            var responseJson = await QueryTorrent(torrentRequest);
            var fetchHistoryResponse = JsonConvert.DeserializeObject<FetchHistoryResponse>(responseJson);
            return fetchHistoryResponse;
        }

        public async Task<FetchHistoryResponse> FetchHistory(string walletAddress, int? countTx, int beginTx = 0)
        {
            if (countTx > MhcConstants.HistoryLimit)
            {
                throw new Exception($"Too many transactions, max transactions: {MhcConstants.HistoryLimit}");
            }

            countTx ??= MhcConstants.HistoryLimit;

            var queryTorrentRequest = new QueryHistoryRequest
            {
                Address = walletAddress,
                CountTx = countTx.Value,
                BeginTx = beginTx
            };

            var torrentRequest = new TorrentRequest { Params = queryTorrentRequest, Method = "fetch-history" };
            var responseJson = await QueryTorrent(torrentRequest);
            var fetchHistoryResponse = JsonConvert.DeserializeObject<FetchHistoryResponse>(responseJson);
            return fetchHistoryResponse;
        }

        public async Task<FetchBalanceResponse> FetchBalance(string walletAddress)
        {
            var queryTorrentRequest = new QueryBalanceRequest
            {
                Address = walletAddress
            };

            var torrentRequest = new TorrentRequest { Params = queryTorrentRequest, Method = "fetch-balance" };
            var responseJson = await QueryTorrent(torrentRequest);
            var fetchBalanceResponse = JsonConvert.DeserializeObject<FetchBalanceResponse>(responseJson);
            return fetchBalanceResponse;
        }

        private async Task<string> QueryTorrent(TorrentRequest torrentRequest)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, TorUrl))
            {
                var json = JsonConvert.SerializeObject(torrentRequest);
                using (var stringContent = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    request.Content = stringContent;

                    var response = await _httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }
    }

    public interface IMhcHttpClient
    {
        Task<FetchHistoryResponse?> FetchHistory(string walletAddress, int? countTx, int beginTx = 0);
        Task<FetchBalanceResponse?> FetchBalance(string walletAddress);
        Task<FetchHistoryResponse> Validate(string walletAddress);
    }
}
