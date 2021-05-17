using Microsoft.Azure.Cosmos.Table;

namespace MetaBoyTipBot.TableEntities
{
    public class UserBalanceHistory : TableEntity
    {
        public UserBalanceHistory()
        {
        }

        public UserBalanceHistory(int userId, long unixTimestamp)
        {
            PartitionKey = userId.ToString();
            RowKey = unixTimestamp.ToString();
        }

        public double In { get; set; }
        public double Out { get; set; }
        public int FromUserId { get; set; }
        public int ToUserId { get; set; }
    }
}