using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NeKzBot.Server;
using NeKzBot.Resources;

namespace NeKzBot.Modules.Leaderboard
{
	public partial class Leaderboard
	{
		internal class AutoUpdater
		{
			public static bool isRunning = false;
			private static Stopwatch refreshWatch;
			private static CancellationTokenSource cancelUpdateSource;
			private static CancellationToken cancelToken;
			private static string cacheKey;

			public static Task Init()
			{
				refreshWatch = new Stopwatch();
				cancelUpdateSource = new CancellationTokenSource();
				cancelToken = cancelUpdateSource.Token;
				cacheKey = "autolb";
				return Task.FromResult(0);
			}

			#region ACTIONS
			public static async Task Start(int serverdelay = 8000)
			{
				// Wait some time till bot is on server
				await Task.Delay(serverdelay);
				await Logging.CON("Lb autoupdater started", ConsoleColor.DarkBlue);
				isRunning = true;

				try
				{
					// Find channel to send to
					var channel = await Utils.GetChannel(Settings.Default.UpdateChannelName);

					// Reserve cache memory
					await Caching.CFile.AddKey(cacheKey);

					// Twitter stuff
					var service = Twitter.Twitter.Account.CreateService(Credentials.Default.TwitterConsumerKey, Credentials.Default.TwitterConsumerSecret, Credentials.Default.TwitterAppToken, Credentials.Default.TwitterAppTokenSecret);

					while (Settings.Default.AutoUpdate)
					{
						await Logging.CON("Lb autoupdater checking", ConsoleColor.DarkBlue);

						// Get cache from file
						var cache = await Caching.CFile.GetFile(cacheKey);

						// Download entry
						var entryUpdate = await GetEntryUpdate("http://board.iverb.me/changelog" + Settings.Default.BoardParameter);

						// Method returns null when an error occurs, ignore it
						if (entryUpdate != null)
						{
							var message = entryUpdate.Item1;
							var tweet = entryUpdate.Item2;
							var newcache = entryUpdate.Item3;

							// Only update if new
							if (cache != newcache)
							{
								// Save cache
								await Logging.CON($"Leaderboard AutoUpdater entry cache -> {Utils.StringInBytes(newcache)} bytes", ConsoleColor.Red);
								await Caching.CFile.Save(cacheKey, newcache);

								// Check if channel name has changed
								if (channel.Name != Settings.Default.UpdateChannelName)
									channel = await Utils.GetChannel(Settings.Default.UpdateChannelName);

								// Send update to Discord channel
								await channel?.SendMessage(message);

								// Send it to Twitter too but make sure it's world record
								if (tweet != string.Empty && Settings.Default.BoardParameter == "?wr=1")
									await Twitter.Twitter.SendTweet(service, entryUpdate.Item2);
							}
						}
						// Wait then refresh
						refreshWatch?.Restart();
						await Task.Delay((int)Settings.Default.RefreshTime * 60000, cancelToken);   // In minutes
					}
				}
				catch (Exception ex)
				{
					await Logging.CHA($"Lb autoupdater cancelled\n{ex.ToString()}", ConsoleColor.DarkBlue);
				}
				isRunning = false;
			}

			// Start auto updater again if it's dead, cancel it when it's alive
			public static async Task<string> ToggleUpdate()
			{
				await Logging.CON("Lb autoupdater requested change", ConsoleColor.DarkBlue);
				if (cancelUpdateSource.IsCancellationRequested || Start().IsCompleted)
				{
					await Task.Factory.StartNew(async () => { await Start(); });
					return "Auto update started.";
				}
				cancelUpdateSource.Cancel();
				return "Auto update cancelled.";
			}

