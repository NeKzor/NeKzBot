using System.Linq;
using Discord.Commands;
using NeKzBot.Server;

namespace NeKzBot
{
	public class BotCmds : Commands
	{
		public static void Load()
		{
			Logging.CON("Loading bot commands", System.ConsoleColor.DarkYellow);
			BotCommands(Settings.Default.BotCmd);
		}

		private static void BotCommands(string s)
		{
			cmd.CreateGroup(s, g =>
			{
				#region EVERYBODY
				g.CreateCommand("test")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} test` just a test.")
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					await e.Channel.SendMessage(await SpeedrunCom.GetNotificationUpdate());
				});

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
						$"**AppName - **{dClient.Config.AppName}"
						+ $"\n**AppVersion - **{dClient.Config.AppVersion}"
						+ $"\n**AppUrl - **{dClient.Config.AppUrl}"
						+ $"\n**Current Game - ** {dClient.CurrentGame.Name}"
						+ $"\n**Client State - ** {dClient.State}"
						+ $"\n**Client Status - ** {dClient.Status.Value}"
						+ $"\n**Regions - ** {dClient.Regions.Count()}"
						+ $"\n**Servers - ** {dClient.Servers.Count()}"
						+ $"\n**Services - ** {dClient.Services.Count()}"
						+ $"\n**SessionID - ** {dClient.SessionId}"
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
				#endregion

				#region SPECIAL ROLES
				g.CreateCommand("cleanconfig")
				.Alias("resetcfg", "restorecfg")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} cleanconfig` resets to the default settings of the application.")
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					if (Utils.RoleCheck(e.User, Settings.Default.AllowedRoles))
					{
						Settings.Default.Reset();
						await e.Channel.SendMessage("Default settings have been restored.");
					}
					else
						await e.Channel.SendMessage(Data.rolesMsg);
				});

				#region INFORMATION
				g.CreateCommand("settings")
				.Alias("debug")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} settings` gives you some debug information about the application.")
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					if (Utils.RoleCheck(e.User, Settings.Default.AllowedRoles, "Developer"))
						await e.Channel.SendMessage(Data.settingsMsg);
					else
						await e.Channel.SendMessage(Data.rolesMsg);
				});
				#endregion

				#region VOICE CHANNEL
				g.CreateCommand("connect")
				.Alias("vc")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} connect` connects the bot to a voice channel.\n**-** It will follow you automatically if you connected to one already.")
				.Do(async (e) =>
				{
					if (Utils.RoleCheck(e.User, Settings.Default.AllowedRoles))
						if (e.User.VoiceChannel == null)
							await VoiceChannel.ConnectVC(e.Server.VoiceChannels.First());
						else
							await VoiceChannel.ConnectVC(e.User.VoiceChannel);
					else
						await e.Channel.SendMessage(Data.rolesMsg);
				});

