using System.Threading.Tasks;
using Discord;
using NeKzBot.Data;

namespace NeKzBot.Services.Notification
{
	public interface INotificationService
	{
		Task Initialize();
		Task StartAsync();
		Task StopAsync();
		Task<bool> SubscribeAsync(IWebhook hook, bool test, string testMessage);
		Task<bool> UnsubscribeAsync(SubscriptionData subscription);
		Task<SubscriptionData> FindSubscription(ulong channelId);
	}
}