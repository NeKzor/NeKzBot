using System;
using System.Linq;
using Discord;
using NeKzBot.Properties;

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
				if (!e.Message.IsAuthor)
				{
					await AutoDownloader.Check(e);
					dClient.Log.Info($"USER : {e.User.ToString()}", e.Message.Text, null);
				}
				else
					dClient.Log.Info($"BOT : {e.User.ToString()}", e.Message.Text, null);
			};

			dClient.UserJoined += async (s, e) =>
			{
				var chan = GetChannelByName(e.Server.Name, "welcome");
				await chan.SendMessage($"**{e.User.Name}** joined the server. Hi {e.User.Mention}! {Utils.RNGString(":smile:", ":ok_hand:")}");
			};

			dClient.UserUpdated += async (s, e) =>
			{
				var channel = GetChannelByName();
				if (e.Before.Status == UserStatus.Offline && e.After.Status != UserStatus.Offline)
					await channel.SendMessage($"{Utils.GetLocalTime()} | **{e.After.Name}** is now **online**.");
				else if (e.After.Status == UserStatus.Offline && e.Before.Status != UserStatus.Offline)
					await channel.SendMessage($"{Utils.GetLocalTime()} | **{e.After.Name}** is now **offline**.");
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
			GetChannelByName()?.SendMessage(s);
		}

		// Write to trace
		public static void TRA(string s, ConsoleColor c = ConsoleColor.Red, bool toupper = true)
		{
			CON(s, c, toupper);
			System.Diagnostics.Trace.WriteLine(s);
		}

		// Get default logging channel
		private static Channel GetChannelByName(string serverName = null, string channelName = null)
		{
			serverName = serverName ?? Settings.Default.ServerName;
			channelName = channelName ?? Settings.Default.LogChannelName;
			return dClient?.FindServers(serverName)?.First().FindChannels(channelName, ChannelType.Text, true)?.First();
		}

		// Show nothing after a random time 
		private static string FormatTime(double time) =>
			time < 5184000 ?
				string.Format("{0:N2}s : ", time) : string.Empty;
	}
}