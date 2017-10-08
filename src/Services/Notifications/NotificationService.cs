using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.Webhook;

namespace NeKzBot.Services.Notifciations
{
	public abstract class NotificationService : INotificationService
	{
		public string UserName { get; set; }
		public string UserAvatar { get; set; }
		public uint SleepTime { get; set;}
		public bool Cancel { get; set; }
		public ConcurrentDictionary<ulong, DiscordWebhookClient> Subscribers { get; set; }

		protected NotificationService()
		{
			UserName = "NeKzBot";
			UserAvatar = "";
			SleepTime = 1 * 60 * 1000;
			Subscribers = new ConcurrentDictionary<ulong, DiscordWebhookClient>();
		}

		public async Task<bool> SubscribeAsync(ulong id, string token, bool test)
		{
			if (Subscribers.ContainsKey(id))
				return false;

			var client = new DiscordWebhookClient(id, token, new DiscordRestConfig
			{
#if DEBUG
				LogLevel = LogSeverity.Debug
#else
				LogLevel = LogSeverity.Error
#endif
			});
			if (test)
				await client.SendMessageAsync("Test Ping!");

			return Subscribers.TryAdd(id, client);
		}

		public async Task<bool> UnsubscribeAsync(ulong id, bool test)
		{
			if (!Subscribers.TryGetValue(id, out var client))
				return false;

			if (test)
				await client.SendMessageAsync("Test Ping!");

			return Subscribers.TryRemove(id, out _);
		}

		public abstract Task StartAsync();
		public abstract Task StopAsync();
	}
}