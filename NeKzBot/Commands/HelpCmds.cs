using NeKzBot.Properties;

namespace NeKzBot
{
	public class HelpCmds : Commands
	{
		public static void Load()
		{
			Logging.CON("Loading help commands", System.ConsoleColor.DarkYellow);
			BasicHelp();
		}

		private static void BasicHelp()
		{
			cmd.CreateCommand("commands")
			.Alias("cmds", "?")
			.Description($"**-** `{Settings.Default.PrefixCmd}commands` shows you a list of commands.")
			.Do(async e =>
			{
				await e.Channel.SendIsTyping();
				if (Utils.RoleCheck(e.User, Settings.Default.AllowedRoles))
					await e.Channel.SendMessage(Data.msgAll);
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
				if (Utils.RoleCheck(e.User, Settings.Default.AllowedRoles))
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
				if (e.User.Id == Settings.Default.MaseterAdminID)
					await e.Channel.SendMessage(Data.gameMsg + Data.adminGameMsg + Data.masterAdminGameMsg);
				else if (Utils.RoleCheck(e.User, Settings.Default.AllowedRoles))
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
				if (e.User.Id == Settings.Default.MaseterAdminID)
					await e.Channel.SendMessage(Data.botMsg + Data.adminBotMsg + Data.masterAdminBotMsg);
				else if (Utils.RoleCheck(e.User, Settings.Default.AllowedRoles))
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
		}
	}
}