using MetaBoyTipBot.Constants;
using Microsoft.Azure.Cosmos.Table;

namespace MetaBoyTipBot.TableEntities
{
    public class UserBalance : TableEntity
    {
        private int _defaultTipAmount;

        public UserBalance()
        {
        }

        public UserBalance(int userId)
        {
            PartitionKey = AzureTableConstants.UserBalance.PartitionKeyName;
            RowKey = userId.ToString();
        }

        public double Balance { get; set; }
        public string Address { get; set; }

        /// <summary>
        /// Specifies the amount of a single tip
        /// </summary>
        public int DefaultTipAmount
        {
            get => _defaultTipAmount == 0 ? 1 : _defaultTipAmount;
            set => _defaultTipAmount = value;
        }
    }
}