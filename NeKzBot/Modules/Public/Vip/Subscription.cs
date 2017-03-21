using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using NeKzBot.Extensions;
using NeKzBot.Resources;
using NeKzBot.Server;
using NeKzBot.Webhooks;

namespace NeKzBot.Modules.Public.Vip
{
	public class Subscription : CommandModule
	{
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Subscription Module", LogColor.Init);
			await SubscriptionCommands(Configuration.Default.BotCmd);
		}

		private static Task SubscriptionCommands(string name)
		{
			CService.CreateGroup(name, GBuilder =>
			{
				GBuilder.CreateCommand("subscribe")
						.Alias("hook", "gethooked", "createhook")
						.Description($"Subscribes you to an automated service with webhooks. Use the keyword _this_ to skip the channel_id parameter and choose the current channel. {Data.SubscriptionListMessage}")
						.Parameter("channel_id", ParameterType.Required)
						.Parameter("service", ParameterType.Required)
						.Parameter("username", ParameterType.Unparsed)
						.AddCheck(Permissions.VipGuildsOnly)
						.AddCheck(Permissions.AdminOnly)
						.Do(async e =>
						{
							// Check permission
							if (!(await Utils.CheckRolesHasPermissionAsync(await Utils.GetBotUserObject(e.Channel), DiscordConstants.ManageWebhooksFlag)))
							{
								await e.Channel.SendIsTyping();
								await e.Channel.SendMessage("The permission to manage webhooks is required.");
								return;
							}

							if (!(ulong.TryParse(e.GetArg("channel_id"), out ulong id)))
								id = e.Channel.Id;

							var type = e.GetArg("service").ToLower();
							if (!(Data.SubscriptionList.Contains(type)))
							{
								await e.Channel.SendIsTyping();
								await e.Channel.SendMessage(Data.SubscriptionListMessage);
								return;
							}

							// Fail safe :)
							var username = (string.IsNullOrEmpty(e.GetArg("username")))
												  ? e.GetArg("username")
												  : "NeKzHook";
							if ((username.Length < 2)
							|| (username.Length > 100)
							|| (username.ToUpper().Contains("CLYDE")))
							{
								await e.Channel.SendIsTyping();
								await e.Channel.SendMessage("Invalid username.");
								return;
							}

							var hook = await WebhookService.CreateWebhookAsync(id, new WebhookData { Name = username });
							if (hook == null)
							{
								await e.Channel.SendIsTyping();
								await e.Channel.SendMessage("**Failed** to create a webhook.");
								return;
							}

							await WebhookData.Watch.RestartAsync();
							var data = new WebhookData(hook.Id, hook.Token, e.Server.Id, e.User.Id);

							var result = default(bool);
							switch (type.ToLower())
							{
								case Data.Portal2WebhookKeyword:
									result = await WebhookData.SubscribeAsync("p2hook", data);
									break;
								case Data.SpeedrunComSourceWebhookKeyword:
									result = await WebhookData.SubscribeAsync("srcomsourcehook", data);
									break;
								case Data.SpeedrunComPortal2WebhookKeyword:
									result = await WebhookData.SubscribeAsync("srcomportal2hook", data);
									break;
								case Data.TwitchTvWebhookKeyword:
									result = await WebhookData.SubscribeAsync("twtvhook", data);
									break;
								default:
									await e.Channel.SendIsTyping();
									await e.Channel.SendMessage(Data.SubscriptionListMessage);
									return;
							}

							if (result)
								await (await e.User.CreatePMChannel()).SendMessage($"Successfully created a webhook subscription for _{type.ToLower()}_.\n• Use this id _{data.Id}_ to unsubscribe from this service.\n• Usage: `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} unsubscribe <id>`.\n• Note: You cannot execute commands in this DM channel.");
							else
							{
								await e.Channel.SendIsTyping();
								await e.Channel.SendMessage("**Failed** to subscribe to this service. A webhook has been generated though.");
							}
						});

				GBuilder.CreateCommand("unsubscribe")
						.Alias("unsub", "unhook", "deletehook")
						.Description("Removes the webhook from the subscription list.")
						.Parameter("id", ParameterType.Required)
						.AddCheck(Permissions.VipGuildsOnly)
						.AddCheck(Permissions.AdminOnly)
						.Do(async e =>
						{
							// Check permission
							if (await Utils.CheckRolesHasPermissionAsync(await Utils.GetBotUserObject(e.Channel), DiscordConstants.ManageWebhooksFlag))
							{
								await e.Channel.SendIsTyping();
								await e.Channel.SendMessage("The permission to manage webhooks is required.");
								return;
							}

							await e.Channel.SendIsTyping();
							if (!(ulong.TryParse(e.GetArg("id"), out ulong id)))
							{
								await e.Channel.SendMessage("Invalid webhook id.");
								return;
							}
							var data = new WebhookData { Id = id };

							var result = default(Subscribers);
							if ((result = await Data.Get<Subscribers>("p2hook")).Subs.FindIndex(x => x.Id == id) == -1)
							{
								if ((result = await Data.Get<Subscribers>("srcomsourcehook")).Subs.FindIndex(x => x.Id == id) == -1)
								{
									if ((result = await Data.Get<Subscribers>("srcomportal2hook")).Subs.FindIndex(x => x.Id == id) == -1)
									{
										if ((result = await Data.Get<Subscribers>("twtvhook")).Subs.FindIndex(x => x.Id == id) == -1)
										{
											await e.Channel.SendMessage("Could not find the given webhook id.");
											return;
										}
									}
								}
							}

							if ((e.User.Id != result.Subs?.FirstOrDefault(x => x.Id == id)?.UserId)
							|| (e.User.Id != Credentials.Default.DiscordBotOwnerId))
							{
								await e.Channel.SendMessage("You are not allowed to manage this webhook.");
								return;
							}

							if (!(await WebhookData.UnsubscribeAsync("p2hook", data)))
							{
								if (!(await WebhookData.UnsubscribeAsync("srcomsourcehook", data)))
								{
									if (!(await WebhookData.UnsubscribeAsync("srcomportal2hook", data)))
									{
										if (!(await WebhookData.UnsubscribeAsync("twtvhook", data)))
										{
											await e.Channel.SendMessage("This id does not exist in any subscription list.");
											return;
										}
									}
								}
							}

							if ((bool)await WebhookService.DeleteWebhookAsync(data))
								await e.Channel.SendMessage("Successfully unsubscribed and delete the webhook.");
							else
								await e.Channel.SendMessage("Successfully unsubscribed but **failed** to delete the webhook:\n• The bot might not have the permissions to manage webhooks.\n• The webhook id does not exist anymore.");
						});
			});
			return Task.FromResult(0);
		}
	}
}