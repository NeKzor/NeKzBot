using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NeKzBot.Server;

namespace NeKzBot
{
	public partial class Leaderboard
	{
		internal class AutoUpdater
		{
			private static Stopwatch refreshWatch;
			private static CancellationTokenSource cancelUpdateSource;
			private static CancellationToken cancelToken;
			private static string cacheKey;

			public static void Init()
			{
				refreshWatch = new Stopwatch();
				cancelUpdateSource = new CancellationTokenSource();
				cancelToken = cancelUpdateSource.Token;
				cacheKey = "autolb";
			}

			#region ACTIONS
			public static async Task Start(int serverdelay = 8000)
			{
				try
				{
					// Wait some time till bot is on server
					await Task.Delay(serverdelay);
					Logging.CON("Lb autoupdater started", ConsoleColor.DarkBlue);

					// Find channel to send to
					var channel = Utils.GetChannel(Settings.Default.UpdateChannelName);

					// Reserve cache memory
					Caching.CFile.AddKey(cacheKey);

					while (Settings.Default.AutoUpdate)
					{
						Logging.CON("Lb autoupdater checking", ConsoleColor.DarkBlue);

						// Get cache from file
						var cache = Caching.CFile.GetFile(cacheKey);

						// Download entry
						var entryUpdate = await GetLatestEntry("http://board.iverb.me/changelog" + Settings.Default.BoardParameter, true);

						// Method returns null when an error occurs, ignore it
						if (entryUpdate != null)
						{
							// Only update if new
							if (entryUpdate != cache)
							{
								// Save cache
								Logging.CON($"CACHING NEW ENTRY {Utils.StringInBytes(entryUpdate)} bytes");
								Caching.CFile.Save(cacheKey, entryUpdate);

								// Check if channel name has changed
								if (channel.Name != Settings.Default.UpdateChannelName)
									channel = Utils.GetChannel(Settings.Default.UpdateChannelName);

								// Send update
								await channel?.SendMessage(entryUpdate);
							}
						}
						// Wait then refresh
						refreshWatch?.Restart();
						await Task.Delay((int)Settings.Default.RefreshTime * 60000, cancelToken);   // In minutes
					}
				}
				catch (Exception ex)
				{
					Logging.CHA($"Lb autoupdater cancelled\n{ex.ToString()}", ConsoleColor.DarkBlue);
				}
				finally
				{
					Logging.CON("Lb autoupdater ended", ConsoleColor.DarkBlue);
				}
			}

			// Start auto updater again if it's dead, cancel it when it's alive
			public static string ToggleUpdate()
			{
				Logging.CON("Lb autoupdater requested change", ConsoleColor.DarkBlue);
				if (cancelUpdateSource.IsCancellationRequested || Start().IsCompleted)
				{
					Task.Factory.StartNew(async () => { await Start(); });
					return "Auto update started.";
				}
				cancelUpdateSource.Cancel();
				return "Auto update cancelled.";
			}

			// Cancel current wait and check for new entry now
			public static string RefreshNow()
			{
				Logging.CON("Lb autoupdater requested refresh", ConsoleColor.DarkBlue);
				if (!cancelUpdateSource.IsCancellationRequested && !Start().IsCompleted)
				{
					cancelUpdateSource.Cancel();
					Task.Factory.StartNew(async () => { await Start(); });
					return "Will refresh soon.";
				}
				return $"Refresh failed. Try `{Settings.Default.PrefixCmd + Settings.Default.LeaderboardCmd} toggleupdate`";
			}

			// Cache which you need to compare for a new check
			public static string CleanEntryCache()
			{
				var bytes = Utils.StringInBytes(Caching.CFile.Get(cacheKey));
				Caching.CFile.Save(cacheKey, string.Empty);
				Logging.CON($"CACHE SIZE CLEANED {bytes} BYTES");
				return $"Cleaned entry cache with a size of {bytes} bytes.";
			}
			#endregion

			#region SETTINGS
			// Show when the the next entry check is
			public static string GetRefreshTime()
			{
				var min = Convert.ToInt16(Settings.Default.RefreshTime) - refreshWatch.Elapsed.Minutes;
				if (min < 1)
					return "Will check soon for an update.";
				return min == 1 ? 
					"Will check in 1 minute for an update." : $"Will check in {min.ToString()} minutes for an update.";

			}

			// Set time when to refresh
			public static string SetResfreshTime(string t)
			{
				if (!Utils.ValidateString(t, "^[1-9]", 4))
					return "Invalid paramter. Use numbers from 1-9 only.";
				var time = Convert.ToInt16(t);
				if (time > 1440)
					return "Invalid value. Time is in minutes.";
				Settings.Default.RefreshTime = (uint)time;
				Settings.Default.Save();
				return $"New refresh time is set to **{t}min**";
			}

			// Set a new channel
			public static string SetUpdateChannel(string s)
			{
				if (s == Settings.Default.UpdateChannelName)
					return "Channel is already set with this name.";

				if (Utils.GetChannel(s) == null)
					return "Channel name doesn't exist on this server.";

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
				return "Invalid state. Try one of these `toggle`, `true`, `false`";
			}

			// Set board parameter after /changelog (example: ?wr=1)
			public static string SetNewBoardParameter(string s)
			{
				if (s == Settings.Default.BoardParameter)
					return "Board parameter is already set with the same value.";
				Settings.Default.BoardParameter = s;
				Settings.Default.Save();
				return s == string.Empty ?
					"Saved. Board parameter isn't set." : $"Saved. New board parameter is to **{s}** now.";
			}
			#endregion
		}
	}
}