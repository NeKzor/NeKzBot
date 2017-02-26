using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using NeKzBot.Tasks;
using NeKzBot.Server;
using NeKzBot.Webhooks;
using NeKzBot.Resources;
using NeKzBot.Tasks.Speedrun;
using NeKzBot.Tasks.Leaderboard;

namespace NeKzBot.Modules
{
	public class Admin : Commands
	{
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Bot Admin Commands", LogColor.Init);
			await AdminCommands(Configuration.Default.BotCmd);
		}

		private static Task AdminCommands(string name)
		{
			CService.CreateGroup(name, GBuilder =>
			{
				GBuilder.CreateCommand("newgame")
						.Alias("playnext", "nextgame", "ng")
						.Description($"• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} newgame` sets a new game for the bot.")
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e => Bot.Client.SetGame(await Utils.RNGAsync(Data.RandomGames) as string));

				GBuilder.CreateCommand("setgame")
						.Alias("play", "sg")
						.Description($"• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} setgame <name>` sets a new playing status for the bot writes his updates.\n• Current game is: *{Bot.Client.CurrentGame}*.")
						.Parameter("name", ParameterType.Unparsed)
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							if (!(string.IsNullOrEmpty(e.Args[0])))
							{
								if (e.Args[0] != Bot.Client.CurrentGame.Name)
								{
									Bot.Client.SetGame(e.Args[0]);
									await e.Channel.SendMessage($"Bot is now playing **{e.Args[0]}**.");
								}
								else
									await e.Channel.SendMessage("Bot is already playing that game.");
							}
							else
								await e.Channel.SendMessage(await Utils.FindDescriptionAsync("setgame"));
						});

