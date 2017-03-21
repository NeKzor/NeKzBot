using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using OxyPlot;
using OxyPlot.Series;
using NeKzBot.Classes;
using NeKzBot.Extensions;
using NeKzBot.Resources;
using NeKzBot.Server;
using NeKzBot.Tasks.Leaderboard;

namespace NeKzBot.Modules.Public
{
	public class Leaderboard : CommandModule
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
			await GetPiePlayerComparison("pie");
			await GetLinePlayerComparison("line");
			await GetLeaderboard("top");
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
									new EmbedField("Date", entry.Date, true)
								}
							};

							var duration = await Utils.GetDuration(entry.DateTime);
							if (duration != default(string))
							{
								embed.AddField(field =>
								{
									field.Name = "Duration";
									field.Value = duration;
								});
							}
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
									new EmbedField("Date", entry.Date, true)
								}
							};

							var duration = await Utils.GetDuration(entry.DateTime);
							if (duration != default(string))
							{
								embed.AddField(field =>
								{
									field.Name = "Duration";
									field.Value = duration;
								});
							}
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
						var collection = await Data.Get<Portal2Maps>("p2maps");
						if (string.IsNullOrEmpty(e.GetArg("mapname")))
							entry = await Portal2.GetLatestEntryAsync($"{url}{collection.Maps[await Utils.RngAsync(collection.Maps.Count)].BestTimeId}");
						else
						{
							var result = await collection.Search(e.GetArg("mapname"));
							if (result == null)
							{
								await e.Channel.SendMessage("Couldn't find that map.");
								return;
							}
							entry = await Portal2.GetLatestEntryAsync($"{url}{result.BestTimeId}");
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
									new EmbedField("Date", entry.Date, true)
								}
							};

							var duration = await Utils.GetDuration(entry.DateTime);
							if (duration != default(string))
							{
								embed.AddField(field =>
								{
									field.Name = "Duration";
									field.Value = duration;
								});
							}
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
					.Alias("pb")
					.Description("Shows your leaderboard stats. Map name parameter is optional.")
					.Parameter("mapname", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						var url = $"http://board.iverb.me/profile/{e.User.Name.Trim()}";
						if (string.IsNullOrEmpty(e.GetArg("mapname")))
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
						else
						{
							var result = await (await Data.Get<Portal2Maps>("p2maps")).Search(e.GetArg("mapname"));
							if (result == null)
							{
								await e.Channel.SendMessage("Couldn't find that map.");
								return;
							}

							var entry = await Portal2.GetLatestEntryAsync($"{url}{result.BestTimeId}");
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
										new EmbedField("Date", entry.Date, true)
									}
								};

								var duration = await Utils.GetDuration(entry.DateTime);
								if (duration != default(string))
								{
									embed.AddField(field =>
									{
										field.Name = "Duration";
										field.Value = duration;
									});
								}
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
					.Description("Shows leaderboard stats about a player (Steam ID64 would also work). Map name parameter is optional.")
					.Parameter("playername", ParameterType.Required)
					.Parameter("mapname", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						var url = $"http://board.iverb.me/profile/{e.GetArg("playername")}";
						if (string.IsNullOrEmpty(e.GetArg("mapname")))
						{
							var profile = await Portal2.GetUserStatsAsync(url);
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
							var result = await (await Data.Get<Portal2Maps>("p2maps")).Search(e.GetArg("mapname"));
							if (result == null)
							{
								await e.Channel.SendMessage("Couldn't find that map.");
								return;
							}

							var entry = await Portal2.GetLatestEntryAsync($"{url}{result.BestTimeId}");
							if (entry != null)
							{
								var embed = new Embed
								{
									Author = new EmbedAuthor(entry.Player.Name, entry.Player.SteamLink, entry.Player.SteamAvatar),
									Color = Data.BoardColor.RawValue,
									Title = "Personal Record",
									Url = url,
									Image = new EmbedImage($"https://board.iverb.me/images/chambers_full/{entry.MapId}.jpg"),
									//Thumbnail = new EmbedThumbnail($"TODO?"),	// Chamber overview would be better as a thumbnail
									Footer = new EmbedFooter("board.iverb.me", Data.Portal2IconUrl),
									Fields = new EmbedField[]
									{
										new EmbedField("Map", entry.Map, true),
										new EmbedField("Rank", entry.Ranking, true),
										new EmbedField("Time", entry.Time, true),
										new EmbedField("Date", entry.Date, true)
									}
								};

								var duration = await Utils.GetDuration(entry.DateTime);
								if (duration != default(string))
								{
									embed.AddField(field =>
									{
										field.Name = "Duration";
										field.Value = duration;
									});
								}
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
						var result = await (await Data.Get<Portal2Maps>("p2maps")).Search(e.GetArg("mapname"));
						if (result == null)
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
										Description = await Utils.CollectionToList(list.Select(profile => profile.Name).ToList(), delimiter: " vs "),
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
						else
						{
							var players = e.GetArg("players").Split(' ');
							if ((players.Length < minimum)
							|| (players.Length > maximum))
								await e.Channel.SendMessage("A minimum of two (maximum three) player names are required for comparison.");
							else
							{
								var list = new List<Portal2Entry>();
								foreach (var player in players)
									list.Add(await Portal2.GetUserRankAsync($"http://board.iverb.me/profile/{player}", result));
								list.RemoveAll(users => users == null);
								if (list.Count < 2)
									await e.Channel.SendMessage("Couldn't parse enough player profiles for a comparison.");
								else
								{
									var embed = new Embed
									{
										Color = Data.BoardColor.RawValue,
										Title = "Player Rank Comparison",
										Description = $"{await Utils.CollectionToList(list.Select(profile => profile.Player.Name).ToList(), delimiter: " vs ")}\non {list.First().Map}",
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
					});
			return Task.FromResult(0);
		}

		private static Task GetLeaderboard(string c)
		{
			CService.CreateCommand(c)
					.Description($"Shows the current top five leaderboard. Try `{Configuration.Default.PrefixCmd + c} <count> <mapname>` to show a specific amount of entries (max. 10).")
					.Parameter("mapname", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						// Parsing
						var count = default(uint);
						var mapname = default(string);
						if (uint.TryParse(e.GetArg("mapname").Split(' ').First(), out var parsed))
						{
							count = parsed;
							mapname = await Utils.GetRest(e.GetArg("mapname").Split(' '), 1, sep: " ");
						}
						else
						{
							count = 5;
							mapname = e.GetArg("mapname");
						}

						var result = await (await Data.Get<Portal2Maps>("p2maps")).Search(e.GetArg("mapname"));
						if (result == null)
						{
							await e.Channel.SendMessage("Couldn't find that map.");
							return;
						}

						var url = $"http://board.iverb.me/chamber/{result.BestTimeId}";
						var leaderboard = await Portal2.GetMapEntriesAsync(url, 0, count, 10);
						if (leaderboard != null)
						{
							await Bot.SendAsync(CustomRequest.SendMessage(e.Channel.Id), new CustomMessage(new Embed
							{
								Color = Data.BoardColor.RawValue,
								Title = $"Portal 2 Top {leaderboard.Entries.Count}",
								Url = url,
								Image = new EmbedImage($"https://board.iverb.me/images/chambers_full/{result.BestTimeId}.jpg"),
								Footer = new EmbedFooter("board.iverb.me", Data.Portal2IconUrl)
							}
							.AddField(async field =>
							{
								field.Name = leaderboard.MapName;
								var output = string.Empty;
								foreach (var item in leaderboard.Entries)
									output += $"\n{item.Ranking} {item.Time} by {await Utils.FormatRawText(item.Player.Name)} ({item.Date.Replace(".", string.Empty)})";
								field.Value = (output != string.Empty)
													  ? output
													  : "Data not found.";
							})));
						}
						else
							await e.Channel.SendMessage("Couldn't parse a leaderboard.");
					});
			return Task.FromResult(0);
		}

		// TODO: make some optimisations
		// NOTE: code below here is really bad
		private static Task GetPiePlayerComparison(string c)
		{
			CService.CreateCommand(c)
					.Description("Creates a pie plot for a player comparison. Calculation might take a while.")
					.Parameter("players", ParameterType.Multiple)
					.AddCheck(Permissions.DevelopersOnly)
					.Hide()
					.Do(async e => await CreatePieAsync(e, MapFilter.Any));

			CService.CreateCommand(c + "sp")
					.Description("Creates a pie plot for a player comparison. Calculation might take a while.")
					.Parameter("players", ParameterType.Multiple)
					.AddCheck(Permissions.DevelopersOnly)
					.Hide()
					.Do(async e => await CreatePieAsync(e, MapFilter.SinglePlayer));

			CService.CreateCommand(c + "mp")
					.Description("Creates a pie plot for a player comparison. Calculation might take a while.")
					.Parameter("players", ParameterType.Multiple)
					.AddCheck(Permissions.DevelopersOnly)
					.Hide()
					.Do(async e => await CreatePieAsync(e, MapFilter.MultiPlayer));
			return Task.FromResult(0);
		}

		private static Task GetLinePlayerComparison(string c)
		{
			CService.CreateCommand(c)
					.Description("Creates a line plot for a player comparison. Calculation might take a while.")
					.Parameter("players", ParameterType.Multiple)
					.AddCheck(Permissions.DevelopersOnly)
					.Hide()
					.Do(async e => await CreateGraphAsync(e, MapFilter.Any));

			CService.CreateCommand(c + "sp")
					.Description("Creates a line plot for a player comparison. Calculation might take a while.")
					.Parameter("players", ParameterType.Multiple)
					.AddCheck(Permissions.DevelopersOnly)
					.Hide()
					.Do(async e => await CreateGraphAsync(e, MapFilter.SinglePlayer));

			CService.CreateCommand(c + "mp")
					.Description("Creates a line plot for a player comparison. Calculation might take a while.")
					.Parameter("players", ParameterType.Multiple)
					.AddCheck(Permissions.DevelopersOnly)
					.Hide()
					.Do(async e => await CreateGraphAsync(e, MapFilter.MultiPlayer));
			return Task.FromResult(0);
		}

		private static async Task CreatePieAsync(CommandEventArgs e, MapFilter filter = default(MapFilter))
		{
			await e.Channel.SendIsTyping();
			var msg = await e.Channel.SendMessage("This might take a while.");

			var players = new Dictionary<Portal2User, List<Portal2Entry>>();
			foreach (var player in e.Args)
			{
				var temp = await Portal2.GetUserStatsAsync($"https://board.iverb.me/profile/{player}");
				if (temp == null)
					continue;

				var pbs = new List<Portal2Entry>();
				var result = (await Data.Get<Portal2Maps>("p2maps")).Maps;
				for (int i = 0; i < result.Count; i++)
				{
					// Ignore unsupported maps
					if (string.IsNullOrEmpty(result[i].BestPortalsId))
						continue;

					switch (result[i].Filter)
					{
						case MapFilter.Any:
						case MapFilter.SinglePlayer:
						case MapFilter.MultiPlayer:
							break;
						default:
							continue;
					}
					pbs.Add(await Portal2.GetUserRankAsync(temp.BoardLink, result[i]));
				}
				players.Add(temp, pbs);
			}

			if ((players.Count > 1)
			&& (players.Count < 4))
			{
				// Don't count maps which you can't compare
				var dontcount = new List<bool>();
				foreach (var pb in players.First().Value)
				{
					if (pb == null)
						dontcount.Add(true);
					else
						dontcount.Add(false);
				}
				foreach (var player in players.Skip(1))
					for (int i = 0; i < dontcount.Count; i++)
						if (player.Value[i] == null)
							dontcount[i] = true;
				// Get the best ranks of each map
				var fastest = new List<string>();
				for (int i = 0; i < players.First().Value.Count; i++)
				{
					if (dontcount[i])
						fastest.Add(null);
					else
						fastest.Add(players.First().Value[i].Ranking);
				}
				foreach (var pbs in players.Values)
				{
					for (int i = 0; i < pbs.Count; i++)
					{
						if (dontcount[i])
							continue;
						var result = int.Parse(pbs[i].Ranking);
						if (result < int.Parse(fastest[i]))
							fastest[i] = result.ToString();
					}
				}
				// Compare the rank of each player
				var pie = new PieSeries();
				var title = string.Empty;
				foreach (var player in players)
				{
					var value = 0;
					for (int i = 0; i < fastest.Count; i++)
					{
						if (dontcount[i])
							continue;
						if (fastest[i] == player.Value[i].Ranking)
							value++;
					}
					title += $"{player.Key.Name} vs ";
					pie.Slices.Add(new PieSlice(player.Key.Name, value));
				}
				// Create plot
				var unknown = dontcount.Count(x => x);
				if (unknown > 0)
					pie.Slices.Add(new PieSlice("Unknown", unknown));
				var plot = new PlotModel()
				{
					Title = "Player comparison: map percentage (including ties and non-cm maps)",
					Subtitle = title.Substring(0, title.Length - " vs ".Length),
					Background = OxyColors.White
				};
				plot.Series.Add(pie);
				// Export as .svg file
				var file = Path.Combine(await Utils.GetAppPath() + "/Resources/Cache/", "lb-pie-plot.svg");
				using (var stream = File.Create(file))
				{
					new SvgExporter
					{
						Width = 800,
						Height = 400
					}
					.Export(plot, stream);
				}
				await msg.Edit("Generated pie plot.");
				await e.Channel.SendFile(file);
			}
			else
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage("A minimum of two (maximum four) player profiles are required.");
			}
		}

		private static async Task CreateGraphAsync(CommandEventArgs e, MapFilter filter = default(MapFilter))
		{
			await e.Channel.SendIsTyping();
			var msg = await e.Channel.SendMessage("This might take a while.");

			var players = new Dictionary<Portal2User, List<Portal2Entry>>();
			foreach (var player in e.Args)
			{
				var temp = await Portal2.GetUserStatsAsync($"https://board.iverb.me/profile/{player}");
				if (temp == null)
					continue;

				var pbs = new List<Portal2Entry>();
				var result = (await Data.Get<Portal2Maps>("p2maps")).Maps;
				for (int i = 0; i < result.Count; i++)
				{
					// Ignore unsupported maps
					if (string.IsNullOrEmpty(result[i].BestPortalsId))
						continue;

					switch (result[i].Filter)
					{
						case MapFilter.Any:
						case MapFilter.SinglePlayer:
						case MapFilter.MultiPlayer:
							break;
						default:
							continue;
					}
					pbs.Add(await Portal2.GetUserRankAsync(temp.BoardLink, result[i]));
				}
				players.Add(temp, pbs);
			}

			if ((players.Count > 1)
			&& (players.Count < 4))
			{
				// Don't count maps which you can't compare
				var dontcount = new List<bool>();
				foreach (var pb in players.First().Value)
				{
					if (pb == null)
						dontcount.Add(true);
					else
						dontcount.Add(false);
				}
				foreach (var player in players.Skip(1))
					for (int i = 0; i < dontcount.Count; i++)
						if (player.Value[i] == null)
							dontcount[i] = true;
				// Create data points, sum player times
				var lines = new List<LineSeries>();
				var title = string.Empty;
				foreach (var player in players)
				{
					title += $"{player.Key.Name} vs ";
					var count = 0;
					var sum = (double)0;
					var series = new LineSeries();
					for (int i = 0; i < player.Value.Count; i++)
					{
						if (dontcount[i])
							continue;
						else
						{
							if (!(TimeSpan.TryParseExact(player.Value[i].Time, "ss'.'ff", CultureInfo.CurrentCulture, out var result)))
								if (!(TimeSpan.TryParseExact(player.Value[i].Time, "s'.'ff", CultureInfo.CurrentCulture, out result)))
									if (!(TimeSpan.TryParseExact(player.Value[i].Time, "m':'ss'.'ff", CultureInfo.CurrentCulture, out result)))
										if (!(TimeSpan.TryParseExact(player.Value[i].Time, "mm':'ss'.'ff", CultureInfo.CurrentCulture, out result)))
											throw new Exception("Couldn't parse entry time. (Leaderboard.CreateGraphAsync)");
							sum += result.TotalSeconds;
							series.Points.Add(new DataPoint(count++, sum));
						}
					}
					lines.Add(series);
				}
				// Create plot
				var plot = new PlotModel()
				{
					Title = "Player comparison: map progression (including non-cm maps, sorted in alphabetical order)",
					Subtitle = title.Substring(0, title.Length - " vs ".Length),
					Background = OxyColors.White
				};
				foreach (var line in lines)
					plot.Series.Add(line);
				// Export as .svg file
				var file = Path.Combine(await Utils.GetAppPath() + "/Resources/Cache/", "lb-line-plot.svg");
				using (var stream = File.Create(file))
				{
					new SvgExporter
					{
						Width = 800,
						Height = 400
					}
					.Export(plot, stream);
				}
				await msg.Edit("Generated line plot.");
				await e.Channel.SendFile(file);
			}
			else
			{
				await e.Channel.SendIsTyping();
				await e.Channel.SendMessage("A minimum of two (maximum four) player profiles are required.");
			}
		}

		private static Task LeaderboardCommands(string c)
		{
			CService.CreateGroup(c, GBuilder =>
			{
				GBuilder.CreateCommand("boardparameter")
						.Alias("bp")
						.Description("Sets a new parameter for the automatic leaderboard updater.")
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
						.Description("Shows the time when the leaderboard cache gets cleared.")
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage(await Portal2.Cache.GetCleanCacheTime());
						});

				GBuilder.CreateCommand("setcachetime")
						.Alias("setct")
						.Description("Sets a new time when the bot will clear the leaderboard cache.")
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