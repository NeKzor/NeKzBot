using System.Threading.Tasks;

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
		Task<bool> SubscribeAsync(ulong id, string token, bool test);
		Task<bool> UnsubscribeAsync(ulong id, bool test);
	}
}