			// Cancel current wait and check for new entry now
			public static async Task<string> RefreshNow()
			{
				await Logging.CON("Lb autoupdater requested refresh", ConsoleColor.DarkBlue);
				if (!cancelUpdateSource.IsCancellationRequested && !Start().IsCompleted)
				{
					cancelUpdateSource.Cancel();
					await Task.Factory.StartNew(async () => { await Start(); });
					return "Will refresh soon.";
				}
				return $"Refresh failed. Try `{Settings.Default.PrefixCmd + Settings.Default.LeaderboardCmd} toggleupdate`";
			}

			// Cache which you need to compare for a new check
			public static async Task<string> CleanEntryCache()
			{
				var bytes = Utils.StringInBytes(await Caching.CFile.Get(cacheKey));
				await Caching.CFile.Save(cacheKey, string.Empty);
				await Logging.CON($"Cleaned leaderboard cache -> {bytes} bytes", ConsoleColor.Red);
				return $"Cleaned entry cache with a size of {bytes} bytes.";
			}
			#endregion

			#region SETTINGS
			// Show when the the next entry check is
			public static Task<string> GetRefreshTime()
			{
				var min = Convert.ToInt16(Settings.Default.RefreshTime) - refreshWatch.Elapsed.Minutes;
				if (min < 1)
					return Task.FromResult("Will check soon for an update.");
				return Task.FromResult(min == 1 ?  "Will check in 1 minute for an update." : $"Will check in {min.ToString()} minutes for an update.");
			}

			// Set time when to refresh
			public static Task<string> SetResfreshTime(string t)
			{
				if (!Utils.ValidateString(t, "^[1-9]", 4))
					return Task.FromResult("Invalid paramter. Use numbers from 1-9 only.");
				var time = Convert.ToInt16(t);
				if (time > 1440)
					return Task.FromResult("Invalid value. Time is in minutes.");
				Settings.Default.RefreshTime = (uint)time;
				Settings.Default.Save();
				return Task.FromResult(Utils.CutMessage($"New refresh time is set to **{t}min**"));
			}

			// Set a new channel
			public static async Task<string> SetUpdateChannel(string s)
			{
				if (s == Settings.Default.UpdateChannelName)
					return "Channel is already set with this name.";

				if (Utils.GetChannel(s) == null)
					return "Channel name doesn't exist on this server.";

				await Logging.CON("New channel name set", ConsoleColor.DarkBlue);
				Settings.Default.UpdateChannelName = s;
				Settings.Default.Save();
				return $"Auto updates will be send to **{s}** now.";
			}

			// Set the state of the updater
			public static Task<string> SetAutoUpdateState(string s)
			{
				var state = s.ToLower();
				if (state == "toggle")
				{
					Settings.Default.AutoUpdate = !Settings.Default.AutoUpdate;
					Settings.Default.Save();
					return Task.FromResult($"Auto leaderboard update state is set to **{Settings.Default.AutoUpdate.ToString()}** now.");
				}
				if (state == "true")
				{
					if (Settings.Default.AutoUpdate.ToString() == state)
						return Task.FromResult("Auto updater already enabled.");
					Settings.Default.AutoUpdate = true;
					Settings.Default.Save();
					return Task.FromResult("Channel will update again.");
				}
				if (state == "false")
				{
					if (Settings.Default.AutoUpdate.ToString() == state)
						return Task.FromResult("Auto updater already disabled.");
					Settings.Default.AutoUpdate = false;
					Settings.Default.Save();
					return Task.FromResult("Channel won't update anymore.");
				}
				return Task.FromResult("Invalid state. Try one of these `toggle`, `true`, `false`");
			}

			// Set board parameter after /changelog (example: ?wr=1)
			public static Task<string> SetNewBoardParameter(string s)
			{
				if (s == Settings.Default.BoardParameter)
					return Task.FromResult("Board parameter is already set with the same value.");
				Settings.Default.BoardParameter = s;
				Settings.Default.Save();
				return Task.FromResult(s == string.Empty ? "Saved. Board parameter isn't set." : Utils.CutMessage($"Saved. New board parameter is to **{s}** now."));
			}
			#endregion
		}
	}
}