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
                    await LogInfo($"Cache: {cache.Notifications.Count()} (ID = {cache.Id})");

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
                    await SendAsync(sending);

                    cache.Notifications = notifications
                        .Take(11);

                    if (!db.Update(cache))
                        throw new Exception("Failed to update cache!");

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
            var title = "Latest Notification";
            var category = string.Empty;
            var description = nf.Text;

            // Old code here, I hope this won't throw an exception :>
            switch (nf.Item.Rel)
            {
                case "post":
                    title = "Thread Response";
                    category = nf.Text.Substring(nf.Text.IndexOf(" in the ") + " in the ".Length, nf.Text.IndexOf(" forum.") - nf.Text.IndexOf(" in the ") - " in the ".Length);
                    description = $"*[{nf.Text.Substring(nf.Text.IndexOf("'") + 1, nf.Text.LastIndexOf("'") - nf.Text.IndexOf("'") - 1)}]({nf.Item.Uri.ToRawText()})*";
                    break;
                case "run":
                    title = "Run Submission";
                    category = nf.Text.Substring(nf.Text.IndexOf("beat the WR in ") + "beat the WR in ".Length, nf.Text.IndexOf(". The new WR is") - nf.Text.IndexOf("beat the WR in ") - "beat the WR in ".Length);
                    description = $"New {(nf.Text.Contains(" beat the WR in ") ? "**World Record**" : "Personal Best")} in [{category.ToRawText()}]({nf.Item.Uri})\nwith a time of {nf.Text.Substring(nf.Text.LastIndexOf(". The new WR is ") + ". The new WR is ".Length)}.";
                    break;
                case "game":
                    // ???
                    break;
                case "guide":
                    title = "New Guide";
                    // ???
                    break;
                case "thread":      // Undocumented API
                    title = "New Thread Post";
                    category = nf.Text.Substring(nf.Text.LastIndexOf(" in the ") + " in the ".Length, nf.Text.IndexOf(" forum:") - nf.Text.IndexOf(" in the ") - "in the ".Length);
                    description = $"*[{nf.Text.Substring(nf.Text.LastIndexOf(" forum: ") + " forum: ".Length)}]({nf.Item.Uri.ToRawText()})*";
                    break;
                case "moderator":   // Undocumented API
                    title = "New Moderator";
                    category = nf.Text.Substring(nf.Text.IndexOf("has been added to ") + "has been added to ".Length, nf.Text.IndexOf(" as a moderator.") - nf.Text.IndexOf("has been added to ") - "has been added to ".Length);
                    description = $"{author.ToRawText()} is now a moderator for {category.ToRawText()}! :heart:";
                    break;
                case "resource":    // Undocumented API
                    category = nf.Text.Substring(nf.Text.IndexOf(" for ") + " for ".Length, nf.Text.LastIndexOf(" has") - nf.Text.IndexOf(" for ") - "for".Length);
                    if (nf.Text.EndsWith("updated."))
                    {
                        title = "Updated Resource";
                        description = $"The resource *{nf.Text.Substring(nf.Text.IndexOf("The tool resource '") + "The tool resource '".Length + 1, nf.Text.LastIndexOf("' for ") - nf.Text.IndexOf("The tool resource '") - "The tool resource '".Length - 1)}* has been updated for {category.ToRawText()}.";
                    }
                    else if (nf.Text.EndsWith("added."))
                    {
                        title = "New Resource";
                        description = $"The resource *{nf.Text.Substring(nf.Text.IndexOf("A new tool resource, ") + "A new tool resource, ".Length + 1, nf.Text.LastIndexOf(", has been added to ") - nf.Text.IndexOf("A new tool resource, ") - "A new tool resource, ".Length - 1)}* has been added for {category.ToRawText()}.";
                    }
                    break;
            }

            var thumbnail = default(string);
            if (!string.IsNullOrEmpty(category))
            {
                var games = await _client.GetGamesAsync(category);
                var game = games?.FirstOrDefault();
                thumbnail = game.Assets.CoverTiny.Uri;
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
                    Name = author,
                    Url = $"https://www.speedrun.com/{author}",
                },
                Title = title,
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