				g.CreateCommand("disconnect")
				.Alias("dc")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} disconnect` disconnects the bot when it's in a voice channel.")
				.Do(async (e) =>
				{
					if (Utils.RoleCheck(e.User, Settings.Default.AllowedRoles))
						await VoiceChannel.DisconnectVC(e.Server.Id);
					else
						await e.Channel.SendMessage(Data.rolesMsg);
				});

				g.CreateCommand("stop")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} stop` stops a currently running audio stream.")
				.Do(async (e) =>
				{
					if (Utils.RoleCheck(e.User, Settings.Default.AllowedRoles))
						if (VoiceChannel.isplaying)
							VoiceChannel.shouldstop = true;
						else
							await e.Channel.SendMessage(Data.rolesMsg);
				});
				#endregion

				#region OTHERS
				g.CreateCommand("newgame")
				.Alias("playnext", "nextgame", "ng")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} newgame` sets a new game for the bot.")
				.Do(async (e) =>
				{
					if (Utils.RoleCheck(e.User, Settings.Default.AllowedRoles))
						dClient.SetGame(Data.randomGames[Utils.RNG(0, Data.randomGames.Count())]);
					else
						await e.Channel.SendMessage(Data.rolesMsg);
				});

				g.CreateCommand("setgame")
				.Alias("play", "sg")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} setgame <name>` sets a new playing status for the bot writes his updates.\n**-** Current game is: *{dClient.CurrentGame}*.")
				.Parameter("p", ParameterType.Required)
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					if (Utils.RoleCheck(e.User, Settings.Default.AllowedRoles))
					{
						if (e.Args[0] != string.Empty)
						{
							if (e.Args[0] != dClient.CurrentGame.Name)
							{
								dClient.SetGame(e.Args[0]);
								await e.Channel.SendMessage("Bot is now playing **{e.Args[0]}**.");
							}
							else
								await e.Channel.SendMessage("Bot is already playing that game.");
						}
						else
							await e.Channel.SendMessage("Invalid paramter.");
					}
					else
						await e.Channel.SendMessage(Data.rolesMsg);
				});
				#endregion
				#endregion

				#region MASTER ADMIN
				CreateKillBotCommands(s, Data.killCmd);

				g.CreateCommand("add")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} add <name> <value1> <value2> <etc.>` adds a new command to the database.")
				.Parameter("p", ParameterType.Unparsed)
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					// Nobody else except the master admin himself is allowed to change data
					if (e.User.Id == Credentials.Default.DiscordMasterAdminID)
					{
						var values = e.Args[0].Split(Utils.seperator);
						var index = 0;
						if (values.Count() < 2)
							await e.Channel.SendMessage("Invalid parameter count.");
						else if (Utils.SearchArray(CmdManager.dataCommands, 0, values[0], out index))
						{
							if ((bool)CmdManager.dataCommands[index, 1])
							{
								bool success = false;
								await e.Channel.SendMessage(Utils.AddData((string)CmdManager.dataCommands[index, 2], Utils.GetRest(values, 1, sep: " ", firstreplace: true), out success));
								if (success)
									if (!CmdManager.Reload(index))
										await e.Channel.SendMessage("**Error**. CmdManager failed to reload command.");
							}
							else
								await e.Channel.SendMessage("This command doesn't allow to be changed.");
						}
						else
							await e.Channel.SendMessage($"Invalid command parameter. Try one of these: {Utils.ListToList(CmdManager.rwCommands, "`")}");
					}
					else
						await e.Channel.SendMessage("You are not allowed to do that. Only the master-server admin is allowed to change data.");
				});

				g.CreateCommand("delete")
				.Alias("remove")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} remove <name> <value>` removes data about that command.")
				.Parameter("p", ParameterType.Unparsed)
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					if (e.User.Id == Credentials.Default.DiscordMasterAdminID)
					{
						var values = e.Args[0].Split(Utils.seperator);
						var index = 0;
						if (values.Count() != 2)
							await e.Channel.SendMessage("Invalid parameter count.");
						else if (Utils.SearchArray(CmdManager.dataCommands, 0, values[0], out index))
						{
							if ((bool)CmdManager.dataCommands[index, 1])
							{
								var msg = Utils.DeleteData((string)CmdManager.dataCommands[index, 2], Utils.GetRest(values, 1));
								if (msg == string.Empty)
								{
									if (CmdManager.Reload(index))
										await e.Channel.SendMessage("Command deleted.");
									else
										await e.Channel.SendMessage("**Error**. CmdManager failed to reload command.");
								}
								else
									await e.Channel.SendMessage(msg);
							}
							else
								await e.Channel.SendMessage("This command doesn't allow to be changed.");
						}
						else
							await e.Channel.SendMessage($"Invalid command parameter. Try one of these: {Utils.ListToList(CmdManager.rwCommands, "`")}");
					}
					else
						await e.Channel.SendMessage("You are not allowed to do that. Only the master-server admin is allowed to change data.");
				});

				g.CreateCommand("revive")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} revive <task>` checks if a task has ended and will restart it when it has.")
				.Parameter("p", ParameterType.Unparsed)
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					if (e.User.Id == Credentials.Default.DiscordMasterAdminID)
					{
						switch (e.Args[0])
						{
							case "lb":
								if (Utils.IsTaskAlive(Leaderboard.AutoUpdater.Start()))
								{
									Leaderboard.AutoUpdater.Start().Start();	// lmao
									await e.Channel.SendMessage("Restarted task.");
								}
								else
									await e.Channel.SendMessage("Task hasn't finished yet.");
								break;
									case "twitch":
								if (Utils.IsTaskAlive(TwitchTv.Start()))
								{
									TwitchTv.Start().Start();
									await e.Channel.SendMessage("Restarted task.");
								}
								else
									await e.Channel.SendMessage("Task hasn't finished yet.");
								break;
							case "nf":
								if (Utils.IsTaskAlive(SpeedrunCom.AutoNotification.Start()))
								{
									SpeedrunCom.AutoNotification.Start().Start();
									await e.Channel.SendMessage("Restarted task.");
								}
								else
									await e.Channel.SendMessage("Task hasn't finished yet.");
								break;
							case "giveaway":
								if (Utils.IsTaskAlive(GiveawayGame.Reset()))
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
					}
					else
						await e.Channel.SendMessage("You are not allowed to do that. Only the master-server admin is allowed to restart tasks.");
				});

				g.CreateCommand("reload")
				.Alias("reloaddata")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} reload` reloads data and all commands.")
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					if (e.User.Id == Credentials.Default.DiscordMasterAdminID)
					{
						Data.Init();
						CmdManager.Init();
						await e.Channel.SendMessage("Reloaded data.");
					}
					else
						await e.Channel.SendMessage("You are not allowed to do that. Only the master-server admin is allowed to reload data/commands.");
				});

				g.CreateCommand("showdata")
				.Alias("debugdata")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} showdata <name>` shows the data of a certain data array.")
				.Parameter("p", ParameterType.Unparsed)
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					if (e.User.Id == Credentials.Default.DiscordMasterAdminID)
					{
						var index = 0;
						var found = false;
						var output = string.Empty;
						
						// Find command
						for (; index < CmdManager.dataCommands.GetLength(0); index++)
						{
							if (e.Args[0] != (string)CmdManager.dataCommands[index, 0])
								continue;
							found = true;
							break;
						}

						if (found)
						{
							var obj = CmdManager.dataCommands[index, 3];
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
							{
								output = output.Substring(0, output.Length - 2).Replace("_", "\\_");
								if (output.Length > 2000)
									await e.Channel.SendMessage(output.Substring(0, 2000));
								else
									await e.Channel.SendMessage(output);
							}
						}
						else
							await e.Channel.SendMessage($"Invalid command parameter. Try one of these: {Utils.ListToList(CmdManager.rwCommands, "`")}");
					}
					else
						await e.Channel.SendMessage("You are not allowed to do that. Only the master-server admin is allowed to show all data.");
				});

				g.CreateCommand("data")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} data` shows you a list of all data variables.")
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					if (e.User.Id == Credentials.Default.DiscordMasterAdminID)
						await e.Channel.SendMessage($"**[Data Commands]**\n{Utils.ListToList(CmdManager.rwCommands, "`", "\n")}");
					else
						await e.Channel.SendMessage("You are not allowed to do that. Only the master-server admin is allowed to show and use these commands.");
				});
				#endregion
			});
		}

		// This prevents from reloading other commands when a new alias has been added
		public static void CreateKillBotCommands(string s, string c)
		{
			cmd.CreateGroup(s, g =>
			{
				g.CreateCommand(c)
				.Alias(Data.killBotAliases)
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} kill` disconnects the bot from the server.")
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					if (e.User.Id == Credentials.Default.DiscordMasterAdminID)
					{
						await e.Channel.SendMessage("WHY? :anguished: ");
						await System.Threading.Tasks.Task.Delay(500);
						await dClient.Disconnect();
					}
					else
						await e.Channel.SendMessage("You are not allowed to do that.");
				});
			});
		}
	}
}
 