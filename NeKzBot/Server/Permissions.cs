using Discord;
using Discord.Commands;
using NeKzBot.Internals.Entities;
using NeKzBot.Resources;
using NeKzBot.Utilities;

namespace NeKzBot.Server
{
	public static class Permissions
	{
		public static bool BotOwnerOnly(Command _, User usr, Channel cha)
		{
			if (usr.Id == Credentials.Default.DiscordBotOwnerId)
				return true;
			cha.SendMessage("Only the bot owner is allowed to execute this command.");
			return false;
		}

		public static bool GuildOwnerOnly(Command _, User usr, Channel cha)
		{
			if (usr.Id == usr.Server.Owner.Id)
				return true;
			cha.SendMessage("Only the server owner is allowed to execute this command.");
			return false;
		}

		public static bool VipGuildsOnly(Command _, User usr, Channel cha)
		{
			if ((Data.Get<Simple>("vips").GetAwaiter().GetResult()).Value.Contains(cha.Server.Id.ToString())
			|| (usr.Id == usr.Server.Owner.Id))
				return true;
			cha.SendMessage("This command only works for VIP servers.");
			return false;
		}

		public static bool AdminOnly(Command _, User usr, Channel cha)
		{
			if (usr.ServerPermissions.Administrator)
				return true;
			cha.SendMessage("Only administrators are allowed to execute this command.");
			return false;
		}

		public static bool LinuxOnly(Command _, User __, Channel cha)
		{
			if (Utils.IsLinux().Result)
				return true;
			cha.SendMessage("The bot is currently not running on the main server host.");
			return false;
		}

		public static bool DisallowBots(Command _, User usr, Channel __)
			=> !(usr.IsBot);

		public static bool DisallowDMs(Command _, User __, Channel cha)
			=> !(cha.IsPrivate);
	}
}