using System;
using System.Threading.Tasks;
using Discord;
using NeKzBot.Resources;

namespace NeKzBot.Server
{
	public static class Logger
	{
		public static async Task SendAsync(LogMessage msg)
			=> Console.WriteLine($"{await Utils.GetLocalTime()} @ SOCKET : {msg.Severity} : {msg.Source} : {msg.Message}");

		public static async Task<object> SendAsync(string message, LogColor color)
		{
			Console.ForegroundColor = (ConsoleColor)color;
			Console.WriteLine($"{await Utils.GetLocalTime()} @ ROOT : {message}");
			Console.ResetColor();
			return null;
		}

		public static async Task<object> SendAsync(string message, Exception e)
		{
			Console.ForegroundColor = (ConsoleColor)LogColor.Error;
			Console.WriteLine($"{await Utils.GetLocalTime()} @ ROOT : {message} : {e.Source} : {e.Message}");
			Console.ResetColor();
			return null;
		}
	}
}