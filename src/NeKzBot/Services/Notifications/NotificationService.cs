using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Webhook;
using Discord.WebSocket;
using LiteDB;
using Microsoft.Extensions.Configuration;
using NeKzBot.Data;
using NeKzBot.Services.Notification;

namespace NeKzBot.Services.Notifications
{
	public abstract class NotificationService : INotificationService
	{
		public event Func<string, Exception, Task> Log;

		protected string _userName { get; set; }
		protected string _userAvatar { get; set; }
		protected uint _sleepTime { get; set; }
		protected string _globalId { get; set; }
		protected bool _isRunning { get; set; }
		protected CancellationTokenSource _cancellation { get; set; }

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
			// I'll never use this anyway...
			if (!_isRunning)
			{
				_cancellation.Cancel();
				_cancellation.Dispose();
				_isRunning = false;
			}
			return Task.CompletedTask;
		}

		// Subscription tasks
		public virtual async Task<bool> SubscribeAsync(IWebhook hook, string helloWorld = null)
		{
			var db = await GetSubscribers();
			var data = db.FindOne(d => d.WebhookId == hook.ChannelId);
			
			// When is the guild id null again???
			if ((data == null) && (hook.GuildId != null))
			{
				// Test message
				if (!string.IsNullOrEmpty(helloWorld))
				{
					using (var wc = new DiscordWebhookClient(hook))
					{
						await wc.SendMessageAsync
						(
							helloWorld,
							username: _userName,
							avatarUrl: _userAvatar
						);
					}
				}

				return db.Upsert
				(
					new SubscriptionData()
					{
						WebhookId = hook.Id,
						WebhookToken = hook.Token,
						GuildId = hook.GuildId
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
		public virtual async Task<(IWebhook, SubscriptionData)> FindSubscriptionAsync(IEnumerable<IWebhook> hooks)
		{
			var db = await GetSubscribers();
			foreach (var hook in hooks) 
			{
				var sub = db.FindOne(d => d.WebhookId == hook.Id);
				if (sub == null) continue;
				return (hook, sub);
			}
			return (default, default);
		}
		
		// Database tasks
		protected Task<LiteCollection<T>> GetTaskCache<T>()
		{
			return Task.FromResult(_dataBase.GetCollection<T>($"{_globalId}_cache"));
		}
		protected Task<LiteCollection<SubscriptionData>> GetSubscribers()
		{
			return Task.FromResult(_dataBase.GetCollection<SubscriptionData>(_globalId));
		}
		protected async Task AutoDeleteAsync(IEnumerable<SubscriptionData> subscribers)
		{
			var db = await GetSubscribers();
			foreach (var sub in subscribers)
			{
				if (db.Delete(d => d.WebhookId == sub.WebhookId) != 1)
					await LogWarning($"Database failed to delete sub ID = {sub.Id}");
				else
					await LogWarning($"Deleted sub ID = {sub.Id}");
			}
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
					await LogWarning($"Deleted sub ID = {sub.Id} in {sub.GuildId ?? 0}");
				else
					await LogWarning($"Database failed to delete sub ID = {sub.Id}");
			}
		}

		// Logging tasks
		protected Task LogInfo(string message)
		{
			_ = Log.Invoke($"{_globalId}\t{message}", null);
			return Task.CompletedTask;
		}
		protected Task LogWarning(string message)
		{
			_ = Log.Invoke($"{_globalId}\t{message}!", null);
			return Task.CompletedTask;
		}
		protected Task LogException(Exception ex)
		{
			_ = Log.Invoke(_globalId, ex);
			return Task.CompletedTask;
		}
	}
}