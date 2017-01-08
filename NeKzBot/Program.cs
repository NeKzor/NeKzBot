using System;
using System.Diagnostics;
using NeKzBot.Properties;

namespace NeKzBot
{
	/// <summary>
	/// Project name:	NeKzBot
	/// Made by:		NeKz
	/// Start date:		03/12/2016
	/// Version:		1.1
	/// Release date:	26/12/2016
	/// Latest update:	08/01/2017
	/// GitHub repo:	https://github.com/NeKzor/NeKzBot
	/// Description:	Test the application here - https://discord.gg/rEazbJn
	/// </summary>
	public class Program
	{
		public static Stopwatch uptimeWatch = new Stopwatch();

		private static void Main()
		{
			uptimeWatch.Start();
			Console.BackgroundColor = ConsoleColor.Black;
			Console.ForegroundColor = ConsoleColor.DarkGreen;
			Console.Title = $"{Settings.Default.AppName} V{Settings.Default.AppVersion} - Discord Server Bot";

			//if (!Debugger.IsAttached)
			//	ThanksMono();

			NBot bot = new NBot();
		}

		// Uptime =/= connection time
		public static string GetUptime() =>
			uptimeWatch.Elapsed.ToString("h\\h\\ m\\m\\ s\\s");

		//// Mono bug workaround
		//public static void ThanksMono()
		//{
		//	// Scope -> User
		//	Settings.Default.UpdateChannelName = Settings.Default.UpdateChannelName;
		//	Settings.Default.RefreshTime = Settings.Default.RefreshTime;
		//	Settings.Default.BoardParameter = Settings.Default.BoardParameter;
		//	Settings.Default.AutoUpdate = Settings.Default.AutoUpdate;
		//	Settings.Default.GiveawayResetTime = Settings.Default.GiveawayResetTime;
		//	Settings.Default.GiveawayMaxTries = Settings.Default.GiveawayMaxTries;
		//	Settings.Default.GiveawayEnabled = Settings.Default.GiveawayEnabled;
		//	Settings.Default.CachingTime = Settings.Default.CachingTime;
		//	Settings.Default.Save();
		//}
	}
}