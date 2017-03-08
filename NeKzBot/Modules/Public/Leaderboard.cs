using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using NeKzBot.Classes;
using NeKzBot.Extensions;
using NeKzBot.Resources;
using NeKzBot.Server;
using NeKzBot.Tasks.Leaderboard;

namespace NeKzBot.Modules.Public
{
	public class Leaderboard : Commands
	{
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Leaderboard Module", LogColor.Init);
			await GetLatestWorldRecord("latestwr");
			await GetCurrentWorldRecord("wr");
			await GetOwnRank("rank");
			await GetUserRank("player");
			await GetLatestLeaderboardEntry("latestentry");
			await GetPlayerComparison("compare");
			await LeaderboardCommands(Configuration.Default.LeaderboardCmd);
		}

		private static Task GetLatestWorldRecord(string c)
		{
			CService.CreateCommand(c)
					.Alias("wrupdate")
					.Description($"Gives you the most recent world record. Try `{Configuration.Default.PrefixCmd + c} yt` to filter wrs by videos only or `{Configuration.Default.PrefixCmd + c} demo` to filter them by demos only.")
					.Parameter("filter", ParameterType.Optional)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						var filter = e.GetArg("filter");
						var url = default(string);
						if (filter == string.Empty)
							url = "http://board.iverb.me/changelog?wr=1";
						else if (filter == "yt")
							url = "http://board.iverb.me/changelog?wr=1&yt=1";
						else if (filter == "demo")
							url = "http://board.iverb.me/changelog?wr=1&demo=1";
						else
						{
							await e.Channel.SendMessage($"Unknown parameter. Try `{Configuration.Default.PrefixCmd + c} yt` or `{Configuration.Default.PrefixCmd + c} demo`.");
							return;
						}

						var entry = await Portal2.GetLatestEntryAsync(url);
						if (entry != null)
						{
							var embed = new Embed
							{
								Author = new EmbedAuthor(entry.Player.Name, entry.Player.SteamLink, entry.Player.SteamAvatar),
								Color = Data.BoardColor.RawValue,
								Title = "Portal 2 World Record",
								Url = url,
								Image = new EmbedImage($"https://board.iverb.me/images/chambers_full/{entry.MapId}.jpg"),
								Footer = new EmbedFooter("board.iverb.me", Data.Portal2IconUrl),
								Fields = new EmbedField[]
								{
									new EmbedField("Map", entry.Map, true),
									new EmbedField("Player", entry.Player.Name, true),
									new EmbedField("Time", entry.Time, true),
									new EmbedField("Date", entry.Date, true),
									new EmbedField("Duration", await Utils.GetDuration(entry.DateTime))
								}
							};

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
							await Bot.SendAsync(CustomRequest.SendMessage(e.Channel.Id), new CustomMessage(embed));
						}
						else
							await e.Channel.SendMessage("Couldn't parse entry.");
					});
			return Task.FromResult(0);
		}

		private static Task GetLatestLeaderboardEntry(string c)
		{
			CService.CreateCommand(c)
					.Alias("entry")
					.Description($"Gives you the most recent leaderboard entry. Try `{Configuration.Default.PrefixCmd + c} yt` to filter entries by videos only or `{Configuration.Default.PrefixCmd + c} demo` to filter by demos only.")
					.Parameter("filter", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						var filter = e.GetArg("filter");
						var url = default(string);
						if (filter == string.Empty)
							url = "http://board.iverb.me/changelog";
						else if (filter == "yt")
							url = "http://board.iverb.me/changelog?yt=1";
						else if (filter == "demo")
							url = "http://board.iverb.me/changelog?demo=1";
						else
						{
							await e.Channel.SendMessage($"Unknown parameter. Try `{Configuration.Default.PrefixCmd + c} yt` or `{Configuration.Default.PrefixCmd + c} demo`.");
							return;
						}

						var entry = await Portal2.GetLatestEntryAsync(url);
						if (entry != null)
						{
							var embed = new Embed
							{
								Author = new EmbedAuthor(entry.Player.Name, entry.Player.SteamLink, entry.Player.SteamAvatar),
								Color = Data.BoardColor.RawValue,
								Title = "Portal 2 Entry",
								Url = url,
								Image = new EmbedImage($"https://board.iverb.me/images/chambers_full/{entry.MapId}.jpg"),
								Footer = new EmbedFooter("board.iverb.me", Data.Portal2IconUrl),
								Fields = new EmbedField[]
								{
									new EmbedField("Map", entry.Map, true),
									new EmbedField("Rank", entry.Ranking, true),
									new EmbedField("Time", entry.Time, true),
									new EmbedField("Date", entry.Date, true),
									new EmbedField("Duration", await Utils.GetDuration(entry.DateTime))
								}
							};

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
							await Bot.SendAsync(CustomRequest.SendMessage(e.Channel.Id), new CustomMessage(embed));
						}
						else
							await e.Channel.SendMessage("Couldn't parse entry.");
					});
			return Task.FromResult(0);
		}

		private static Task GetCurrentWorldRecord(string c)
		{
			CService.CreateCommand(c)
					.Alias("currentwr", "p2wr")
					.Description($"Shows you the latest world record of a map. Try `{Configuration.Default.PrefixCmd + c}` to show a random wr entry.")
					.Parameter("mapname", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						const string url = "http://board.iverb.me/changelog?wr=1&chamber=";
						var entry = default(Portal2Entry);
						if (e.GetArg("mapname") == string.Empty)
							entry = await Portal2.GetLatestEntryAsync($"{url}{await Utils.RngAsync(Data.Portal2Maps, 0)}");
						else if (await Utils.SearchArray(Data.Portal2Maps, 2, e.GetArg("mapname"), out var index))
							entry = await Portal2.GetLatestEntryAsync($"{url}{Data.Portal2Maps[index, 0]}");
						else if (await Utils.SearchArray(Data.Portal2Maps, 3, e.GetArg("mapname"), out index))
							entry = await Portal2.GetLatestEntryAsync($"{url}{Data.Portal2Maps[index, 0]}");
						else if (await Utils.SearchArray(Data.Portal2Maps, 5, e.GetArg("mapname"), out index))
							entry = await Portal2.GetLatestEntryAsync($"{url}{Data.Portal2Maps[index, 0]}");
						else
						{
							await e.Channel.SendMessage("Couldn't find that map.");
							return;
						}

						if (entry != null)
						{
							var embed = new Embed
							{
								Author = new EmbedAuthor(entry.Player.Name, entry.Player.SteamLink, entry.Player.SteamAvatar),
								Color = Data.BoardColor.RawValue,
								Title = "Portal 2 World Record",
								Url = url,
								Image = new EmbedImage($"https://board.iverb.me/images/chambers_full/{entry.MapId}.jpg"),
								Footer = new EmbedFooter("board.iverb.me", Data.Portal2IconUrl),
								Fields = new EmbedField[]
								{
									new EmbedField("Map", entry.Map, true),
									new EmbedField("Player", entry.Player.Name, true),
									new EmbedField("Time", entry.Time, true),
									new EmbedField("Date", entry.Date, true),
									new EmbedField("Duration", await Utils.GetDuration(entry.DateTime))
								}
							};

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
							await Bot.SendAsync(CustomRequest.SendMessage(e.Channel.Id), new CustomMessage(embed));
						}
						else
							await e.Channel.SendMessage("Couldn't parse entry.");
					});
			return Task.FromResult(0);
		}

		private static Task GetOwnRank(string c)
		{
			CService.CreateCommand(c)
					.Alias("me", "pb")
					.Description($"Shows your leaderboard stats. Try `{Configuration.Default.PrefixCmd + c} <mapname>` to show your personal best of a map.")
					.Parameter("mapname", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						var url = $"http://board.iverb.me/profile/{e.User.Name.Trim()}";
						var profile = default(Portal2User);
						var entry = default(Portal2Entry);

						if (e.GetArg("mapname") == string.Empty)
						{
							profile = await Portal2.GetUserStatsAsync(url);
							if ((profile == null)
							&& (e.User.Nickname != null))
								profile = await Portal2.GetUserStatsAsync(url = $"http://board.iverb.me/profile/{e.User.Nickname.Trim()}");

							if (profile != null)
							{
								await Bot.SendAsync(CustomRequest.SendMessage(e.Channel.Id), new CustomMessage(new Embed
								{
									Author = new EmbedAuthor(profile.Name, profile.SteamLink, profile.SteamAvatar),
									Color = Data.BoardColor.RawValue,
									Title = "Portal 2 Profile",
									Url = url,
									Footer = new EmbedFooter("board.iverb.me", Data.Portal2IconUrl),
									Fields = new EmbedField[]
									{
										new EmbedField("Rank", $"Single Player • {profile.SinglePlayerRank}\nCooperative • {profile.CooperativeRank}\nOverall • {profile.OverallRank}", true),
										new EmbedField("Points", $"Single Player • {profile.SinglePlayerPoints}\nCooperative • {profile.CooperativePoints}\nOverall • {profile.OverallPoints}", true),
										new EmbedField("Best Rank", $"{profile.BestPlaceRank} on {profile.BestPlaceMap}", true),
										new EmbedField("Average Rank", $"Single Player • {profile.AverageSinglePlayerRank}\nCooperative • {profile.AverageCooperativeRank}\nOverall • {profile.AverageOverallRank}", true),
										new EmbedField("World Records", (profile.SinglePlayerRank != "0")
																								  ? $"Single Player • {profile.SinglePlayerWorldRecords}\nCooperative • {profile.CooperativeWorldRecords}\nOverall • {profile.OverallWorldRecords}"
																								  : "None.", true),
										new EmbedField("Worst Rank", $"{profile.WorstPlaceRank} on {profile.WorstPlaceMap}", true)
									}
								}));
							}
							else
								await e.Channel.SendMessage("Couldn't parse player's profile.");
						}
						else
						{
							if (await Utils.SearchArray(Data.Portal2Maps, 2, e.GetArg("mapname"), out var index))
								entry = await Portal2.GetUserRankAsync(url, index);
							else if (await Utils.SearchArray(Data.Portal2Maps, 3, e.GetArg("mapname"), out index))
								entry = await Portal2.GetUserRankAsync(url, index);
							else if (await Utils.SearchArray(Data.Portal2Maps, 5, e.GetArg("mapname"), out index))
								entry = await Portal2.GetUserRankAsync(url, index);
							else
							{
								await e.Channel.SendMessage("Couldn't find that map.");
								return;
							}

							if (entry != null)
							{
								var embed = new Embed
								{
									Author = new EmbedAuthor(entry.Player.Name, entry.Player.SteamLink, entry.Player.SteamAvatar),
									Color = Data.BoardColor.RawValue,
									Title = "Personal Record",
									Url = url,
									Image = new EmbedImage($"https://board.iverb.me/images/chambers_full/{entry.MapId}.jpg"),
									Footer = new EmbedFooter("board.iverb.me", Data.Portal2IconUrl),
									Fields = new EmbedField[]
									{
										new EmbedField("Map", entry.Map, true),
										new EmbedField("Rank", entry.Ranking, true),
										new EmbedField("Time", entry.Time, true),
										new EmbedField("Date", entry.Date, true),
										new EmbedField("Duration", await Utils.GetDuration(entry.DateTime))
									}
								};

								if ((entry.Demo != string.Empty)
								|| (entry.Demo != string.Empty))
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
								await Bot.SendAsync(CustomRequest.SendMessage(e.Channel.Id), new CustomMessage(embed));
							}
							else
								await e.Channel.SendMessage("Couldn't parse player's entry.");
						}
					});
			return Task.FromResult(0);
		}

		private static Task GetUserRank(string c)
		{
			CService.CreateCommand(c)
					.Alias("profile")
					.Description($"Shows leaderboard stats about that player. Try `{Configuration.Default.PrefixCmd + c} <playername> <mapname>` to show the ranking of a specific map. This `{Configuration.Default.PrefixCmd + c} <steamid>` would also work.")
					.Parameter("playername", ParameterType.Optional)
					.Parameter("mapname", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						var url = (e.GetArg("playername") == string.Empty)
														  ? $"http://board.iverb.me/profile/{e.User.Name.Trim()}"
														  : $"http://board.iverb.me/profile/{e.Args[0]}";

						var entry = default(Portal2Entry);
						if (e.GetArg("mapname") != string.Empty)
						{
							if (await Utils.SearchArray(Data.Portal2Maps, 2, e.GetArg("mapname"), out var index))
								entry = await Portal2.GetUserRankAsync(url, index);
							else if (await Utils.SearchArray(Data.Portal2Maps, 3, e.GetArg("mapname"), out index))
								entry = await Portal2.GetUserRankAsync(url, index);
							else if (await Utils.SearchArray(Data.Portal2Maps, 5, e.GetArg("mapname"), out index))
								entry = await Portal2.GetUserRankAsync(url, index);
							else
							{
								await e.Channel.SendMessage("Couldn't find that map.");
								return;
							}

							if (entry != null)
							{
								var embed = new Embed
								{
									Author = new EmbedAuthor(entry.Player.Name, entry.Player.SteamLink, entry.Player.SteamAvatar),
									Color = Data.BoardColor.RawValue,
									Title = "Personal Record",
									Url = url,
									Image = new EmbedImage($"https://board.iverb.me/images/chambers_full/{entry.MapId}.jpg"),
									Footer = new EmbedFooter("board.iverb.me", Data.Portal2IconUrl),
									Fields = new EmbedField[]
									{
										new EmbedField("Map", entry.Map, true),
										new EmbedField("Rank", entry.Ranking, true),
										new EmbedField("Time", entry.Time, true),
										new EmbedField("Date", entry.Date, true),
										new EmbedField("Duration", await Utils.GetDuration(entry.DateTime))
									}
								};

								if ((entry.Demo != string.Empty)
								|| (entry.Demo != string.Empty))
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
								await Bot.SendAsync(CustomRequest.SendMessage(e.Channel.Id), new CustomMessage(embed));
							}
							else
								await e.Channel.SendMessage("Couldn't parse player's entry.");
						}
						else
						{
							var profile = await Portal2.GetUserStatsAsync(url);
							if ((profile == null)
							&& (e.User.Nickname != null))
								profile = await Portal2.GetUserStatsAsync(url = $"http://board.iverb.me/profile/{e.User.Nickname.Trim()}");

							if (profile != null)
							{
								await Bot.SendAsync(CustomRequest.SendMessage(e.Channel.Id), new CustomMessage(new Embed
								{
									Author = new EmbedAuthor(profile.Name, profile.SteamLink, profile.SteamAvatar),
									Color = Data.BoardColor.RawValue,
									Title = "Portal 2 Profile",
									Url = url,
									Footer = new EmbedFooter("board.iverb.me", Data.Portal2IconUrl),
									Fields = new EmbedField[]
									{
										new EmbedField("Rank", $"Single Player • {profile.SinglePlayerRank}\nCooperative • {profile.CooperativeRank}\nOverall • {profile.OverallRank}", true),
										new EmbedField("Points", $"Single Player • {profile.SinglePlayerPoints}\nCooperative • {profile.CooperativePoints}\nOverall • {profile.OverallPoints}", true),
										new EmbedField("Best Rank", $"{profile.BestPlaceRank} on {profile.BestPlaceMap}", true),
										new EmbedField("Average Rank", $"Single Player • {profile.AverageSinglePlayerRank}\nCooperative • {profile.AverageCooperativeRank}\nOverall • {profile.AverageOverallRank}", true),
										new EmbedField("World Records", (profile.SinglePlayerRank != "0")
																				? $"Single Player • {profile.SinglePlayerWorldRecords}\nCooperative • {profile.CooperativeWorldRecords}\nOverall • {profile.OverallWorldRecords}"
																				: "None.", true),
										new EmbedField("Worst Rank", $"{profile.WorstPlaceRank} on {profile.WorstPlaceMap}", true)
									}
								}));
							}
							else
								await e.Channel.SendMessage("Couldn't parse player's profile.");
						}
					});
			return Task.FromResult(0);
		}

		private static Task GetPlayerComparison(string c)
		{
			CService.CreateCommand(c)
					.Alias("comparison", "vs")
					.Description($"Compares profiles of players. Try `{Configuration.Default.PrefixCmd + c} <mapname> <players>` to compare to a specific map (you have to write the map name in one word).")
					.Parameter("mapname", ParameterType.Optional)
					.Parameter("players", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						const uint minimum = 2;
						const uint maximum = 3;
						if (!(await Utils.SearchArray(Data.Portal2Maps, 2, e.GetArg("mapname"), out var index)))
							if (!(await Utils.SearchArray(Data.Portal2Maps, 3, e.GetArg("mapname"), out index)))
								if (!(await Utils.SearchArray(Data.Portal2Maps, 5, e.GetArg("mapname"), out index)))
								{
								}

						if (index != -1)
						{
							var players = e.GetArg("players").Split(' ');
							if ((players.Length < minimum)
							|| (players.Length > maximum))
								await e.Channel.SendMessage("A minimum of two (maximum three) player names are required for comparison.");
							else
							{
								var list = new List<Portal2Entry>();
								foreach (var player in players)
									list.Add(await Portal2.GetUserRankAsync($"http://board.iverb.me/profile/{player}", index));
								list.RemoveAll(users => users == null);
								if (list.Count < 2)
									await e.Channel.SendMessage("Couldn't parse enough player profiles for a comparison.");
								else
								{
									var embed = new Embed
									{
										Color = Data.BoardColor.RawValue,
										Title = "Player Rank Comparison",
										Description = $"{await Utils.ListToList(list.Select(profile => profile.Player.Name).ToList(), delimiter: " vs ")}\non {list.First().Map}",
										Url = "https://board.iverb.me",
										Footer = new EmbedFooter("board.iverb.me", Data.Portal2IconUrl),
										Fields = new EmbedField[] { }
									};
									foreach (var entry in list)
									{
										embed.AddField(field =>
										{
											field.Name = "Rank";
											field.Value = entry.Ranking;
											field.Inline = true;
										});
										embed.AddField(field =>
										{
											field.Name = "Time";
											field.Value = entry.Time;
											field.Inline = true;
										});
										embed.AddField(field =>
										{
											field.Name = "Date";
											field.Value = entry.Date;
											field.Inline = true;
										});
									}
									await Bot.SendAsync(CustomRequest.SendMessage(e.Channel.Id), new CustomMessage(embed));
								}
							}
						}
						else
						{
							var players = e.GetArg("players").Split(' ');
							if ((players.Length < minimum - 1)
							|| (players.Length > maximum - 1))
								await e.Channel.SendMessage("A minimum of two (maximum three) player names are required for a comparison.");
							else
							{
								var list = new List<Portal2User> { await Portal2.GetUserStatsAsync($"http://board.iverb.me/profile/{e.GetArg("mapname")}") };
								foreach (var player in players)
									list.Add(await Portal2.GetUserStatsAsync($"http://board.iverb.me/profile/{player}"));
								list.RemoveAll(users => users == null);
								if (list.Count < 2)
									await e.Channel.SendMessage("Couldn't parse enough player profiles for a comparison.");
								else
								{
									var embed = new Embed
									{
										Color = Data.BoardColor.RawValue,
										Title = "Player Profile Comparison",
										Description = await Utils.ListToList(list.Select(profile => profile.Name).ToList(), delimiter: " vs "),
										Url = "https://board.iverb.me",
										Footer = new EmbedFooter("board.iverb.me", Data.Portal2IconUrl),
										Fields = new EmbedField[] { }
									};
									foreach (var profile in list)
									{
										embed.AddField(field =>
										{
											field.Name = "Rank";
											field.Value = $"Single Player • {profile.SinglePlayerRank}\nCooperative • {profile.CooperativeRank}\nOverall • {profile.OverallRank}";
											field.Inline = true;
										});
										embed.AddField(field =>
										{
											field.Name = "Average Rank";
											field.Value = $"Single Player • {profile.AverageSinglePlayerRank}\nCooperative • {profile.AverageCooperativeRank}\nOverall • {profile.AverageOverallRank}";
											field.Inline = true;
										});
										embed.AddField(field =>
										{
											field.Name = "Points";
											field.Value = $"Single Player • {profile.SinglePlayerPoints}\nCooperative • {profile.CooperativePoints}\nOverall • {profile.OverallPoints}";
											field.Inline = true;
										});
									}
									await Bot.SendAsync(CustomRequest.SendMessage(e.Channel.Id), new CustomMessage(embed));
								}
							}
						}
					});
			return Task.FromResult(0);
		}

		private static Task LeaderboardCommands(string c)
		{
			CService.CreateGroup(c, GBuilder =>
			{
				GBuilder.CreateCommand("boardparameter")
						.Alias("bp")
						.Description("Sets a new parameter for the leaderboard auto updater.")
						.Parameter("name", ParameterType.Required)
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage(await Portal2.AutoUpdater.SetNewBoardParameterAsync(e.Args[0]));
						});

				GBuilder.CreateCommand("cachetime")
						.Alias("ct")
						.Description("Shows you when the bot clears all data about leaderboard entries and stats.")
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage(await Portal2.Cache.GetCleanCacheTime());
						});

				GBuilder.CreateCommand("setcachetime")
						.Alias("setct")
						.Description("Sets a new time when the bot will clean the leaderboard cache.")
						.Parameter("value", ParameterType.Required)
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage(await Portal2.Cache.SetCleanCacheTimeAsync(e.Args[0]));
						});
			});
			return Task.FromResult(0);
		}
	}
}