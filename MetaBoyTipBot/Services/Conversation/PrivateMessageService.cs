using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MetaBoyTipBot.Configuration;
using MetaBoyTipBot.Constants;
using MetaBoyTipBot.Extensions;
using MetaBoyTipBot.Repositories;
using MetaBoyTipBot.TableEntities;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MetaBoyTipBot.Services.Conversation
{
    public class PrivateMessageService : IMessageService
    {
        private readonly IOptions<BotConfiguration> _botConfiguration;
        private readonly IBotService _botService;
        private readonly IWalletUserRepository _walletUserRepository;

        public PrivateMessageService(IOptions<BotConfiguration> botConfiguration, IBotService botService, IWalletUserRepository walletUserRepository)
        {
            _botConfiguration = botConfiguration ?? throw new ArgumentNullException(nameof(botConfiguration));
            _botService = botService ?? throw new ArgumentNullException(nameof(botService));
            _walletUserRepository = walletUserRepository;
        }

        public async Task Handle(Update update)
        {
            if (update.Message.ReplyToMessage?.Text == ReplyConstants.EnterMetahashWallet)
            {
                var match = Regex.Match(update.Message.Text, "0[xX][0-9a-fA-F]+");

                if (!match.Success)
                {
                    await _botService.Client.SendTextMessageAsync(
                        chatId: update.Message.Chat,
                        text: ReplyConstants.InvalidWalletAddress,
                        parseMode: ParseMode.Markdown,
                        disableNotification: true
                    );
                }
                else
                {
                    var userId = update.Message.From.Id;
                    var existingWalletUser = _walletUserRepository.GetByWalletId(match.Value);
                    if (existingWalletUser != null)
                    {
                        var isOwnWallet = existingWalletUser.RowKey != userId.ToString();
                        if (isOwnWallet)
                        {
                            await _walletUserRepository.Delete(new WalletUser(match.Value, userId));
                            await AddUserWallet(update, match.Value, userId, update.Message.Chat.Id);
                        }
                        else
                        {
                            await _botService.Client.SendTextMessageAsync(
                                chatId: update.Message.Chat,
                                text: ReplyConstants.WalletAddressInUse,
                                parseMode: ParseMode.Markdown,
                                disableNotification: true
                            );
                        }
                    }
                    else
                    {
                        await AddUserWallet(update, match.Value, userId, update.Message.Chat.Id);
                    }
                }
            }
            else
            {
                await _botService.Client.ShowMenu(update.Message.Chat, update.Message.MessageId);
            }
        }
        
        private async Task AddUserWallet(Update update, string walletAddress, int userId, long chatId)
        {
            await _walletUserRepository.AddOrUpdate(new WalletUser(walletAddress, userId) { PrivateChatId = chatId });

            await _botService.Client.SendTextMessageAsync(
                chatId: update.Message.Chat,
                text: string.Format(ReplyConstants.TransferToDonationWallet, walletAddress),
                parseMode: ParseMode.Markdown,
                disableNotification: true
            );

            await _botService.Client.SendTextMessageAsync(
                chatId: update.Message.Chat,
                text: _botConfiguration.Value.TipWalletAddress,
                parseMode: ParseMode.Markdown,
                disableNotification: true
            );
        }
    }
}