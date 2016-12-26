using System;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NeKzBot.Properties;

namespace NeKzBot
{
	public class AutoUpdater
	{
		private static Stopwatch refreshWatch;
		private static CancellationTokenSource cancelUpdateSource;
		private static CancellationToken cancelToken;

		#region ACTIONS
		// Timer
		public static async Task AutoUpdate(int serverdelay = 8000)
		{
			try
			{
				Logging.CON("Auto updating", ConsoleColor.DarkBlue);

				cancelUpdateSource = new CancellationTokenSource();
				cancelToken = cancelUpdateSource.Token;

				// Wait some time till bot is on server
				await Task.Delay(serverdelay);

				// Find channel to send to
				var channel = GetChannelByName();

				while (Settings.Default.AutoUpdate)
				{
					Logging.CON("Checking for entry", ConsoleColor.DarkBlue);

					// Download entry
					var entryUpdate = Leaderboard.GetLatestEntry("http://board.iverb.me/changelog" + Settings.Default.BoardParameter, true);

					// Only update if new
					if (entryUpdate != Settings.Default.EntryCache)
					{
						Logging.CON($"CACHING NEW ENTRY {Utils.StringInBytes(entryUpdate)} bytes");
						Settings.Default.EntryCache = entryUpdate;
						Settings.Default.Save();

						// Check if channel name has changed
						if (channel.Name != Settings.Default.UpdateChannelName)
							channel = GetChannelByName();

						await channel.SendMessage(entryUpdate);
					}
					// Wait then refresh
					refreshWatch = new Stopwatch();
					refreshWatch.Start();
					Logging.CON("AutoUpdate is now sleeping", ConsoleColor.DarkBlue);
					await Task.Delay((int)Settings.Default.RefreshTime * 60000, cancelToken);   // In minutes
				}
			}
			catch
			{
				Logging.CON("AutoUpdate cancelled", ConsoleColor.DarkBlue);
			}
			finally
			{
				Logging.CON("AutoUpdate ended", ConsoleColor.DarkBlue);
			}
		}

		// Start auto updater again if it's dead, cancel it when it's alive
		public static string ToggleUpdate()
		{
			Logging.CON("Requested AutoUpdate change", ConsoleColor.DarkBlue);
			if (cancelUpdateSource.IsCancellationRequested || AutoUpdate().IsCompleted)
			{
				Task.Factory.StartNew(async () =>
				{
					await AutoUpdate();
				});
				return "Auto update started.";
			}
			cancelUpdateSource.Cancel();
			return "Auto update cancelled.";
		}

		// Cancel current wait and check for new entry now
		public static string RefreshNow()
		{
			Logging.CON("Requested new refresh", ConsoleColor.DarkBlue);
			if (!cancelUpdateSource.IsCancellationRequested && !AutoUpdate().IsCompleted)
			{
				cancelUpdateSource.Cancel();
				Task.Factory.StartNew(async () =>
				{
					await AutoUpdate();
				});
				return "Will refresh soon.";
			}
			return $"Refresh failed. Try: `{Settings.Default.PrefixCmd + Settings.Default.LeaderboardCmd} toggleupdate`";
		}

		// Cache which you need to compare for a new check
		public static string CleanEntryCache()
		{
			var bytes = Utils.StringInBytes(Settings.Default.EntryCache);
			Settings.Default.EntryCache = string.Empty;
			Settings.Default.Save();
			Logging.CON($"CACHE SIZE CLEANED {bytes} BYTES");
			return $"Cleaned entry cache with a size of {bytes} bytes.";
		}
		#endregion

		#region SETTINGS
		// Show when the the next entry check is
		public static string GetRefreshTime()
		{
			int min = Convert.ToInt16(Settings.Default.RefreshTime) - refreshWatch.Elapsed.Minutes;
			if (min < 1)
				return "Will check soon for an update.";
			if (min == 1)
				return "Will check in 1 minute for an update.";
			return "Will check in " + min.ToString() + " minutes for an update.";

		}

		// Set time when to refresh
		public static string SetResfreshTime(string t)
		{
			if (!Utils.ValidateString(t, "^[0-9]", 4))
				return "Invalid paramter.";
			int time = Convert.ToInt16(t);
			if (time < 1 || time > 1440)
				return "Invalid paramter. Time is in minutes.";
			Settings.Default.RefreshTime = (uint)time;
			Settings.Default.Save();
			return "New refresh time is set to **" + t + "min**";
		}

		// Set a new channel
		public static string SetUpdateChannel(string s)
		{
			if (s == Settings.Default.UpdateChannelName)
				return "Channel is already set with this name.";

			try
			{
				var newChannel = NBot.dClient.FindServers(Settings.Default.ServerName).First().FindChannels(s, Discord.ChannelType.Text, true).First();
			}
			catch
			{
				return "Channel name doesn't exist on this server.";
			}

			Logging.CON("New channel name set", ConsoleColor.DarkBlue);
			Settings.Default.UpdateChannelName = s;
			Settings.Default.Save();
			return $"Auto updates will be send to **{s}** now.";
		}

		// Set the state of the updater
		public static string SetAutoUpdateState(string s)
		{
			var state = s.ToLower();
			if (state == "toggle")
			{
				Settings.Default.AutoUpdate = !Settings.Default.AutoUpdate;
				Settings.Default.Save();
				return $"Auto leaderboard update state is set to **{Settings.Default.AutoUpdate.ToString()}** now.";
			}
			if (state == "true")
			{
				if (Settings.Default.AutoUpdate.ToString() == state)
					return "Auto updater already enabled.";
				Settings.Default.AutoUpdate = true;
				Settings.Default.Save();
				return "Channel will update again.";
			}
			if (state == "false")
			{
				if (Settings.Default.AutoUpdate.ToString() == state)
					return "Auto updater already disabled.";
				Settings.Default.AutoUpdate = false;
				Settings.Default.Save();
				return "Channel won't update anymore.";
			}
			return "Invalid state.\nTry one of these: `toggle`, `true`, `false`";
		}

		// Set board parameter after /changelog (example: ?wr=1)
		public static string SetNewBoardParameter(string s)
		{
			if (s == Settings.Default.BoardParameter)
				return "Board parameter is already set with the same value.";
			Settings.Default.BoardParameter = s;
			Settings.Default.Save();
			return s == string.Empty ? "Saved. Board parameter isn't set." : $"Saved. New board parameter is to **{s}** now.";
		}
		#endregion

		private static Discord.Channel GetChannelByName(string serverName = null, string channelName = null)
		{
			serverName = serverName ?? Settings.Default.ServerName;
			channelName = channelName ?? Settings.Default.UpdateChannelName;
			return NBot.dClient.FindServers(serverName).First().FindChannels(channelName, Discord.ChannelType.Text, true).First();
		}
	}
}