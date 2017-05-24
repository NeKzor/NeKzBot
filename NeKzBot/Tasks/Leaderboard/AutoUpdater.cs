using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Portal2Boards.Net;
using Portal2Boards.Net.API;
using Portal2Boards.Net.Entities;
using Portal2Boards.Net.Extensions;
using TweetSharp;
using NeKzBot.Classes;
using NeKzBot.Extensions;
using NeKzBot.Internals;
using NeKzBot.Internals.Entities;
using NeKzBot.Resources;
using NeKzBot.Server;
using NeKzBot.Utilities;
using NeKzBot.Webhooks;

namespace NeKzBot.Tasks.Leaderboard
{
	public static partial class Portal2Board
	{
		internal static class AutoUpdater
		{
			public static bool IsRunning { get; set; } = false;
			public static InternalWatch Watch { get; } = new InternalWatch();
			internal static TwitterService LeaderboardTwitterAccount { get; private set; }
			private static Stopwatch _refreshWatch;
			private static string _cacheKey;
			private static uint _refreshTime;
			private static uint _rateLimit;
			private static ChangelogParameters _latestWorldRecords { get; set; }
			private static Portal2BoardsClient _client { get; set; }

			public static async Task InitAsync()
			{
				await Logger.SendAsync("Initializing Portal2Board AutoUpdater", LogColor.Init);
				LeaderboardTwitterAccount = await Twitter.Account.CreateServiceAsync(Credentials.Default.TwitterConsumerKey,
																					 Credentials.Default.TwitterConsumerSecret,
																					 Credentials.Default.TwitterAppToken,
																					 Credentials.Default.TwitterAppTokenSecret);
				_refreshWatch = new Stopwatch();
				_cacheKey = "autolb";
				_refreshTime = 5 * 60 * 1000;  // 5 minutes
				_rateLimit = 10;				// ^Max 10 wrs in this time

				// My library :)
				_latestWorldRecords = new ChangelogParameters { [Parameters.WorldRecord] = 1 };
				_client = new Portal2BoardsClient(_latestWorldRecords, await new Fetcher().GetClient());
			}

			public static async Task StartAsync(int serverdelay = 8000)
			{
				// Wait some time till bot is on server
				await Task.Delay(serverdelay);
				await Logger.SendAsync("Portal2Board AutoUpdater Started", LogColor.Leaderboard);
				IsRunning = true;

				try
				{
					// Reserve cache memory
					await Caching.CFile.AddKeyAsync(_cacheKey);

					// Twitter stuff
					await Twitter.UpdateDescriptionAsync(LeaderboardTwitterAccount, $"{Configuration.Default.TwitterDescription} #ONLINE");

					for (;;)
					{
						// Get cache from file
						var cache = await Caching.CFile.GetFileAsync(_cacheKey);

						// Download changelog
						var entryupdates = await _client.GetChangelogAsync();
						if (entryupdates != null)
						{
							var sendupdates = new List<EntryData>();
							if (!(string.IsNullOrEmpty(cache)))
							{
								// Find the last entry
								foreach (var update in entryupdates)
								{
									// Cache is now the entry id
									if (cache != $"{update.Id}")
										sendupdates.Add(update);
									else
										break;
								}
								cache = null;
							}
							else
								sendupdates.Add(entryupdates.First());

							// Rate limit for sending webhooks and tweets
							if (sendupdates.Count <= _rateLimit)
							{
								// Send every new entry
								sendupdates.Reverse();
								foreach (var update in sendupdates)
								{
									// Inject a nice feature which the leaderboard doesn't have
									var delta = await GetWorldRecordDelta(update) ?? -1;
									var wrdelta = (delta != -1)
															? $" (-{delta.ToString("N2")})"
															: string.Empty;

									// RIP channel messages, webhooks are the future
									foreach (var item in (await Data.Get<Subscription>("p2hook")).Subscribers)
									{
										await WebhookService.ExecuteWebhookAsync(item, new Webhook
										{
											UserName = "Portal2Records",
											AvatarUrl = "https://pbs.twimg.com/profile_images/822441679529635840/eqTCg0eb.jpg",
											Embeds = new Embed[] { await CreateEmbedAsync(update, wrdelta) }
										});
									}

									// Send it to Twitter too but make sure it's world record
									var tweet = await FormatMainTweetAsync($"New World Record in {update.Map.Name}\n" +
																		   $"{update.Score.Current.AsTime()}{wrdelta} by {update.Player.Name}\n" +
																		   $"{update.Date?.ToUniversalTime().ToString("yyyy-MM-dd hh:mm:ss")} (UTC)", update.DemoLink, $"https://youtu.be/{update.YouTubeId}");
									if ((tweet != string.Empty)
									&& (_client.Parameters[Parameters.WorldRecord] == 1 as object))
									{
										var send = await Twitter.SendTweetAsync(LeaderboardTwitterAccount, tweet);

										// Send player comment as reply to the sent tweet
										var reply = await FormatReplyTweetAsync(update.Player.Name, update.Comment);
										if ((send?.Response.StatusCode == HttpStatusCode.OK)
										&& (reply != string.Empty))
											await Twitter.SendReplyAsync(LeaderboardTwitterAccount, reply, send.Value.Id);
									}
								}

								if (sendupdates.Count >= 1)
								{
									// Save last entry for caching
									var last = sendupdates.Last();
									var newcache = $"{last.Id}";
									await Logger.SendAsync($"Portal2Board.AutoUpdater.StartAsync Caching -> {await Utils.StringInBytes(newcache)} bytes", LogColor.Caching);
									await Caching.CFile.SaveCacheAsync(_cacheKey, newcache);

									// Bad joke about Twitter location
									foreach (var item in entryupdates)
									{
										if (!(Data.TwitterLocations.Contains($"{last.Player.Name}'s basement")))
											Data.TwitterLocations.Add($"{last.Player.Name}'s basement");
									}
									newcache = null;
									last = null;
								}
							}
							else
								await Logger.SendAsync("Portal2Board.AutoUpdater.StartAsync Rate Limit Exceeded", LogColor.Error);

							entryupdates = null;
							sendupdates = null;
						}
						// Wait then refresh
						_refreshWatch?.Restart();
						var delay = (int)(_refreshTime) - await Watch.GetElapsedTime(debugmsg: "Portal2Board.AutoUpdater.StartAsync Delay Took -> ");
						await Task.Delay((delay > 0) ? delay : 0);	// I don't think this will ever happen but who knows...
						await Watch.RestartAsync();
					}
				}
				catch (Exception e)
				{
					await Logger.SendToChannelAsync("Portal2Board.Autoupdater.StartAsync Cancelled/Error", e);
				}
				IsRunning = false;
				await Logger.SendToChannelAsync("Portal2Board.Autoupdater.StartAsync Ended", LogColor.Leaderboard);
				await Twitter.UpdateDescriptionAsync(LeaderboardTwitterAccount, $"{Configuration.Default.TwitterDescription} #OFFLINE");
			}

