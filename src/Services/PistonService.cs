using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using LiteDB;
using Microsoft.Extensions.Configuration;
using NeKzBot.API;
using NeKzBot.Data;

namespace NeKzBot.Services
{
    public class PistonService
    {        
        public readonly string PistonCollection = nameof(PistonService);

        private IReadOnlyCollection<PistonVersion>? _supportedVersions;
        private PistonApiClient? _client;

        private readonly DiscordSocketClient _discord;
        private readonly IConfiguration _config;
        private readonly LiteDatabase _dataBase;

        public PistonService(DiscordSocketClient discord, IConfiguration config, LiteDatabase dataBase)
        {
            _discord = discord;
            _config = config;
            _dataBase = dataBase;
        }

        public async Task Initialize()
        {
            _discord.MessageUpdated += MessageUpdated;
            _client = new PistonApiClient(_config["user_agent"]);

            //_dataBase.DropCollection(BoardCollection);

           await UpdateVersions();
        }

        private async Task MessageUpdated(
            Cacheable<IMessage, ulong> cache,
            SocketMessage message,
            ISocketMessageChannel channel)
        {
            if (channel is not SocketGuildChannel) return;
            if (message.Author is not IGuildUser || message.Author.IsBot) return;

            var tracker = _dataBase
                .GetCollection<CodeTrackerData>(PistonCollection)
                .FindOne(tracker => tracker.MessageId == message.Id);

            if (tracker is null) return;

            var language = ParseLanguage(message.Content.Trim());
            if (language is null) return;

            var version = FindVersion(language);
            if (version is null) return;

            var code = message.Content.Trim();
            var codeIndex = code.IndexOf("```");
            if (codeIndex == -1) return;

            var (source, stdIn, args) = Parse(code.Substring(codeIndex));
            if (source is null) return;

            var replyMessage = await channel.GetMessageAsync(tracker.ReplyId);
            if (replyMessage is SocketUserMessage reply)
            {
                var result = await Run(version, source, stdIn, args);
                if (result is null) return;

                await reply.ModifyAsync(reply => reply.Content = GetOutput(result));
            }
        }

        public string? ParseLanguage(string content)
        {
            if (content.StartsWith(".code.r") || content.StartsWith(".code.e"))
            {
                var segments = content.Split(new[] { " ", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                return segments.ElementAtOrDefault(1);
            }

            return null;
        }

        public Task TrackResult(ulong messageId, ulong userId, ulong guildId, ulong channelId, ulong replyId)
        {
            _dataBase
                .GetCollection<CodeTrackerData>(PistonCollection)
                .Upsert(new CodeTrackerData()
                {
                    UserId = userId,
                    MessageId = messageId,
                    ReplyId = replyId,
                    GuildId = guildId,
                    ChannelId = channelId,
                });

            return Task.CompletedTask;
        }

        public async Task UpdateVersions()
        {
            if (_client is null)
                throw new System.Exception("Piston API Client not initialized");

            var versions = await _client.GetVersions();
            if (versions is null)
               throw new System.Exception("Unable to get supported Piston languages");

            _supportedVersions = versions;
        }

        public PistonVersion? FindVersion(string lang)
        {
            return _supportedVersions.FirstOrDefault(version => version.Name == lang.ToLower() || version.Aliases?.Contains(lang.ToLower()) == true);
        }

        public (string?, string, List<string>) Parse(string code)
        {
            var input = code.Trim().Split("\n").ToList();
            var codeIndex = input.FindIndex(line => line.Trim().StartsWith("```"));
            var lastCodeIndex = input.FindLastIndex(line => line.Trim().StartsWith("```"));

            if (codeIndex == -1 || codeIndex == lastCodeIndex)
            {
                return (default, string.Empty, new());
            }

            var args = input.Take(codeIndex);
            var source = input.Skip(codeIndex + 1).Take(lastCodeIndex - (codeIndex + 1));
            var stdIn = input.Skip(lastCodeIndex).FirstOrDefault() ?? string.Empty;

            return (string.Join("\n", source), stdIn, args.ToList());
        }

        public Task<PistonResult?> Run(PistonVersion version, string source, string stdIn, List<string>? args)
        {
            if (_client is null)
                throw new System.Exception("Piston API Client not initialized");

            return _client.Execute(new PistonCode()
            {
                Language = version.Name,
                Source = source,
                StdIn = stdIn,
                Args = args,
            });
        }

        public string GetOutput(PistonResult result)
        {
            return $@"```
{result.Message ?? result.Output?.Substring(0, Math.Min(1024, result.Output?.Length ?? 0)) ?? "No output."}
```";
        }
    }
}
