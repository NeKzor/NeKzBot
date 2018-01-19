using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.Webhook;
using LiteDB;
using Microsoft.Extensions.Configuration;
using NeKzBot.Data;
using NeKzBot.Extensions;
using Portal2Boards.Net;
using Portal2Boards.Net.API;
using Portal2Boards.Net.Entities;
using Portal2Boards.Net.Extensions;

namespace NeKzBot.Services.Notifications
{
	public class Portal2NotificationService : NotificationService
	{
		private Portal2BoardsClient _client;

		public Portal2NotificationService(IConfiguration config, LiteDatabase dataBase)
			: base(config, dataBase)
		{
		}

		public override Task Initialize()
		{
			_ = base.Initialize();

			_userName = "Portal2Boards";
			_userAvatar = "https://raw.githubusercontent.com/NeKzor/NeKzBot/master/public/resources/avatars/portal2boards_avatar.jpg";
			_sleepTime = 5 * 60 * 1000;

			// API client to board.iverb.me
			var http = new HttpClient();
			http.DefaultRequestHeaders.UserAgent.ParseAdd(_config["user_agent"]);
			_client = new Portal2BoardsClient
			(
				new ChangelogParameters
				{
					[Parameters.WorldRecord] = 1,
					// Worst case I've found was 45
					[Parameters.MaxDaysAgo] = 52
				},
				http,	// Set user agent
				false	// Disable auto caching
			);

			// Insert new cache if it doesn't exist yet
			var db = GetTaskCache<Portal2CacheData>()
				.GetAwaiter()
				.GetResult();
			
			var cache = db
				.FindAll()
				.FirstOrDefault();
			
			if (cache == null)
			{
				_ = LogWarning("Creating new cache");
				cache = new Portal2CacheData();
				db.Insert(cache);
			}
			return Task.CompletedTask;
		}

		public override async Task StartAsync()
		{
			try
			{
				await base.StartAsync();

				while (_isRunning)
				{
					await LogInfo("Checking...");

					var watch = Stopwatch.StartNew();

					var db = await GetTaskCache<Portal2CacheData>();
					var cache = db
						.FindAll()
						.FirstOrDefault();
					
					if (cache == null)
						throw new Exception("Task cache not found!");
					
					var clog = await _client.GetChangelogAsync();
					var entries = clog.Where(e => !e.IsBanned);
					var sending = new List<EntryData>();

					// Will skip for the very first time
					if (cache.EntryIds.Any())
					{
						// Check cached entries
						foreach (var id in cache.EntryIds)
						{
							foreach (var entry in entries)
							{
								if (id >= entry.Id)
									goto send;
								sending.Add(entry);
							}
						}
						throw new Exception("Could not find the cached entry in new changelog!");
					}
				send:
					
					await LogInfo($"Found {sending.Count} new entries");
					await LogInfo($"Cache: {cache.EntryIds.Count()} (ID = {cache.Id})");

					if (sending.Count > 0)
					{
						if (sending.Count < 11)
						{
							var subscribers = (await GetSubscribers())
								.FindAll()
								.ToList();

							await LogInfo($"{subscribers.Count} subs found");

							if (subscribers.Count > 0)
							{
								await LogInfo("Sending hooks");

								sending.Reverse();
								var deletion = new List<SubscriptionData>();

								foreach (var entry in sending)
								{
									var delta = await GetWorldRecordDelta(entry) ?? -1;
									var feature = (delta != default)
										? $" (-{delta.ToString("N2")})"
										: string.Empty;
									
									var embed = await CreateEmbed(entry, feature);

									foreach (var sub in subscribers)
									{
										if (deletion.Contains(sub)) continue;

										try
										{
											using (var wc = new DiscordWebhookClient(sub.WebhookId, sub.WebhookToken))
											{
												await wc.SendMessageAsync
												(
													string.Empty,
													embeds: new Embed[] { embed },
													username: _userName,
													avatarUrl: _userAvatar
													/* ,options: new RequestOptions()
													{
														RetryMode = RetryMode.RetryRatelimit
													} */
												);
											}
										}
										// Make sure to catch only on this special exception
										// which tells us that this webhook doesn't exist
										catch (InvalidOperationException ex)
											when (ex.Message == "Could not find a webhook for the supplied credentials.")
										{
											deletion.Add(sub);
											await LogWarning($"Sub ID = {sub.Id} not found");
										}
									}
								}
								await AutoDeleteAsync(deletion);
							}
						}
						else
							throw new Exception("Webhook rate limit exceeded!");
					}

					// Cache
					cache.EntryIds = entries
						.Select(e => e.Id)
						.Take(11);
					
					if (!db.Update(cache))
						throw new Exception("Failed to update cache!");

					// Sleep
					var delay = (int)(_sleepTime - watch.ElapsedMilliseconds);
					if (delay < 0)
						throw new Exception($"Task took too long ({delay}ms)");
					
					await Task.Delay(delay, _cancellation.Token);
				}
			}
			catch (Exception ex)
			{
				await LogException(ex);
			}

			await LogWarning("Task ended");
		}

