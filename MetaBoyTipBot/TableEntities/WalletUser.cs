using System;
using Microsoft.Azure.Cosmos.Table;

namespace MetaBoyTipBot.TableEntities
{
    public enum WithdrawalState
    {
        None = 0,
        Created = 1,
        Verification = 2,
        Completed = 3,
        Failed = 4
    }

    public class UserWithdrawal : TableEntity
    {
        private int _state;

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

        public int StateInt
        {
            get => _state;
            set => _state = value;
        }

        public WithdrawalState State
        {
            get => (WithdrawalState)_state;
            set => StateInt = (int)value;
        }

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