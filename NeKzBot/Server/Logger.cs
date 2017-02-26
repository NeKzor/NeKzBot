using System;
using System.Threading.Tasks;
using Discord;
using NeKzBot.Resources;

namespace NeKzBot.Server
{
	public static class Logger
	{
		public static uint ErrorCount { get; private set; } = 0;

		public static async Task InitAsync()
		{
			await SendAsync("Initializing Logger", LogColor.Init);
			// Check for non-command execution events
			Bot.Client.MessageReceived += async (_, e) => await Events.OnReceiveAsync(e);
			// Log this to all servers
			Bot.Client.UserJoined += async (_, e) => await Events.OnUserJoinedAsync(e);
			Bot.Client.UserLeft += async (_, e) => await Events.OnUserLeftAsync(e);
			Bot.Client.UserBanned += async (_, e) => await Events.OnUserBannedAsync(e);
			Bot.Client.UserUnbanned += async (_, e) => await Events.OnUserUnbannedAsync(e);
			Bot.Client.UserUpdated += async (_, e) => await Events.OnUserUpdatedAsync(e);
			// Log this to the main server only
			Bot.Client.LeftServer += async (_, e) => await Events.OnLeftServerAsync(e);
			Bot.Client.JoinedServer += async (_, e) => await Events.OnJoinedServerAsync(e);

			await Task.FromResult(0);
		}

		// Write to console
		public static void CON(object _, LogMessageEventArgs e)
			=> Console.WriteLine($"{Utils.GetLocalTime().Result} @ SOCKET : {e.Severity} : {e.Source} : {e.Message}");

		public static async Task<object> SendAsync(string message, LogColor color)
		{
			Console.ForegroundColor = (ConsoleColor)color;
			Console.WriteLine($"{await Utils.GetLocalTime()} @ ROOT : {await FormatTime(await Utils.GetUptime())}{message}");
			Console.ResetColor();
			return null;
		}

		public static async Task<object> SendAsync(string message, Exception e)
		{
			ErrorCount++;
			Console.ForegroundColor = (ConsoleColor)LogColor.Error;
			Console.WriteLine($"{await Utils.GetLocalTime()} @ ROOT : {await FormatTime(await Utils.GetUptime())}{message} : {e.Source} : {e.Message}");
			Console.ResetColor();
			return null;
		}

		// Write to channel
		public static async Task<object> SendToChannelAsync(string message, LogColor color)
		{
			await SendAsync(message, color);
			(await Utils.FindTextChannelByName(Configuration.Default.LogChannelName))?.SendMessage(await Utils.CutMessage($"**{await Utils.GetLocalTime()}**\n{message}"));
			return null;
		}

		public static async Task<object> SendToChannelAsync(string message, Exception e)
		{
			await SendAsync(message, e);
			(await Utils.FindTextChannelByName(Configuration.Default.LogChannelName))?.SendMessage(await Utils.CutMessage($"**{await Utils.GetLocalTime()} -> {message}**\n**Source** {e.Source}\n**Message** {e.Message}"));
			return null;
		}

		// Show nothing after a random time
		private static Task<string> FormatTime(TimeSpan time)
			=> Task.FromResult((time.TotalSeconds > 100)
												  ? string.Empty
												  : string.Format("{0:N2}s : ", time.TotalSeconds));
	}
}