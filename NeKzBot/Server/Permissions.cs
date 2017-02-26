using Discord;
using Discord.Commands;

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
			cha.SendMessage("Only administrators are allowed to execute this command.");
			return false;
		}

		public static bool DisallowBots(Command _, User usr, Channel __)
			=> !usr.IsBot;

		public static bool DisallowDMs(Command _, User __, Channel cha)
			=> !cha.IsPrivate;

		// For CommandBuilders
		public static bool AdminOnly(User usr)
			=> usr.ServerPermissions.Administrator;

		public static bool BotOwnerOnly(User usr)
			=> usr.Id == Credentials.Default.DiscordBotOwnerId;

		public static bool MainServerOnly(Discord.Server ser)
			=> ser.Id == Credentials.Default.DiscordMainServerId;
	}
}