using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Webhook;
using LiteDB;
using Microsoft.Extensions.Configuration;
using NeKzBot.Data;

namespace NeKzBot.Services.Notifications
{
    public abstract class NotificationService : INotificationService
    {
        public event Func<string, Exception?, Task>? Log;
        public bool IsRunning => _isRunning;
        public string? Name =>  _globalId;

        protected string? _userName { get; set; }
        protected string? _userAvatar { get; set; }
        protected uint _sleepTime { get; set; }
        protected uint _retryTime { get; set; }
        protected string? _globalId { get; set; }
        protected bool _isRunning { get; set; }
        protected CancellationTokenSource? _cancellation { get; set; }
        protected Func<object, Task<Embed?>>? _embedBuilder { get; set; }

        protected readonly IConfiguration _config;
        protected readonly LiteDatabase _dataBase;

        protected NotificationService(IConfiguration config, LiteDatabase dataBase)
        {
            _config = config;
            _dataBase = dataBase;
        }

        public virtual Task Initialize()
        {
            // Name of derived class
            _globalId = GetType().Name;
            return Task.CompletedTask;
        }

        // Notification tasks
        public virtual Task StartAsync()
        {
            if (_isRunning)
                throw new InvalidOperationException("Task already started!");

            _cancellation = new CancellationTokenSource();
            _isRunning = true;
            return Task.CompletedTask;
        }
        public virtual Task StopAsync()
        {
            if (_isRunning && _cancellation is {})
            {
                _isRunning = false;
                _cancellation.Cancel();
                _cancellation.Dispose();
            }
            return Task.CompletedTask;
        }
        public virtual async Task SendAsync(IEnumerable<object> notifications)
        {
            if (_embedBuilder is null)
                throw new System.Exception("Embed builder is not set");

            var db = await GetSubscribers();
            var subscribers = db
                .FindAll()
                .ToList();

            await LogInfo($"{subscribers.Count} subs found");
            if (subscribers.Count == 0) return;

            await LogInfo("Sending hooks");

            var failed = new List<SubscriptionData>();
            foreach (var nf in notifications)
            {
                var embed = await _embedBuilder(nf);
                if (embed is null) continue;

                foreach (var sub in subscribers)
                {
                    if (failed.Contains(sub)) continue;

                    try
                    {
                        using var wc = new DiscordWebhookClient(sub.WebhookId, sub.WebhookToken);
                        await wc.SendMessageAsync
                        (
                            string.Empty,
                            embeds: new Embed[] { embed },
                            username: _userName,
                            avatarUrl: _userAvatar
                        );
                    }
                    // Make sure to catch only on this special exception
                    // which tells us that this webhook doesn't exist
                    catch (InvalidOperationException ex)
                        when (ex.Message.StartsWith("Could not find a webhook"))
                    {
                        failed.Add(sub);
                        await LogWarning($"Sub ID = {sub.Id} not found");
                    }
                }
            }

            // Delete failed subscribers
            foreach (var sub in failed)
            {
                if (db.Delete(d => d.WebhookId == sub.WebhookId) != 1)
                    await LogWarning($"Database failed to delete sub ID = {sub.Id}");
                else
                    await LogWarning($"Deleted sub ID = {sub.Id}");
            }
        }

        // Subscription tasks
        public virtual async Task<bool> SubscribeAsync(IWebhook hook, string? helloWorldMessage = null)
        {
            var db = await GetSubscribers();
            var data = db.FindOne(d => d.WebhookId == hook.ChannelId);

            // When is the guild id null again???
            if ((data is null) && (hook.GuildId != null))
            {
                // Test message
                if (!string.IsNullOrEmpty(helloWorldMessage))
                {
                    using var wc = new DiscordWebhookClient(hook);
                    await wc.SendMessageAsync
                    (
                        helloWorldMessage,
                        username: _userName,
                        avatarUrl: _userAvatar
                    );
                }

                return db.Upsert
                (
                    new SubscriptionData()
                    {
                        WebhookId = hook.Id,
                        WebhookToken = hook.Token,
                        GuildId = hook.GuildId.Value
                    }
                );
            }
            return default;
        }
        public virtual async Task<bool> UnsubscribeAsync(SubscriptionData subscription)
        {
            var db = await GetSubscribers();
            return (db.Delete(d => d.WebhookId == subscription.WebhookId) == 1);
        }
        public virtual async Task<(IWebhook?, SubscriptionData?)> FindSubscriptionAsync(IEnumerable<IWebhook> webhooks)
        {
            var db = await GetSubscribers();
            foreach (var hook in webhooks)
            {
                var sub = db.FindOne(d => d.WebhookId == hook.Id);
                if (sub is null) continue;
                return (hook, sub);
            }
            return (default, default);
        }

        // Database tasks
        protected Task<LiteCollection<T>> GetTaskCache<T>()
        {
            return Task.FromResult(_dataBase.GetCollection<T>($"{_globalId}_cache"));
        }
        protected Task<bool> DropTaskCache()
        {
            return Task.FromResult(_dataBase.DropCollection($"{_globalId}_cache"));
        }
        protected Task<LiteCollection<SubscriptionData>> GetSubscribers()
        {
            return Task.FromResult(_dataBase.GetCollection<SubscriptionData>(_globalId));
        }
        internal async Task CleanupAsync()
        {
            var db = await GetSubscribers();
            foreach (var sub in db.FindAll())
            {
                try
                {
                    // This will send a GET request
                    _ = new DiscordWebhookClient(sub.WebhookId, sub.WebhookToken);
                }
                catch (InvalidOperationException ex)
                    when (ex.Message == "Could not find a webhook for the supplied credentials.")
                {
                    await LogWarning($"Deleting {sub.Id}...");
                }

                if (db.Delete(sub.Id))
                    await LogWarning($"Deleted sub ID = {sub.Id} in {sub.GuildId}");
                else
                    await LogWarning($"Database failed to delete sub ID = {sub.Id}");
            }
        }

        // Logging tasks
        protected Task LogInfo(string message)
        {
#if DEBUG
            _ = Log?.Invoke($"{_globalId}\t{message}", null);
#endif
            return Task.CompletedTask;
        }
        protected Task LogWarning(string message)
        {
            _ = Log?.Invoke($"{_globalId}\t{message}!", null);
            return Task.CompletedTask;
        }
        protected Task LogException(Exception ex)
        {
            if (_globalId is null)
                throw new System.Exception("Service not initialized");

            _ = Log?.Invoke(_globalId, ex);
            return Task.CompletedTask;
        }
    }
}