			private static async Task<float?> GetWorldRecordDelta(EntryData wr)
			{
				// Don't allow this for cooperative wrs because:
				// 1.) Partner entries do not always match their date (<- the actual issue)
				// 2.) One of the world record holder can tie it again with another partner
				var map = await Portal2.GetMapByName(wr.Map.Name);
				if (map.Type == MapType.SinglePlayer)
				{
					var found = false;
					foreach (var entry in await _client.GetChangelogAsync($"?wr=1&chamber={map.BestTimeId}"))
					{
						if (found)
						{
							var oldwr = entry.Score.Current.AsTime();
							var newwr = wr.Score.Current.AsTime();
							if (oldwr == newwr)
								return 0;
							if (newwr < oldwr)
								return oldwr - newwr;
							break;
						}

						// Search current wr, then take the next one
						if (entry.Date == wr.Date)  // This will do it for now, single player only
							found = true;
					}
				}
				return default(float?);
			}

			// Embedding <3
			private static async Task<Embed> CreateEmbedAsync(EntryData wr, string feature)
			{
				var embed = new Embed
				{
					Author = new EmbedAuthor(wr.Player.Name, wr.Player.Link, wr.Player.SteamAvatarLink),
					Title = "New Portal 2 World Record",
					Url = "https://board.iverb.me/changelog?wr=1",
					Color = Data.BoardColor.RawValue,
					Image = new EmbedImage(wr.Map.ImageLinkFull),
					Timestamp = DateTime.UtcNow.ToString("s"),  // Close enough
					Footer = new EmbedFooter("board.iverb.me", Data.Portal2IconUrl),
					Fields = new EmbedField[]
					{
						new EmbedField("Map", wr.Map.Name, true),
						new EmbedField("Time", $"{wr.Score.Current.AsTimeToString()}{feature}", true),
						new EmbedField("Player", await Utils.AsRawText(wr.Player.Name), true),
						new EmbedField("Date", wr.Date?.DateTimeToString() + " UTC", true)
					}
				};

				if ((wr.DemoExists)
				|| (wr.VideoExists))
				{
					embed.AddField(field =>
					{
						field.Name = "Demo File";
						field.Value = (wr.DemoExists) ? $"[Download]({wr.DemoLink})" : "_Not available._";
						field.Inline = true;
					});
					embed.AddField(field =>
					{
						field.Name = "Video Link";
						field.Value = (wr.VideoExists) ? $"[Watch]({wr.VideoLink})" : "_Not available._";
						field.Inline = true;
					});
				}
				if (wr.CommentExists)
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