		private Task<Embed> CreateEmbed(EntryData wr, string feature)
		{
			var embed = new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					Name = wr.Player.Name,
					Url = wr.Player.Link,
					IconUrl = wr.Player.SteamAvatarLink
				},
				Title = "New Portal 2 World Record",
				Url = "https://board.iverb.me/changelog?wr=1",
				Color = new Color(4, 128, 165),
				ImageUrl = wr.Map.ImageLinkFull,
				Timestamp = DateTime.UtcNow,
				Footer = new EmbedFooterBuilder
				{
					Text = "board.iverb.me",
					IconUrl = "https://raw.githubusercontent.com/NeKzor/NeKzBot/master/public/resources/icons/portal2boards_icon.png"
				}
			};
			embed.AddField("Map", wr.Map.Name, true);
			// WR delta time, a feature which the leaderboard doesn't have :>
			embed.AddField("Time", wr.Score.Current.AsTimeToString() + feature, true);
			embed.AddField("Player", wr.Player.Name.ToRawText(), true);
			embed.AddField("Date", wr.Date?.DateTimeToString(), true);
			if ((wr.DemoExists) || (wr.VideoExists))
			{
				embed.AddField("Demo File", (wr.DemoExists) ? $"[Download]({wr.DemoLink})" : "_Not available._", true);
				embed.AddField("Video Link", (wr.VideoExists) ? $"[Watch]({wr.VideoLink})" : "_Not available._", true);
			}
			if (wr.CommentExists)
				embed.AddField("Comment", wr.Comment.ToRawText());
			return Task.FromResult(embed.Build());
		}
		// My head always hurts when I look at this...
		private async Task<float?> GetWorldRecordDelta(EntryData wr)
		{
			var map = await Portal2.GetMapByName(wr.Map.Name);
			var found = false;
			var foundcoop = false;

			foreach (var entry in await _client.GetChangelogAsync($"?wr=1&chamber={map.BestTimeId}"))
			{
				if (entry.IsBanned)
					continue;

				if (found)
				{
					var oldwr = entry.Score.Current.AsTime();
					var newwr = wr.Score.Current.AsTime();
					if (map.Type == MapType.Cooperative)
					{
						if (foundcoop)
						{
							if (oldwr == newwr)
								return 0;
							if (newwr < oldwr)
								return oldwr - newwr;
						}
						// Tie or partner score
						else if (oldwr == newwr)
						{
							// Cooperative world record without a partner
							// will be ignored, sadly that's a thing :>
							foundcoop = true;
							continue;
						}
						else if (newwr < oldwr)
						{
							return oldwr - newwr;
						}
					}
					else if (map.Type == MapType.SinglePlayer)
					{
						if (oldwr == newwr)
							return 0;
						if (newwr < oldwr)
							return oldwr - newwr;
					}
					break;
				}
				if (entry.Id == wr.Id)
					found = true;
			}

			// Warning
			_ = LogWarning("Could not calculate the world record delta");
			return default;
		}
	}
}