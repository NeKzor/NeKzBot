using System.Linq;
using Discord.Commands;
using NeKzBot.Properties;

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
				g.CreateCommand("uptime")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} uptime shows you how long the bot is running for.")
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					await e.Channel.SendMessage($"Bot is running for {Utils.AsBold(Program.GetUptime())}");
				});

				g.CreateCommand("location")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} location` gives you information about the server where the bot is located.")
				.Do(async e =>
				{
					await e.Channel.SendIsTyping();
					await e.Channel.SendMessage($"**Bot Location -** Graz, Austria :flag_at:");
				});

				g.CreateCommand("info")
				.Alias("status", "?")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} info` shows some information about the bot.")
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					await e.Channel.SendMessage(
						$"**AppName - **{NBot.dClient.Config.AppName}"
						+ $"\n**AppVersion - **{NBot.dClient.Config.AppVersion}"
						+ $"\n**AppUrl - **{NBot.dClient.Config.AppUrl}"
						+ $"\n**Current Game - ** {NBot.dClient.CurrentGame.Name}"
						+ $"\n**Client State - ** {NBot.dClient.State}"
						+ $"\n**Client Status - ** {NBot.dClient.Status.Value}"
						+ $"\n**Regions - ** {NBot.dClient.Regions.Count()}"
						+ $"\n**Servers - ** {NBot.dClient.Servers.Count()}"
						+ $"\n**Services - ** {NBot.dClient.Services.Count()}"
						+ $"\n**SessionID - ** {NBot.dClient.SessionId}"
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

				g.CreateCommand("data")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} data` shows you a list of all data variables.")
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					if (Utils.RoleCheck(e.User, Settings.Default.AllowedRoles))
						await e.Channel.SendMessage($"**[Data Commands]**\n{Utils.ListToList(CmdManager.rwCommands, "`", "\n")}");
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
						NBot.dClient.SetGame(Data.randomGames[Utils.RNG(0, Data.randomGames.Count())]);
					else
						await e.Channel.SendMessage(Data.rolesMsg);
				});

				g.CreateCommand("setgame")
				.Alias("play", "sg")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} setgame <name>` sets a new playing status for the bot writes his updates.\n**-** Current game is: *{NBot.dClient.CurrentGame}*.")
				.Parameter("p", ParameterType.Required)
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					if (Utils.RoleCheck(e.User, Settings.Default.AllowedRoles))
					{
						if (e.Args[0] != string.Empty)
						{
							if (e.Args[0] != NBot.dClient.CurrentGame.Name)
							{
								NBot.dClient.SetGame(e.Args[0]);
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
					if (e.User.Id == Settings.Default.MaseterAdminID)
					{
						var values = e.Args[0].Split(Utils.seperator);
						int index = 0;
						if (values.Count() < 2)
							await e.Channel.SendMessage("Invalid parameter count.");
						else if (Utils.SearchArray(CmdManager.dataCommands, 0, values[0], out index))
						{
							if ((bool)CmdManager.dataCommands[index, 1])
							{
								bool success = false;
								await e.Channel.SendMessage(Utils.AddData((string)CmdManager.dataCommands[index, 2], Utils.GetRest(values, 1, 0, " ", true), out success));
								if (success)
								{
									try
									{
										// Reload new data
										((System.Action)CmdManager.dataCommands[index, 5]).Invoke();
										// Reload command
										((System.Action)CmdManager.dataCommands[index, 4])?.Invoke();
										// Reload manager
										CmdManager.Init();
									}
									catch
									{
										await e.Channel.SendMessage("Reload failed.");
									}
								}
							}
							else
								await e.Channel.SendMessage("This command doesn't allow to be changed.");
						}
						else
							await e.Channel.SendMessage($"Invalid command parameter.\nTry one of these: {Utils.ListToList(CmdManager.rwCommands, "`")}");
					}
					else
						await e.Channel.SendMessage("You are not allowed to do that.\nOnly the master/server admin is allowed to change data.");
				});

				g.CreateCommand("delete")
				.Alias("remove")
				.Description($"**-** `{Settings.Default.PrefixCmd + Settings.Default.BotCmd} remove <name> <value>` removes data about that command.")
				.Parameter("p", ParameterType.Unparsed)
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					if (e.User.Id == Settings.Default.MaseterAdminID)
					{
						var values = e.Args[0].Split(Utils.seperator);
						int index = 0;
						if (values.Count() != 2)
							await e.Channel.SendMessage("Invalid parameter count.");
						else if (Utils.SearchArray(CmdManager.dataCommands, 0, values[0], out index))
						{
							if ((bool)CmdManager.dataCommands[index, 1])
								await e.Channel.SendMessage(Utils.DeleteData((string)CmdManager.dataCommands[index, 2], Utils.GetRest(values, 1, 0, " ", true)));
							else
								await e.Channel.SendMessage("This command doesn't allow to be changed.");
						}
						else
							await e.Channel.SendMessage($"Invalid command parameter.\nTry one of these: {Utils.ListToList(CmdManager.rwCommands, "`")}");
					}
					else
						await e.Channel.SendMessage("You are not allowed to do that.\nOnly the master/server admin is allowed to change data.");
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
					if (e.User.Id == Settings.Default.MaseterAdminID)
					{
						await e.Channel.SendMessage("WHY? :anguished: ");
						await System.Threading.Tasks.Task.Delay(500);
						await NBot.dClient.Disconnect();
					}
					else
						await e.Channel.SendMessage("You are not allowed to do that.");
				});
			});
		}
	}
}