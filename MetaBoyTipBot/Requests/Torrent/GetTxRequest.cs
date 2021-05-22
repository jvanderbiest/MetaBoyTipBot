using Newtonsoft.Json;

namespace MetaBoyTipBot.Requests.Torrent
{
    public class GetTxRequest
    {
        [JsonProperty("hash")]
        public string TxId { get; set; }
    }
}