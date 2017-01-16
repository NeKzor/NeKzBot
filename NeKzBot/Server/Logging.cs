using System;
using Discord;
using NeKzBot.Server;

namespace NeKzBot
{
	public class Logging : NBot
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
		 *	Others		White		*/

		public static void Init()
		{
			CON("Initializing logging", ConsoleColor.DarkYellow);

			dClient.MessageReceived += async(s, e) =>
			{
				if (!e.Message.IsAuthor && e.Server.Id == Credentials.Default.DiscordMainServerID)
					await AutoDownloader.Check(e);
			};

			dClient.UserJoined += async (s, e) =>
			{
				await Utils.GetChannel(e.Server.DefaultChannel.Name)?.SendMessage($"**{e.User.Name}** joined the server. Hi {e.User.Mention}! {Utils.RNGString(":smile:", ":ok_hand:")}");
			};

			dClient.UserUpdated += async (s, e) =>
			{
				var channel = Utils.GetChannel(Settings.Default.LogChannelName, e.Server);
				if (e.Before.Status == UserStatus.Offline && e.After.Status != UserStatus.Offline)
					await channel?.SendMessage($"{Utils.GetLocalTime()} | **{e.After.Name}** is now **online**.");
				else if (e.After.Status == UserStatus.Offline && e.Before.Status != UserStatus.Offline)
					await channel?.SendMessage($"{Utils.GetLocalTime()} | **{e.After.Name}** is now **offline**.");
			};
		}

		// Write to console
		public static void CON(object sender, LogMessageEventArgs e) =>
			Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} @ SOCKET : {e.Severity} : {e.Source} : {e.Message}");

		public static void CON(string s, ConsoleColor c = ConsoleColor.Red, bool toupper = true)
		{
			Console.ForegroundColor = c;
			Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss @ ROOT : ")}{FormatTime(Program.uptimeWatch.Elapsed.TotalSeconds)}{Utils.UpperString(s, toupper)}");
			Console.ResetColor();
		}

		// Write to channel
		public static void CHA(string s, ConsoleColor c = ConsoleColor.Red, bool toupper = true)
		{
			CON(s, c, toupper);
			if (s.Length < 2000)
				Utils.GetChannel(Settings.Default.LogChannelName)?.SendMessage(s);
			else
				Utils.GetChannel(Settings.Default.LogChannelName)?.SendMessage(s.Substring(0, 2000));
		}

		// Write to trace
		public static void TRA(string s, ConsoleColor c = ConsoleColor.Red, bool toupper = true)
		{
			CON(s, c, toupper);
			System.Diagnostics.Trace.WriteLine(s);
		}

		// Show nothing after a random time 
		private static string FormatTime(double time) =>
			time > 100000 ?
			string.Empty : string.Format("{0:N2}s : ", time);
	}
}