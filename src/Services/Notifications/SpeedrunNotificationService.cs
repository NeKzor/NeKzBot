using System.Threading.Tasks;
using LiteDB;
using Microsoft.Extensions.Configuration;

namespace NeKzBot.Services.Notifciations
{
	public class SpeedrunNotificationService : INotificationService
	{
		public string UserName { get; set; }
		public string UserAvatar { get; set; }
		public uint SleepTime { get; set; }
		public bool Cancel { get; set; }

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

		public Task StartAsync()
		{
			return Task.CompletedTask;
		}

		public Task StopAsync()
		{
			return Task.CompletedTask;
		}

		public Task<bool> SubscribeAsync(ulong id, string token, bool test)
		{
			throw new System.NotImplementedException();
		}

		public Task<bool> UnsubscribeAsync(ulong id, bool test)
		{
			throw new System.NotImplementedException();
		}
	}
}