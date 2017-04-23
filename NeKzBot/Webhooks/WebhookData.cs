using System.Threading.Tasks;
using Newtonsoft.Json;
using NeKzBot.Extensions;
using NeKzBot.Internals;
using NeKzBot.Resources;
using NeKzBot.Server;
using NeKzBot.Utilities;

namespace NeKzBot.Webhooks
{
	[JsonObject("webhook")]
	public sealed class WebhookData
	{
		public static InternalWatch TestPingWatch { get; set; } = new InternalWatch();

		[JsonProperty("id")]
		public ulong Id { get; set; }
		[JsonProperty("token")]
		public string Token { get; set; }
		[JsonProperty("guild_id")]
		public ulong GuildId { get; set; }
		[JsonProperty("user_id")]
		public ulong UserId { get; set; }
		[JsonProperty("name")]
		public string Name { get; set; }

		public WebhookData()
		{
		}
		public WebhookData(ulong id, string token, ulong guildid, ulong userid)
		{
			Id = id;
			Token = token;
			GuildId = guildid;
			UserId = userid;
		}

		public static async Task<bool> SubscribeAsync(string subscription, WebhookData data)
		{
			if (await Data.Get(subscription) != null)
				if (await Utils.ChangeDataAsync(subscription, $"{data.Id}{Utils.DataSeparator}{data.Token}{Utils.DataSeparator}{data.GuildId}{Utils.DataSeparator}{data.UserId}", DataChangeMode.Add) == DataChangeResult.Success)
					return await SendTestPing(data);
			return false;
		}

		public static async Task<bool> UnsubscribeAsync(string subscription, WebhookData data)
		{
			if (await Data.Get(subscription) != null)
				if (await Utils.ChangeDataAsync(subscription, data.Id.ToString(), DataChangeMode.Delete) == DataChangeResult.Success)
					return true;
			return false;
		}

		public static async Task<bool> SendTestPing(WebhookData data)
		{
			return (bool)(await WebhookService.ExecuteWebhookAsync(data, new Webhook
			{
				UserName = (string.IsNullOrEmpty(data.Name))
								  ? "NeKzHook"
								  : data.Name,
				AvatarUrl = Bot.Client.CurrentUser.AvatarUrl,
				Embeds = new Embed[]
				{
					new Embed
					{
						Title = "Webhook Ping Test!",
						Description = $"{await Utils.RngStringAsync(Data.BotGreetings)} {await Utils.RngStringAsync(Data.BotFeelings)}\nThis test took {await TestPingWatch.GetElapsedTime()} milliseconds.\nThe webhook id has been sent as a direct message.",
						Color = Data.BasicColor.RawValue,
						Url = Configuration.Default.AppUrl,
						Footer = new EmbedFooter("NeKzHook Service 2017")
					}
				}
			}));
		}
	}
}