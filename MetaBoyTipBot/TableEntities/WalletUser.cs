using Microsoft.Azure.Cosmos.Table;

namespace MetaBoyTipBot.TableEntities
{
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