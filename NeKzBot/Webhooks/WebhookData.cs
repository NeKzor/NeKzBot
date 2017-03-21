using System.Collections.Generic;
using System.Threading.Tasks;
using NeKzBot.Extensions;
using NeKzBot.Internals;
using NeKzBot.Resources;
using NeKzBot.Server;

namespace NeKzBot.Webhooks
{
	public class Subscribers
	{
		public List<WebhookData> Subs { get; set; }
		public Subscribers()
			=> Subs = new List<WebhookData>();
		public Subscribers(List<WebhookData> subs)
			=> Subs = subs;
	}

	public class WebhookData
	{
		public static InternalWatch Watch { get; set; } = new InternalWatch();

		public ulong Id { get; set; }
		public string Token { get; set; }
		public ulong GuildId { get; set; }
		public ulong UserId { get; set; }
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

		public static async Task<List<WebhookData>> ParseDataAsync(string file)
		{
			var list = new List<WebhookData>();
			var temp = await Utils.ReadFromFileAsync(file) as string[,];
			for (int i = 0; i < temp.GetLength(0); i++)
			{
				list.Add(new WebhookData
				{
					Id = ulong.Parse(temp[i, 0]),
					Token = temp[i, 1],
					GuildId = ulong.Parse(temp[i, 2]),
					UserId = ulong.Parse(temp[i, 3])
				});
			}
			return list;
		}

		public static async Task<bool> SubscribeAsync(string subscription, WebhookData data)
		{
			if (await Data.Get<Subscribers>(subscription) is IData sub)
				if (await Utils.ChangeDataAsync(sub, $"{data.Id}{Utils.Separator}{data.Token}{Utils.Separator}{data.GuildId}{Utils.Separator}{data.UserId}", DataChangeMode.Add) == string.Empty)
					return await SendTestPing(data);
			return false;
		}

		public static async Task<bool> UnsubscribeAsync(string subscription, WebhookData data)
		{
			if (await Data.Get<Subscribers>(subscription) is IData sub)
				if (await Utils.ChangeDataAsync(sub, $"{data.Id}", DataChangeMode.Add) == string.Empty)
					return true;
			return false;
		}

		public static async Task<bool> SendTestPing(WebhookData data)
		{
			return (bool)(await WebhookService.ExecuteWebhookAsync(data, new Webhook
			{
				UserName = (string.IsNullOrEmpty(data.Name))
								  ? data.Name
								  : "NeKzHook",
				AvatarUrl = Bot.Client.CurrentUser.AvatarUrl,
				Embeds = new Embed[]
				{
					new Embed
					{
						Title = "Webhook Ping Test!",
						Description = $"{await Utils.RngStringAsync(Data.BotGreetings)} {await Utils.RngStringAsync(Data.BotFeelings)}\nThis test took {await Watch.GetElapsedTime()} milliseconds.\nThe webhook id has been sent as a direct message.",
						Color = Data.BasicColor.RawValue,
						Url = Configuration.Default.AppUrl,
						Footer = new EmbedFooter("NeKzHook Service 2017")
					}
				}
			}));
		}
	}
}