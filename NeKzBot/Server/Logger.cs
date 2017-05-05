using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using NeKzBot.Utilities;

namespace NeKzBot.Server
{
	public static class Logger
	{
		public static List<string> Errors = new List<string>();

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
			// Log this to console only
#if DEBUG
			Bot.Client.ServerAvailable += async (_, e) => await Events.OnServerAvailableAsync(e);
#endif
			Bot.Client.ServerUnavailable += async (_, e) => await Events.OnServerUnavailableAsync(e);
			Bot.Client.Ready += async (_, e) => await Events.OnReadyAsync(e);

			await Task.FromResult(0);
		}

		// Write to console
		public static void CON(object _, LogMessageEventArgs e)
			=> Console.WriteLine($"[{Utils.GetLocalTimeAsync().GetAwaiter().GetResult()}] : " +
								 $"{FormatTime(Utils.GetUptime().GetAwaiter().GetResult()).GetAwaiter().GetResult()}" +
								 $"[SOCKET] : [{e.Severity}] : [{e.Source}] : {e.Message}");

		public static async Task<object> SendAsync(string message, LogColor color = LogColor.Default)
		{
			Console.ForegroundColor = (ConsoleColor)color;
			Console.WriteLine($"[{await Utils.GetLocalTimeAsync()}] : {await FormatTime(await Utils.GetUptime())}[SERVER] : {await GetSource(color)} : {message}");
			Console.ResetColor();
			return default(object);
		}

		public static async Task<object> SendAsync(string message, Exception e)
		{
			Errors.Add(message);
			Console.ForegroundColor = (ConsoleColor)LogColor.Error;
			Console.WriteLine($"[{await Utils.GetLocalTimeAsync()}] : {await FormatTime(await Utils.GetUptime())}[SERVER] : [Error] : {message}\n" +
							  $"Source: {e.Source}\n" +
							  $"Message: {e.Message}");
			Console.ResetColor();
			return default(object);
		}

		// Write to channel
		public static async Task<object> SendToChannelAsync(string message, LogColor color)
		{
			await SendAsync(message, color);
			(await Utils.FindTextChannel(Configuration.Default.LogChannelName))?.SendMessage($"**{await Utils.GetLocalTimeAsync()}**\n{message}");
			return default(object);
		}

		public static async Task<object> SendToChannelAsync(string message, Exception e)
		{
			await SendAsync(message, e);
			(await Utils.FindTextChannel(Configuration.Default.LogChannelName))?.SendMessage($"**{await Utils.GetLocalTimeAsync()} -> {message}**\n**Source** {e.Source}\n**Message** {e.Message}");
			return default(object);
		}

		// Show nothing after a random time
		private static Task<string> FormatTime(TimeSpan time)
			=> Task.FromResult((time.TotalSeconds > 100)
												  ? string.Empty
												  : $"[{string.Format("{0:N2}s", time.TotalSeconds)}] : ");

		private static Task<string> GetSource(LogColor color)
		{
			var source = "[Error]";
			switch (color)
			{
				case LogColor.Audio:
					source = "[Audio]";
					break;
				case LogColor.Caching:
					source = "[Caching]";
					break;
				case LogColor.Default:
				case LogColor.Init:
					source = "[Info]";
					break;
				case LogColor.Dropbox:
					source = "[Dropbox]";
					break;
				case LogColor.Giveaway:
					source = "[Giveaway]";
					break;
				case LogColor.Leaderboard:
					source = "[Leaderboard]";
					break;
				case LogColor.Speedrun:
					source = "[Speedrun]";
					break;
				case LogColor.Twitch:
					source = "[Twitch]";
					break;
				case LogColor.Twitter:
					source = "[Twitter]";
					break;
				case LogColor.Watch:
					source = "[Watch]";
					break;
			}
			return Task.FromResult(source);
		}
	}
}