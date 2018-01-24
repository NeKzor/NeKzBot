using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Addons.Preconditions;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using NeKzBot.Extensions;
using Portal2Boards.Net;
using Portal2Boards.Net.API.Models;
using Portal2Boards.Net.Entities;
using Portal2Boards.Net.Extensions;

namespace NeKzBot.Modules.Public
{
	public class Portal2Module : ModuleBase<SocketCommandContext>
	{
		[Group("portal2boards"), Alias("p2b", "p2")]
		public class Portal2Boards : InteractiveBase<SocketCommandContext>
		{
			private readonly IConfiguration _config;
			private readonly Portal2BoardsClient _client;

			public Portal2Boards(IConfiguration config)
			{
				_config = config;
				var http = new HttpClient();
				http.DefaultRequestHeaders.UserAgent.ParseAdd(_config["user_agent"]);
				_client = new Portal2BoardsClient(http);
			}

			[Command("?"), Alias("info", "help")]
			public Task QuestionMark()
			{
				var embed = new EmbedBuilder()
					.WithColor(Color.Blue)
					.WithDescription("[Powered by Portal2Boards.Net (v1.1)](https://github.com/NeKzor/Portal2Boards.Net)");
				
				return ReplyAndDeleteAsync(string.Empty, embed: embed.Build());
			}
			[Ratelimit(3, 1, Measure.Minutes)]
			[Command("leaderboard"), Alias("lb")]
			public async Task Leaderboard([Remainder] string mapName = null)
			{
				if (!string.IsNullOrEmpty(mapName))
				{
					var map = await Portal2.GetMapByName(mapName);
					if (map != null)
					{
						if (map.BestTimeId != null)
						{
							var board = await _client.GetLeaderboardAsync(map);

							var page = string.Empty;
							var pages = new List<string>();
							var count = 0;

							foreach (var entry in board.Take(100))
							{
								if ((count % 5 == 0) && (count != 0))
								{
									pages.Add(page);
									page = string.Empty;
								}

								page += $"\n{entry.ScoreRank.FormatRankToString()} "+
									$"{entry.Score.AsTimeToString()} " +
									$"by [{entry.Player.Name.ToRawText()}]({entry.Player.Link})";
								
								count++;
							}
							pages.Add(page);

							await PagedReplyAsync
							(
								new PaginatedMessage
								{
									Color = Color.Blue,
									Pages = pages,
									Title = $"Top 100 - {map.Alias}",
									Options = new PaginatedAppearanceOptions
									{
										DisplayInformationIcon = false,
										Timeout = TimeSpan.FromSeconds(5 * 60)
									}
								},
								false // Allow other users to control the pages too
							);
						}
						else
							await ReplyAndDeleteAsync("This map does not have a leaderboard.", timeout: TimeSpan.FromSeconds(10));
					}
					else
						await ReplyAndDeleteAsync($"Could not find a map named *{mapName.ToRawText()}*.", timeout: TimeSpan.FromSeconds(10));
				}
				else
					await ReplyAndDeleteAsync("Invalid map name.", timeout: TimeSpan.FromSeconds(10));
			}
			[Ratelimit(3, 1, Measure.Minutes)]
			[Command("changelog"), Alias("cl", "clog")]
			public async Task Changelog([Remainder] string mapName)
			{
				var map = await Portal2.GetMapByName(mapName);
				if (map == null)
				{
					await ReplyAndDeleteAsync("Invalid map name.", timeout: TimeSpan.FromSeconds(10));
					return;
				}
				if (map.BestTimeId == null)
				{
					await ReplyAndDeleteAsync("This map does not have a leaderboard.", timeout: TimeSpan.FromSeconds(10));
					return;
				}

				var changelog = await _client.GetChangelogAsync($"?chamber={map.BestTimeId}");

				var page = string.Empty;
				var pages = new List<string>();
				var count = 0;

				foreach (var entry in changelog.Take(20))
				{
					if ((count % 5 == 0) && (count != 0))
					{
						pages.Add(page);
						page = string.Empty;
					}
					
					page += $"\n{entry.Rank.Current.FormatRankToString("WR")}" +
						$" {entry.Score.Current.AsTimeToString()}" +
						((entry.Score.Improvement != default)
							? $" (-{entry.Score.Improvement.AsTimeToString()})"
							: string.Empty) +
						$" by [{entry.Player.Name.ToRawText()}]({entry.Player.Link})";
					
					count++;
				}
				pages.Add(page);

				await PagedReplyAsync
				(
					new PaginatedMessage
					{
						Color = Color.Blue,
						Pages = pages,
						Title = $"Latest 20 - {map.Alias}",
						Options = new PaginatedAppearanceOptions
						{
							DisplayInformationIcon = false,
							Timeout = TimeSpan.FromSeconds(5 * 60)
						}
					},
					false // Allow other users to control the pages too
				);
			}
			[Ratelimit(3, 1, Measure.Minutes)]
			[Command("profile"), Alias("pro", "user")]
			public async Task Profile([Remainder] string userNameOrSteamId64 = null)
			{
				var profile = default(Profile);
				if (string.IsNullOrEmpty(userNameOrSteamId64))
				{
					// Get user's linked SteamId64 when the DAPI supports this
					// This will never happen right :(?
					var nick = (Context.User as SocketGuildUser)?.Nickname;
					var name = Context.User.Username;
					if (nick != null)
						profile = await _client.GetProfileAsync(nick);
					if (profile == null)
						profile = await _client.GetProfileAsync(name);
				}
				else
				{
					profile = await _client.GetProfileAsync(userNameOrSteamId64);
				}

				if (profile != null)
				{
					var user = profile.Data.Convert();
					
					var pages = new List<string>
					{
						$"\nSP WRs | {user.Times.SinglePlayer.Chapters.WorldRecordCount}" +
							$"\nMP WRs | {user.Times.Cooperative.Chapters.WorldRecordCount}" +
							$"\nTotal | {user.Times.WorldRecordCount}",
						$"\nSP Points | {user.Points.SinglePlayer.Score.FormatPointsToString()}" +
							$"\nMP Points | {user.Points.Cooperative.Score.FormatPointsToString()}" +
							$"\nTotal | {user.Points.Global.Score.FormatPointsToString()}",
						$"\nSP Rank | {user.Times.SinglePlayer.PlayerRank.FormatRankToString()}" +
							$"\nMP Rank | {user.Times.Cooperative.PlayerRank.FormatRankToString()}" +
							$"\nOverall | {user.Times.Global.PlayerRank.FormatRankToString()}",
						$"\nSP Avg Rank | {user.Times.SinglePlayer.Chapters.AveragePlace.FormatAveragePlaceToString()}" +
							$"\nMP Avg Rank | {user.Times.Cooperative.Chapters.AveragePlace.FormatAveragePlaceToString()}" +
							$"\nOverall | {user.Times.GlobalAveragePlace.FormatAveragePlaceToString()}",
						$"\nSP Best Rank | {user.Times.SinglePlayer.Chapters.BestScore.PlayerRank.FormatRankToString()} on {user.Times.SinglePlayer.Chapters.BestScore.ParsedMap?.Alias ?? user.Times.SinglePlayer.Chapters.BestScore.MapId}" +
							$"\nMP Best Rank | {user.Times.Cooperative.Chapters.BestScore.PlayerRank.FormatRankToString()} on {user.Times.Cooperative.Chapters.BestScore.ParsedMap?.Alias ?? user.Times.Cooperative.Chapters.BestScore.MapId}",
						$"\nSP Oldest Score | {user.Times.SinglePlayer.Chapters.OldestScore.Score.AsTimeToString()} on {user.Times.SinglePlayer.Chapters.OldestScore.ParsedMap?.Alias ?? user.Times.SinglePlayer.Chapters.OldestScore.MapId}" +
							$"\nMP Oldest Score | {user.Times.Cooperative.Chapters.OldestScore.Score.AsTimeToString()} on {user.Times.Cooperative.Chapters.OldestScore.ParsedMap?.Alias ?? user.Times.Cooperative.Chapters.OldestScore.MapId}",
						$"\nSP Newest Score | {user.Times.SinglePlayer.Chapters.NewestScore.Score.AsTimeToString()} on {user.Times.SinglePlayer.Chapters.NewestScore.ParsedMap?.Alias ?? user.Times.SinglePlayer.Chapters.NewestScore.MapId}" +
							$"\nMP Newest Score | {user.Times.Cooperative.Chapters.NewestScore.Score.AsTimeToString()} on {user.Times.Cooperative.Chapters.NewestScore.ParsedMap?.Alias ?? user.Times.Cooperative.Chapters.NewestScore.MapId}"
					};

					var page = string.Empty;
					var count = 0;

					foreach (var map in Portal2.CampaignMaps)
					{
						var chamber = await Portal2.GetMapData(user.Times, map);
						if (chamber == null) continue;

						if ((count % 5 == 0) && (count != 0))
						{
							pages.Add(page);
							page = string.Empty;
						}

						page += $"\n[{map.Alias}]({map.Link}) | " +
							$"{chamber.ScoreRank.FormatRankToString("WR")} | " +
							$"{chamber.Score.AsTimeToString()}";

						count++;
					}
					pages.Add(page);

					var lastpage = $"{user.Title}\n[Steam]({user.SteamLink})";
					if (!string.IsNullOrEmpty(user.YouTubeLink))
						lastpage += $"\n[YouTube](https://youtube.com{user.YouTubeLink})";
					if (!string.IsNullOrEmpty(user.TwitchLink))
						lastpage += $"\n[Twitch](https://twitch.tv/{user.TwitchLink})";
					pages.Add(lastpage);

					await PagedReplyAsync
					(
						new PaginatedMessage
						{
							Color = Color.Blue,
							Pages = pages,
							Author = new EmbedAuthorBuilder
							{
								Name = user.DisplayName,
								IconUrl = user.SteamAvatarLink,
								Url = user.Link
							},
							Options = new PaginatedAppearanceOptions
							{
								DisplayInformationIcon = false,
								Timeout = TimeSpan.FromSeconds(5 * 60)
							}
						},
						false // Allow other users to control the pages too
					);
				}
				else
					await ReplyAndDeleteAsync("Invalid user name or id.", timeout: TimeSpan.FromSeconds(10));
			}
			[Ratelimit(3, 1, Measure.Minutes)]
			[Command("aggregated"), Alias("agg")]
			public async Task Aggregated()
			{
				var agg = await _client.GetAggregatedAsync();

				var page = string.Empty;
				var pages = new List<string>();
				var count = 0;

				foreach (var entry in agg.DataPoints.Select(e => e.Value).Take(20))
				{
					if ((count % 5 == 0) && (count != 0))
					{
						pages.Add(page);
						page = string.Empty;
					}

					page += $"\n{entry.ScoreData.Score}" +
						$"\t[{entry.UserData.BoardName}](https://board.iverb.me/profile/{entry.UserData.BoardName})";
					
					count++;
				}
				pages.Add(page);

				await PagedReplyAsync
				(
					new PaginatedMessage
					{
						Color = Color.Blue,
						Pages = pages,
						Options = new PaginatedAppearanceOptions
						{
							DisplayInformationIcon = false,
							Timeout = TimeSpan.FromSeconds(5 * 60)
						}
					},
					false // Allow other users to control the pages too
				);
			}
		}
	}
}