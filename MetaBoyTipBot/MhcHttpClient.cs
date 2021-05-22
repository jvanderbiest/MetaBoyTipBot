using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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
       
        public async Task<FetchHistoryFilterResponse> FetchHistory(string walletAddress)
        {
            var queryTorrentRequest = new QueryHistoryFilterRequest
            {
                Address = walletAddress,
                CountTx = 999,
                BeginTx = 0
            };

            var torrentRequest = new MhcRequest { Params = queryTorrentRequest, Method = "fetch-history" };
            var responseJson = await QueryTorrent(torrentRequest);
            var fetchHistoryResponse = JsonConvert.DeserializeObject<FetchHistoryFilterResponse>(responseJson);
            return fetchHistoryResponse;
        }

        public async Task<FetchBalanceResponse> FetchBalance(string walletAddress)
        {
            var queryTorrentRequest = new QueryBalanceRequest
            {
                Address = walletAddress
            };

            var torrentRequest = new MhcRequest { Params = queryTorrentRequest, Method = "fetch-balance" };
            var responseJson = await QueryTorrent(torrentRequest);
            var fetchBalanceResponse = JsonConvert.DeserializeObject<FetchBalanceResponse>(responseJson);
            return fetchBalanceResponse;
        }

        public async Task<GetTxResponse> GetTx(string txId)
        {
            var getTxRequest = new GetTxRequest
            {
                 TxId = txId
            };

            var torrentRequest = new MhcRequest { Params = getTxRequest, Method = "get-tx" };
            var responseJson = await QueryTorrent(torrentRequest);
            var getTxResponse = JsonConvert.DeserializeObject<GetTxResponse>(responseJson);
            return getTxResponse;
        }

        private async Task<string> QueryTorrent(MhcRequest mhcRequest)
        {
            return await Send(mhcRequest, TorUrl);
        }

        private async Task<string> Send(MhcRequest mhcRequest, string url)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                var json = JsonConvert.SerializeObject(mhcRequest);
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
}