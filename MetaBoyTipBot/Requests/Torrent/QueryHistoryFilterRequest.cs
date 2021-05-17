using Newtonsoft.Json;

namespace MetaBoyTipBot.Requests.Torrent
{
    public class QueryHistoryFilterRequest
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("countTxs")]
        public int CountTx { get; set; }

        [JsonProperty("beginTx")]
        public int BeginTx { get; set; }
    }
}