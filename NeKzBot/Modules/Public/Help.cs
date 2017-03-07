using System.Threading.Tasks;
using Discord.Commands;
using NeKzBot.Resources;
using NeKzBot.Server;

namespace NeKzBot.Modules.Public
{
	public class Help : Commands
	{
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Help Module", LogColor.Init);
			await HelpCommands();
		}

		private static Task HelpCommands()
		{
			CService.CreateCommand("help")
					.Alias("?")
					.Description("Returns the description of a command.")
					.Parameter("command", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(await Utils.FindDescriptionAsync(e.Args[0]));
					});

			CService.CreateCommand("commands")
					.Alias("cmds")
					.Description("Shows you a list of commands.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						var msg = "**[All Available Commands]**\nInvoke one of them to show the full list."
								+ $"\n• `{Configuration.Default.PrefixCmd}fun`"
								+ $"\n• `{Configuration.Default.PrefixCmd}info`"
								+ $"\n• `{Configuration.Default.PrefixCmd}portal2`"
								+ $"\n• `{Configuration.Default.PrefixCmd}speedrun`"
								+ $"\n• `{Configuration.Default.PrefixCmd}other`"
								+ $"\n• `{Configuration.Default.PrefixCmd}raspberry`"
								+ $"\n• `{Configuration.Default.PrefixCmd}resource`"
								+ $"\n• `{Configuration.Default.PrefixCmd}rest`"
								+ $"\n• `{Configuration.Default.PrefixCmd}vip`";
						if (e.Server.Id == Credentials.Default.DiscordMainServerId)
							await e.Channel.SendMessage($"{msg}\n• `{Configuration.Default.PrefixCmd}development`");
						else if (e.User.Id == Credentials.Default.DiscordBotOwnerId)
							await e.Channel.SendMessage($"{msg}\n• `{Configuration.Default.PrefixCmd}development`\n• `{Configuration.Default.PrefixCmd}private`\n• `{Configuration.Default.PrefixCmd}hidden`");
						else
							await e.Channel.SendMessage(msg);
					});

			CService.CreateCommand("fun")
					.Description("Shows you a list of commands.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(Data.FunPublic);
					});

			CService.CreateCommand("info")
					.Description("Shows you a list of commands.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(Data.InfoPublic);
					});

			CService.CreateCommand("portal2")
					.Alias("leaderboard")
					.Description("Shows you a list of commands.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(Data.LeaderboardPublic);
					});

			CService.CreateCommand("speedrun")
					.Description("Shows you a list of commands.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(Data.SpeedrunComPublic);
					});

			CService.CreateCommand("other")
					.Alias("others")
					.Description("Shows you a list of commands.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(Data.OthersPublic);
					});

			CService.CreateCommand("raspberry")
					.Alias("rpi")
					.Description("Shows you a list of commands.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(Data.LinuxOnly);
					});

			CService.CreateCommand("resource")
					.Alias("resources")
					.Description("Shows you a list of commands.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(Data.ResourcesPublic);
					});

			CService.CreateCommand("rest")
					.Description("Shows you a list of commands.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(Data.RestPublic);
					});

			CService.CreateCommand("vip")
					.Description("Shows you a list of commands.")
					.AddCheck(Permissions.VipGuildsOnly)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(Data.VipServersOnly);
					});

			CService.CreateCommand("development")
					.Alias("developer", "dev")
					.AddCheck(Permissions.MainServerOnly)
					.Hide()
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(Data.MainServerOnly);
					});

			CService.CreateCommand("private")
					.AddCheck(Permissions.BotOwnerOnly)
					.Hide()
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"{Data.MainServerOnly}\n{Data.LeaderboardPrivate}\n{Data.BotOwnerOnly}");
					});

			CService.CreateCommand("hidden")
					.AddCheck(Permissions.VipGuildsOnly)
					.Hide()
					.Do(async e =>
					{
						await (await e.User.CreatePMChannel())?.SendMessage("**[Hidden Commands & Tricks]**\n• `10=tick` converts 10 ticks into seconds with the default Portal 2 tickrate 60.\n"
																		  + "• `1=sec` converts 1 second into ticks with the default Portal 2 tickrate 60.\n"
																		  + "• You can also set a custom tickrate like this: `1=sec66`.\n"
																		  + "• You can create a startdemos command very quickly with this: `10->demo_`.\n"
																		  + "• You can view the image preview of a Steam workshop item by just sending a valid uri which should have `https` or `http` in the beginning.\n"
																		  + $"• The prefix command doesn't have to start at the beginning of the input: `this would also work {Configuration.Default.PrefixCmd}{Configuration.Default.BotCmd}`.\n"
																		  + $"• You can also use a mention to execute commands: `{Bot.Client.CurrentUser.Mention} commands`.\n"
																		  + "• Every Portal 2 challenge mode map has its own abbreviation e.g. `PGN` means Portal Gun, you don't have to write it in caps.");
					});

			CService.CreateCommand("sound")
					.AddCheck(Permissions.VipGuildsOnly)
					.Hide()
					.Do(async e => await (await e.User.CreatePMChannel())?.SendMessage($"**[Sound Commands - VIP Servers Only]**\n{await Utils.ArrayToList(Data.SoundNames, 0, string.Empty, "`, ", $"`{Configuration.Default.PrefixCmd}", 2)}"));

			// Hints
			CService.CreateCommand("meme")
					.AddCheck(Permissions.VipGuildsOnly)
					.Description("Hints you a meme command.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"Try `{Configuration.Default.PrefixCmd}{await Utils.RngAsync(Data.MemeCommands, 0) as string}`.");
					});

			CService.CreateCommand("tool")
					.AddCheck(Permissions.VipGuildsOnly)
					.Description("Hints you a tool command.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"Try `{Configuration.Default.PrefixCmd}{await Utils.RngAsync(Data.ToolCommands, 0) as string}`.");
					});

			CService.CreateCommand("link")
					.Description("Hints you a link command.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"Try `{Configuration.Default.PrefixCmd}{await Utils.RngAsync(Data.LinkCommands, 0) as string}`.");
					});

			CService.CreateCommand("text")
					.AddCheck(Permissions.VipGuildsOnly)
					.Description("Hints you a text command.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"Try `{Configuration.Default.PrefixCmd}quote {await Utils.RngAsync(Data.QuoteNames, 0) as string}`.");
					});

			return Task.FromResult(0);
		}
	}
}