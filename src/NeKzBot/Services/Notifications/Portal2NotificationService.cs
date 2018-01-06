using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
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
			base.Initialize();

			UserName = "Portal2Boards";
			UserAvatar = "https://raw.githubusercontent.com/NeKzor/NeKzBot/master/public/resources/avatars/portal2boards_avatar.jpg";
			SleepTime = 5 * 60 * 1000;

			var http = new HttpClient();
			http.DefaultRequestHeaders.UserAgent.ParseAdd(_config["user_agent"]);
			var parameters = new ChangelogParameters
			{
				[Parameters.WorldRecord] = 1,
				[Parameters.MaxDaysAgo] = 31	// Increase this if needed
			};
			_client = new Portal2BoardsClient(parameters, http, false);

			var data = _dataBase.GetCollection<Portal2CacheData>();
			var cache = data.FindOne(d => d.Identifier == GlobalId);
			if (cache == null)
			{
				cache = new Portal2CacheData()
				{
					Identifier = GlobalId,
					Entries = new List<EntryData>()
				};
				data.Insert(cache);
			}
			return Task.CompletedTask;
		}

		// Notification tasks
		public override async Task StartAsync()
		{
			try
			{
				while (!Cancel)
				{
					var watch = Stopwatch.StartNew();

					var db = _dataBase.GetCollection<Portal2CacheData>();
					var data = db.FindOne(d => d.Identifier == GlobalId);
					if (data == null)
						throw new Exception("Data cache not found!");

					var cache = data.Entries;
					var entries = (await _client.GetChangelogAsync()).Where(x => !x.IsBanned);
					var sending = new List<EntryData>();

					// Will skip for the very first time
					if (cache.Any())
					{
						// Check other cached entries too if the first one wasn't found (old score could be deleted/banned etc.)
						foreach (var old in cache)
						{
							foreach (var entry in entries)
							{
								if (old.Id >= entry.Id)
									goto send;
								sending.Add(entry);
							}
						}
						throw new Exception("Could not find the cached entry in new changelog!");
					}
send:
					var subscribers = _dataBase.GetCollection<SubscriptionData>(GlobalId);
					if (subscribers.Count() > 0)
					{
						if (sending.Count <= 10)
						{
							sending.Reverse();
							foreach (var tosend in sending)
							{
#if DEBUG
								var watch2 = Stopwatch.StartNew();
#endif
								var feature = await GetWorldRecordDelta(tosend) ?? -1;
#if DEBUG
								watch2.Stop();
								Console.WriteLine($"{nameof(GetWorldRecordDelta)} took: {watch2.ElapsedMilliseconds}ms");
#endif
								var payload = (feature != default) ? $" (-{feature.ToString("N2")})" : string.Empty;
								var embed = await CreateEmbed(tosend, payload);

								// There might be a problem if we have lots of subscribers (retry then?)
								foreach (var subscriber in subscribers.FindAll())
								{
									using (var vc = new DiscordWebhookClient(subscriber.WebhookId, subscriber.WebhookToken))
									{
										await vc.SendMessageAsync("", embeds: new Embed[] { embed });
									}
								}
							}
						}
						else
							throw new Exception("Webhook rate limit exceeded!");
					}

					// Cache
					data.Entries = entries.Take(11);
					if (!db.Update(data))
						throw new Exception("Failed to update cache!");

					// Sleep
					var delay = (int)(SleepTime - watch.ElapsedMilliseconds);
					if (delay < 0)
						throw new Exception($"Task took too long ({delay}ms)");
					await Task.Delay(delay);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[{nameof(Portal2NotificationService)}] Exception:\n" + ex);
			}
		}

		private Task<Embed> CreateEmbed(EntryData wr, string payload)
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
			// "Inject" a nice feature which the leaderboard doesn't have :v
			embed.AddField("Time", wr.Score.Current.AsTimeToString() + payload, true);
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
		// My head still hurts when I look at this...
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
						else if (oldwr == newwr)
						{
							foundcoop = true;
							continue;
						}
						else
						{
							if (newwr < oldwr)
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
			Console.WriteLine($"[{nameof(Portal2NotificationService)}] Could not calculate the wr delta!");
			return default;
		}
	}
}