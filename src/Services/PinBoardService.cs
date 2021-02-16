using System;
using System.Collections.Generic;
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

        public readonly string BoardCollection = nameof(PinBoardService);
        public readonly string MessageCollection = nameof(PinBoardService) + "_messages";

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

            //_dataBase.DropCollection(BoardCollection);
            //_dataBase.DropCollection(MessageCollection);
            return Task.CompletedTask;
        }

        private async Task ReactionAdded(
            Cacheable<IUserMessage, ulong> cache,
            ISocketMessageChannel channel,
            SocketReaction _reaction)
        {
            var guildChannel = channel as SocketGuildChannel;
            if (guildChannel is null) return;

            var board = Get(guildChannel.Guild.Id);
            if (board is null || board.PinEmoji is null) return;

            var message = await cache.GetOrDownloadAsync();
            if (message.CreatedAt < board.CreatedAt) return;

            var pin = GetMessage(message.Id);
            if (pin is {}) return;

            var reaction = message.Reactions.FirstOrDefault(x => x.Key.Name == board.PinEmoji);
            if (reaction.Value.ReactionCount >= board.MinimumReactions)
            {
                Pin(message, guildChannel);
                await Send(message, board);
            }
        }

        public PinBoardData? Get(ulong guildId)
        {
            return _dataBase
                .GetCollection<PinBoardData>(BoardCollection)
                .FindOne(board => board.GuildId == guildId);
        }

        public bool Delete(ulong guildId)
        {
            return _dataBase
                .GetCollection<PinBoardData>(BoardCollection)
                .Delete(board => board.GuildId == guildId) == 1;
        }

        public PinMessageData GetMessage(ulong messageId)
        {
            return _dataBase
                .GetCollection<PinMessageData>(MessageCollection)
                .FindOne(message =>  message.MessageId == messageId);
        }

        public IEnumerable<PinMessageData> GetMessages(ulong guildId)
        {
            return _dataBase
                .GetCollection<PinMessageData>(MessageCollection)
                .Find(message =>  message.GuildId == guildId);
        }

        public PinBoardData? Create(IWebhook hook)
        {
            if (hook is null) return default;

            var board = new PinBoardData()
            {
                GuildId = hook.GuildId ?? 0ul,
                WebhookId = hook.Id,
                WebhookToken = hook.Token,
                CreatedAt = hook.CreatedAt
            };

            _dataBase
                .GetCollection<PinBoardData>(BoardCollection)
                .Insert(board);

            return board;
        }

        public void Update(PinBoardData board)
        {
            _dataBase
                .GetCollection<PinBoardData>(BoardCollection)
                .Upsert(board);
        }

        private void Pin(IMessage message, SocketGuildChannel source)
        {
            if (message is null) return;

            var pin = new PinMessageData()
            {
                MessageId = message.Id,
                ChannelId = source.Id,
                GuildId = source.Guild.Id
            };

            _dataBase
                .GetCollection<PinMessageData>(MessageCollection)
                .Insert(pin);
        }

        public async Task Send(IMessage message, PinBoardData? board)
        {
            if (_client is null)
                throw new System.Exception("Service is not initialized");

            var author = message.Author as IGuildUser;
            if (author is null)
                throw new System.Exception("Author is not a guild user");

            if (board is null)
                throw new System.Exception("Board not found");

            var jumpButton = new EmbedBuilder()
                    .WithColor(await message.Author.GetRoleColor())
                    .WithDescription($"[Jump]({message.GetJumpUrl()})");

            try
            {
                using var wc = new DiscordWebhookClient(board.WebhookId, board.WebhookToken);

                var embeds = message.Embeds.ToList();
                var pinId = 0ul;

                var embed = embeds.FirstOrDefault();
                var attachment = message.Attachments.FirstOrDefault();

                bool IsVideoFormat(string url)
                {
                    return url.EndsWith(".mp4") || url.EndsWith(".mov") || url.EndsWith(".webm");
                }

                if (embed is {}
                    && (embed.Type == EmbedType.Gifv || embed.Type == EmbedType.Image || (embed.Type == EmbedType.Video && IsVideoFormat(embed.Video!.Value.Url))))
                {
                    var url = embed.Type == EmbedType.Image
                        ? embed.Image!.Value.ProxyUrl
                        : embed.Video!.Value.Url;

                    var (success, file) = await _client.GetStreamAsync(url);
                    if (success && file is {} && file.Length <= 8 * 1000 * 1000)
                    {
                        pinId = await wc.SendFileAsync
                        (
                            stream: file,
                            filename: url.Substring(url.LastIndexOf("/")),
                            text: message.Content,
                            embeds: new[] { jumpButton.Build() },
                            username: author.Nickname ?? author.Username,
                            avatarUrl: author.GetAvatarUrl()
                        );
                    }
                }
                else if (attachment is {} && attachment.Size <= 8 * 1000 * 1000)
                {
                    var (success, file) = await _client.GetStreamAsync(attachment.Url);
                    if (success)
                    {
                        pinId = await wc.SendFileAsync
                        (
                            stream: file,
                            filename: attachment.Filename,
                            text: message.Content,
                            embeds: new[] { jumpButton.Build() },
                            username: author.Nickname ?? author.Username,
                            avatarUrl: author.GetAvatarUrl()
                        );
                    }
                }
                else
                {
                    if (embeds.Count < 10)
                        embeds.Add(jumpButton.Build());

                    pinId = await wc.SendMessageAsync
                    (
                        text: message.Content,
                        embeds: embeds.Cast<Embed>(),
                        username: author.Nickname ?? author.Username,
                        avatarUrl: author.GetAvatarUrl()
                    );
                }

                if (pinId == 0ul)
                    throw new System.Exception("Failed to send webhook");

                // Pin pin in order to prevent pin pins, pin pin pins etc.
                var pin = new PinMessageData()
                {
                    MessageId = pinId,
                    GuildId = board.GuildId
                };

                _dataBase
                    .GetCollection<PinMessageData>(MessageCollection)
                    .Insert(pin);
            }
            catch (InvalidOperationException ex)
                when (ex.Message.StartsWith("Could not find a webhook"))
            {
                if (Delete(board.GuildId))
                    _ = LogWarning($"Webhook not found. Deleted board for {board.GuildId}");
                else
                    _ = LogWarning($"Webhook not found. Failed to delete board for {board.GuildId}");   
            }
            catch (Exception ex)
            {
                _ = LogException(ex);
            }
        }

        protected Task LogWarning(string message)
        {
            _ = Log?.Invoke($"{nameof(PinBoardService)}\t{message}!", null);
            return Task.CompletedTask;
        }
        protected Task LogException(Exception ex)
        {
            _ = Log?.Invoke(nameof(PinBoardService), ex);
            return Task.CompletedTask;
        }
    }
}
