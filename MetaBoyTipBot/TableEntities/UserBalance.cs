using MetaBoyTipBot.Constants;
using Microsoft.Azure.Cosmos.Table;

namespace MetaBoyTipBot.TableEntities
{
    public class UserBalance : TableEntity
    {
        private double _defaultTipAmount;

        public UserBalance()
        {
        }

        public UserBalance(int userId)
        {
            PartitionKey = AzureTableConstants.UserBalance.PartitionKeyName;
            RowKey = userId.ToString();
        }

        public double Balance { get; set; }
        public double TotalTipsGiven { get; set; }
        public double TotalTipsReceived { get; set; }
        public string Address { get; set; }

        /// <summary>
        /// Specifies the amount of a single tip
        /// </summary>
        public double DefaultTipAmount
        {
            get => _defaultTipAmount == 0 ? 1 : _defaultTipAmount;
            set => _defaultTipAmount = value;
        }

        public void ReceiveTip(double amount)
        {
            Balance += amount;
            TotalTipsReceived += amount;
        }

        public void GiveTip(double amount)
        {
            Balance -= amount;
            TotalTipsGiven += amount;
        }
    }
}