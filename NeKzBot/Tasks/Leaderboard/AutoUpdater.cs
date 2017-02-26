using System;
using System.Net;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using TweetSharp;
using NeKzBot.Server;
using NeKzBot.Classes;
using NeKzBot.Resources;
using NeKzBot.Internals;
using NeKzBot.Classes.Discord;
using NeKzBot.Webhooks;

namespace NeKzBot.Tasks.Leaderboard
{
	public static partial class Portal2
	{
		internal static class AutoUpdater
		{
			public static bool IsRunning { get; set; } = false;
			public static InternalWatch Watch { get; } = new InternalWatch();
			internal static TwitterService LeaderboardTwitterAccount { get; private set; }
			private static Stopwatch _refreshWatch;
			private static CancellationTokenSource _cancelUpdateSource;
			private static CancellationToken _cancelToken;
			private static string _cacheKey;
			private static uint _entryCount;

			public static async Task InitAsync()
			{
				await Logger.SendAsync("Initializing Portal2 AutoUpdater", LogColor.Init);
				LeaderboardTwitterAccount = await Twitter.Account.CreateServiceAsync(Credentials.Default.TwitterConsumerKey, Credentials.Default.TwitterConsumerSecret, Credentials.Default.TwitterAppToken, Credentials.Default.TwitterAppTokenSecret);
				_refreshWatch = new Stopwatch();
				_cancelUpdateSource = new CancellationTokenSource();
				_cancelToken = _cancelUpdateSource.Token;
				_cacheKey = "autolb";
				_entryCount = 10;
			}

			#region ACTIONS
			public static async Task StartAsync(int serverdelay = 8000)
			{
				// Wait some time till bot is on server
				await Task.Delay(serverdelay);
				await Logger.SendAsync("Portal2 AutoUpdater Started", LogColor.Leaderboard);
				IsRunning = true;

				try
				{
					// Reserve cache memory
					await Caching.CFile.AddKeyAsync(_cacheKey);

					// Twitter stuff
					await Twitter.UpdateDescriptionAsync(LeaderboardTwitterAccount, $"{Configuration.Default.TwitterDescription} #ONLINE");

					while (Configuration.Default.AutoUpdate)
					{
						await Logger.SendAsync("Portal2.AutoUpdater.StartAsync Checking", LogColor.Leaderboard);
						// Get cache from file
						var cache = await Caching.CFile.GetFileAsync(_cacheKey);
						if (cache == null)
							_entryCount = 1;

						// Download entry
						var entryUpdates = await GetEntryUpdateAsync("http://board.iverb.me/changelog" + Configuration.Default.BoardParameter, _entryCount);
						var sendUpdates = new List<Portal2EntryUpdate>();

						// Method returns null when an error occurs, ignore it
						if ((entryUpdates != null)
						&& (entryUpdates?.Count > 0))
						{
							// Find the last entry
							foreach (var update in entryUpdates)
							{
								if (cache != update.CacheFormat)
									sendUpdates.Add(update);
								else
									break;
							}

							// Send new updates
							if (sendUpdates?.Count > 0)
							{
								// Send every new entry
								sendUpdates.Reverse();
								foreach (var update in sendUpdates)
								{
									// RIP channel messages, webhooks are the future
									foreach (var item in Data.P2Subscribers)
									{
										await WebhookService.ExecuteWebhookAsync(item, new Webhook
										{
											UserName = item.UserName,
											AvatarUrl = "https://pbs.twimg.com/profile_images/822441679529635840/eqTCg0eb.jpg",
											Embeds = new Embed[] { await CreateEmbed(update.Global) }
										});
									}

									// Send it to Twitter too but make sure it's world record
									var tweet = update.TweetMessage;
									if ((tweet != string.Empty)
									&& (Configuration.Default.BoardParameter == "?wr=1"))
									{
										var send = await Twitter.SendTweetAsync(LeaderboardTwitterAccount, tweet);

										// Send player comment as reply to the sent tweet
										var reply = update.TweetCache.CommentMessage;
										if ((send?.Response.StatusCode == HttpStatusCode.OK)
										&& (reply != string.Empty))
											await Twitter.SendReplyAsync(LeaderboardTwitterAccount, reply, send.Value.Id);
									}
								}

								// Save last entry for caching
								var newcache = sendUpdates[sendUpdates.Count - 1].CacheFormat;
								await Logger.SendAsync($"Portal2.AutoUpdater.StartAsync Caching -> {await Utils.StringInBytes(newcache)} bytes", LogColor.Caching);
								await Caching.CFile.SaveCacheAsync(_cacheKey, newcache);

								// Bad joke about Twitter location
								foreach (var item in entryUpdates)
								{
									var name = item.TweetCache.Location;
									if (!(Data.TwitterLocations.Contains($"{name}'s basement")))
										Data.TwitterLocations.Add($"{name}'s basement");
								}
							}
						}
						// Update Twitter location
						await Twitter.UpdateLocationAsync(LeaderboardTwitterAccount, await Utils.RNGStringAsync(Data.TwitterLocations));

						// Wait then refresh
						_refreshWatch?.Restart();
						await Task.Delay(((int)Configuration.Default.RefreshTime * 60000) - await Watch.GetElapsedTimeAsync(message: "Portal2.AutoUpdater.StartAsync Delay Took -> "), _cancelToken);   // In minutes
						await Watch.RestartAsync();
					}
				}
				catch (Exception e)
				{
					await Logger.SendToChannelAsync("Portal2.Autoupdater.StartAsync Cancelled/Error", e);
				}
				IsRunning = false;
				await Logger.SendToChannelAsync("Portal2.Autoupdater.StartAsync Ended", LogColor.Leaderboard);
				await Twitter.UpdateDescriptionAsync(LeaderboardTwitterAccount, $"{Configuration.Default.TwitterDescription} #OFFLINE");
			}

