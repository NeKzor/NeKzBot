using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using LiteDB;
using Microsoft.Extensions.Configuration;
using NeKzBot.API;
using NeKzBot.Data;

namespace NeKzBot.Services.Notifications.Speedrun
{
    public class SpeedrunNotificationService : NotificationService
    {
        private SpeedrunComApiClient? _client;

        public SpeedrunNotificationService(IConfiguration config, LiteDatabase dataBase)
            : base(config, dataBase)
        {
            _embedBuilder = x => BuildEmbedAsync(x);
        }

        public override Task Initialize()
        {
            _ = base.Initialize();

            _userName = "SpeedrunCom";
            _userAvatar = "https://raw.githubusercontent.com/NeKzor/NeKzBot/master/public/resources/avatars/speedruncom_avatar.png";
            _sleepTime = 1 * 60 * 1000;
            _retryTime = 1 * 60 * 1000;

            _client = new SpeedrunComApiClient
            (
                _config["user_agent"],
                _config["speedrun_token"]
            );

            var db = GetTaskCache<SpeedrunCacheData>()
                .GetAwaiter()
                .GetResult();

            var cache = db
                .FindAll()
                .FirstOrDefault();

            if (cache is null)
            {
                _ = LogWarning("Creating new cache");
                db.Insert(new SpeedrunCacheData());
            }
            return Task.CompletedTask;
        }

        public override async Task StartAsync()
        {
            if (_client is null)
                throw new Exception("Service not initialized");

            task_start:
            try
            {
                await base.StartAsync();

                while (_isRunning)
                {
                    //await LogInfo("Checking...");

                    var watch = Stopwatch.StartNew();

                    var db = await GetTaskCache<SpeedrunCacheData>();
                    var cache = db
                        .FindAll()
                        .FirstOrDefault();

                    if (cache is null)
                    {
                        await LogWarning("Task cache not found!");
                        goto retry;
                    }

                    var notifications = await _client.GetNotificationsAsync(21);

                    void UpdateCache()
                    {
                        cache.Notifications = notifications.Take(11);

                        if (!db.Update(cache))
                            throw new Exception("Failed to update cache!");
                    }

                    if (!notifications.Any())
                    {
                        await LogWarning("Fetch failed");
                        goto retry;
                    }

                    if (!cache.Notifications.Any())
                    {
                        UpdateCache();
                        goto retry;
                    }

                    var sending = new List<SpeedrunNotification>();
                    foreach (var old in cache.Notifications)
                    {
                        sending.Clear();
                        foreach (var notification in notifications)
                        {
                            if (old.Id == notification.Id)
                                goto send;
                            sending.Add(notification);
                        }
                    }

                send:

                    //sending.AddRange(notifications.Take(5));

                    //sending = notifications.Take(11).ToList();

                    if (sending.Count > 0)
                    {
                        await LogInfo($"Found {sending.Count} new notifications to send");

                        if (sending.Count >= 11)
                            await LogWarning("Webhook rate limit exceeded: " + sending.Count);

                        await SendAsync(sending);
                        //return;

                        UpdateCache();
                    }

                retry:
                    var delay = (int)(_sleepTime - watch.ElapsedMilliseconds);
                    if (delay < 0)
                        await LogWarning($"Task took too long: {delay}ms");

                    await Task.Delay(delay, _cancellation!.Token);
                }
            }
            catch (Exception ex)
            {
                await LogException(ex);
                await StopAsync();
                await Task.Delay((int)_retryTime);
                goto task_start;
            }

            await LogWarning("Task ended");
        }

        private async Task<Embed> BuildEmbedAsync(object notification)
        {
            if (_client is null)
                throw new System.Exception("Service not initialized");

            if (!(notification is SpeedrunNotification nf))
                throw new System.Exception("Notification object was not type of SpeedrunNotification");

            static SpeedrunNotificationType ResolveNotificationType(string? type) => type switch
            {
                "thread" => new ThreadNotification(),
                "post" => new PostNotification(),
                "guide" => new GuideNotification(),
                "resource" => new ResourceNotification(),
                "run" => new RunNotification(),
                "moderator" => new ModeratorNotification(),
                _ => throw new Exception("Unknown rel notification type: " + type)
            };

            var (author, game, description) = ResolveNotificationType(nf.Item?.Rel).Get(nf);

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

            var embed = new EmbedBuilder
            {

                Title = game,
                Url = nf.Item?.Uri,
                Description = description,
                Color = new Color(229, 227, 87),
                Timestamp = DateTime.Now,
                Footer = new EmbedFooterBuilder
                {
                    Text = "speedrun.com",
                    IconUrl = "https://raw.githubusercontent.com/NeKzor/NeKzBot/master/public/resources/icons/speedruncom_icon.png"
                }
            };

            if (!string.IsNullOrEmpty(thumbnail))
                embed.WithThumbnailUrl(thumbnail);

            if (!string.IsNullOrEmpty(author) && !author.Contains(" "))
            {
                embed.WithAuthor(new EmbedAuthorBuilder
                {
                    Name = author,
                    Url = $"https://www.speedrun.com/{author}",
                });

                using var wc = new WebClient(_config["user_agent"]);
                var avatar = $"https://www.speedrun.com/themes/user/{author}/image.png";
                var (success, _) = await wc.Ping(avatar);
                if (success)
                    embed.Author.WithIconUrl(avatar);
            }

            return embed.Build();
        }
    }
}
