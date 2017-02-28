using System.Collections.Generic;
using System.Threading.Tasks;
using NeKzBot.Classes.Discord;
using NeKzBot.Internals;
using NeKzBot.Resources;
using NeKzBot.Server;

namespace NeKzBot.Webhooks
{
	public class WebhookData
	{
		public static InternalWatch Watch { get; set; } = new InternalWatch();

		public ulong Id { get; set; }
		public string Token { get; set; }
		public string UserName { get; set; }
		public ulong UserId { get; set; }

		public WebhookData()
		{
		}

		public WebhookData(ulong id, string token, string username, ulong userid)
		{
			Id = id;
			Token = token;
			UserName = username;
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
					UserName = temp[i, 2],
					UserId = ulong.Parse(temp[i, 3])
				});
			}
			return list;
		}

		public static async Task<bool> SubscribeAsync(string subscription, WebhookData data)
		{
			if (await Data.DataExists(subscription, out var index))
				if (await Utils.AddDataAsync(index, $"{data.Id}{Utils.Separator}{data.Token}{Utils.Separator}{data.UserName}{Utils.Separator}{data.UserId}") == string.Empty)
					return await SendTestPing(data);
			return false;
		}

		public static async Task<bool> UnsubscribeAsync(string subscription, WebhookData data)
		{
			if (await Data.DataExists(subscription, out var index) )
				if (await Utils.DeleteDataAsync(index, $"{data.Id}") == string.Empty)
					return true;
			return false;
		}

		public static async Task<bool> SendTestPing(WebhookData data)
		{
			return (bool)(await WebhookService.ExecuteWebhookAsync(data, new Webhook
			{
				UserName = data.UserName,
				AvatarUrl = Bot.Client.CurrentUser.AvatarUrl,
				Embeds = new Embed[]
				{
					new Embed
					{
						Title = "Webhook Ping Test!",
						Description = $"{await Utils.RNGStringAsync(Data.BotGreetings)} {await Utils.RNGStringAsync(Data.BotFeelings)}\nThis test took {await Watch.GetElapsedTimeAsync(InternalWatch.Time.Milliseconds)} milliseconds.\nThe webhook id has been sent as a direct message.",
						Color = Data.BasicColor.RawValue,
						Url = Configuration.Default.AppUrl,
						Footer = new EmbedFooter { Text = "NeKzHook Service 2017" }
					}
				}
			}));
		}
	}
}