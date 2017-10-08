using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord.Webhook;

namespace NeKzBot.Services.Notifciations
{
	public interface INotificationService
	{
		string UserName { get; set; }
		string UserAvatar { get; set; }
		uint SleepTime { get; set; }
		bool Cancel { get; set; }
		ConcurrentDictionary<ulong, DiscordWebhookClient> Subscribers { get; set; }
		Task<bool> SubscribeAsync(ulong id, string token, bool test);
		Task<bool> UnsubscribeAsync(ulong id, bool test);
		Task StartAsync();
		Task StopAsync();
	}
}