			// Embedding <3
			private static Task<Embed> CreateEmbed(Portal2Entry wr)
			{
				return Task.FromResult(new Embed
				{
					Author = new EmbedAuthor(wr.Player.Name, $"https://board.iverb.me{wr.Player.ProfileLink}", wr.Player.SteamAvatar),
					Title = "New Portal 2 World Record",
					Url = "https://board.iverb.me/changelog?wr=1",
					Color = Data.BoardColor.RawValue,
					Image = new EmbedImage($"https://board.iverb.me/images/chambers_full/{wr.MapID}.jpg"),
					Timestamp = DateTime.UtcNow.ToString("s"),  // Close enough
					Footer = new EmbedFooter("board.iverb.me", "https://lh5.ggpht.com/uOc3iqkehwJddeJ1d1HtaAQdSAVaViqPydyRfDFN8GGU9zrTkxKA5x7YDJ_3fkJSZA=w300"),
					Fields = new EmbedField[]
					{
						new EmbedField("Map",           wr.Map, true),
						new EmbedField("Time",          wr.Time, true),
						new EmbedField("Player",        wr.Player.Name, true),
						new EmbedField("Date",          wr.Date, true),
						new EmbedField("Demo File",     wr.Demo != string.Empty ? $"[Download]({wr.Demo})" : "_Not available._", true),
						new EmbedField("Video Link",    wr.YouTube != string.Empty ? $"[Watch]({wr.YouTube})" : "_Not available._", true),
						new EmbedField("Comment",       wr.YouTube != string.Empty ? wr.Comment : "_No comment._")
					}
				});
			}

			// Start auto updater again if it's dead, cancel it when it's alive
			public static async Task<string> ToggleUpdateAsync()
			{
				await Logger.SendAsync("Portal2 Autoupdater Requested Change", LogColor.Leaderboard);
				if ((_cancelUpdateSource.IsCancellationRequested)
				|| (StartAsync().IsCompleted))
				{
					await Task.Factory.StartNew(async () => await StartAsync());
					return "Auto update started.";
				}
				_cancelUpdateSource.Cancel();
				return "Auto update cancelled.";
			}

