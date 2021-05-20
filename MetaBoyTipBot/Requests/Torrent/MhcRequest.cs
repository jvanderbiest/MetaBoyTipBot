using System;
using Newtonsoft.Json;

namespace MetaBoyTipBot.Requests.Torrent
{
    public class MhcRequest
    {
        public MhcRequest()
        {
            JsonRpc = "2.0";
            Id = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        [JsonProperty("params")]
        public object Params { get; set; }

        [JsonProperty("id")]
        public int Id { get; }

        [JsonProperty("jsonrpc")]
        public string JsonRpc { get; }

        [JsonProperty("method")]
        public string Method { get; set; }
    }
}