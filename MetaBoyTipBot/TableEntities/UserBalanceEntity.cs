using MetaBoyTipBot.Constants;
using Microsoft.Azure.Cosmos.Table;

namespace MetaBoyTipBot.TableEntities
{
    public class UserBalanceEntity : TableEntity
    {
        private int _defaultTipAmount;

        public UserBalanceEntity()
        {
        }

        public UserBalanceEntity(string userId)
        {
            PartitionKey = AzureTableConstants.Balance.PartitionKeyName;
            RowKey = userId;
        }

        public int Balance { get; set; }
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