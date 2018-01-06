using System.Threading.Tasks;
using Discord;
using Discord.Webhook;
using LiteDB;
using Microsoft.Extensions.Configuration;
using NeKzBot.Data;
using NeKzBot.Services.Notification;

namespace NeKzBot.Services.Notifications
{
	public abstract class NotificationService : INotificationService
	{
		public string UserName { get; set; }
		public string UserAvatar { get; set; }
		public uint SleepTime { get; set; }
		public bool Cancel { get; set; }
		public string GlobalId { get; set; }

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
			GlobalId = GetType().Name;
			return Task.CompletedTask;
		}
		public virtual Task StartAsync()
		{
			return Task.CompletedTask;
		}
		public virtual Task StopAsync()
		{
			if (!Cancel)
				Cancel = true;
			return Task.CompletedTask;
		}
		public virtual async Task<bool> SubscribeAsync(IWebhook hook, bool test, string testMessage)
		{
			var db = _dataBase.GetCollection<SubscriptionData>(GlobalId);
			var data = db.FindOne(d => d.ChannelId == hook.ChannelId);

			if (data == null)
			{
				if (test)
				{
					using (var wc = new DiscordWebhookClient(hook))
						await wc.SendMessageAsync(testMessage, username: UserName, avatarUrl: UserAvatar);
				}

				return db.Upsert(new SubscriptionData()
				{
					ChannelId = hook.ChannelId,
					WebhookId = hook.Id,
					WebhookToken = hook.Token
				});
			}
			return default;
		}
		public virtual Task<bool> UnsubscribeAsync(SubscriptionData subscription)
		{
			var db = _dataBase.GetCollection<SubscriptionData>(GlobalId);
			return Task.FromResult(db.Delete(d => d.ChannelId == subscription.ChannelId) == 1);
		}
		public virtual Task<SubscriptionData> FindSubscription(ulong channelId)
		{
			var db = _dataBase.GetCollection<SubscriptionData>(GlobalId);
			var data = db.FindOne(d => d.ChannelId == channelId);
			return Task.FromResult(data);
		}
	}
}