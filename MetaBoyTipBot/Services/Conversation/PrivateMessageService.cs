using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MetaBoyTipBot.Configuration;
using MetaBoyTipBot.Constants;
using MetaBoyTipBot.Repositories;
using MetaBoyTipBot.TableEntities;
using Microsoft.Extensions.Options;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace MetaBoyTipBot.Services.Conversation
{
    public enum WalletAddressAction
    {
        None,
        WalletSet,
        Duplicate
    }

    public class PrivateMessageService : IMessageService
    {
        private readonly IOptions<BotConfiguration> _botConfiguration;
        private readonly IBotService _botService;
        private readonly IWalletUserRepository _walletUserRepository;
        private readonly IWithdrawalService _withdrawalService;
        private readonly ISettingsService _settingsService;

        public PrivateMessageService(IOptions<BotConfiguration> botConfiguration, IBotService botService, IWalletUserRepository walletUserRepository,
            IWithdrawalService withdrawalService, ISettingsService settingsService)
        {
            _botConfiguration = botConfiguration ?? throw new ArgumentNullException(nameof(botConfiguration));
            _botService = botService ?? throw new ArgumentNullException(nameof(botService));
            _walletUserRepository = walletUserRepository;
            _withdrawalService = withdrawalService ?? throw new ArgumentNullException(nameof(withdrawalService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        }

        public async Task Handle(Update update)
        {
            var isReplyMessage = update.Message.ReplyToMessage != null;
            if (!isReplyMessage)
            {
                await _botService.ShowMainButtonMenu(update.Message.Chat.Id, null);
            }
            else
            {
                var replyMessage = update.Message.ReplyToMessage?.Text;
                switch (replyMessage)
                {
                    case ReplyConstants.EnterDefaultTipAmount:
                        {
                            if (double.TryParse(update.Message.Text, out double defaultTipAmount))
                            {
                                await _settingsService.SetDefaultTipAmount(update.Message.Chat, update.Message.From.Id, defaultTipAmount);
                            }
                            else
                            {
                                await _botService.SendTextMessage(update.Message.Chat.Id, ReplyConstants.InvalidAmount);
                            }
                            break;
                        }
                    case ReplyConstants.EnterWithdrawalWallet:
                        {
                            var walletAddress = ValidateWallet(update.Message.Text);
                            var walletAddressAction = await TrySetWallet(walletAddress, update);

                            await SendWalletSetReply(update, walletAddressAction, walletAddress, true);
                            break;
                        }
                    case ReplyConstants.EnterTopUpMetahashWallet:
                        {
                            var walletAddress = ValidateWallet(update.Message.Text);
                            var walletAddressAction = await TrySetWallet(walletAddress, update);

                            await SendWalletSetReply(update, walletAddressAction, walletAddress, false);
                            break;
                        }
                    case ReplyConstants.EnterWithdrawalAmount:
                        if (double.TryParse(update.Message.Text, out double amount))
                        {
                            await _withdrawalService.Handle(update.Message.Chat, update.Message.From.Id, amount);
                        }
                        else
                        {
                            await _botService.SendTextMessage(update.Message.Chat.Id, ReplyConstants.InvalidAmount);
                        }
                        break;
                    default:
                        {
                            await _botService.ShowMainButtonMenu(update.Message.Chat.Id, update.Message.MessageId);
                            break;
                        }
                }
            }
        }

        private async Task SendWalletSetReply(Update update, WalletAddressAction walletAddressAction, string walletAddress, bool isWithdrawal)
        {
            switch (walletAddressAction)
            {
                case WalletAddressAction.None:
                    await _botService.SendTextMessage(update.Message.Chat.Id, ReplyConstants.InvalidWalletAddress);
                    break;
                case WalletAddressAction.WalletSet:
                    if (isWithdrawal)
                    {
                        await _botService.SendTextMessage(update.Message.Chat.Id, string.Format(ReplyConstants.WithdrawalWalletConfirmation, walletAddress));
                        await _botService.SendTextMessage(update.Message.Chat.Id, ReplyConstants.EnterWithdrawalAmount, new ForceReplyMarkup { Selective = false });
                    }
                    else
                    {
                        await _botService.SendTextMessage(update.Message.Chat.Id, string.Format(ReplyConstants.TransferToDonationWallet, walletAddress));
                        await _botService.SendTextMessage(update.Message.Chat.Id, _botConfiguration.Value.TipWalletAddress);
                    }
                    break;
                case WalletAddressAction.Duplicate:
                    await _botService.SendTextMessage(update.Message.Chat.Id, ReplyConstants.WalletAddressInUse);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task<WalletAddressAction> TrySetWallet(string newWallet, Update update)
        {
            if (!string.IsNullOrEmpty(newWallet))
            {
                var userId = update.Message.From.Id;

                var isUniqueWallet = _walletUserRepository.GetByWalletId(newWallet) == null;
                if (isUniqueWallet)
                {
                    await DeleteWalletsIfExists(userId);

                    await AddUserWallet(update, newWallet, userId, update.Message.Chat.Id);
                    return WalletAddressAction.WalletSet;
                }

                return WalletAddressAction.Duplicate;
            }

            return WalletAddressAction.None;
        }

        /// <summary>
        /// To cleanup for users who had multiple wallets set because of a previous issue.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private async Task DeleteWalletsIfExists(int userId)
        {
            var userWallets = _walletUserRepository.GetByUserIdDuplicates(userId);

            foreach (var userWallet in userWallets)
            {
                await _walletUserRepository.Delete(userWallet);
            }
        }

        private string ValidateWallet(string messageText)
        {
            // remove whitespaces
            messageText = Regex.Replace(messageText, @"\s+", "");
            var match = Regex.Match(messageText, "0[xX][0-9a-fA-F]+");

            if (!match.Success)
            {
                return null;
            }

            return match.Value;
        }

        private async Task AddUserWallet(Update update, string walletAddress, int userId, long chatId)
        {
            await _walletUserRepository.AddOrUpdate(new WalletUser(walletAddress, userId) { PrivateChatId = chatId });
        }
    }
}