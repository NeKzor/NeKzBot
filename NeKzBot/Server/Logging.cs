using System;
using System.Threading.Tasks;
using Discord;
using NeKzBot.Resources;
using NeKzBot.Modules.Message;

namespace NeKzBot.Server
{
	public class Logging
	{
		/*	Event		Color
		 *	-----------------------
		 *	Error		Red
		 *	Caching		Red
		 *	Init		DarkYellow
		 *	Twitch		DarkMagenta
		 *	Speedrun	DarkRed
		 *	Leaderboard	DarkBlue
		 *	Giveaway	DarkCyan
		 *	Audio		DarkGreen
		 *	Dropbox		Blue
		 *	Others		White*/

		public static uint errorCount = 0;

		public static async Task Init()
		{
			await CON("Initializing await Logging", ConsoleColor.DarkYellow);

			Bot.dClient.MessageReceived += async(s, e) =>
			{
				if (!e.Message.IsAuthor && e.Server?.Id == Credentials.Default.DiscordMainServerID)
					await AutoDownloader.Check(e);
			};

			Bot.dClient.JoinedServer += async (s, e) =>
			{
				await (await Utils.GetChannel(Settings.Default.LogChannelName))?.SendMessage($"**{Utils.GetLocalTime()}**\nBot joined the server.");
			};
			await Task.FromResult(0);
		}

		// Write to console
		public static void CON(object sender, LogMessageEventArgs e) =>
			Console.WriteLine($"{Utils.GetLocalTime()} @ SOCKET : {e.Severity} : {e.Source} : {e.Message}");

		public static async Task CON(string msg, ConsoleColor cc, bool toupper = true)
		{
			Console.ForegroundColor = cc;
			Console.WriteLine($"{Utils.GetLocalTime()} @ ROOT : {await FormatTime(Program.uptimeWatch.Elapsed.TotalSeconds)}{Utils.UpperString(msg, toupper)}");
			Console.ResetColor();
		}

		public static async Task CON(string msg, Exception ex)
		{
			errorCount++;
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"{Utils.GetLocalTime()} @ ROOT : {await FormatTime(Program.uptimeWatch.Elapsed.TotalSeconds)}{Utils.UpperString(msg)} : {ex.Source} : {ex.Message}");
			Console.ResetColor();
		}

		// Write to channel
		public static async Task CHA(string msg, ConsoleColor cc, bool toupper = true)
		{
			await CON(msg, cc, toupper);
			(await Utils.GetChannel(Settings.Default.LogChannelName))?.SendMessage(Utils.CutMessage($"**{Utils.GetLocalTime()}**\n{msg}"));
		}

		public static async Task CHA(string msg, Exception ex)
		{
			await CON(msg, ex);
			(await Utils.GetChannel(Settings.Default.LogChannelName))?.SendMessage(Utils.CutMessage($"**{Utils.GetLocalTime()} -> {msg}**\n**Source** {ex.Source}\n**Message** {ex.Message}"));
		}

		// Show nothing after a random time
		private static Task<string> FormatTime(double time) =>
			Task.FromResult(time > 100 ? string.Empty : string.Format("{0:N2}s : ", time));
	}
}