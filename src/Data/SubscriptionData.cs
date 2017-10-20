using Discord.Webhook;

namespace NeKzBot.Data
{
	public class SubscriptionData
	{
		public ulong Id { get; set; }
		public DiscordWebhookClient Client { get; set; }
	}
}