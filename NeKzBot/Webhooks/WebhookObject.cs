using Newtonsoft.Json;

namespace NeKzBot.Webhooks
{
	public sealed class WebhookObject
	{
		[JsonProperty("id")]
		internal ulong Id { get; set; }
		[JsonProperty("guild_id")]
		internal ulong GuildId { get; set; }
		[JsonProperty("channel_id")]
		internal ulong ChannelId { get; set; }
		[JsonProperty("user")]
		internal UserObject User { get; set; }
		[JsonProperty("name")]
		internal string Name { get; set; }      // Note: 2-100 characters
		[JsonProperty("avatar")]
		public string Avatar { get; set; }      // Note: base64 128x128 jpeg image (data:image/jpeg;base64,MY_BASE64_IMAGE_DATA_HERE)
		[JsonProperty("token")]
		public string Token { get; set; }
	}

	internal sealed class UserObject
	{
		[JsonProperty("username")]
		internal string Name { get; set; }
		[JsonProperty("discriminator")]
		internal string Discriminator { get; set; }
		[JsonProperty("id")]
		internal ulong Id { get; set; }
		[JsonProperty("avatar")]
		internal string Avatar { get; set; }
	}
}