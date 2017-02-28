using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;
using NeKzBot.Resources;
using NeKzBot.Server;
using NeKzBot.Tasks;
using NeKzBot.Tasks.Leaderboard;
using NeKzBot.Tasks.Speedrun;
using NeKzBot.Webhooks;

namespace NeKzBot.Modules.Private.MainServer
{
	public class Debugging : Commands
	{
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Debugging Module", LogColor.Init);
			await DebuggingCommands(Configuration.Default.BotCmd);
		}

		private static Task DebuggingCommands(string name)
		{
			CService.CreateGroup(name, GBuilder =>
			{
				GBuilder.CreateCommand("cleanconfig")
						.Alias("resetcfg", "restorecfg")
						.Description("Resets the default settings of the application.")
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							Configuration.Default.Reset();
							await e.Channel.SendMessage("Default settings have been restored.");
						});

				GBuilder.CreateCommand("revive")
						.Description("Checks if a task has ended and will restart it when it has.")
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
									await e.Channel.SendMessage("Couldn't find task name. Try one of these: `lb`, `twitch`, `nf`, `giveaway`.");
									break;
							}
						});

				GBuilder.CreateCommand("taskstatus")
						.Alias("tasks")
						.Description("Returns the current status of a task.")
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
									await e.Channel.SendMessage("Couldn't find task name. Try one of these: `lb`, `twitch`, `nf`, `giveaway`.");
									break;
							}
						});

				GBuilder.CreateCommand("watches")
						.Description("Returns the current status of all tasks watches.")
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

				GBuilder.CreateCommand("webhooktest")
						.Alias("pinghooks", "hookcleanup")
						.Description("Sends a ping to all webhooks and automatically deletes the ones who give no response.")
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							var p2hooks = (await Data.GetDataByName("p2hook", out _))?.Data as List<WebhookData>;
							var srcomhooks = (await Data.GetDataByName("srcomhook", out _))?.Data as List<WebhookData>;
							var twtvhooks = (await Data.GetDataByName("twtvhook", out _))?.Data as List<WebhookData>;
							if (new List<List<WebhookData>> { p2hooks, srcomhooks, twtvhooks }.Contains(null))
							{
								await e.Channel.SendMessage("**Failed** to load data from data manager.");
								return;
							}

							var p2count = 0;
							var srcomcount = 0;
							var twtvcount = 0;
							var errorcount = 0;
							var totalhooks = p2hooks?.Count + srcomhooks?.Count + twtvhooks?.Count;

							foreach (var data in p2hooks)
							{
								if (await WebhookService.GetWebhookAsync(data, true) == null)
								{
									if (await WebhookData.UnsubscribeAsync("p2hook", data))
									{
										await Logger.SendAsync($"Auto unsubscribed {data.Id} (USER {data.UserName})(ID {data.UserId}) from p2hook", LogColor.Default);
										p2count++;
									}
									else
										errorcount++;
								}
							}
							foreach (var data in srcomhooks)
							{
								if (await WebhookService.GetWebhookAsync(data, true) == null)
								{
									if (await WebhookData.UnsubscribeAsync("srcomhook", data))
									{
										await Logger.SendAsync($"Unhooked {data.Id} (USER {data.UserName})(ID {data.UserId}) from srcomhook", LogColor.Default);
										srcomcount++;
									}
									else
										errorcount++;
								}
							}
							foreach (var data in twtvhooks)
							{
								if (await WebhookService.GetWebhookAsync(data, true) == null)
								{
									if (await WebhookData.UnsubscribeAsync("twtvhook", data))
									{
										await Logger.SendAsync($"Auto unsubscribed {data.Id} (USER {data.UserName})(ID {data.UserId}) from twtvhook", LogColor.Default);
										twtvcount++;
									}
									else
										errorcount++;
								}
							}

							await Logger.SendAsync($"Cleaned webhook data. File I/O errors: {errorcount}", LogColor.Default);
							await e.Channel.SendMessage($"Sent {totalhooks} ping test{((totalhooks == 1) ? string.Empty : "s")} in total and removed:"
													  + $"\n• {p2count} webhook{((p2count == 1) ? string.Empty : "s")} from {Data.Portal2WebhookKeyword}"
													  + $"\n• {srcomcount} webhook{((srcomcount == 1) ? string.Empty : "s")} from {Data.SpeedrunComWebhookKeyword}"
													  + $"\n• {twtvcount} webhook{((twtvcount == 1) ? string.Empty : "s")} from {Data.TwitchTvWebhookKeyword}");
						});
			});
			return Task.FromResult(0);
		}
	}
}