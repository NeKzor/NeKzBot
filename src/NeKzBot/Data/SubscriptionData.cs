using LiteDB;

namespace NeKzBot.Data
{
	public class SubscriptionData
	{
		[BsonId]
		public ulong ChannelId { get; set; }
		public ulong WebhookId { get; set; }
		public string WebhookToken { get; set; }
	}
}