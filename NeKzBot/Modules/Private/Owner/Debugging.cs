using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using NeKzBot.Internals.Entities;
using NeKzBot.Resources;
using NeKzBot.Server;
using NeKzBot.Tasks;
using NeKzBot.Tasks.Leaderboard;
using NeKzBot.Tasks.Speedrun;
using NeKzBot.Webhooks;

namespace NeKzBot.Modules.Private.Owner
{
	public class Debugging : CommandModule
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
							Configuration.Default.Save();
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
									if (!(Portal2Board.AutoUpdater.IsRunning))
									{
										await Task.Factory.StartNew(async () => await Portal2Board.AutoUpdater.StartAsync());
										await e.Channel.SendMessage("Restarted task.");
									}
									else
										await e.Channel.SendMessage("Task hasn't finished yet.");
									break;
								case "twitch":
									if (!(TwitchTv.IsRunning))
									{
										await Task.Factory.StartNew(async () => await TwitchTv.StartAsync());
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
								default:
									await e.Channel.SendMessage("Couldn't find task name. Try one of these: `lb`, `twitch`, `nf`.");
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
									await e.Channel.SendMessage($"{(Portal2Board.AutoUpdater.IsRunning ? "Is running" : "Dead")}");
									break;
								case "twitch":
									await e.Channel.SendMessage($"{(TwitchTv.IsRunning ? "Is running" : "Dead")}");
									break;
								case "nf":
									await e.Channel.SendMessage($"{(SpeedrunCom.AutoNotification.IsRunning ? "Is running" : "Dead")}");
									break;
								case "":
									await e.Channel.SendMessage($"Leaderboard{(Portal2Board.AutoUpdater.IsRunning ? " - Running" : " - Dead")}\n" +
																$"TwitchTv{(TwitchTv.IsRunning ? " - Running" : " - Dead")}\n" +
																$"SpeedrunCom{(SpeedrunCom.AutoNotification.IsRunning ? " - Running" : " - Dead")}");
									break;
								default:
									await e.Channel.SendMessage("Couldn't find task name. Try one of these: `lb`, `twitch`, `nf`.");
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
							await e.Channel.SendMessage($"Portal2.AutoUpdater • *{(Portal2Board.AutoUpdater.Watch.IsRunning ? "Running" : $"On Hold ({Portal2Board.AutoUpdater.Watch.LastCheckedTimeValue})")}*" +
														$"\nServer.Timer • *{(Timer.IsRunning ? "Running" : $"On Hold ({Timer.Watch.LastCheckedTimeValue})")}*" +
														$"\nSpeedrunCom.AutoNotification • *{(SpeedrunCom.AutoNotification.Watch.IsRunning ? "Running" : $"On Hold ({SpeedrunCom.AutoNotification.Watch.LastCheckedTimeValue})")}*" +
														$"\nTwitch • *{(TwitchTv.Watch.IsRunning ? "Running" : $"On Hold ({TwitchTv.Watch.LastCheckedTimeValue})")}*");
						});

				GBuilder.CreateCommand("webhooktest")
						.Alias("pinghooks", "hookcleanup")
						.Description("Sends a ping to all webhooks and automatically deletes the ones who give no response.")
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							var p2hooks = (await Data.Get<Subscription>("p2hook"))?.Subscribers;
							var srcomsourcehooks = (await Data.Get<Subscription>("srcomsourcehook"))?.Subscribers;
							var srcomportal2hooks = (await Data.Get<Subscription>("srcomportal2hook"))?.Subscribers;
							var twtvhooks = (await Data.Get<Subscription>("twtvhook"))?.Subscribers;
							if (new List<List<WebhookData>> { p2hooks, srcomsourcehooks, srcomportal2hooks, twtvhooks }.Contains(null))
							{
								await e.Channel.SendMessage("**Failed** to load data from data manager.");
								return;
							}

							var p2count = 0;
							var srcomsourcecount = 0;
							var srcomportal2count = 0;
							var twtvcount = 0;
							var errorcount = 0;
							var totalhooks = p2hooks?.Count + srcomsourcehooks?.Count + srcomportal2hooks?.Count + twtvhooks?.Count;

							// Converting to array, whatever lol
							foreach (var data in p2hooks.ToArray())
							{
								if (await WebhookService.GetWebhookAsync(data, true) == null)
								{
									if (await WebhookData.UnsubscribeAsync("p2hook", data))
									{
										await Logger.SendAsync($"Unhooked {data.Id} (GUILD {data.GuildId})(USER {data.UserId}) from p2hook");
										p2count++;
									}
									else
										errorcount++;
								}
							}
							foreach (var data in srcomsourcehooks.ToArray())
							{
								if (await WebhookService.GetWebhookAsync(data, true) == null)
								{
									if (await WebhookData.UnsubscribeAsync("srcomsourcehook", data))
									{
										await Logger.SendAsync($"Unhooked {data.Id} (GUILD {data.GuildId})(USER {data.UserId}) from srcomsourcehook");
										srcomsourcecount++;
									}
									else
										errorcount++;
								}
							}
							foreach (var data in srcomportal2hooks.ToArray())
							{
								if (await WebhookService.GetWebhookAsync(data, true) == null)
								{
									if (await WebhookData.UnsubscribeAsync("srcomportal2hook", data))
									{
										await Logger.SendAsync($"Unhooked {data.Id} (GUILD {data.GuildId})(USER {data.UserId}) from srcomportal2hook");
										srcomportal2count++;
									}
									else
										errorcount++;
								}
							}
							foreach (var data in twtvhooks.ToArray())
							{
								if (await WebhookService.GetWebhookAsync(data, true) == null)
								{
									if (await WebhookData.UnsubscribeAsync("twtvhook", data))
									{
										await Logger.SendAsync($"Unhooked {data.Id} (GUILD {data.GuildId})(USER {data.UserId}) from twtvhook");
										twtvcount++;
									}
									else
										errorcount++;
								}
							}

							await Logger.SendAsync($"Cleaned webhook data. File I/O errors: {errorcount}");
							await e.Channel.SendMessage($"Sent {totalhooks} ping test{((totalhooks == 1) ? string.Empty : "s")} in total and removed:" +
														$"\n• {p2count} webhook{((p2count == 1) ? string.Empty : "s")} from {Data.Portal2WebhookKeyword}" +
														$"\n• {srcomsourcecount} webhook{((srcomsourcecount == 1) ? string.Empty : "s")} from {Data.SpeedrunComSourceWebhookKeyword}" +
														$"\n• {srcomportal2count} webhook{((srcomportal2count == 1) ? string.Empty : "s")} from {Data.SpeedrunComPortal2WebhookKeyword}" +
														$"\n• {twtvcount} webhook{((twtvcount == 1) ? string.Empty : "s")} from {Data.TwitchTvWebhookKeyword}");
						});

				GBuilder.CreateCommand("errormessages")
						.Alias("errormsgs", "errors")
						.Description("Returns the five latest errors.")
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							var output = string.Empty;
							foreach (var error in Logger.Errors.Take(5))
								output += $"\n• {error}";
							await e.Channel.SendMessage((string.IsNullOrEmpty(output)) ? "Zero errors have occurred since the last restart. :ok_hand:" : output);
						});
			});
			return Task.FromResult(0);
		}
	}
}