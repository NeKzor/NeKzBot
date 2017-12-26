using System.Threading.Tasks;
using Discord;
using LiteDB;
using Microsoft.Extensions.Configuration;
using NeKzBot.Data;

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

		public Task<bool> SubscribeAsync(IWebhook hook, bool test)
		{
			throw new System.NotImplementedException();
		}

		public Task<bool> UnsubscribeAsync(SubscriptionData subscription)
		{
			throw new System.NotImplementedException();
		}

		public Task<SubscriptionData> FindSubscription(ulong channelId)
		{
			throw new System.NotImplementedException();
		}
	}
}