using Newtonsoft.Json;

namespace MetaBoyTipBot.Requests.Torrent
{
    public class ValidateRequest
    {
        [JsonProperty("address")]
        public string Address { get; set; }
    }
}