using System.Threading.Tasks;
using Discord.Commands;

namespace NeKzBot.Server
{
	public class Commands
	{
		public static CommandService cmd;

		public static async Task Init()
		{
			await Logging.CON("Initializing commands", System.ConsoleColor.DarkYellow);

			Bot.dClient.UsingCommands(x =>
			{
				//x.PrefixChar = Settings.Default.PrefixCmd;
				//x.AllowMentionPrefix = true;
				x.CustomPrefixHandler = PrefixHandler;
				x.ErrorHandler = ErrorHandler;
			});

			cmd = Bot.dClient.GetService<CommandService>();
			cmd.Root.AddCheck(Permission.DisallowDMs);
			//cmd.Root.AddCheck(DisallowBots);
		}

		private static int PrefixHandler(Discord.Message msg)
		{
			var text = msg.RawText;
			var prefix = Settings.Default.PrefixCmd.ToString();
			var botname = Bot.dClient.CurrentUser.Mention;

			// Don't do stuff if there's a single character or if there's a single prefix at the end
			if (text.Length == 1 || text.IndexOf(prefix) + 1 == text.Length)
				return -1;

			// Prefix
			if (text.StartsWith(prefix))
				return prefix.Length;
			if (text.Contains(prefix))
				return ((text[text.IndexOf(prefix) + 1] == ' ') || (text[text.IndexOf(prefix) - 1] != ' ')) ?
					-1 : text.IndexOf(prefix) + prefix.Length;

			// Bot mentioned
			if (text.StartsWith(botname))
				return botname.Length;
			if (text.Contains(botname))
				return text.IndexOf(botname) + botname.Length;

			return -1;
		}

		private static async void ErrorHandler(object sender, CommandErrorEventArgs e)
		{
			switch (e.ErrorType)
			{
				case CommandErrorType.Exception:
					await e.Channel.SendMessage("**Error.**");
					await Logging.CHA("Unhandled exception", e.Exception);
					break;
				case CommandErrorType.UnknownCommand:
					//e.Channel.SendMessage("Unknown command.");
					break;
				case CommandErrorType.BadPermissions:
					await e.Channel.SendMessage("You don't have the permission to do that.");
					break;
				case CommandErrorType.BadArgCount:
					await e.Channel.SendMessage(await Resources.Utils.FindDescription(e.Message.RawText.Substring(PrefixHandler(e.Message)).Split(' ')[0]));
					break;
				case CommandErrorType.InvalidInput:
					await e.Channel.SendMessage("**Invalid input.**");
					break;
				default:
					break;
			}
		}
	}
}