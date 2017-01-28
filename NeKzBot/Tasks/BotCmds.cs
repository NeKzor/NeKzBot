using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using NeKzBot.Server;
using NeKzBot.Resources;
using NeKzBot.Modules;
using NeKzBot.Modules.Games;
using NeKzBot.Modules.Twitch;
using NeKzBot.Modules.Speedrun;
using NeKzBot.Modules.Leaderboard;

namespace NeKzBot.Tasks
{
	public class BotCmds : Commands
	{
		public static async Task Load()
		{
			await Logging.CON("Loading bot commands", System.ConsoleColor.DarkYellow);
			await BotCommands(Settings.Default.BotCmd);
		}

		private static Task BotCommands(string s)
		{
			cmd.CreateGroup(s, g =>
			{
				#region EVERYBODY
				g.CreateCommand("uptime")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} uptime` shows you how long the bot is running for.")
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					await e.Channel.SendMessage($"Bot is running for **{Program.GetUptime()}**");
				});

				g.CreateCommand("location")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} location` gives you information about the server where the bot is located.")
				.Do(async e =>
				{
					await e.Channel.SendIsTyping();
					await e.Channel.SendMessage($"**Bot Location -** Graz, Austria :flag_at:");
				});

				g.CreateCommand("info")
				.Alias("status")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} info` shows some information about the bot.")
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					await e.Channel.SendMessage(
						$"**AppName - **{Bot.dClient.Config.AppName}"
						+ $"\n**AppVersion - **{Bot.dClient.Config.AppVersion}"
						+ $"\n**AppUrl - **{Bot.dClient.Config.AppUrl}"
						+ $"\n**Log Level - ** {Bot.dClient.Config.LogLevel.ToString()}"
						+ $"\n**Connection Timeout - ** {Bot.dClient.Config.ConnectionTimeout.ToString()}"
						+ $"\n**Reconnect Delay- ** {Bot.dClient.Config.ReconnectDelay.ToString()}"
						+ $"\n**Failed Reconnect Delay - ** {Bot.dClient.Config.FailedReconnectDelay.ToString()}"
						+ $"\n**Total Shards - ** {Bot.dClient.Config.TotalShards.ToString()}"
						+ $"\n**Cache Dir - ** {Bot.dClient.Config.CacheDir}"
						+ $"\n**User Agent - ** {Bot.dClient.Config.UserAgent}"
						+ $"\n**User Discriminator - ** {Bot.dClient.CurrentUser.Discriminator.ToString()}"
						+ $"\n**User ID - ** {Bot.dClient.CurrentUser.Id.ToString()}"
						+ $"\n**User Is Verfied - ** {Bot.dClient.CurrentUser.IsVerified.ToString()}"
						+ $"\n**User Status - ** {Bot.dClient.CurrentUser.Status.ToString()}"
						+ $"\n**GatewaySocket Hosts - ** {Bot.dClient.GatewaySocket.Host.Count().ToString()}"
						+ $"\n**GatewaySocket State - ** {Bot.dClient.GatewaySocket.State.ToString()}"
						+ $"\n**Current Game - ** {Bot.dClient.CurrentGame.Name}"
						+ $"\n**Client State - ** {Bot.dClient.State.ToString()}"
						+ $"\n**Client Status - ** {Bot.dClient.Status.Value}"
						+ $"\n**Regions - ** {Bot.dClient.Regions.Count().ToString()}"
						+ $"\n**Servers - ** {Bot.dClient.Servers.Count().ToString()}"
						+ $"\n**Commands -** {cmd.AllCommands.Count().ToString()}"
						+ $"\n**Services - ** {Bot.dClient.Services.Count().ToString()}"
						+ $"\n**SessionID - ** {Bot.dClient.SessionId}"
						+ $"\n**Error Count - ** {Logging.errorCount.ToString()}"
					);
				});

				g.CreateCommand("help")
				.Alias("ayy")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} help` doesn't help you at all.")
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					await e.Channel.SendMessage("AYY, WADDUP!");
				});

