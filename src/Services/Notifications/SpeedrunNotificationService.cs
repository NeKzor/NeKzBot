using System.Threading.Tasks;
using LiteDB;
using Microsoft.Extensions.Configuration;

namespace NeKzBot.Services.Notifciations
{
	public class SpeedrunNotificationService : NotificationService
	{
		private readonly IConfiguration _config;
		private readonly LiteDatabase _dataBase;

		public SpeedrunNotificationService(IConfiguration config, LiteDatabase dataBase)
		{
			_config = config;
			_dataBase = dataBase;
		}

		public Task Initialize()
		{
			UserName = "SpeedrunCom";
			UserAvatar = "https://github.com/NeKzor/NeKzBot/blob/old/NeKzBot/Resources/Public/speedruncom_webhookavatar.png";
			SleepTime = 1 * 60 * 1000;
			return Task.CompletedTask;
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