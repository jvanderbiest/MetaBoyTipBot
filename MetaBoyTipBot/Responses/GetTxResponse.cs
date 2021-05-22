using Newtonsoft.Json;

namespace MetaBoyTipBot.Responses
{
    public class GetTxResponse
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("result")]
        public TxResult TxResult { get; set; }
    }

    public class TxResult
    {
        [JsonProperty("transaction")]
        public Transaction Transaction { get; set; }

        [JsonProperty("countBlocks")]
        public long CountBlocks { get; set; }

        [JsonProperty("knownBlocks")]
        public long KnownBlocks { get; set; }
    }

    public class Transaction
    {
        [JsonProperty("from")]
        public string From { get; set; }

        [JsonProperty("to")]
        public string To { get; set; }

        [JsonProperty("value")]
        public long Value { get; set; }

        [JsonProperty("transaction")]
        public string TransactionTransaction { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("blockNumber")]
        public long BlockNumber { get; set; }

        [JsonProperty("blockIndex")]
        public long BlockIndex { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }

        [JsonProperty("publickey")]
        public string Publickey { get; set; }

        [JsonProperty("fee")]
        public long Fee { get; set; }

        [JsonProperty("realFee")]
        public long RealFee { get; set; }

        [JsonProperty("nonce")]
        public long Nonce { get; set; }

        [JsonProperty("intStatus")]
        public long IntStatus { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}