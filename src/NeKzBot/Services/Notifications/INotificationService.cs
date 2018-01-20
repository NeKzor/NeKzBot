using System.Collections.Generic;
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
		Task SendAsync(IEnumerable<object> notifications);
		Task<bool> SubscribeAsync(IWebhook hook, string helloWorldMessage);
		Task<bool> UnsubscribeAsync(SubscriptionData subscription);
		Task<(IWebhook, SubscriptionData)> FindSubscriptionAsync(IEnumerable<IWebhook> webhooks);
	}
}