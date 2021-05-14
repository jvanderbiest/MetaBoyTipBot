using System.Collections.Generic;
using Newtonsoft.Json;

namespace MetaBoyTipBot.Responses
{
    public class FetchHistoryResponse
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("result")]
        public List<FetchHistoryResponseResult> Result { get; set; }
    }

    public class FetchHistoryResponseResult
    {
        [JsonProperty("from")]
        public string From { get; set; }

        [JsonProperty("to")]
        public string To { get; set; }

        [JsonProperty("value")]
        public int Value { get; set; }

        [JsonProperty("transaction")]
        public string Transaction { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; }

        [JsonProperty("timestamp")]
        public int Timestamp { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("blockNumber")]
        public int BlockNumber { get; set; }

        [JsonProperty("blockIndex")]
        public int BlockIndex { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }

        [JsonProperty("publickey")]
        public string Publickey { get; set; }

        [JsonProperty("fee")]
        public int Fee { get; set; }

        [JsonProperty("realFee")]
        public int RealFee { get; set; }

        [JsonProperty("nonce")]
        public int Nonce { get; set; }

        [JsonProperty("intStatus")]
        public int IntStatus { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("isDelegate")]
        public bool IsDelegate { get; set; }

        [JsonProperty("delegate_info")]
        public DelegateInfo DelegateInfo { get; set; }

        [JsonProperty("delegate")]
        public int Delegate { get; set; }

        [JsonProperty("delegateHash")]
        public string DelegateHash { get; set; }
    }

    public class DelegateInfo
    {
        [JsonProperty("isDelegate")]
        public bool IsDelegate { get; set; }

        [JsonProperty("delegate")]
        public int Delegate { get; set; }

        [JsonProperty("delegateHash")]
        public string DelegateHash { get; set; }
    }
}
