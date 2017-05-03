using Discord;
using Discord.Commands;
using NeKzBot.Modules.Public;

namespace NeKzBot.Server
{
	public static class Handlers
	{
		public static int PrefixHandler(Message msg)
		{
			// For safety
			if (msg == null)
				return -1;

			// Make sure that it's not from a DM channel
			if (msg.Channel.IsPrivate)
			{
				// Execute DM specific commands here
				Contest.CheckCommand(msg).GetAwaiter();
				return -1;
			}

			var text = msg.RawText;
			var prefix = Configuration.Default.PrefixCmd.ToString();
			var botname = Bot.Client.CurrentUser.Mention;

			// Don't do stuff if there's a single character or if there's a single prefix at the end
			if ((text.Length == 1)
			|| (text.IndexOf(prefix) + 1 == text.Length))
				return -1;

			// Prefix
			if (text.StartsWith(prefix))
				return prefix.Length;
			if (text.Contains(prefix))
				return ((text[text.IndexOf(prefix) + 1] == ' ')
					|| (text[text.IndexOf(prefix) - 1] != ' '))
														? -1
														: text.IndexOf(prefix) + prefix.Length;

			// Bot mention
			return (text.StartsWith(botname))
						? botname.Length
						: (text.Contains(botname))
							   ? text.IndexOf(botname) + botname.Length
							   : -1;
		}

		public static async void ErrorHandlerAsync(object _, CommandErrorEventArgs e)
		{
			if (e.Channel.IsPrivate)
				return;

			switch (e.ErrorType)
			{
				case CommandErrorType.Exception:
					await e.Channel.SendMessage("**Error.**");
					await Logger.SendToChannelAsync("Commands.ErrorHandler Unhandled Exception", e.Exception);
					break;
				case CommandErrorType.InvalidInput:
					await e.Channel.SendMessage("**Invalid input.**");
					break;
			}
		}
	}
}