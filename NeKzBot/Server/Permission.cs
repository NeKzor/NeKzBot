using Discord;
using Discord.Commands;

namespace NeKzBot.Server
{
	public class Permission
	{
		public static bool DisallowDMs(Command cmd, User usr, Channel cha) => !cha.IsPrivate;

		public static bool DisallowBots(Command cmd, User usr, Channel cha) => !usr.IsBot;

		public static bool BotOwnerOnly(Command cmd, User usr, Channel cha)
		{
			if (usr.Id == Credentials.Default.DiscordBotOwnerID)
				return true;
			cha.SendMessage("You are not allowed to execute this command.");
			return false;
		}

		public static bool MainServerOnly(Command cmd, User usr, Channel cha)
		{
			if (cha.Server.Id == Credentials.Default.DiscordMainServerID)
				return true;
			cha.SendMessage("This command only works on the main server.");
			return false;
		}

		public static bool AdminOnly(Command cmd, User usr, Channel cha)
		{
			if (usr.ServerPermissions.Administrator)
				return true;
			cha.SendMessage("Only administrators are allowed to execute this command.");
			return false;
		}
	}
}