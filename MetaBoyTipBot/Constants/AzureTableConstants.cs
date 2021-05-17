namespace MetaBoyTipBot.Constants
{
    public class AzureTableConstants
    {
        public class UserBalance
        {
            public const string TableName = "UserBalance";
            public const string PartitionKeyName = "users";
        }

        public class UserBalanceHistory
        {
            public const string TableName = "UserBalanceHistory";
        }

        public class TransactionHistory
        {
            public const string TableName = "TransactionHistory";
        }

        public class TransactionCheckHistory
        {
            public const string TableName = "TransactionCheckHistory";
            public const string PartitionKey = "scheduler";
        }

        public class WalletUser
        {
            public const string TableName = "WalletUser";
        }
    }
}