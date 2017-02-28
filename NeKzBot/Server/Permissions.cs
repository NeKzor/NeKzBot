using Discord;
using Discord.Commands;
using NeKzBot.Resources;

namespace NeKzBot.Server
{
	public static class Permissions
	{
		// For CommandServices
		public static bool BotOwnerOnly(Command _, User usr, Channel cha)
		{
			if (usr.Id == Credentials.Default.DiscordBotOwnerId)
				return true;
			cha.SendMessage("You are not allowed to execute this command.");
			return false;
		}

		public static bool MainServerOnly(Command _, User __, Channel cha)
		{
			if (cha.Server.Id == Credentials.Default.DiscordMainServerId)
				return true;
			cha.SendMessage("This command only works on the main server.");
			return false;
		}

		public static bool AdminOnly(Command _, User usr, Channel cha)
		{
			if (usr.ServerPermissions.Administrator)
				return true;
			cha.SendMessage("Only an administrator is allowed to execute this command.");
			return false;
		}

		public static bool DisallowBots(Command _, User usr, Channel __)
			=> !(usr.IsBot);

		public static bool DisallowDMs(Command _, User __, Channel cha)
			=> !(cha.IsPrivate);

		public static bool LinuxOnly(Command _, User __, Channel cha)
		{
			if (Utils.IsLinux().Result)
				return true;
			cha.SendMessage("The bot is currently not running on the main server host.");
			return false;
		}

		// In CommandBuilders
		public static bool AdminOnly(User usr)
			=> usr.ServerPermissions.Administrator;

		public static bool BotOwnerOnly(User usr)
			=> usr.Id == Credentials.Default.DiscordBotOwnerId;

		public static bool MainServerOnly(Discord.Server ser)
			=> ser.Id == Credentials.Default.DiscordMainServerId;
	}
}