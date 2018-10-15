//#define TEST
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Webhook;
using LiteDB;
using Microsoft.Extensions.Configuration;
using NeKzBot.API;
using NeKzBot.Data;
using NeKzBot.Extensions;

namespace NeKzBot.Services.Notifications
{
    public class SpeedrunNotificationService : NotificationService
    {
        private SpeedrunComApiClient _client;

        public SpeedrunNotificationService(IConfiguration config, LiteDatabase dataBase)
            : base(config, dataBase)
        {
            _embedBuilder = x => BuildEmbedAsync(x as SpeedrunNotification);
        }

        public override Task Initialize()
        {
            _ = base.Initialize();

            _userName = "SpeedrunCom";
            _userAvatar = "https://raw.githubusercontent.com/NeKzor/NeKzBot/master/public/resources/avatars/speedruncom_avatar.png";
            _sleepTime = 1 * 60 * 1000;

            _client = new SpeedrunComApiClient
            (
                _config["user_agent"],
                _config["speedrun_token"]
            );

            var data = GetTaskCache<SpeedrunCacheData>()
                .GetAwaiter()
                .GetResult();

            var cache = data
                .FindAll()
                .FirstOrDefault();

            if (cache == null)
            {
                _ = LogWarning("Creating new cache");
                cache = new SpeedrunCacheData();
                data.Insert(cache);
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

                    var db = await GetTaskCache<SpeedrunCacheData>();
                    var cache = db
                        .FindAll()
                        .FirstOrDefault();

                    if (cache == null)
                        throw new Exception("Task cache not found!");
                    //await LogInfo($"Cache: {cache.Notifications.Count()} (ID = {cache.Id})");

                    var notifications = await _client.GetNotificationsAsync(21);
                    var sending = new List<SpeedrunNotification>();

                    if (cache.Notifications.Any())
                    {
                        foreach (var old in cache.Notifications)
                        {
                            foreach (var notification in notifications)
                            {
                                if (old.Id == notification.Id)
                                    goto send;
                                sending.Add(notification);
                            }
                        }
                        throw new Exception("Could not find the last notification entry!");
                    }

                send:
#if TEST
					sending.Add(notifications.First());
#endif
                    if (sending.Count > 0)
                    {
                        await LogInfo($"Found {sending.Count} new notifications to send");

                        if (sending.Count >= 11)
                            throw new Exception("Webhook rate limit exceeded!");

                        await SendAsync(sending);

                        cache.Notifications = notifications.Take(11);

                        if (!db.Update(cache))
                            throw new Exception("Failed to update cache!");
                    }

                    var delay = (int)(_sleepTime - watch.ElapsedMilliseconds);
                    if (delay < 0)
                        throw new Exception($"Task took too long ({delay}ms)");
                    await Task.Delay(delay, _cancellation.Token);
                }
            }
            catch (Exception ex)
            {
                await LogException(ex);
            }

            await LogWarning("Task ended");
        }

