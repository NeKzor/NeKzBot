using System.Threading.Tasks;
using NeKzBot.Server;
using NeKzBot.Resources;

namespace NeKzBot.Tasks
{
	public class HelpCmds : Commands
	{
		public static async Task Load()
		{
			await Logging.CON("Loading help commands", System.ConsoleColor.DarkYellow);
			await BasicHelp();
		}

		private static Task BasicHelp()
		{
			cmd.CreateCommand("help")
			.Alias("?")
			.Description("Returns information about commands.")
			.Parameter("command", Discord.Commands.ParameterType.Unparsed)
			.Do(async e =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage(await Utils.FindDescription(e.Args[0]));
			});

			cmd.CreateCommand("commands")
			.Alias("cmds")
			.Description($"**-** `{Settings.Default.PrefixCmd}commands` shows you a list of commands.")
			.Do(async e =>
			{
				await e.Channel.SendIsTyping();
				if (Permission.BotOwnerOnly(null, e.User, null))
					// Thanks for the 2k character limit, Discord
					await e.Channel.SendMessage(Data.funMsg + Data.lbMsg + Data.vcMsg + Data.gameMsg + Data.srcomMsg + Data.rpiMsg + Data.dropboxMsg + Data.botMsg + Data.msgEnd);
				else
					await e.User.SendMessage(Data.funMsg + Data.lbMsg + Data.vcMsg + Data.gameMsg + Data.botMsg + Data.msgEnd);
			});

			cmd.CreateCommand("fun")
			.Alias("fun?")
			.Description($"**-** `{Settings.Default.PrefixCmd}fun` shows you a list of \"fun\" commands.")
			.Do(async e =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage(Data.funMsg);
			});

			cmd.CreateCommand("leaderboard")
			.Alias("lb", "lb?")
			.Description($"**-** `{Settings.Default.PrefixCmd}leaderboard` shows you a list of leaderboard commands.")
			.Do(async e =>
			{
				await e.Channel.SendIsTyping();
				if (Permission.BotOwnerOnly(null, e.User, null))
					await e.Channel.SendMessage(Data.lbMsg + Data.adminLbMsg);
				else
					await e.Channel.SendMessage(Data.lbMsg);
			});

			cmd.CreateCommand("sounds")
			.Alias("vc?")
			.Description($"**-** `{Settings.Default.PrefixCmd}sounds` shows you a list of sound commands.")
			.Do(async e =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage(Data.vcMsg);
			});

			cmd.CreateCommand("games")
			.Alias("games?")
			.Description($"**-** `{Settings.Default.PrefixCmd}games` shows you a list of game commands.")
			.Do(async e =>
			{
				await e.Channel.SendIsTyping();
				if (e.User.Id == Credentials.Default.DiscordBotOwnerID)
					await e.Channel.SendMessage(Data.gameMsg + Data.adminGameMsg + Data.masterAdminGameMsg);
				else if (Permission.BotOwnerOnly(null, e.User, null))
					await e.Channel.SendMessage(Data.gameMsg + Data.adminGameMsg);
				else
					await e.Channel.SendMessage(Data.gameMsg);
			});

			cmd.CreateCommand("botcommands")
			.Alias("botcmds", "bot?")
			.Description($"**-** `{Settings.Default.PrefixCmd}botcommands` shows you a list of bot commands.")
			.Do(async e =>
			{
				await e.Channel.SendIsTyping();
				if (e.User.Id == Credentials.Default.DiscordBotOwnerID)
					await e.Channel.SendMessage(Data.botMsg + Data.adminBotMsg + Data.masterAdminBotMsg);
				else if (Permission.BotOwnerOnly(null, e.User, null))
					await e.Channel.SendMessage(Data.botMsg + Data.adminBotMsg);
				else
					await e.Channel.SendMessage(Data.botMsg);
			});

			cmd.CreateCommand("rpi")
			.Alias("rpi?")
			.Description($"**-** `{Settings.Default.PrefixCmd}rpi` shows you a list of commands which give you information about the server.")
			.Do(async e =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage(Data.rpiMsg);
			});

			cmd.CreateCommand("speedrun")
			.Alias("speedruncom", "srcom", "srcom?")
			.Description($"**-** `{Settings.Default.PrefixCmd}speedrun` shows you a list of speedruncom commands.")
			.Do(async e =>
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage(Data.srcomMsg);
			});

			cmd.CreateCommand("dropbox")
			.Alias("dropbox", "db?")
			.Description($"**-** `{Settings.Default.PrefixCmd}dropbox` shows you a list of Dropbox commands.")
			.Do(async e =>
			{
				await e.Channel.SendIsTyping();
				if (e.User.Id == Credentials.Default.DiscordBotOwnerID)
					await e.Channel.SendMessage(Data.dropboxMsg + Data.masterAdminDropboxMsg);
				else
					await e.Channel.SendMessage(Data.dropboxMsg);
			});
			return Task.FromResult(0);
		}
	}
}