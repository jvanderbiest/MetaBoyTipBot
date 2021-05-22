using System;
using Microsoft.Azure.Cosmos.Table;

namespace MetaBoyTipBot.TableEntities
{
    public enum WithdrawalState
    {
        Created,
        Verification,
        Completed,
        Failed
    }

    public class UserWithdrawal : TableEntity
    {
        public UserWithdrawal()
        {

        }

        public UserWithdrawal(int userId)
        {
            PartitionKey = userId.ToString();
            RowKey = (DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks).ToString();
        }

        public string WalletAddress { get; set; }
        public double Amount { get; set; }
        public WithdrawalState State { get; set; }
        public string TxId { get; set; }
        public DateTime StartDate { get; set; }
    }

    public class WalletUser : TableEntity
    {
        public WalletUser()
        {
        }

        public WalletUser(string walletAddress, int userId)
        {
            PartitionKey = walletAddress;
            RowKey = userId.ToString();
        }

        public long? PrivateChatId { get; set; }

        public int? GetUserId()
        {
            if (int.TryParse(RowKey, out int userId))
            {
                return userId;
            }

            return null;
        }
    }
}