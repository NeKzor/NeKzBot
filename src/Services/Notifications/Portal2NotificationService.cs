//#define TEST
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.Webhook;
using LiteDB;
using Microsoft.Extensions.Configuration;
using NeKzBot.Data;
using NeKzBot.Extensions;
using Portal2Boards;
using Portal2Boards.Extensions;

namespace NeKzBot.Services.Notifications
{
    public class Portal2NotificationService : NotificationService
    {
        private Portal2BoardsClient _client;
        private ChangelogQuery _changelogQuery;

        public Portal2NotificationService(IConfiguration config, LiteDatabase dataBase)
            : base(config, dataBase)
        {
            _embedBuilder = x => BuildEmbedAsync(x as ChangelogEntry);
        }

        public override Task Initialize()
        {
            _ = base.Initialize();

            _userName = "Portal2Boards";
            _userAvatar = "https://raw.githubusercontent.com/NeKzor/NeKzBot/master/public/resources/avatars/portal2boards_avatar.jpg";
            _sleepTime = 5 * 60 * 1000;

            // API client to board.iverb.me
            _client = new Portal2BoardsClient(_config["user_agent"], false);
            _changelogQuery = new ChangelogQueryBuilder()
                .WithWorldRecord(true)
                .WithMaxDaysAgo(52) // Insert worst case here
                .Build();

            // Insert new cache if it doesn't exist yet
            var db = GetTaskCache<Portal2BoardsData>()
                .GetAwaiter()
                .GetResult();

            var cache = db
                .FindAll()
                .FirstOrDefault();

            if (cache == null)
            {
                _ = LogWarning("Creating new cache");
                cache = new Portal2BoardsData();
                db.Insert(cache);
            }
            return Task.CompletedTask;
        }

        public override async Task StartAsync()
        {
            try
            {
                await base.StartAsync();

                while (_isRunning)
                {
                    await LogInfo("Checking...");

                    var watch = Stopwatch.StartNew();

                    var db = await GetTaskCache<Portal2BoardsData>();
                    var cache = db
                        .FindAll()
                        .FirstOrDefault();

                    if (cache == null)
                    {
                        await LogWarning("Task cache not found");
                        goto retry;
                    }

                    var clog = await _client.GetChangelogAsync(() => _changelogQuery);
                    if (clog == null)
                    {
                        await LogWarning("Fetch failed");
                        goto retry;
                    }

                    var entries = clog.Entries.Where(e => !e.IsBanned);
                    var sending = new List<object>();

                    // Will skip for the very first time
                    if (cache.EntryIds.Any())
                    {
                        // Check cached entries
                        foreach (var id in cache.EntryIds)
                        {
                            sending.Clear();
                            foreach (ChangelogEntry entry in entries)
                            {
                                if (id >= entry.Id)
                                    goto send;
                                sending.Add(entry);
                            }
                        }
                        throw new Exception("Could not find the cached entry in new changelog!");
                    }

                send:
#if TEST
					sending.Add(entries.First());
#endif
                    if (sending.Count > 0)
                    {
                        await LogInfo($"Found {sending.Count} new notifications to send");

                        if (sending.Count >= 11)
                            throw new Exception("Webhook rate limit exceeded!");

                        await SendAsync(sending);

                        // Cache
                        cache.EntryIds = entries
                            .Select(e => (uint)(e as ChangelogEntry).Id)
                            .Take(11);

                        if (!db.Update(cache))
                            throw new Exception("Failed to update cache!");
                    }

                retry:
                    // Sleep
                    var delay = (int)(_sleepTime - watch.ElapsedMilliseconds);
                    if (delay < 0)
                        await LogWarning($"Task took too long: {delay}ms");

                    await Task.Delay(delay, _cancellation.Token);
                }
            }
            catch (Exception ex)
            {
                await LogException(ex);
            }

            await LogWarning("Task ended");
        }

        private async Task<Embed> BuildEmbedAsync(ChangelogEntry wr)
        {
            var delta = await GetWorldRecordDelta(wr) ?? -1;
            var feature = (delta != default)
                ? $" (-{delta.ToString("N2")})"
                : string.Empty;

            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = wr.Player.Name,
                    Url = (wr.Player as SteamUser).Url,
                    IconUrl = (wr.Player as SteamUser).AvatarUrl
                },
                Title = "New Portal 2 World Record",
                Url = "https://board.iverb.me/changelog?wr=1",
                Color = new Color(4, 128, 165),
                ImageUrl = wr.ImageFullUrl,
                Timestamp = DateTime.UtcNow,
                Footer = new EmbedFooterBuilder
                {
                    Text = "board.iverb.me",
                    IconUrl = "https://raw.githubusercontent.com/NeKzor/NeKzBot/master/public/resources/icons/portal2boards_icon.png"
                }
            };
            embed.AddField("Map", wr.Name, true);
            // WR delta time, a feature which the leaderboard doesn't have :>
            embed.AddField("Time", wr.Score.Current.AsTimeToString() + feature, true);
            embed.AddField("Player", wr.Player.Name.ToRawText(), true);
            embed.AddField("Date", $"{wr.Date?.DateTimeToString()} (CST)", true);
            if ((wr.DemoExists) || (wr.VideoExists))
            {
                embed.AddField("Demo File", (wr.DemoExists) ? $"[Download]({wr.DemoUrl})" : "_Not available._", true);
                embed.AddField("Video Link", (wr.VideoExists) ? $"[Watch]({wr.VideoUrl})" : "_Not available._", true);
            }
            if (wr.CommentExists)
                embed.AddField("Comment", wr.Comment.ToRawText());
            return embed.Build();
        }
        // My head always hurts when I look at this...
        private async Task<float?> GetWorldRecordDelta(ChangelogEntry wr)
        {
            var map = Portal2Map.Search(wr.Name);
            var found = false;
            var foundcoop = false;

            var clog = await _client.GetChangelogAsync($"?wr=1&chamber={map.BestTimeId}");
            foreach (var entry in clog.Entries)
            {
                if (entry.IsBanned)
                    continue;

                if (found)
                {
                    var oldwr = entry.Score.Current.AsTime();
                    var newwr = wr.Score.Current.AsTime();
                    if (map.Type == Portal2MapType.Cooperative)
                    {
                        if (foundcoop)
                        {
                            if (oldwr == newwr)
                                return 0;
                            if (newwr < oldwr)
                                return oldwr - newwr;
                        }
                        // Tie or partner score
                        else if (oldwr == newwr)
                        {
                            // Cooperative world record without a partner
                            // will be ignored, sadly that's a thing :>
                            foundcoop = true;
                            continue;
                        }
                        else if (newwr < oldwr)
                        {
                            return oldwr - newwr;
                        }
                    }
                    else if (map.Type == Portal2MapType.SinglePlayer)
                    {
                        if (oldwr == newwr)
                            return 0;
                        if (newwr < oldwr)
                            return oldwr - newwr;
                    }
                    break;
                }
                if ((entry as ChangelogEntry).Id == wr.Id)
                    found = true;
            }

            // Warning
            _ = LogWarning("Could not calculate the world record delta");
            return default;
        }
    }
}
