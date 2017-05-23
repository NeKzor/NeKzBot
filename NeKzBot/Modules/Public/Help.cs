using System.Threading.Tasks;
using Discord.Commands;
using NeKzBot.Internals.Entities;
using NeKzBot.Resources;
using NeKzBot.Server;
using NeKzBot.Utilities;

namespace NeKzBot.Modules.Public
{
	public class Help : CommandModule
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
					.Alias("cmds", "modules")
					.Description("Lists you all available modules.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (e.Server.Id == Credentials.Default.DiscordMainServerId)
							await e.Channel.SendMessage($"{Data.ListModules}\n• `{Configuration.Default.PrefixCmd}development`{Data.MoreInformation}");
						else if (e.User.Id == Credentials.Default.DiscordBotOwnerId)
							await e.Channel.SendMessage($"{Data.ListModules}\n• `{Configuration.Default.PrefixCmd}development`\n• `{Configuration.Default.PrefixCmd}private`\n• `{Configuration.Default.PrefixCmd}hidden`{Data.MoreInformation}");
						else
							await e.Channel.SendMessage(Data.ListModules + Data.MoreInformation);
					});

			CService.CreateCommand("fun")
					.Description("Lists you all commands of the fun module.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(Data.FunPublic);
					});

			CService.CreateCommand("info")
					.Description("Lists you all commands of the info module.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(Data.InfoPublic);
					});

			CService.CreateCommand("portal2")
					.Alias("leaderboard")
					.Description("Lists you all commands of the portal2 module.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(Data.LeaderboardPublic);
					});

			CService.CreateCommand("speedrun")
					.Description("Lists you all commands of the speedrun module.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(Data.SpeedrunComPublic);
					});

			CService.CreateCommand("other")
					.Alias("others")
					.Description("Lists you all commands of the other module.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(Data.OthersPublic);
					});

			CService.CreateCommand("raspberry")
					.Alias("rpi")
					.Description("Lists you all commands of the raspberry module.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(Data.LinuxOnly);
					});

			CService.CreateCommand("resource")
					.Alias("resources")
					.Description("Lists you all commands of the resource module.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(Data.ResourcesPublic);
					});

			CService.CreateCommand("rest")
					.Description("Lists you all commands of the rest module.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(Data.RestPublic);
					});

			CService.CreateCommand("vip")
					.Description("Lists you all commands of the vip module.")
					.AddCheck(Permissions.VipGuildsOnly)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(Data.VipServersOnly);
					});

			CService.CreateCommand("private")
					.AddCheck(Permissions.BotOwnerOnly)
					.Hide()
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"{Data.LeaderboardPrivate}\n{Data.BotOwnerOnly}");
					});

			CService.CreateCommand("special")
					.Hide()
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(Data.SpecialPermissionsOnly);
					});

			CService.CreateCommand("hidden")
					.AddCheck(Permissions.VipGuildsOnly)
					.Hide()
					.Do(async e => await (await e.User.CreatePMChannel())?.SendMessage(Data.HiddenMessage));

			CService.CreateCommand("sound")
					.Alias("sounds")
					.AddCheck(Permissions.VipGuildsOnly)
					.Hide()
					.Do(async e => await (await e.User.CreatePMChannel())?.SendMessage("**[Sound Commands - VIP Servers Only]**\n" +
																					   $"Usage: `{Configuration.Default.PrefixCmd}sound <name>`. All available names:\n" +
																					   $"{await Utils.CollectionToList((await Data.Get<Complex>("sounds")).Cast(), string.Empty, ", ", string.Empty, 2)}\n\n" +
																					   $"Known aliases: {await Utils.CollectionToList((await Utils.FindCommandByName("sound")).Aliases, "`")}"));

			// Hints
			CService.CreateCommand("meme")
					.Alias("memes")
					.Description("Hints you a meme command.")
					.AddCheck(Permissions.VipGuildsOnly)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"Try `{Configuration.Default.PrefixCmd}{await Utils.RngAsync((await Data.Get<Complex>("memes")).Values)}`.");
					});

			CService.CreateCommand("tool")
					.Alias("tools")
					.Description("Hints you a tool command.")
					.AddCheck(Permissions.VipGuildsOnly)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"Try `{Configuration.Default.PrefixCmd}{await Utils.RngAsync((await Data.Get<Complex>("tools")).Values)}`.");
					});

			CService.CreateCommand("link")
					.Alias("links")
					.Description("Hints you a link command.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"Try `{Configuration.Default.PrefixCmd}{await Utils.RngAsync((await Data.Get<Complex>("links")).Values)}`.");
					});

			CService.CreateCommand("quote")
					.Alias("quotes")
					.Description("Hints you a quote command.")
					.AddCheck(Permissions.VipGuildsOnly)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"Try `{Configuration.Default.PrefixCmd}quote {await Utils.RngAsync((await Data.Get<Complex>("quotes")).Values)}`.");
					});

			return Task.FromResult(0);
		}
	}
}