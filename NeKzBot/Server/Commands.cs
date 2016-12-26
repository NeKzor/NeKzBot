using Discord.Commands;

namespace NeKzBot
{
	public class Commands
	{
		public static CommandService cmd;

		public static void Init()
		{
			Logging.CON("Initializing commands", System.ConsoleColor.DarkYellow);

			NBot.dClient.UsingCommands(x =>
			{
				x.PrefixChar = Properties.Settings.Default.PrefixCmd;
				x.AllowMentionPrefix = true;
				x.HelpMode = HelpMode.Public;
			});

			cmd = NBot.dClient.GetService<CommandService>();
		}
	}
}