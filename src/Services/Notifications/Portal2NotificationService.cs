using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using LiteDB;
using Microsoft.Extensions.Configuration;
using NeKzBot.Data;
using NeKzBot.Extensions;
using Portal2Boards.Net;
using Portal2Boards.Net.API;
using Portal2Boards.Net.Entities;
using Portal2Boards.Net.Extensions;

namespace NeKzBot.Services.Notifciations
{
	public class Portal2NotificationService : NotificationService
	{
		public IConfiguration Config { get; set; }
		public LiteDatabase DataBase { get; set; }

		private Portal2BoardsClient _client { get; }
		private ChangelogParameters _parameters { get; }

		public Portal2NotificationService()
		{
			UserName = "Portal2Records";
			UserAvatar = "https://github.com/NeKzor/NeKzBot/blob/old/NeKzBot/Resources/Public/portal2records_webhookavatar.jpg";
			SleepTime = 5 * 60 * 1000;

			var http = new HttpClient();
			http.DefaultRequestHeaders.UserAgent.ParseAdd(Config["user_agent"]);
			var parameters = new ChangelogParameters
			{
				[Parameters.WorldRecord] = 1,
				[Parameters.MaxDaysAgo] = 14,
			};
			_client = new Portal2BoardsClient(parameters, http, false);
		}

		public override async Task StartAsync()
		{
			try
			{
				while (!Cancel)
				{
					var watch = Stopwatch.StartNew();

					var data = DataBase.GetCollection<Portal2NotificationCache>();
					var cache = data.FindAll().FirstOrDefault();

					var entries = await _client.GetChangelogAsync();
					var tosend = new List<EntryData>();

					if (cache != default(Portal2NotificationCache))
					{
						foreach (var entry in entries)
						{
							if (cache.Id >= entry.Id)
								break;
							tosend.Add(entry);
						}
						cache.Id = default(uint);
					}
					else
						tosend.Add(entries.First());

					// Rate limit, just to be safe
					if (tosend.Count <= 10)
					{
						tosend.Reverse();
						foreach (var send in tosend)
						{
							// "Inject" a nice feature which the leaderboard doesn't have
							var delta = await GetWorldRecordDelta(send) ?? -1;
							var inject = (delta != -1) ? $" (-{delta.ToString("N2")})" : string.Empty;

							if (Subscribers.Count > 0)
							{
								var embed = await CreateEmbed(send, inject);
								foreach (var subscriber in Subscribers.Select(s => s.Value))
									await subscriber.SendMessageAsync("", embeds: new Embed[] { embed });
							}

							// TODO: Send to Twitter again?
						}
					}
					else
						Console.WriteLine("[Portal2NotificationService] Webhook rate limit exceeded!");

					var delay = (int)(SleepTime - watch.ElapsedMilliseconds);
					await Task.Delay((delay > 0) ? delay : 0);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("[Portal2NotificationService] Exception:\n" + ex);
			}
		}

		public override Task StopAsync()
		{
			return Task.CompletedTask;
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
				Color = new Color(0, 0, 0),
				ImageUrl = wr.Map.ImageLinkFull,
				Timestamp = DateTime.UtcNow,
				Footer = new EmbedFooterBuilder
				{
					Text = "board.iverb.me",
					IconUrl = "https://raw.githubusercontent.com/NeKzor/NeKzBot/old/NeKzBot/Resources/Public/portal2records_icon.png"
				}
			};
			embed.AddField("Map", wr.Map.Name, true);
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
							if (oldwr == newwr)
								return 0;
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
			return default(float?);
		}
	}
}