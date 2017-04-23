using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NeKzBot.Extensions;

namespace NeKzBot.Webhooks
{
	public sealed class Webhook
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
		public Webhook(string username, string avatarurl, IEnumerable<Embed> embeds)
		{
			UserName = username;
			AvatarUrl = avatarurl;
			Embeds = embeds.ToArray();
		}
	}
}