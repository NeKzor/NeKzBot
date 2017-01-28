﻿using System;
using System.Diagnostics;
using NeKzBot.Server;

namespace NeKzBot
{
	/// <summary>
	/// Project name:	NeKzBot
	/// Made by:		NeKz
	/// Version:		1.3
	/// Start date:		03/12/2016
	/// Release date:	26/12/2016
	/// Latest update:	28/01/2017
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
			Console.ForegroundColor = ConsoleColor.White;
			Console.Title = $"{Settings.Default.AppName} V{Settings.Default.AppVersion} - Discord Server Bot";
			new Bot().Start().GetAwaiter().GetResult();
		}

		// Uptime =/= connection time
		public static string GetUptime() =>
			uptimeWatch.Elapsed.ToString("h\\h\\ m\\m\\ s\\s");
	}
}