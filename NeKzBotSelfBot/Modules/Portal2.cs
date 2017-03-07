using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using NeKzBot.Classes;
using NeKzBot.Resources;
using NeKzBot.Tasks;

namespace NeKzBot.Modules
{
	public class Portal2 : ModuleBase
	{
		// Single entry
		[Command("p2wr")]
		public async Task GetWorldRecord([Remainder]string map = "")
		{
			var url = default(string);
			if (map != string.Empty)
			{
				if (!(await Utils.SearchInClassByName(Data.Portal2Maps, typeof(Portal2Map), "Name", map, out var index)))
					if (!(await Utils.SearchInClassByName(Data.Portal2Maps, typeof(Portal2Map), "ChallengeModeName", map, out index)))
						if (!(await Utils.SearchInClassByName(Data.Portal2Maps, typeof(Portal2Map), "ThreeLetterCode", map, out index)))
							return;
				url = $"https://board.iverb.me/changelog?chamber={Data.Portal2Maps[index].BestTimeId}&wr=1";
			}
			else
				url = "https://board.iverb.me/changelog?wr=1";

			var entry = await Leaderboard.GetLatestEntryAsync(url);
			if (entry == null)
				return;

			var embed = new EmbedBuilder
			{
				Color = Data.BoardColor,
				Title = "Portal 2 World Record",
				Url = url,
				ImageUrl = $"https://board.iverb.me/images/chambers_full/{entry.MapId}.jpg",
				Footer = new EmbedFooterBuilder
				{
					Text = "board.iverb.me",
					IconUrl = "https://lh5.ggpht.com/uOc3iqkehwJddeJ1d1HtaAQdSAVaViqPydyRfDFN8GGU9zrTkxKA5x7YDJ_3fkJSZA=w300"
				}
			}
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Map";
				field.Value = entry.Map;
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Player";
				field.Value = entry.Player;
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Time";
				field.Value = entry.Time;
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Date";
				field.Value = $"{entry.Date} UTC";
			})
			.AddField(async field =>
			{
				field.Name = "Duration";
				var duration = await Utils.GetDuration(DateTime.ParseExact(entry.Date, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
				field.Value = (duration != string.Empty)
										? duration
										: "Unknown.";
			});

			if ((entry.Demo != string.Empty)
			|| (entry.YouTube != string.Empty))
			{
				embed.AddField(field =>
				{
					field.Name = "Links";
					var output = string.Empty;
					if (entry.Demo != string.Empty)
						output += $"[Demo Download]({entry.Demo})";
					if (entry.YouTube != string.Empty)
						output += $"{(entry.Demo != string.Empty ? "\n" : string.Empty)}[YouTube Video]({entry.YouTube})";
					field.Value = output;
				});
			}
			if (entry.Comment != string.Empty)
			{
				embed.AddField(field =>
				{
					field.Name = "Comment";
					field.Value = entry.Comment;
				});
			}
			await Message.EditAsync(Context.Message, embed);
		}

		[Command("entry")]
		public async Task GetLatestMapEntry([Remainder]string map)
		{
			if (!(await Utils.SearchInClassByName(Data.Portal2Maps, typeof(Portal2Map), "Name", map, out var index)))
				if (!(await Utils.SearchInClassByName(Data.Portal2Maps, typeof(Portal2Map), "ChallengeModeName", map, out index)))
					if (!(await Utils.SearchInClassByName(Data.Portal2Maps, typeof(Portal2Map), "ThreeLetterCode", map, out index)))
						return;
			await GetLatestMapEntry($"https://board.iverb.me/changelog?chamber={Data.Portal2Maps[index].BestTimeId}", Context.Message);
		}

		[Command("entry")]
		public async Task GetEntryInfoFromUri(Uri link)
			=> await GetLatestMapEntry(link.AbsoluteUri, Context.Message);

		[Command("playerentry"), Alias("eplayer")]
		public async Task GetLatestPlayerEntry(string player)
			=> await GetLatestMapEntry($"https://board.iverb.me/changelog?boardName={player}", Context.Message);

		// Player statistics
		[Command("player")]
		public async Task GetPlayerStats(string player)
			=> await GetPlayerStats($"https://board.iverb.me/profile/{player}", Context.Message);

		[Command("profile")]
		public async Task GetPlayerInfoFromUri(Uri link)
			=> await GetPlayerStats(link.AbsoluteUri, Context.Message);

		[Command("compare"), Alias("vs")]
		public async Task GetStatsOfPlayers(params string[] profiles)
		{
			if ((profiles == null)
			|| (profiles?.Length < 2)
			|| (profiles?.Length > 8))
				return;

			var users = new List<Portal2User>();
			foreach (var profile in profiles)
				users.Add(await Leaderboard.GetUserStatsAsync($"https://board.iverb.me/profile/{profile}"));

			var description = string.Empty;
			foreach (var user in users)
				if (user != null)
					description += user.PlayerName + " vs ";
			description = description.Substring(0, description.Length - " vs ".Length);

			var embed = new EmbedBuilder
			{
				Color = Data.BoardColor,
				Title = "Portal 2 Player Comparison",
				Description = description,
				Url = "https://board.iverb.me/changelog",
				Footer = new EmbedFooterBuilder
				{
					Text = "board.iverb.me",
					IconUrl = "https://lh5.ggpht.com/uOc3iqkehwJddeJ1d1HtaAQdSAVaViqPydyRfDFN8GGU9zrTkxKA5x7YDJ_3fkJSZA=w300"
				}
			};

			foreach (var user in users)
			{
				if (users != null)
				{
					embed.AddField(field =>
					{
						field.IsInline = true;
						field.Name = "Rank";
						field.Value = $"Single Player • {user.SinglePlayerRank}\nCooperative • {user.CooperativeRank}\nOverall • {user.OverallRank}";
					})
					.AddField(field =>
					{
						field.IsInline = true;
						field.Name = "Points";
						field.Value = $"Single Player • {user.SinglePlayerPoints}\nCooperative • {user.CooperativePoints}\nOverall • {user.OverallPoints}";
					})
					.AddField(field =>
					{
						field.IsInline = true;
						field.Name = "World Records";
						if (user.SinglePlayerRank != "0")
							field.Value = $"Single Player • {user.SinglePlayerWorldRecords}\nCooperative • {user.CooperativeWorldRecords}\nOverall • {user.OverallWorldRecords}";
						else
							field.Value = "None.";
					});
				}
			}
			await Message.EditAsync(Context.Message, embed);
		}

		// Multiple entries
		[Command("leaderboard"), Alias("lb")]
		public async Task GetMapLeaderboard(int top, [Remainder]string map)
		{
			if (!(await Utils.SearchInClassByName(Data.Portal2Maps, typeof(Portal2Map), "Name", map, out var index)))
				if (!(await Utils.SearchInClassByName(Data.Portal2Maps, typeof(Portal2Map), "ChallengeModeName", map, out index)))
					if (!(await Utils.SearchInClassByName(Data.Portal2Maps, typeof(Portal2Map), "ThreeLetterCode", map, out index)))
						return;

			var id = Data.Portal2Maps[index].BestTimeId;
			var url = $"https://board.iverb.me/chamber/{id}";
			var leaderboard = await Leaderboard.GetMapEntriesAsync(url, 0, top);
			if (leaderboard == null)
				return;

			await Message.EditAsync(Context.Message, new EmbedBuilder
			{
				Color = Data.BoardColor,
				Title = $"Portal 2 Top {top}",
				Url = url,
				ImageUrl = $"https://board.iverb.me/images/chambers_full/{id}.jpg",
				Footer = new EmbedFooterBuilder
				{
					Text = "board.iverb.me",
					IconUrl = "https://lh5.ggpht.com/uOc3iqkehwJddeJ1d1HtaAQdSAVaViqPydyRfDFN8GGU9zrTkxKA5x7YDJ_3fkJSZA=w300"
				}
			}
			.AddField(field =>
			{
				field.Name = leaderboard.MapName;
				var output = string.Empty;
				foreach (var item in leaderboard.Entries)
					output += $"\n{item.Ranking} {item.Time} by {item.Player.Replace("_", "\\_")} ({item.Date} UTC)";
				field.Value = (output != string.Empty)
									  ? output
									  : "Data not found.";
			}));
		}

		// Others
		[Command("preview"), Alias("overview", "view")]
		public async Task GetMapPreview([Remainder]string name = "")
		{
			var map = default(Portal2Map);
			if (name != string.Empty)
			{
				if (!(await Utils.SearchInClassByName(Data.Portal2Maps, typeof(Portal2Map), "Name", name, out var index)))
					if (!(await Utils.SearchInClassByName(Data.Portal2Maps, typeof(Portal2Map), "ChallengeModeName", name, out index)))
						if (!(await Utils.SearchInClassByName(Data.Portal2Maps, typeof(Portal2Map), "ThreeLetterCode", name, out index)))
							return;
				map = Data.Portal2Maps[index];
			}
			else
				map = Data.Portal2Maps[await Utils.RNGInt(Data.Portal2Maps)];

			await Message.EditAsync(Context.Message, new EmbedBuilder
			{
				Color = Data.BoardColor,
				Title = "Portal 2 Map Preview",
				Description = $"{map.Name} • {map.ChallengeModeName}",
				Url = $"https://board.iverb.me/chambers/{map.BestTimeId}",
				ImageUrl = $"https://board.iverb.me/images/chambers_full/{map.BestTimeId}.jpg",
				Footer = new EmbedFooterBuilder
				{
					Text = "board.iverb.me",
					IconUrl = "https://lh5.ggpht.com/uOc3iqkehwJddeJ1d1HtaAQdSAVaViqPydyRfDFN8GGU9zrTkxKA5x7YDJ_3fkJSZA=w300"
				}
			});
		}

		[Command("elevator"), Alias("dialogue", "dialog", "timing")]
		public async Task GetElevatorTiming([Remainder]string name)
		{
			if (!(await Utils.SearchInClassByName(Data.Portal2Maps, typeof(Portal2Map), "Name", name, out var index)))
				if (!(await Utils.SearchInClassByName(Data.Portal2Maps, typeof(Portal2Map), "ChallengeModeName", name, out index)))
					if (!(await Utils.SearchInClassByName(Data.Portal2Maps, typeof(Portal2Map), "ThreeLetterCode", name, out index)))
						return;

			var map = Data.Portal2Maps[index];
			await Message.EditAsync(Context.Message, new EmbedBuilder
			{
				Color = Data.BoardColor,
				Title = "Portal 2 Elevator Timing",
				Description = map.Name,
				Url = "https://speedrun.com/Portal_2",
				ImageUrl = $"https://board.iverb.me/images/chambers_full/{map.BestTimeId}.jpg",
				Footer = new EmbedFooterBuilder
				{
					Text = "board.iverb.me",
					IconUrl = "https://lh5.ggpht.com/uOc3iqkehwJddeJ1d1HtaAQdSAVaViqPydyRfDFN8GGU9zrTkxKA5x7YDJ_3fkJSZA=w300"
				}
			}
			.AddField(field =>
			{
				field.Name = map.ChallengeModeName;
				field.Value = map.ElevatorTiming;
			}));
		}

		// Removing redundancy
		private async Task GetLatestMapEntry(string url, IUserMessage message)
		{
			var entry = await Leaderboard.GetLatestEntryAsync(url);
			if (entry == null)
				return;

			var embed = new EmbedBuilder
			{
				Color = Data.BoardColor,
				Title = "Portal 2 Entry",
				Url = url,
				ImageUrl = $"https://board.iverb.me/images/chambers_full/{entry.MapId}.jpg",
				Footer = new EmbedFooterBuilder
				{
					Text = "board.iverb.me",
					IconUrl = "https://lh5.ggpht.com/uOc3iqkehwJddeJ1d1HtaAQdSAVaViqPydyRfDFN8GGU9zrTkxKA5x7YDJ_3fkJSZA=w300"
				}
			}
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Map";
				field.Value = entry.Map;
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Player";
				field.Value = entry.Player;
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Time";
				field.Value = entry.Time;
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Date";
				field.Value = $"{entry.Date} UTC";
			})
			.AddField(async field =>
			{
				field.Name = "Duration";
				var duration = await Utils.GetDuration(DateTime.ParseExact(entry.Date, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
				field.Value = (duration != string.Empty)
										? duration
										: "Unknown.";
			});

			if ((entry.Demo != string.Empty)
			|| (entry.YouTube != string.Empty))
			{
				embed.AddField(field =>
				{
					field.Name = "Links";
					var output = string.Empty;
					if (entry.Demo != string.Empty)
						output += $"[Demo Download]({entry.Demo})";
					if (entry.YouTube != string.Empty)
						output += $"{(entry.Demo != string.Empty ? "\n" : string.Empty)}[YouTube Video]({entry.YouTube})";
					field.Value = output;
				});
			}
			if (entry.Comment != string.Empty)
			{
				embed.AddField(field =>
				{
					field.Name = "Comment";
					field.Value = entry.Comment;
				});
			}
			await Message.EditAsync(message, embed);
		}

		private async Task GetPlayerStats(string url, IUserMessage message)
		{
			var profile = await Leaderboard.GetUserStatsAsync(url);
			if (profile == null)
				return;

			await Message.EditAsync(message, new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					Name = profile.PlayerName,
					Url = profile.PlayerSteamLink,
					IconUrl = profile.PlayerSteamAvatar
				},
				Color = Data.BoardColor,
				Title = "Portal 2 Profile",
				Url = url,
				Footer = new EmbedFooterBuilder
				{
					Text = "board.iverb.me",
					IconUrl = "https://lh5.ggpht.com/uOc3iqkehwJddeJ1d1HtaAQdSAVaViqPydyRfDFN8GGU9zrTkxKA5x7YDJ_3fkJSZA=w300"
				}
			}
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Rank";
				field.Value = $"Single Player • {profile.SinglePlayerRank}\nCooperative • {profile.CooperativeRank}\nOverall • {profile.OverallRank}";
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Points";
				field.Value = $"Single Player • {profile.SinglePlayerPoints}\nCooperative • {profile.CooperativePoints}\nOverall • {profile.OverallPoints}";
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Best Rank";
				field.Value = $"{profile.BestPlaceRank} on {profile.BestPlaceMap}";
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Average Rank";
				field.Value = $"Single Player • {profile.AverageSinglePlayerRank}\nCooperative • {profile.AverageCooperativeRank}\nOverall • {profile.AverageOverallRank}";
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "World Records";
				field.Value = (profile.SinglePlayerRank != "0")
														? $"Single Player • {profile.SinglePlayerWorldRecords}\nCooperative • {profile.CooperativeWorldRecords}\nOverall • {profile.OverallWorldRecords}"
														: "None.";
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Worst Rank";
				field.Value = $"{profile.WorstPlaceRank} on {profile.WorstPlaceMap}";
			}));
		}
	}
}