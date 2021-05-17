using System;
using MetaBoyTipBot.Constants;
using Microsoft.Azure.Cosmos.Table;

namespace MetaBoyTipBot.TableEntities
{
    public class TransactionCheckHistory : TableEntity
    {
        public TransactionCheckHistory()
        {
            PartitionKey = AzureTableConstants.TransactionCheckHistory.PartitionKey;
            RowKey = (DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks).ToString();
        }

        public int? LastTransactionTime { get; set; }
        public int HandledTransactions { get; set; }
    }

    public class TransactionHistory : TableEntity
    {
        public TransactionHistory()
        {
        }

        public TransactionHistory(string walletAddress, int transactionUnixTimeStamp)
        {
            PartitionKey = walletAddress;
            RowKey = transactionUnixTimeStamp.ToString();
        }

        public string From { get; set; }
        public string To { get; set; }
        public double Value { get; set; }
        public string Transaction { get; set; }
        public int BlockNumber { get; set; }
        public int BlockIndex { get; set; }
        public string Status { get; set; }
        public int ToChatUserId { get; set; }
    }
}