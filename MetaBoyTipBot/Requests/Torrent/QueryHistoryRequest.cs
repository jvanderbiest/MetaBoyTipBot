using Newtonsoft.Json;

namespace MetaBoyTipBot.Requests.Torrent
{
    public class QueryHistoryRequest
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("countTx")]
        public int CountTx { get; set; }

        [JsonProperty("beginTx")]
        public int BeginTx { get; set; }
    }
}