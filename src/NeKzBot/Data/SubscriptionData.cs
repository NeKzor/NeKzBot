using LiteDB;
using Discord;
using Discord.Webhook;

namespace NeKzBot.Data
{
	public class SubscriptionData
	{
		[BsonId(true)]
		public int Id { get; set; }
		public IWebhook Webhook { get; set; }
	}
}