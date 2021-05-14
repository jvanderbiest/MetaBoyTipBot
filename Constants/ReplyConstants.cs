namespace MetaBoyTipBot.Constants
{
    public class ReplyConstants
    {
        private const string MetaHashDonationAddress = "0x006b17f5ce3e6b2a02d9231d787d2c890fa1941fc7c37abd00";

        public const string EnterMetahashWallet = "Enter your metahash wallet id (ex: 0x006...)";
        public const string TransferToDonationWallet = "We registered your wallet address \n\r{0}. \n\r\n\rTo add funds, please send your desired amount of MHC to the following tip wallet address:";
        public static string TransferToDonationWalletId = $"*{MetaHashDonationAddress}*";
        public const string InvalidWalletAddress = "The provided wallet address is invalid";
    }
}