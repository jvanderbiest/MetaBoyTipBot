using System;
using MetaBoyTipBot.Extensions;
using Newtonsoft.Json;

namespace MetaBoyTipBot.Requests.Torrent
{
    public class MhcRequest
    {
        public MhcRequest()
        {
            JsonRpc = "2.0";
            Id = DateTime.UtcNow.GetUnixEpochTimestamp();
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