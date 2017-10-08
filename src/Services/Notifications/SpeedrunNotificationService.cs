using System.Threading.Tasks;

namespace NeKzBot.Services.Notifciations
{
	public class SpeedrunNotificationService : NotificationService
	{
		public SpeedrunNotificationService()
		{
			UserName = "SpeedrunCom";
			UserAvatar = "https://github.com/NeKzor/NeKzBot/blob/old/NeKzBot/Resources/Public/speedruncom_webhookavatar.png";
			SleepTime = 1 * 60 * 1000;
		}

		public override Task StartAsync()
		{
			return Task.CompletedTask;
		}

		public override Task StopAsync()
		{
			return Task.CompletedTask;
		}
	}
}