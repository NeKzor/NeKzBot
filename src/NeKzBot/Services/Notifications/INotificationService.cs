using System.Threading.Tasks;
using Discord;
using NeKzBot.Data;

namespace NeKzBot.Services.Notifciations
{
	public interface INotificationService
	{
		string UserName { get; set; }
		string UserAvatar { get; set; }
		uint SleepTime { get; set; }
		bool Cancel { get; set; }
		Task Initialize();
		Task StartAsync();
		Task StopAsync();
		Task<bool> SubscribeAsync(IWebhook hook, bool test);
		Task<bool> UnsubscribeAsync(SubscriptionData subscription);
		Task<SubscriptionData> FindSubscription(ulong channelId);
	}
}