        private async Task<Embed> BuildEmbedAsync(SpeedrunNotification nf)
        {
            var author = nf.Text.Split(' ')[0];
            var game = default(string);
            var category = default(string);
            var description = nf.Text;

            // Local function
            (string, string) ExtractGame(string str)
            {
                var temp = str.Split(new[] { " - " }, 2, StringSplitOptions.None);
                return (temp.Length == 2) ? (temp[0], temp[1]) : (str, default);
            }

            // Old code here, I hope this won't throw an exception :>
            switch (nf.Item.Rel)
            {
                case "post":
                    (game, _) = ExtractGame(nf.Text.Substring(nf.Text.IndexOf(" in the ") + " in the ".Length, nf.Text.IndexOf(" forum.") - nf.Text.IndexOf(" in the ") - " in the ".Length));
                    description = $"*[{nf.Text.Substring(nf.Text.IndexOf("'") + 1, nf.Text.LastIndexOf("'") - nf.Text.IndexOf("'") - 1)}]({nf.Item.Uri.ToRawText()})*";
                    break;
                case "run":
                    (game, category) = ExtractGame(nf.Text.Substring(nf.Text.IndexOf("beat the WR in ") + "beat the WR in ".Length, nf.Text.IndexOf(". The new WR is") - nf.Text.IndexOf("beat the WR in ") - "beat the WR in ".Length));
                    description = $"**New {(nf.Text.Contains(" beat the WR in ") ? "World Record" : "Personal Best")}**\n{(!string.IsNullOrEmpty(category) ? $"{category}\n" : string.Empty)}{nf.Text.Substring(nf.Text.LastIndexOf(". The new WR is ") + ". The new WR is ".Length).TrimEnd(new char[1])}";
                    break;
                case "game":
                    break;
                case "guide":
                    break;
                case "thread":      // Undocumented API
                    (game, _) = ExtractGame(nf.Text.Substring(nf.Text.LastIndexOf(" in the ") + " in the ".Length, nf.Text.IndexOf(" forum:") - nf.Text.IndexOf(" in the ") - "in the ".Length));
                    description = $"*[{nf.Text.Substring(nf.Text.LastIndexOf(" forum: ") + " forum: ".Length)}]({nf.Item.Uri.ToRawText()})*";
                    break;
                case "moderator":   // Undocumented API
                    (game, _) = ExtractGame(nf.Text.Substring(nf.Text.IndexOf("has been added to ") + "has been added to ".Length, nf.Text.IndexOf(" as a moderator.") - nf.Text.IndexOf("has been added to ") - "has been added to ".Length));
                    description = $"{author.ToRawText()} is now a moderator! :heart:";
                    break;
                case "resource":    // Undocumented API
                    (game, _) = ExtractGame(nf.Text.Substring(nf.Text.IndexOf(" for ") + " for ".Length, nf.Text.LastIndexOf(" has") - nf.Text.IndexOf(" for ") - "for".Length));
                    if (nf.Text.EndsWith("updated."))
                        description = $"The resource *{nf.Text.Substring(nf.Text.IndexOf("The tool resource '") + "The tool resource '".Length + 1, nf.Text.LastIndexOf("' for ") - nf.Text.IndexOf("The tool resource '") - "The tool resource '".Length - 1)}* has been updated!";
                    else if (nf.Text.EndsWith("added."))
                        description = $"The resource *{nf.Text.Substring(nf.Text.IndexOf("A new tool resource, ") + "A new tool resource, ".Length + 1, nf.Text.LastIndexOf(", has been added to ") - nf.Text.IndexOf("A new tool resource, ") - "A new tool resource, ".Length - 1)}* has been added!";
                    break;
            }

            var thumbnail = default(string);
            if (!string.IsNullOrEmpty(game))
            {
                var games = await _client.GetGamesAsync(game);
                thumbnail = games?.FirstOrDefault()?.Assets?.CoverTiny?.Uri ?? string.Empty;
            }
            else
            {
                game = "?";
            }

            // API doesn't support user avatar nice...
            // Let's try to download it
            var avatar = false;
            using (var wc = new WebClient(_config["user_agent"]))
            {
                var (success, _) = await wc.TryGetBytesAsync($"https://www.speedrun.com/themes/user/{author}/image.png");
                avatar = success;
            }

            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = author.ToRawText(),
                    Url = $"https://www.speedrun.com/{author}",
                },
                Title = game,
                Url = nf.Item.Uri,
                Description = description,
                Color = new Color(229, 227, 87),
                Timestamp = DateTime.UtcNow,
                Footer = new EmbedFooterBuilder
                {
                    Text = "speedrun.com",
                    IconUrl = "https://raw.githubusercontent.com/NeKzor/NeKzBot/master/public/resources/icons/speedruncom_icon.png"
                }
            };

            if (!string.IsNullOrEmpty(thumbnail))
                embed.WithThumbnailUrl(thumbnail);
            if (avatar)
                embed.Author.WithIconUrl($"https://www.speedrun.com/themes/user/{author}/image.png");

            return embed.Build();
        }
    }
}
