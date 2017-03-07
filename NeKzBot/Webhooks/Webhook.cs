using Newtonsoft.Json;
using NeKzBot.Extensions;

namespace NeKzBot.Webhooks
{
	public class Webhook
	{
		[JsonProperty("username")]
		public string UserName { get; set; }
		[JsonProperty("avatar_url")]
		public string AvatarUrl { get; set; }
		[JsonProperty("embeds")]
		public Embed[] Embeds { get; set; }

		public Webhook()
		{
		}

		public Webhook(string username, string avatarurl)
		{
			UserName = username;
			AvatarUrl = avatarurl;
		}

		public Webhook(string username, string avatarurl, Embed[] embeds)
		{
			UserName = username;
			AvatarUrl = avatarurl;
			Embeds = embeds;
		}
	}

	// TODO: retry sending images because I didn't know about the base64 header :>
	public class WebhookObject
	{
		[JsonProperty("id")]
		internal ulong Id { get; set; }
		[JsonProperty("guild_id")]
		internal ulong GuildId { get; set; }
		[JsonProperty("channel_id")]
		internal ulong ChannelId { get; set; }
		[JsonProperty("user")]
		internal InternalUser User { get; set; }
		[JsonProperty("name")]
		internal string Name { get; set; }      // Note: 2-100 characters
		[JsonProperty("avatar")]
		public string Avatar { get; set; }      // Note: base64 128x128 jpeg image (data:image/jpeg;base64,MY_BASE64_IMAGE_DATA_HERE)
		[JsonProperty("token")]
		public string Token { get; set; }

		internal class InternalUser
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
}