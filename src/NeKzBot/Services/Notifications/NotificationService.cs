using System;
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

		protected readonly IConfiguration _config;
		protected readonly LiteDatabase _dataBase;

		public string _userName;
		public string _userAvatar;
		public uint _sleepTime;
		public string _globalId;
		protected bool _isRunning;

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
			return Task.CompletedTask;
		}
		public virtual Task StopAsync()
		{
			if (!_isRunning)
				_isRunning = false;
			return Task.CompletedTask;
		}

		// Subscription tasks
		public virtual async Task<bool> SubscribeAsync(IWebhook hook, bool test, string testMessage)
		{
			var db = _dataBase.GetCollection<SubscriptionData>(_globalId);
			var data = db.FindOne(d => d.Webhook.ChannelId == hook.ChannelId);

			if (data == null)
			{
				if (test)
				{
					using (var wc = new DiscordWebhookClient(hook))
						await wc.SendMessageAsync(testMessage, username: _userName, avatarUrl: _userAvatar);
				}

				return db.Upsert(new SubscriptionData(){ Webhook = hook });
			}
			return default;
		}
		public virtual Task<bool> UnsubscribeAsync(SubscriptionData subscription)
		{
			var db = _dataBase.GetCollection<SubscriptionData>(_globalId);
			return Task.FromResult(db.Delete(d => d.Webhook.ChannelId == subscription.Webhook.ChannelId) == 1);
		}
		public virtual Task<SubscriptionData> FindSubscription(ulong channelId)
		{
			var db = _dataBase.GetCollection<SubscriptionData>(_globalId);
			var data = db.FindOne(d => d.Webhook.ChannelId == channelId);
			return Task.FromResult(data);
		}
		
		internal async Task CleanupAsync()
		{
			var db = _dataBase.GetCollection<SubscriptionData>(_globalId);
			foreach (var sub in db.FindAll())
			{
				var hook = await sub.Webhook.Channel.GetWebhookAsync(sub.Webhook.Id);
				if (hook == null)
				{
					db.Delete(sub.Id);
				}
			}
		}

		protected Task LogWarning(string message)
		{
			_ = Log.Invoke($"[{_globalId}] {message}", null);
			return Task.CompletedTask;
		}
		protected Task LogException(Exception ex)
		{
			_ = Log.Invoke(_globalId, ex);
			return Task.CompletedTask;
		}
	}
}