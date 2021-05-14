using Newtonsoft.Json;

namespace MetaBoyTipBot.Requests.Torrent
{
    public class QueryBalanceRequest
    {
        [JsonProperty("address")]
        public string Address { get; set; }
    }
}
