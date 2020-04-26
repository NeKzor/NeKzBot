using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Webhook;
using Discord.WebSocket;
using LiteDB;
using Microsoft.Extensions.Configuration;
using NeKzBot.API;
using NeKzBot.Data;
using NeKzBot.Extensions;

namespace NeKzBot.Services
{
    public class PinBoardService
    {
        public event Func<string, Exception?, Task>? Log;

        private WebClient? _client;

        private readonly DiscordSocketClient _discord;
        private readonly IConfiguration _config;
        private readonly LiteDatabase _dataBase;

        public PinBoardService(DiscordSocketClient discord, IConfiguration config, LiteDatabase dataBase)
        {
            _discord = discord;
            _config = config;
            _dataBase = dataBase;
        }

        public Task Initialize()
        {
            _discord.ReactionAdded += ReactionAdded;
            _client = new WebClient(_config["user_agent"]);
            return Task.CompletedTask;
        }

        private async Task ReactionAdded(
            Cacheable<IUserMessage, ulong> cache,
            ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            var guildChannel = channel as SocketGuildChannel;
            if (guildChannel is null) return;

            var board = Get(guildChannel.Guild.Id);
            if (board is null || board.PinEmoji is null) return;

            var message = await cache.GetOrDownloadAsync();
            var pin = GetMessage(message.Id);
            if (pin is {}) return;

            var reactions = message.Reactions.FirstOrDefault(x => x.Key.Name == board.PinEmoji);
            if (reactions.Value.ReactionCount >= board.MinimumReactions)
            {
                Pin(message, guildChannel);
                await Send(message, board);
            }
        }

        public PinBoardData Get(ulong guildId)
        {
            return _dataBase
                .GetCollection<PinBoardData>(nameof(PinBoardService))
                .FindOne(data => data.GuildId == guildId);
        }

        public PinMessageData GetMessage(ulong messageId)
        {
            return _dataBase
                .GetCollection<PinMessageData>(nameof(PinBoardService))
                .FindOne(data => data.Id == messageId);
        }

        public void Create(IWebhook hook)
        {
            if (hook is null) return;

            var data = new PinBoardData()
            {
                GuildId = hook.GuildId ?? 0ul,
                WebhookId = hook.Id,
                WebhookToken = hook.Token
            };

            _dataBase
                .GetCollection<PinBoardData>(nameof(PinBoardService))
                .Upsert(data);
        }

        public void Update(PinBoardData data)
        {
            _dataBase
                .GetCollection<PinBoardData>(nameof(PinBoardService))
                .Upsert(data);
        }

        private void Pin(IMessage message, SocketGuildChannel source)
        {
            if (message is null) return;

            var data = new PinMessageData()
            {
                Id = message.Id,
                ChannelId = source.Id,
                GuildId = source.Guild.Id
            };

            _dataBase
                .GetCollection<PinMessageData>(nameof(PinBoardService))
                .Upsert(data);
        }

        public async Task Send(IMessage message, PinBoardData board)
        {
            if (_client is null)
                throw new System.Exception("Service is not initialized");

            var author = message.Author as IGuildUser;
            if (author is null)
                throw new System.Exception("Author is not a guild user");

            var embed = new EmbedBuilder()
                    .WithColor(await message.Author.GetRoleColor())
                    .WithDescription($"[Jump]({message.GetJumpUrl()})");

            using var wc = new DiscordWebhookClient(board.WebhookId, board.WebhookToken);

            var attachment = message.Attachments.FirstOrDefault();
            if (attachment is {})
            {
                var (success, file) = await _client.GetStreamAsync(attachment.Url);
                if (success)
                {
                    await wc.SendFileAsync
                    (
                        stream: file,
                        filename: attachment.Filename,
                        text: message.Content,
                        embeds: new[] { embed.Build() },
                        username: author.Nickname ?? author.Username,
                        avatarUrl: author.GetAvatarUrl()
                    );
                }
            }
            else
            {
                await wc.SendMessageAsync
                (
                    text: message.Content,
                    embeds: new[] { embed.Build() },
                    username: author.Nickname ?? author.Username,
                    avatarUrl: author.GetAvatarUrl()
                );
            }
        }

        protected Task LogWarning(string message)
        {
            _ = Log?.Invoke($"{nameof(SourceDemoData)}\t{message}!", null);
            return Task.CompletedTask;
        }
        protected Task LogException(Exception ex)
        {
            _ = Log?.Invoke(nameof(SourceDemoData), ex);
            return Task.CompletedTask;
        }
    }
}
