using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using TweetSharp;
using NeKzBot.Classes;
using NeKzBot.Extensions;
using NeKzBot.Internals;
using NeKzBot.Resources;
using NeKzBot.Server;
using NeKzBot.Utilities;
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
			private static string _cacheKey;
			private static uint _entryCount;
			private static uint _refreshTime;

			public static async Task InitAsync()
			{
				await Logger.SendAsync("Initializing Portal2 AutoUpdater", LogColor.Init);
				LeaderboardTwitterAccount = await Twitter.Account.CreateServiceAsync(Credentials.Default.TwitterConsumerKey,
																					 Credentials.Default.TwitterConsumerSecret,
																					 Credentials.Default.TwitterAppToken,
																					 Credentials.Default.TwitterAppTokenSecret);
				_refreshWatch = new Stopwatch();
				_cacheKey = "autolb";
				_entryCount = 10;
				_refreshTime = 20 * 60 * 1000;  // 20 minutes
			}

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

					for (;;)
					{
						await Logger.SendAsync("Portal2 AutoUpdater Checking", LogColor.Leaderboard);
						// Get cache from file
						var cache = await Caching.CFile.GetFileAsync(_cacheKey);
						if (cache == null)
							_entryCount = 1;

						// Download entry
						var entryupdates = await GetEntryUpdateAsync("http://board.iverb.me/changelog" + Configuration.Default.BoardParameter, _entryCount);
						var sendupdates = new List<Portal2EntryUpdate>();

						// Method returns null when an error occurs, ignore it
						if ((entryupdates != null)
						&& (entryupdates?.Count > 0))
						{
							// Find the last entry
							foreach (var update in entryupdates)
							{
								if (cache != update.CacheFormat)
									sendupdates.Add(update);
								else
									break;
							}
							cache = null;

							// Send new updates
							if (sendupdates.Count > 0)
							{
								// Send every new entry
								sendupdates.Reverse();
								foreach (var update in sendupdates)
								{
									// RIP channel messages, webhooks are the future
									foreach (var item in (await Data.Get<Subscription>("p2hook")).Subscribers)
									{
										await WebhookService.ExecuteWebhookAsync(item, new Webhook
										{
											UserName = "Portal2Records",
											AvatarUrl = "https://pbs.twimg.com/profile_images/822441679529635840/eqTCg0eb.jpg",
											Embeds = new Embed[] { await CreateEmbedAsync(update.Entry) }
										});
									}

									// Send it to Twitter too but make sure it's world record
									var tweet = update.Tweet.Message;
									if ((tweet != string.Empty)
									&& (Configuration.Default.BoardParameter == "?wr=1"))
									{
										var send = await Twitter.SendTweetAsync(LeaderboardTwitterAccount, tweet);

										// Send player comment as reply to the sent tweet
										var reply = update.Tweet.CommentMessage;
										if ((send?.Response.StatusCode == HttpStatusCode.OK)
										&& (reply != string.Empty))
											await Twitter.SendReplyAsync(LeaderboardTwitterAccount, reply, send.Value.Id);
									}
								}

								// Save last entry for caching
								var newcache = sendupdates[sendupdates.Count - 1].CacheFormat;
								await Logger.SendAsync($"Portal2.AutoUpdater.StartAsync Caching -> {await Utils.StringInBytes(newcache)} bytes", LogColor.Caching);
								await Caching.CFile.SaveCacheAsync(_cacheKey, newcache);
								newcache = null;

								// Bad joke about Twitter location
								foreach (var item in entryupdates)
								{
									var name = item.Tweet.Location;
									if (!(Data.TwitterLocations.Contains($"{name}'s basement")))
										Data.TwitterLocations.Add($"{name}'s basement");
								}
							}
							entryupdates = null;
							sendupdates = null;
						}
						// Update Twitter location
						await Twitter.UpdateLocationAsync(LeaderboardTwitterAccount, await Utils.RngStringAsync(Data.TwitterLocations));

						// Wait then refresh
						_refreshWatch?.Restart();
						var delay = (int)(_refreshTime) - await Watch.GetElapsedTime(debugmsg: "Portal2.AutoUpdater.StartAsync Delay Took -> ");
						await Task.Delay((delay > 0) ? delay : 0);	// I don't think this will ever happen but who knows...
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

			// Set board parameter after /changelog (example: ?wr=1)
			public static async Task<string> SetNewBoardParameterAsync(string s)
			{
				if (s == Configuration.Default.BoardParameter)
					return "Board parameter is already set with the same value.";
				Configuration.Default.BoardParameter = s;
				Configuration.Default.Save();
				return (s == string.Empty)
						  ? "Saved. Board parameter isn't set."
						  : await Utils.CutMessageAsync($"Saved. New board parameter is to **{s}** now.", badchars: false);
			}

			// Embedding <3
			private static async Task<Embed> CreateEmbedAsync(Portal2Entry wr)
			{
				var embed = new Embed
				{
					Author = new EmbedAuthor(wr.Player.Name, wr.Player.SteamLink, wr.Player.SteamAvatar),
					Title = "New Portal 2 World Record",
					Url = "https://board.iverb.me/changelog?wr=1",
					Color = Data.BoardColor.RawValue,
					Image = new EmbedImage(wr.ImageLink),
					Timestamp = DateTime.UtcNow.ToString("s"),  // Close enough
					Footer = new EmbedFooter("board.iverb.me", Data.Portal2IconUrl),
					Fields = new EmbedField[]
					{
						new EmbedField("Map", wr.Map, true),
						new EmbedField("Time", wr.Time, true),
						new EmbedField("Player", await Utils.AsRawText(wr.Player.Name), true),
						new EmbedField("Date", wr.Date, true)
					}
				};

				if ((wr.Demo != string.Empty)
				|| (wr.YouTube != string.Empty))
				{
					embed.AddField(field =>
					{
						field.Name = "Demo File";
						field.Value = (wr.Demo != string.Empty) ? $"[Download]({wr.Demo})" : "_Not available._";
						field.Inline = true;
					});
					embed.AddField(field =>
					{
						field.Name = "Video Link";
						field.Value = (wr.YouTube != string.Empty) ? $"[Watch]({wr.YouTube})" : "_Not available._";
						field.Inline = true;
					});
				}
				if (wr.Comment != string.Empty)
				{
					embed.AddField(async field =>
					{
						field.Name = "Comment";
						field.Value = await Utils.AsRawText(wr.Comment);
					});
				}
				return embed;
			}
		}
	}
}