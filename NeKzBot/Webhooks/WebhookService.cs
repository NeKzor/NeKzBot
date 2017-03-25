﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NeKzBot.Classes;
using NeKzBot.Extensions;
using NeKzBot.Server;

namespace NeKzBot.Webhooks
{
	// Note: only tested 4 out of 7
	// Resource: https://discordapp.com/developers/docs/resources/webhook
	public static class WebhookService
	{
		private static HttpClient _client;

		public static async Task InitAsync()
		{
			_client = await new Fetcher(new List<WebHeader>()
			{
				new WebHeader("User-Agent", $"{Configuration.Default.AppName}/{Configuration.Default.AppVersion}"),
				new WebHeader("Authorization", $"Bot {Credentials.Default.DiscordBotToken}")
			})
			.GetClient();
		}

		public static async Task<WebhookObject> CreateWebhookAsync(ulong channelid, WebhookData data)
		{
			try
			{
				var result = await _client.PostAsync($"{DiscordConstants.BaseApiUrl}/channels/{channelid}/webhooks", new StringContent(JsonConvert.SerializeObject(new WebhookObject
				{
					ChannelId = channelid,
					Name = data.Name
				}), Encoding.UTF8, "application/json"));

				if (!(result.IsSuccessStatusCode))
					return await Logger.SendAsync($"Webhook.CreateWebhookAsync POST Error ({data.GuildId})(CHANNEL_ID {channelid})\n{result.Content}", LogColor.Error) as WebhookObject;
				return JsonConvert.DeserializeObject<WebhookObject>(await result.Content.ReadAsStringAsync());
			}
			catch (Exception e)
			{
				return await Logger.SendAsync("Webhook.CreateWebhookAsync Error", e) as WebhookObject;
			}
		}

		public static async Task<List<WebhookObject>> GetGuildWebhooksAsync(WebhookData data)
		{
			try
			{
				var result = await _client.GetAsync($"{DiscordConstants.BaseApiUrl}/guilds/{data.Id}/webhooks");
				if (result.IsSuccessStatusCode)
					return await Logger.SendAsync($"WebhookService.GetGuildWebhooksAsync GET Error (ID {data.Id})\n{result.Content}", LogColor.Error) as List<WebhookObject>;
				return JsonConvert.DeserializeObject<List<WebhookObject>>(await result.Content.ReadAsStringAsync());
			}
			catch (Exception e)
			{
				return await Logger.SendAsync("WebhookService.GetGuildWebhooksAsync Error", e) as List<WebhookObject>;
			}
		}

		public static async Task<List<WebhookObject>> GetChannelWebhooksAsync(WebhookData data)
		{
			try
			{
					var result = await _client.GetAsync($"{DiscordConstants.BaseApiUrl}/channels/{data.Id}/webhooks");
					if (!(result.IsSuccessStatusCode))
						await Logger.SendAsync($"WebhookService.GetChannelWebhooksAsync GET Error (ID {data.Id})\n{result.Content}", LogColor.Error);
					else
						return JsonConvert.DeserializeObject<List<WebhookObject>>(await result.Content.ReadAsStringAsync());
			}
			catch (Exception e)
			{
				await Logger.SendAsync("WebhookService.GetChannelWebhooksAsync Error", e);
			}
			return null;
		}

		public static async Task<WebhookObject> GetWebhookAsync(WebhookData data, bool ispingtest = false)
		{
			try
			{
				var result = await _client.GetAsync($"{DiscordConstants.BaseApiUrl}/webhooks/{data.Id}{(string.IsNullOrEmpty(data.Token) ? string.Empty : $"/{data.Token}")}");
				if (!(result.IsSuccessStatusCode))
				{
					return (ispingtest)
								? default(WebhookObject)
								: await Logger.SendAsync($"WebhookService.GetWebhookAsync GET Error ({data.GuildId})(ID {data.Id})\n{result.Content}", LogColor.Error) as WebhookObject;
				}
				return JsonConvert.DeserializeObject<WebhookObject>(await result.Content.ReadAsStringAsync());
			}
			catch (Exception e)
			{
				return await Logger.SendAsync("Webhook.GetWebhookAsync Error", e) as WebhookObject;
			}
		}

		public static async Task<WebhookObject> ModifyWebhookAsync(WebhookData data, string username = default(string), string avatar = default(string))
		{
			try
			{
				if ((string.IsNullOrEmpty(username))
				&& string.IsNullOrEmpty(avatar))
					return null;

				var hook = (string.IsNullOrEmpty(username))
								  ? new Webhook { AvatarUrl = avatar }
								  : (string.IsNullOrEmpty(avatar))
										   ? new Webhook { UserName = username }
										   : new Webhook { UserName = username, AvatarUrl = avatar };

				var result = await _client.SendAsync(new HttpRequestMessage(new HttpMethod("PATCH"), $"{DiscordConstants.BaseApiUrl}/webhooks/{data.Id}{(string.IsNullOrEmpty(data.Token) ? string.Empty : $"/{data.Token}")}")
				{
					Content = new StringContent(JsonConvert.SerializeObject(hook))
				});
				if (!(result.IsSuccessStatusCode))
					return await Logger.SendAsync($"WebhookService.ModifyWebhookAsync PATCH Error (ID {data.Id})\n{result.Content}", LogColor.Error) as WebhookObject;
				return JsonConvert.DeserializeObject<WebhookObject>(await result.Content.ReadAsStringAsync());
			}
			catch (Exception e)
			{
				return await Logger.SendAsync("WebhookService.ModifyWebhookAsync Error", e) as WebhookObject;
			}
		}

		public static async Task<bool?> DeleteWebhookAsync(WebhookData data)
		{
			try
			{
				var result = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"{DiscordConstants.BaseApiUrl}/webhooks/{data.Id}{(string.IsNullOrEmpty(data.Token) ? string.Empty : $"/{data.Token}")}"));
				if (!(result.IsSuccessStatusCode))
					await Logger.SendAsync($"WebhookService.DeleteWebhookAsync DELETE Error (ID {data.Id})\n{result.Content}", LogColor.Error);
				else
					return true;
			}
			catch (Exception e)
			{
				return await Logger.SendAsync("WebhookService.DeleteWebhookAsync Error", e) as bool?;
			}
			return false;
		}

		public static async Task<bool?> ExecuteWebhookAsync(WebhookData data, Webhook hook)
		{
			try
			{
				var result = await _client.PostAsync($"{DiscordConstants.BaseApiUrl}/webhooks/{data.Id}/{data.Token}", new StringContent(JsonConvert.SerializeObject(hook), Encoding.UTF8, "application/json"));
				if (!(result.IsSuccessStatusCode))
					await Logger.SendAsync($"WebhookService.ExecuteWebhookAsync POST Error ({data.GuildId} ID {data.Id})\n{result.Content}", LogColor.Error);
				else
					return true;
			}
			catch (Exception e)
			{
				return await Logger.SendAsync("WebhookService.ExecuteWebhookAsync Error", e) as bool?;
			}
			return false;
		}
	}
}