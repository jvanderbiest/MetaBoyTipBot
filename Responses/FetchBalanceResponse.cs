using Newtonsoft.Json;

namespace MetaBoyTipBot.Responses
{
    public class FetchBalanceResponse
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("result")]
        public FetchBalanceResponseResult Result { get; set; }
    }

    public class FetchBalanceResponseResult
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("received")]
        public int Received { get; set; }

        [JsonProperty("spent")]
        public int Spent { get; set; }

        [JsonProperty("count_received")]
        public int CountReceived { get; set; }

        [JsonProperty("count_spent")]
        public int CountSpent { get; set; }

        [JsonProperty("count_txs")]
        public int CountTxs { get; set; }

        [JsonProperty("block_number")]
        public int BlockNumber { get; set; }

        [JsonProperty("currentBlock")]
        public int CurrentBlock { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("countDelegatedOps")]
        public int CountDelegatedOps { get; set; }

        [JsonProperty("delegate")]
        public int Delegate { get; set; }

        [JsonProperty("undelegate")]
        public int Undelegate { get; set; }

        [JsonProperty("delegated")]
        public int Delegated { get; set; }

        [JsonProperty("undelegated")]
        public int Undelegated { get; set; }

        [JsonProperty("reserved")]
        public int Reserved { get; set; }
    }
}