				g.CreateCommand("settings")
				.Alias("debug")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} settings` gives you some debug information about the application.")
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					await e.Channel.SendMessage(Data.settingsMsg);
				});
				#endregion

				#region MAIN SERVER ONLY
				g.CreateCommand("connect")
				.Alias("vc")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} connect` connects the bot to a voice channel.\n**-** It will follow you automatically if you connected to one already.")
				.AddCheck(Permission.MainServerOnly)
				.Do(async (e) =>
				{
					if (e.User.VoiceChannel == null)
						await VoiceChannel.ConnectVC(e.Server.VoiceChannels.First());
					else
						await VoiceChannel.ConnectVC(e.User.VoiceChannel);
				});

				g.CreateCommand("disconnect")
				.Alias("dc")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} disconnect` disconnects the bot when it's in a voice channel.")
				.AddCheck(Permission.MainServerOnly)
				.Do(async (e) =>
				{
					await VoiceChannel.DisconnectVC(e.Server.Id);
				});

				g.CreateCommand("stop")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} stop` stops a currently running audio stream.")
				.AddCheck(Permission.MainServerOnly)
				.Do(e =>
				{
					if (VoiceChannel.isplaying)
						VoiceChannel.shouldstop = true;
				});
				#endregion

				#region BOT OWNER ONLY
				CreateKillBotCommands(s, Data.killCmd);

				g.CreateCommand("newgame")
				.Alias("playnext", "nextgame", "ng")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} newgame` sets a new game for the bot.")
				.AddCheck(Permission.BotOwnerOnly).Hide()
				.Do(e =>
				{
					Bot.dClient.SetGame(Data.randomGames[Utils.RNG(0, Data.randomGames.Count())]);
				});

				g.CreateCommand("setgame")
				.Alias("play", "sg")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} setgame <name>` sets a new playing status for the bot writes his updates.\n**-** Current game is: *{Bot.dClient.CurrentGame}*.")
				.Parameter("p", ParameterType.Required)
				.AddCheck(Permission.BotOwnerOnly).Hide()
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					if (e.Args[0] != Bot.dClient.CurrentGame.Name)
					{
						Bot.dClient.SetGame(e.Args[0]);
						await e.Channel.SendMessage($"Bot is now playing **{e.Args[0]}**.");
					}
					else
						await e.Channel.SendMessage("Bot is already playing that game.");
				});

				g.CreateCommand("cleanconfig")
				.Alias("resetcfg", "restorecfg")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} cleanconfig` resets to the default settings of the application.")
				.AddCheck(Permission.BotOwnerOnly).Hide()
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					Settings.Default.Reset();
					await e.Channel.SendMessage("Default settings have been restored.");
				});

				g.CreateCommand("add")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} add <name> <value1> <value2> <etc.>` adds a new command to the database.")
				.Parameter("p", ParameterType.Unparsed)
				.AddCheck(Permission.BotOwnerOnly).Hide()
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					var values = e.Args[0].Split(Utils.seperator);
					var index = 0;
					if (values.Count() < 2)
						await e.Channel.SendMessage("Invalid parameter count.");
					else if (Utils.SearchArray(DataManager.dataCommands, 0, values[0], out index))
					{
						if ((bool)DataManager.dataCommands[index, 1])
							await e.Channel.SendMessage(await Utils.AddData(index, e.Args[0]));
						else
							await e.Channel.SendMessage("This command doesn't allow to be changed.");
					}
					else
						await e.Channel.SendMessage($"Invalid command parameter. Try one of these: {Utils.ListToList(DataManager.rwCommands, "`")}");
				});

				g.CreateCommand("delete")
				.Alias("remove")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} remove <name> <value>` removes data about that command.")
				.Parameter("p", ParameterType.Unparsed)
				.AddCheck(Permission.BotOwnerOnly).Hide()
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					var values = e.Args[0].Split(Utils.seperator);
					var index = 0;
					if (values.Count() != 2)
						await e.Channel.SendMessage("Invalid parameter count.");
					else if (Utils.SearchArray(DataManager.dataCommands, 0, values[0], out index))
					{
						if ((bool)DataManager.dataCommands[index, 1])
						{
							var msg = await Utils.DeleteData((string)DataManager.dataCommands[index, 2], Utils.GetRest(values, 1));
							if (msg == string.Empty)
							{
								if (await DataManager.Reload(index))
									await e.Channel.SendMessage("Data deleted.");
								else
									await e.Channel.SendMessage("**Error**. DataManager failed to reload data.");
							}
							else
								await e.Channel.SendMessage(msg);
						}
						else
							await e.Channel.SendMessage("This command doesn't allow to be changed.");
					}
					else
						await e.Channel.SendMessage($"Invalid command parameter. Try one of these: {Utils.ListToList(DataManager.rwCommands, "`")}");
				});

				g.CreateCommand("revive")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} revive <task>` checks if a task has ended and will restart it when it has.")
				.Parameter("p", ParameterType.Unparsed)
				.AddCheck(Permission.BotOwnerOnly).Hide()
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					switch (e.Args[0])
					{
						case "lb":
							if (!Leaderboard.AutoUpdater.isRunning)
							{
								Leaderboard.AutoUpdater.Start().Start();	// lmao
								await e.Channel.SendMessage("Restarted task.");
							}
							else
								await e.Channel.SendMessage("Task hasn't finished yet.");
							break;
								case "twitch":
							if (!TwitchTv.isRunning)
							{
								TwitchTv.Start().Start();
								await e.Channel.SendMessage("Restarted task.");
							}
							else
								await e.Channel.SendMessage("Task hasn't finished yet.");
							break;
						case "nf":
							if (!SpeedrunCom.AutoNotification.isRunning)
							{
								SpeedrunCom.AutoNotification.Start().Start();
								await e.Channel.SendMessage("Restarted task.");
							}
							else
								await e.Channel.SendMessage("Task hasn't finished yet.");
							break;
						case "giveaway":
							if (!GiveawayGame.isRunning)
							{
								GiveawayGame.Reset().Start();
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

				g.CreateCommand("reload")
				.Alias("reloaddata")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} reload` reloads data and all commands.")
				.AddCheck(Permission.BotOwnerOnly).Hide()
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					await Data.Init();
					await DataManager.Init();
					await e.Channel.SendMessage("Reloaded data.");
				});

				g.CreateCommand("showdata")
				.Alias("debugdata")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} showdata <name>` shows the data of a certain data array.")
				.Parameter("p", ParameterType.Unparsed)
				.AddCheck(Permission.BotOwnerOnly).Hide()
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					var index = 0;
					var found = false;
					var output = string.Empty;
						
					// Find command
					for (; index < DataManager.dataCommands.GetLength(0); index++)
					{
						if (e.Args[0] != (string)DataManager.dataCommands[index, 0])
							continue;
						found = true;
						break;
					}

					if (found)
					{
						var obj = DataManager.dataCommands[index, 3];
						if (obj.GetType() == typeof(string[]))
						{
							foreach (var item in (string[])obj)
								output += $"{item}, ";
						}
						else if (obj.GetType() == typeof(string[,]))
						{
							// Only show first dimension
							for (int i = 0; i < ((string[,])obj).GetLength(0); i++)
								output += $"{((string[,])obj)[i, 0]}, ";
						}
						else
							await e.Channel.SendMessage("**Error**");

						if (output != string.Empty)
							await e.Channel.SendMessage(Utils.CutMessage(output.Substring(0, output.Length - 2).Replace("_", "\\_")));
					}
					else
						await e.Channel.SendMessage($"Invalid command parameter. Try one of these: {Utils.ListToList(DataManager.rwCommands, "`")}");
				});

				g.CreateCommand("data")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} data` shows you a list of all data variables.")
				.AddCheck(Permission.BotOwnerOnly).Hide()
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					await e.Channel.SendMessage($"**[Data Commands]**\n{Utils.ListToList(DataManager.rwCommands, "`", "\n")}");
				});

				g.CreateCommand("say")
				.Alias("speak", "echo", "write")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} say <channel> <message>` returns the given message.")
				.Parameter("p", ParameterType.Unparsed)
				.AddCheck(Permission.BotOwnerOnly).Hide()
				.Do(async (e) =>
				{
					var channel = await Utils.GetChannel(e.Args[0].Split(' ').First());
					if (channel == null)
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(Utils.CutMessage(e.Args[0]));
					}
					else
					{
						await channel.SendIsTyping();
						await channel.SendMessage(Utils.CutMessage(string.Join(" ", e.Args[0].Split(' ').Skip(1))));
					}
				});

				g.CreateCommand("taskstatus")
				.Alias("tasks")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} taskstatus <name>` returns the current status of a task.")
				.Parameter("p", ParameterType.Unparsed)
				.AddCheck(Permission.BotOwnerOnly).Hide()
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					switch (e.Args[0])
					{
						case "lb":
							await e.Channel.SendMessage($"{(Leaderboard.AutoUpdater.isRunning ? "Is running" : "Dead")}");
							break;
						case "twitch":
							await e.Channel.SendMessage($"{(TwitchTv.isRunning ? "Is running" : "Dead")}");
							break;
						case "nf":
							await e.Channel.SendMessage($"{(SpeedrunCom.AutoNotification.isRunning ? "Is running" : "Dead")}");
							break;
						case "giveaway":
							await e.Channel.SendMessage($"{(GiveawayGame.isRunning ? "Is running" : "Dead")}");
							break;
						case "":
							await e.Channel.SendMessage(
								$"Leaderboard{(Leaderboard.AutoUpdater.isRunning ? " - Running" : " - Dead")}\n"
								+ $"TwitchTv{(TwitchTv.isRunning ? " - Running" : " - Dead")}\n"
								+ $"SpeedrunCom{(SpeedrunCom.AutoNotification.isRunning ? " - Running" : " - Dead")}\n"
								+ $"Giveaway{(GiveawayGame.isRunning ? " - Running" : " - Dead")}");
							break;
						default:
							await e.Channel.SendMessage("Couldn't find task name. Try one of these: `lb` `twitch` `nf` `giveaway`");
							break;
					}
				});
				#endregion
			});
			return Task.FromResult(0);
		}

		// This prevents from reloading other commands when a new alias has been added
		public static Task CreateKillBotCommands(string s, string c)
		{
			cmd.CreateGroup(s, g =>
			{
				g.CreateCommand(c)
				.Alias(Data.killBotAliases)
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} kill` disconnects the bot from the server.")
				.AddCheck(Permission.BotOwnerOnly).Hide()
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					await e.Channel.SendMessage("WHY? :anguished: ");
					await Task.Delay(500);
					await Bot.dClient.Disconnect();
				});
			});
			return Task.FromResult(0);
		}
	}
}