			// Cancel current wait and check for new entry now
			public static async Task<string> RefreshNowAsync()
			{
				await Logger.SendAsync("Portal2 Autoupdater Requested Refresh", LogColor.Leaderboard);
				if (!(_cancelUpdateSource.IsCancellationRequested)
				&& !(StartAsync().IsCompleted))
				{
					_cancelUpdateSource.Cancel();
					await Task.Factory.StartNew(async () => await StartAsync());
					return "Will refresh soon.";
				}
				return $"Refresh failed. Try `{Configuration.Default.PrefixCmd + Configuration.Default.LeaderboardCmd} toggleupdate`";
			}

			// Cache which you need to compare for a new check
			public static async Task<string> CleanEntryCacheAsync()
			{
				var bytes = await Utils.StringInBytes(await Caching.CFile.GetCacheAsync(_cacheKey));
				await Caching.CFile.SaveCacheAsync(_cacheKey, string.Empty);
				await Logger.SendAsync($"Portal2.AutoUpdater.CleanEntryCacheAsync Caching -> {bytes} bytes", LogColor.Caching);
				return $"Cleaned entry cache with a size of {bytes} bytes.";
			}
			#endregion

			#region SETTINGS
			// Show when the the next entry check is
			public static Task<string> GetRefreshTime()
			{
				var min = Convert.ToInt16(Configuration.Default.RefreshTime) - _refreshWatch.Elapsed.Minutes;
				return Task.FromResult(
					(min < 1)
						 ? "Will check soon for an update."
						 : (min == 1)
								? "Will check in 1 minute for an update."
								: $"Will check in {min} minutes for an update.");
			}

			// Set time when to refresh
			public static async Task<string> SetResfreshTimeAsync(string t)
			{
				if (!(await Utils.ValidateString(t, "^[1-9]", 4)))
					return "Invalid paramter. Use numbers from 1-9 only.";
				var time = Convert.ToInt16(t);
				if (time > 1440)
					return "Invalid value. Time is in minutes.";
				Configuration.Default.RefreshTime = (uint)time;
				Configuration.Default.Save();
				return await Utils.CutMessage($"New refresh time is set to **{t}min**");
			}

			// Set the state of the updater
			public static Task<string> SetAutoUpdateState(string s)
			{
				var state = s.ToLower();
				if (state == "toggle")
				{
					Configuration.Default.AutoUpdate = !Configuration.Default.AutoUpdate;
					Configuration.Default.Save();
					return Task.FromResult($"Auto leaderboard update state is set to **{Configuration.Default.AutoUpdate}** now.");
				}
				if (state == "true")
				{
					if (Configuration.Default.AutoUpdate.ToString() == state)
						return Task.FromResult("Auto updater already enabled.");
					Configuration.Default.AutoUpdate = true;
					Configuration.Default.Save();
					return Task.FromResult("Channel will update again.");
				}
				if (state == "false")
				{
					if (Configuration.Default.AutoUpdate.ToString() == state)
						return Task.FromResult("Auto updater already disabled.");
					Configuration.Default.AutoUpdate = false;
					Configuration.Default.Save();
					return Task.FromResult("Channel won't update anymore.");
				}
				return Task.FromResult("Invalid state. Try one of these `toggle`, `true`, `false`");
			}

			// Set board parameter after /changelog (example: ?wr=1)
			public static async Task<string> SetNewBoardParameterAsync(string s)
			{
				if (s == Configuration.Default.BoardParameter)
					return "Board parameter is already set with the same value.";
				Configuration.Default.BoardParameter = s;
				Configuration.Default.Save();
				return (s == string.Empty)
						  ? "Saved. Board parameter isn't set."
						  : await Utils.CutMessage($"Saved. New board parameter is to **{s}** now.");
			}
			#endregion
		}
	}
}