				GBuilder.CreateCommand("cleanconfig")
						.Alias("resetcfg", "restorecfg")
						.Description($"• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} cleanconfig` resets to the default settings of the application.")
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							Configuration.Default.Reset();
							await e.Channel.SendMessage("Default settings have been restored.");
						});

				GBuilder.CreateCommand("add")
						.Description($"• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} add <name> <values>` adds a new command to the database.\n• Use the separator _{Utils.Separator}_ for data arrays.")
						.Parameter("name", ParameterType.Required)
						.Parameter("values", ParameterType.Unparsed)
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							if (await Data.DataExists(e.GetArg("name"), out var index))
							{
								if ((Data.Manager[index].ReadingAllowed)
								&& (Data.Manager[index].WrittingAllowed))
								{
									var result = await Utils.AddDataAsync(index, e.GetArg("values"));
									await e.Channel.SendMessage(result == string.Empty ? "Data has been added." : result);
								}
								else
									await e.Channel.SendMessage("This command doesn't allow to be changed.");
							}
							else
								await e.Channel.SendMessage($"Invalid data name. Try one of these: {await Utils.ListToList(await Data.GetDataNames(), "`")}");
						});

				GBuilder.CreateCommand("delete")
						.Alias("remove")
						.Description($"• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} remove <name> <value>` removes specified data with the given name.\n• The parameter value is the the first value in a data array.")
						.Parameter("name", ParameterType.Required)
						.Parameter("value", ParameterType.Required)
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							if (await Data.DataExists(e.GetArg("name"), out var index))
							{
								if ((Data.Manager[index].ReadingAllowed)
								&& (Data.Manager[index].WrittingAllowed))
								{
									var msg = await Utils.DeleteDataAsync(index, e.GetArg("value"));
									if (msg == string.Empty)
									{
										if (await Data.ReloadAsync(index))
											await e.Channel.SendMessage("Data deleted.");
										else
											await e.Channel.SendMessage("Data deleted but **failed** to reload data.");
									}
									else
										await e.Channel.SendMessage(msg);
								}
								else
									await e.Channel.SendMessage("This command doesn't allow to be changed.");
							}
							else
								await e.Channel.SendMessage($"Invalid data name. Try one of these: {await Utils.ListToList(await Data.GetDataNames(), "`")}");
						});

				GBuilder.CreateCommand("revive")
						.Description($"• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} revive <task>` checks if a task has ended and will restart it when it has.")
						.Parameter("task", ParameterType.Unparsed)
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							switch (e.Args[0])
							{
								case "lb":
									if (!(Portal2.AutoUpdater.IsRunning))
									{
										await Task.Factory.StartNew(async () => await Portal2.AutoUpdater.StartAsync());
										await e.Channel.SendMessage("Restarted task.");
									}
									else
										await e.Channel.SendMessage("Task hasn't finished yet.");
									break;
										case "twitch":
									if (!(Twitch.IsRunning))
									{
										await Task.Factory.StartNew(async () => await Twitch.StartAsync());
										await e.Channel.SendMessage("Restarted task.");
									}
									else
										await e.Channel.SendMessage("Task hasn't finished yet.");
									break;
								case "nf":
									if (!(SpeedrunCom.AutoNotification.IsRunning))
									{
										await Task.Factory.StartNew(async () => await SpeedrunCom.AutoNotification.StartAsync());
										await e.Channel.SendMessage("Restarted task.");
									}
									else
										await e.Channel.SendMessage("Task hasn't finished yet.");
									break;
								case "giveaway":
									if (!(Giveaway.IsRunning))
									{
										await Task.Factory.StartNew(async () => await Giveaway.ResetAsync());
										await e.Channel.SendMessage("Restarted task.");
									}
									else
										await e.Channel.SendMessage("Task hasn't finished yet.");
									break;
								default:
									await e.Channel.SendMessage("Couldn't find task name. Try one of these: `lb` `twitch` `nf` `giveaway`");
									break;
							}
						});

				GBuilder.CreateCommand("reload")
						.Alias("reloaddata")
						.Description($"• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} reload` reloads data and all commands.")
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await Data.InitAsync();
							await e.Channel.SendMessage("Reloaded data.");
						});

				GBuilder.CreateCommand("showdata")
						.Alias("debugdata")
						.Description($"• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} showdata <name>` shows the data of a certain data array.")
						.Parameter("name", ParameterType.Unparsed)
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							var index = 0;
							var found = false;
							var output = string.Empty;

							// Find command
							for (; index < Data.Manager.Count; index++)
							{
								if (e.Args[0] != Data.Manager[index].Name)
									continue;
								found = true;
								break;
							}

							if (found)
							{
								var obj = Data.Manager[index].Data;
								if (obj.GetType() == typeof(string[]))
								{
									foreach (var item in obj as string[])
										output += $"{item}, ";
								}
								else if (obj.GetType() == typeof(string[,]))
								{
									// Only show first dimension
									for (int i = 0; i < (obj as string[,]).GetLength(0); i++)
										output += $"{(obj as string[,])[i, 0]}, ";
								}
								else
									await e.Channel.SendMessage("**Error**");

								if (output != string.Empty)
									await e.Channel.SendMessage(await Utils.CutMessage(output.Substring(0, output.Length - 2).Replace("_", "\\_")));
							}
							else
								await e.Channel.SendMessage($"Invalid command parameter. Try one of these: {await Utils.ListToList(await Data.GetDataNames(), "`")}");
						});

				GBuilder.CreateCommand("data")
						.Description($"• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} data` shows you a list of all data variables.")
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage($"**[Data Commands]**\n{await Utils.ListToList(await Data.GetDataNames(), "`", "\n")}");
						});

				GBuilder.CreateCommand("say")
						.Alias("speak", "echo", "write")
						.Description($"• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} say <message>` returns the given message.")
						.Parameter("message", ParameterType.Required)
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage(await Utils.CutMessage(e.GetArg("message")));
						});

					GBuilder.CreateCommand("say")
						.Alias("speak", "echo", "write")
						.Description($"• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} say <server id or name> <channel id or name> <message>` returns the given message.")
						.Parameter("server", ParameterType.Required)
						.Parameter("channel", ParameterType.Required)
						.Parameter("message", ParameterType.Unparsed)
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							if (e.GetArg("message") == string.Empty)
								return;

							var server = default(Discord.Server);
							if (ulong.TryParse(e.GetArg("server"), out var guiid))
								server = await Utils.FindServerById(guiid);
							else
								server = await Utils.FindServerByName(e.GetArg("server"));

							var channel = default(Channel);
							if (server != null)
							{
								if (ulong.TryParse(e.GetArg("channel"), out var chaid))
									channel = await Utils.FindTextChannelById(chaid, server);
								else
									channel = await Utils.FindTextChannelByName(e.GetArg("channel"), server);
							}
							else
							{
								if (ulong.TryParse(e.GetArg("channel"), out var chaid))
									channel = await Utils.FindTextChannelById(chaid, e.Server);
								else
									channel = await Utils.FindTextChannelByName(e.GetArg("channel"), e.Server);
							}

							if (channel != null)
							{
								await channel.SendIsTyping();
								await channel.SendMessage(await Utils.CutMessage(e.GetArg("message")));
							}
							else
							{
								await e.Channel.SendIsTyping();
								await e.Channel.SendMessage(await Utils.CutMessage(e.GetArg("message")));
							}
						});

				GBuilder.CreateCommand("taskstatus")
						.Alias("tasks")
						.Description($"• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} taskstatus <name>` returns the current status of a task.")
						.Parameter("name", ParameterType.Unparsed)
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							switch (e.Args[0])
							{
								case "lb":
									await e.Channel.SendMessage($"{(Portal2.AutoUpdater.IsRunning ? "Is running" : "Dead")}");
									break;
								case "twitch":
									await e.Channel.SendMessage($"{(Twitch.IsRunning ? "Is running" : "Dead")}");
									break;
								case "nf":
									await e.Channel.SendMessage($"{(SpeedrunCom.AutoNotification.IsRunning ? "Is running" : "Dead")}");
									break;
								case "giveaway":
									await e.Channel.SendMessage($"{(Giveaway.IsRunning ? "Is running" : "Dead")}");
									break;
								case "":
									await e.Channel.SendMessage($"Leaderboard{(Portal2.AutoUpdater.IsRunning ? " - Running" : " - Dead")}\n"
															  + $"TwitchTv{(Twitch.IsRunning ? " - Running" : " - Dead")}\n"
															  + $"SpeedrunCom{(SpeedrunCom.AutoNotification.IsRunning ? " - Running" : " - Dead")}\n"
															  + $"Giveaway{(Giveaway.IsRunning ? " - Running" : " - Dead")}");
									break;
								default:
									await e.Channel.SendMessage("Couldn't find task name. Try one of these: `lb` `twitch` `nf` `giveaway`");
									break;
							}
						});

				GBuilder.CreateCommand("watches")
						.Description($"• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} watches` returns the current status of all taks watches.")
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage($"Portal2.AutoUpdater • *{(Portal2.AutoUpdater.Watch.IsRunning ? "Running" : $"On Hold ({Portal2.AutoUpdater.Watch.LastCheckedTimeValue})")}*"
													  + $"\nPortal2.Cache • *{(Portal2.Cache.Watch.IsRunning ? "Running" : $"On Hold ({Portal2.Cache.Watch.LastCheckedTimeValue})")}*"
													  + $"\nSpeedrunCom.AutoNotification • *{(SpeedrunCom.AutoNotification.Watch.IsRunning ? "Running" : $"On Hold ({SpeedrunCom.AutoNotification.Watch.LastCheckedTimeValue})")}*"
													  + $"\nTwitch • *{(Twitch.Watch.IsRunning ? "Running" : $"On Hold ({Twitch.Watch.LastCheckedTimeValue})")}*"
													  + $"\nGiveaway • *{(Giveaway.Watch.IsRunning ? "Running" : $"On Hold({ Giveaway.Watch.LastCheckedTimeValue})")}*");
						});

				GBuilder.CreateCommand("subscribe")
						.Alias("hook", "gethooked", "createhook")
						.Description($"• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} subscribe <channel_id> <service> (username)` subscribes you to an automated service with webhooks. Use the keyword _this_ to skip the channel_id parameter and choose the current channel. List of available subscriptions:\n• `p2wrs` upates you about the latest Portal 2 challenge mode world records.\n• `srnfs` notifies you about the latest speedrun.com updates.\n• `twitch` notifies you when somebody from the streaming list goes live.")
						.Parameter("channel_id", ParameterType.Required)
						.Parameter("service", ParameterType.Required)
						.Parameter("username", ParameterType.Unparsed)
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							if (!(ulong.TryParse(e.GetArg("channel_id"), out ulong id)))
								id = e.Channel.Id;

							var type = e.GetArg("service").ToLower();
							var username = e.GetArg("username") ?? "NeKzHook";

							// Fail safe :)
							if ((username.Length < 2)
							|| (username.Length > 100)
							|| (username.ToUpper().Contains("CLYDE")))
							{
								await e.Channel.SendIsTyping();
								await e.Channel.SendMessage("Invalid username.");
								return;
							}

							var hook = await WebhookService.CreateWebhookAsync(id, new WebhookData { UserName = username });
							if (hook == null)
							{
								await e.Channel.SendIsTyping();
								await e.Channel.SendMessage("**Failed** to create a webhook. The bot might not have the permissions to manage webhooks.");
								return;
							}

							await WebhookData.Watch.RestartAsync();
							var data = new WebhookData
							{
								Id = hook.Id,
								Token = hook.Token,
								UserName = hook.Name,
								UserId = e.User.Id
							};

							var result = default(bool);
							switch (type.ToLower())
							{
								case "p2wrs":
									result = await WebhookData.SubscribeAsync("p2hook", data);
									break;
								case "srnfs":
									result = await WebhookData.SubscribeAsync("srcomhook", data);
									break;
								case "twitch":
									result = await WebhookData.SubscribeAsync("twtvhook", data);
									break;
								default:
									await e.Channel.SendIsTyping();
									await e.Channel.SendMessage("List of available subscriptions:\n• `p2wrs` upates you about the latest Portal 2 challenge mode world records.\n• `srnfs` notifies you about the latest speedrun.com updates.\n• `twitch` notifies you when somebody from the streaming list goes live.");
									return;
							}

							if (result)
								await (await e.User.CreatePMChannel()).SendMessage($"Sucessfully created a webhook subscription for _{type}_.\n• Use this id _{data.Id}_ to unsubscribe from this service.\n• Usage: `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} unsubscribe <id>`.\n• Note: You cannot execute commands in this DM channel.");
							else
							{
								await e.Channel.SendIsTyping();
								await e.Channel.SendMessage("**Failed** to subscribe to this service.");
							}
						});

				GBuilder.CreateCommand("unsubscribe")
						.Alias("unsub", "unhook", "deletehook")
						.Description($"• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} unsubscribe <id>` removes the webhook from the subscription list.")
						.Parameter("id", ParameterType.Required)
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							if (!(ulong.TryParse(e.GetArg("id"), out ulong id)))
							{
								await e.Channel.SendMessage("Invalid webhook id.");
								return;
							}
							var data = new WebhookData { Id = id };

							if (((await Data.GetDataByName("p2hook", out var index)).Data as List<WebhookData>).FindIndex(x => x.Id == id) == -1)
							{
								if (((await Data.GetDataByName("srcomhook", out index)).Data as List<WebhookData>).FindIndex(x => x.Id == id) == -1)
								{
									if (((await Data.GetDataByName("twtvhook", out index)).Data as List<WebhookData>).FindIndex(x => x.Id == id) == -1)
									{
										await e.Channel.SendMessage("Could not find the given webhook id.");
										return;
									}
								}
							}

							// RIP 0.9 doesn't have webhook permissions :c
							if ((e.User.Id != (Data.Manager[index].Data as List<WebhookData>)?.FirstOrDefault(x => x.Id == id)?.UserId)
							|| (e.User.Id != Credentials.Default.DiscordBotOwnerId))
							{
								await e.Channel.SendMessage("You are not allowed manage this webhook.");
								return;
							}

							if (!(await WebhookData.UnsubscribeAsync("p2hook", data)))
							{
								if (!(await WebhookData.UnsubscribeAsync("srcomhook", data)))
								{
									if (!(await WebhookData.UnsubscribeAsync("twtvhook", data)))
									{
										await e.Channel.SendMessage("This id does not exist in any subscription list.");
										return;
									}
								}
							}

							if ((bool)await WebhookService.DeleteWebhookAsync(data))
								await e.Channel.SendMessage("Sucessfully unsubscribed and delete the webhook.");
							else
								await e.Channel.SendMessage("Sucessfully unsubscribed but **failed** to delete the webhook:\n• The bot might not have the permissions to manage webhooks.\n• The webhook id does not exist anymore.");
						});
			});
			return Task.FromResult(0);
		}
	}
}