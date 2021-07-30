namespace MetaBoyTipBot.Constants
{
    public class ReplyConstants
    {
        public const string EnterWithdrawalAmount = "Enter your withdrawal amount in MHC";
        public const string EnterTopUpMetahashWallet = "Enter your metahash wallet id (ex: 0x006...)";
        public const string EnterDefaultTipAmount = "Enter the default amount to use for a single tip (ex.: default amount of 1 will send 1 MHC when you send a 👍)";
        public const string EnterWithdrawalWallet = "Enter your withdrawal metahash wallet id (ex: 0x006...)";
        public const string WithdrawalWalletConfirmation = "We registered your withdrawal wallet address \n\r{0}.";
        public const string DefaultTipAmountConfirmation = "Your default tip amount is now set to *{0}* MHC.";
        public const string TransferToDonationWallet = "We registered your wallet address \n\r{0} \n\r\n\rTo add funds, please send your desired amount of MHC to the following tip wallet address:";
        public const string CurrentWallet = "Your wallet address is \n\r{0} \n\r\n\rTo add funds, please send your desired amount of MHC to the following tip wallet address:";
        public const string InvalidWalletAddress = "The provided wallet address is invalid";
        public const string UnableToWithdraw = "We're sorry but the withdrawal network transaction failed. Get in touch with an admin if the problem persists (for more than 20 minutes).";
        public const string WithdrawalSuccess = "Withdrawal succeeded";
        public const string WithdrawVerification = "Give us 10 seconds while we verify the withdrawal process...";
        public const string WithdrawLimit = "Withdrawal denied. You can only withdraw once every 4h!";
        public const string WithdrawVerificationLonger = "It's taking longer than expected, just a few more moments...";
        public const string WalletAddressInUse = "The wallet address is already registered, either by you or another user";
        public const string InvalidAmount = "We provided amount is invalid";
        public const string InsufficientBalance = "We are sorry but your balance of {0} MHC does not allow the witdrawal amount of {1} MHC.";
        public const string Balance = "Your balance is *{0} MHC*";
        public const string TopUp = "We topped up your account with *{0} MHC*, happy tipping!";
    }
}