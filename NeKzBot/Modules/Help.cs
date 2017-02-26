using System.Threading.Tasks;
using Discord.Commands;
using NeKzBot.Server;
using NeKzBot.Resources;

namespace NeKzBot.Modules
{
	public class Help : Commands
	{
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Help Commands", LogColor.Init);
			await HelpCommands();
		}

		private static Task HelpCommands()
		{
			CService.CreateCommand("help")
					.Alias("?")
					.Description($"• `{Configuration.Default.PrefixCmd}help <command>` returns information about that command.")
					.Parameter("command", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(await Utils.FindDescriptionAsync(e.Args[0]));
					});

			CService.CreateCommand("commands")
					.Alias("cmds")
					.Description($"• `{Configuration.Default.PrefixCmd}commands` shows you a list of commands.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (Permissions.BotOwnerOnly(e.User))
							// Thanks for the 2k character limit, Discord
							await e.Channel.SendMessage(Data.funMsg + Data.lbMsg + Data.vcMsg + Data.mainServerMsg + Data.srcomMsg + Data.rpiMsg + Data.dropboxMsg + Data.botMsg + Data.botOwnerBotMsg + Data.msgEnd);
						else if (Permissions.MainServerOnly(e.Server))
							await e.User.SendMessage(Data.funMsg + Data.lbMsg + Data.vcMsg + Data.mainServerMsg + Data.srcomMsg + Data.rpiMsg + Data.dropboxMsg + Data.botMsg + Data.msgEnd);
						else
							await e.User.SendMessage(Data.funMsg + Data.lbMsg + Data.vcMsg + Data.botMsg + Data.msgEnd);
					});

			CService.CreateCommand("fun")
					.Alias("fun?")
					.Description($"• `{Configuration.Default.PrefixCmd}fun` shows you a list of \"fun\" commands.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(Data.funMsg);
					});

			CService.CreateCommand("leaderboard")
					.Alias("lb", "lb?")
					.Description($"• `{Configuration.Default.PrefixCmd}leaderboard` shows you a list of leaderboard commands.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (Permissions.BotOwnerOnly(e.User))
							await e.Channel.SendMessage(Data.lbMsg + Data.botOwnerLbMsg);
						else
							await e.Channel.SendMessage(Data.lbMsg);
					});

			CService.CreateCommand("sounds")
					.Alias("vc?")
					.Description($"• `{Configuration.Default.PrefixCmd}sounds` shows you a list of sound commands.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(Data.vcMsg);
					});

			CService.CreateCommand("game")
					.Alias("game?")
					.Description($"• `{Configuration.Default.PrefixCmd}sounds` shows you a list of sound commands.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (Permissions.BotOwnerOnly(e.User))
							await e.Channel.SendMessage(Data.botOwnerGameMsg);
						else if (Permissions.MainServerOnly(e.Server))
							await e.Channel.SendMessage($"Try `{Configuration.Default.PrefixCmd}giveaway`.");
						else
							await e.Channel.SendMessage("There are no games on this server.");
					});

			CService.CreateCommand("rpi")
					.Alias("rpi?")
					.Description($"• `{Configuration.Default.PrefixCmd}rpi` shows you a list of commands which give you information about the server.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(Data.rpiMsg);
					});

			CService.CreateCommand("speedrun")
					.Alias("speedruncom", "srcom", "srcom?")
					.Description($"• `{Configuration.Default.PrefixCmd}speedrun` shows you a list of speedruncom commands.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(Data.srcomMsg);
					});

			CService.CreateCommand("dropbox")
					.Alias("dropbox", "db?")
					.Description($"• `{Configuration.Default.PrefixCmd}dropbox` shows you a list of Dropbox commands.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (Permissions.BotOwnerOnly(e.User))
							await e.Channel.SendMessage(Data.dropboxMsg + Data.botOwnerDropboxMsg);
						else
							await e.Channel.SendMessage(Data.dropboxMsg);
					});

			CService.CreateCommand("server")
					.Alias("servercmds", "mainserver")
					.Description($"• `{Configuration.Default.PrefixCmd}server` shows you a list of server exclusive commands.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage(Data.mainServerMsg);
					});

			CService.CreateCommand("botcommands")
					.Alias("botcmds", "bot?")
					.Description($"• `{Configuration.Default.PrefixCmd}botcommands` shows you a list of bot commands.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (Permissions.BotOwnerOnly(e.User))
							await e.Channel.SendMessage(Data.botOwnerBotMsg);
						else
							await e.Channel.SendMessage(Data.botMsg);
					});
			return Task.FromResult(0);
		}
	}
}