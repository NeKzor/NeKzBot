using Discord;
using Discord.Commands;

namespace NeKzBot
{
	public class Commands : NBot
	{
		public static CommandService cmd;

		public static void Init()
		{
			Logging.CON("Initializing commands", System.ConsoleColor.DarkYellow);

			dClient.UsingCommands(x =>
			{
				x.PrefixChar = Server.Settings.Default.PrefixCmd;
				x.HelpMode = HelpMode.Public;
			});

			cmd = dClient.GetService<CommandService>();
			cmd.Root.AddCheck(Permission);
		}

		// Ignore commands from private chat rooms and messages from other bots
		private static bool Permission(Command cmd, User usr, Channel cha) =>
			cha.IsPrivate || usr.IsBot ?
			false : true;
	}
}