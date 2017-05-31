using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Portal2Boards.Net;
using Portal2Boards.Net.API.Models;
using Portal2Boards.Net.Entities;
using Portal2Boards.Net.Extensions;
using SourceDemoParser.Net;
using NeKzBot.Extensions;
using NeKzBot.Resources;
using NeKzBot.Server;
using NeKzBot.Tasks.Leaderboard;
using NeKzBot.Utilities;

namespace NeKzBot.Modules.Public
{
	public class Leaderboard : CommandModule
	{
		private static Portal2BoardsClient _client;

		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Leaderboard Module", LogColor.Init);
			_client = new Portal2BoardsClient(await new Fetcher().GetClient(), noSsl: true);
			await GetLatestWorldRecord("latestwr");
			await GetCurrentWorldRecord("wr");
			await GetLatestLeaderboardEntry("latestentry");
			await GetOwnRank("rank");
			await GetUserRank("player");
			await GetPlayerComparison("compare");
			await GetLeaderboard("top");
			await GetDemoInformation("analyze");
		}

		private static Task GetLatestWorldRecord(string c)
		{
			CService.CreateCommand(c)
					.Alias("wrupdate")
					.Description($"Returns the most recent world record. Try `{Configuration.Default.PrefixCmd + c} yt` to filter wrs by videos only or `{Configuration.Default.PrefixCmd + c} demo` to filter them by demos only.")
					.Parameter("filter", ParameterType.Optional)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						var filter = e.GetArg("filter");
						var query = default(string);
						if (string.IsNullOrEmpty(filter))
							query = "?wr=1";
						else if (string.Equals(filter, "yt", StringComparison.CurrentCultureIgnoreCase))
							query = "?wr=1&yt=1";
						else if (string.Equals(filter, "demo", StringComparison.CurrentCultureIgnoreCase))
							query = "?wr=1&demo=1";
						else
						{
							await e.Channel.SendMessage($"Unknown parameter. Try `{Configuration.Default.PrefixCmd + c} yt` or `{Configuration.Default.PrefixCmd + c} demo`.");
							return;
						}

						var changelog = await _client.GetChangelogAsync(query);
						if (changelog != null)
						{
							var entry = (EntryData)changelog.Data.FirstOrDefault();
							var embed = new Embed
							{
								Author = new EmbedAuthor(entry.Player.Name, entry.Player.Link, entry.Player.SteamAvatarLink),
								Color = Data.BoardColor.RawValue,
								Title = "Portal 2 World Record",
								Url = changelog.RequestUrl,
								Image = new EmbedImage(entry.ImageLinkFull),
								Footer = new EmbedFooter("board.iverb.me", Data.Portal2IconUrl),
								Fields = new EmbedField[]
								{
									new EmbedField("Map", entry.Map.Name, true),
									new EmbedField("Player", await Utils.AsRawText(entry.Player.Name), true),
									new EmbedField("Time", entry.Score.Current.AsTimeToString(), true),
									new EmbedField("Date", entry.Date.DateTimeToString(), true)
								}
							};

							var duration = await Utils.GetDurationAsync(entry.Date);
							if (duration != default(string))
							{
								embed.AddField(field =>
								{
									field.Name = "Duration";
									field.Value = duration;
								});
							}
							if ((entry.DemoExists)
							|| (entry.VideoExists))
							{
								embed.AddField(field =>
								{
									field.Name = "Links";
									var output = string.Empty;
									if (entry.DemoExists)
										output += $"[Demo Download]({entry.DemoLink})";
									if (entry.VideoExists)
										output += $"{((entry.DemoExists) ? "\n" : string.Empty)}[YouTube Video]({entry.VideoLink})";
									field.Value = output;
								});
							}
							if (entry.CommentExists)
							{
								embed.AddField(async field =>
								{
									field.Name = "Comment";
									field.Value = await Utils.AsRawText(entry.Comment);
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
					.Description($"Returns the most recent leaderboard entry. Try `{Configuration.Default.PrefixCmd + c} yt` to filter entries by videos only or `{Configuration.Default.PrefixCmd + c} demo` to filter by demos only.")
					.Parameter("filter", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						var filter = e.GetArg("filter");
						var query = default(string);
						if (string.IsNullOrEmpty(filter))
							query = string.Empty;
						else if (string.Equals(filter, "yt", StringComparison.CurrentCultureIgnoreCase))
							query = "?yt=1";
						else if (string.Equals(filter, "demo", StringComparison.CurrentCultureIgnoreCase))
							query = "?demo=1";
						else
						{
							await e.Channel.SendMessage($"Unknown parameter. Try `{Configuration.Default.PrefixCmd + c} yt` or `{Configuration.Default.PrefixCmd + c} demo`.");
							return;
						}

						var changelog = await _client.GetChangelogAsync(query);
						if (changelog != null)
						{
							var entry = (EntryData)changelog.Data.FirstOrDefault();
							var embed = new Embed
							{
								Author = new EmbedAuthor(entry.Player.Name, entry.Player.Link, entry.Player.SteamAvatarLink),
								Color = Data.BoardColor.RawValue,
								Title = "Portal 2 Entry",
								Url = changelog.RequestUrl,
								Image = new EmbedImage(entry.ImageLinkFull),
								Footer = new EmbedFooter("board.iverb.me", Data.Portal2IconUrl),
								Fields = new EmbedField[]
								{
									new EmbedField("Map", entry.Map.Name, true),
									new EmbedField("Rank", entry.Rank.Current.ToString(), true),
									new EmbedField("Time", entry.Score.Current.AsTimeToString(), true),
									new EmbedField("Date", entry.Date.DateTimeToString(), true)
								}
							};

							var duration = await Utils.GetDurationAsync(entry.Date);
							if (duration != default(string))
							{
								embed.AddField(field =>
								{
									field.Name = "Duration";
									field.Value = duration;
								});
							}
							if ((entry.DemoExists)
							|| (entry.VideoExists))
							{
								embed.AddField(field =>
								{
									field.Name = "Links";
									var output = string.Empty;
									if (entry.DemoExists)
										output += $"[Demo Download]({entry.DemoLink})";
									if (entry.VideoExists)
										output += $"{((entry.DemoExists) ? "\n" : string.Empty)}[YouTube Video]({entry.VideoLink})";
									field.Value = output;
								});
							}
							if (entry.CommentExists)
							{
								embed.AddField(async field =>
								{
									field.Name = "Comment";
									field.Value = await Utils.AsRawText(entry.Comment);
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
					.Description($"Returns latest world record of a map. Try `{Configuration.Default.PrefixCmd + c}` to show a random wr entry.")
					.Parameter("map_name", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						var changelog = default(Changelog);
						if (string.IsNullOrEmpty(e.GetArg("map_name")))
							changelog = await _client.GetChangelogAsync($"?wr=1&chamber={(await Utils.RngAsync(Portal2.CampaignMaps.Where(m => m.IsOfficial))).BestTimeId}");
						else
						{
							var map = await Portal2.GetMapByName(e.GetArg("map_name"));
							if (map == null)
							{
								await e.Channel.SendMessage("Couldn't find that map.");
								return;
							}
							changelog = await _client.GetChangelogAsync($"?wr=1&chamber={map.BestTimeId}");
						}

						if (changelog != null)
						{
							var entry = (EntryData)changelog.Data.FirstOrDefault();
							var embed = new Embed
							{
								Author = new EmbedAuthor(entry.Player.Name, entry.Player.Link, entry.Player.SteamAvatarLink),
								Color = Data.BoardColor.RawValue,
								Title = "Portal 2 World Record",
								Url = changelog.RequestUrl,
								Image = new EmbedImage(entry.ImageLinkFull),
								Footer = new EmbedFooter("board.iverb.me", Data.Portal2IconUrl),
								Fields = new EmbedField[]
								{
									new EmbedField("Map", entry.Map.Name, true),
									new EmbedField("Player", await Utils.AsRawText(entry.Player.Name), true),
									new EmbedField("Time", entry.Score.Current.AsTimeToString(), true),
									new EmbedField("Date", entry.Date.DateTimeToString(), true)
								}
							};

							var duration = await Utils.GetDurationAsync(entry.Date);
							if (duration != default(string))
							{
								embed.AddField(field =>
								{
									field.Name = "Duration";
									field.Value = duration;
								});
							}
							if ((entry.DemoExists)
							|| (entry.VideoExists))
							{
								embed.AddField(field =>
								{
									field.Name = "Links";
									var output = string.Empty;
									if (entry.DemoExists)
										output += $"[Demo Download]({entry.DemoLink})";
									if (entry.VideoExists)
										output += $"{((entry.DemoExists) ? "\n" : string.Empty)}[YouTube Video]({entry.VideoLink})";
									field.Value = output;
								});
							}
							if (entry.CommentExists)
							{
								embed.AddField(async field =>
								{
									field.Name = "Comment";
									field.Value = await Utils.AsRawText(entry.Comment);
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
					.Parameter("map_name", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (string.IsNullOrEmpty(e.GetArg("map_name")))
						{
							var profile = await _client.GetProfileAsync(e.User.Name);
							if ((profile == null)
							&& (e.User.Nickname != null))
								profile = await _client.GetProfileAsync(e.User.Nickname);

							if (profile != null)
							{
								var user = (UserData)profile.Data;
								await Bot.SendAsync(CustomRequest.SendMessage(e.Channel.Id), new CustomMessage(new Embed
								{
									Author = new EmbedAuthor(user.DisplayName, user.Link, user.SteamAvatarLink),
									Color = Data.BoardColor.RawValue,
									Title = "Portal 2 Profile",
									Url = profile.RequestUrl,
									Footer = new EmbedFooter("board.iverb.me", Data.Portal2IconUrl),
									Fields = new EmbedField[]
									{
										new EmbedField("Rank", $"Single Player • {user.Points.SinglePlayer.PlayerRank.FormatRankToString()}\n" +
															   $"Cooperative • {user.Points.Cooperative.PlayerRank.FormatRankToString()}\n" +
															   $"Overall • {user.Points.Global.PlayerRank.FormatRankToString()}", true),
										new EmbedField("Points", $"Single Player • {user.Points.SinglePlayer.Score.FormatPointsToString()}\n" +
																 $"Cooperative • {user.Points.Cooperative.Score.FormatPointsToString()}\n" +
																 $"Overall • {user.Points.Global.Score.FormatPointsToString()}", true),
										new EmbedField("Best Rank", $"{user.Times.BestScore.PlayerRank.FormatRankToString()} on {user.Times.BestScore.ParsedMap?.Alias ?? "serveral chambers"}", true),
										new EmbedField("Average Rank", $"Single Player • {user.Times.SinglePlayer.Chapters.AveragePlace.FormatAveragePlaceToString()}\n" +
																	   $"Cooperative • {user.Times.Cooperative.Chapters.AveragePlace.FormatAveragePlaceToString()}\n" +
																	   $"Overall • {user.Times.GlobalAveragePlace.FormatAveragePlaceToString()}", true),
										new EmbedField("World Records", (user.Times.WorldRecordCount >= 1)
																								  ? $"Single Player • {user.Times.SinglePlayer.Chapters.WorldRecordCount}\n" +
																									$"Cooperative • {user.Times.Cooperative.Chapters.WorldRecordCount}\n" +
																									$"Overall • {user.Times.WorldRecordCount}"
																								  : "None.", true),
										new EmbedField("Worst Rank", $"{user.Times.WorstScore.PlayerRank.FormatRankToString()} on {user.Times.WorstScore.ParsedMap?.Alias ?? "several chambers"}", true)
									}
								}));
							}
							else
								await e.Channel.SendMessage("Couldn't parse player's profile.");
						}
						else
						{
							var map = await Portal2.GetMapByName(e.GetArg("map_name"));
							if (map == null)
							{
								await e.Channel.SendMessage("Couldn't find that map.");
								return;
							}

							var profile = await _client.GetProfileAsync(e.User.Name);
							if ((profile == null)
							&& (e.User.Nickname != null))
								profile = await _client.GetProfileAsync(e.User.Nickname);

							if (profile != null)
							{
								var user = (UserData)profile.Data;
								var data = await user.Times.GetMapData(map);
								var embed = new Embed
								{
									Author = new EmbedAuthor(user.DisplayName, user.Link, user.SteamAvatarLink),
									Color = Data.BoardColor.RawValue,
									Title = "Personal Record",
									Url = profile.RequestUrl,
									Image = new EmbedImage(map.ImageLinkFull),
									Footer = new EmbedFooter("board.iverb.me", Data.Portal2IconUrl),
									Fields = new EmbedField[]
									{
										new EmbedField("Map", map.Alias, true),
										new EmbedField("Rank", data.PlayerRank.ToString(), true),
										new EmbedField("Time", data.Score.AsTimeToString(), true),
										new EmbedField("Date", data.Date.DateTimeToString(), true)
									}
								};

								var duration = await Utils.GetDurationAsync(data.Date);
								if (duration != default(string))
								{
									embed.AddField(field =>
									{
										field.Name = "Duration";
										field.Value = duration;
									});
								}
								if ((data.DemoExists)
								|| (data.VideoExists))
								{
									embed.AddField(field =>
									{
										field.Name = "Links";
										var output = string.Empty;
										if (data.DemoExists)
											output += $"[Demo Download]({data.DemoLink})";
										if (data.VideoExists)
											output += $"{((data.DemoExists) ? "\n" : string.Empty)}[YouTube Video]({data.VideoLink})";
										field.Value = output;
									});
								}
								if (data.CommentExists)
								{
									embed.AddField(async field =>
									{
										field.Name = "Comment";
										field.Value = await Utils.AsRawText(data.Comment);
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
					.Parameter("board_name", ParameterType.Required)
					.Parameter("map_name", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (string.IsNullOrEmpty(e.GetArg("map_name")))
						{
							var profile = await _client.GetProfileAsync(e.GetArg("board_name"));
							if (profile != null)
							{
								var user = (UserData)profile.Data;
								await Bot.SendAsync(CustomRequest.SendMessage(e.Channel.Id), new CustomMessage(new Embed
								{
									Author = new EmbedAuthor(user.DisplayName, user.SteamLink, user.SteamAvatarLink),
									Color = Data.BoardColor.RawValue,
									Title = "Portal 2 Profile",
									Url = profile.RequestUrl,
									Footer = new EmbedFooter("board.iverb.me", Data.Portal2IconUrl),
									Fields = new EmbedField[]
									{
										new EmbedField("Rank", $"Single Player • {user.Points.SinglePlayer.PlayerRank.FormatRankToString()}\n" +
															   $"Cooperative • {user.Points.Cooperative.PlayerRank.FormatRankToString()}\n" +
															   $"Overall • {user.Points.Global.PlayerRank.FormatRankToString()}", true),
										new EmbedField("Points", $"Single Player • {user.Points.SinglePlayer.Score.FormatPointsToString()}\n" +
																 $"Cooperative • {user.Points.Cooperative.Score.FormatPointsToString()}\n" +
																 $"Overall • {user.Points.Global.Score.FormatPointsToString()}", true),
										new EmbedField("Best Rank", $"{user.Times.BestScore.PlayerRank.FormatRankToString()} on {user.Times.BestScore.ParsedMap?.Alias ?? "several chambers"}", true),
										new EmbedField("Average Rank", $"Single Player • {user.Times.SinglePlayer.Chapters.AveragePlace.FormatAveragePlaceToString()}\n" +
																	   $"Cooperative • {user.Times.Cooperative.Chapters.AveragePlace.FormatAveragePlaceToString()}\n" +
																	   $"Overall • {user.Times.GlobalAveragePlace.FormatAveragePlaceToString()}", true),
										new EmbedField("World Records", (user.Times.WorldRecordCount >= 1)
																								  ? $"Single Player • {user.Times.SinglePlayer.Chapters.WorldRecordCount}\n" +
																									$"Cooperative • {user.Times.Cooperative.Chapters.WorldRecordCount}\n" +
																									$"Overall • {user.Times.WorldRecordCount}"
																								  : "None.", true),
										new EmbedField("Worst Rank", $"{user.Times.WorstScore.PlayerRank.FormatRankToString()} on {user.Times.WorstScore.ParsedMap?.Alias ?? "several chambers"}", true)
									}
								}));
							}
							else
								await e.Channel.SendMessage("Couldn't parse player's profile.");
						}
						else
						{
							var map = await Portal2.GetMapByName(e.GetArg("map_name"));
							if (map == null)
							{
								await e.Channel.SendMessage("Couldn't find that map.");
								return;
							}

							var profile = await _client.GetProfileAsync(e.GetArg("board_name"));
							if (profile != null)
							{
								var user = (UserData)profile.Data;
								var data = await user.Times.GetMapData(map);
								var embed = new Embed
								{
									Author = new EmbedAuthor(user.DisplayName, user.Link, user.SteamAvatarLink),
									Color = Data.BoardColor.RawValue,
									Title = "Personal Record",
									Url = profile.RequestUrl,
									Image = new EmbedImage(map.ImageLinkFull),
									Footer = new EmbedFooter("board.iverb.me", Data.Portal2IconUrl),
									Fields = new EmbedField[]
									{
										new EmbedField("Map", map.Alias, true),
										new EmbedField("Rank", data.PlayerRank.ToString(), true),
										new EmbedField("Time", data.Score.AsTimeToString(), true),
										new EmbedField("Date", data.Date.DateTimeToString(), true)
									}
								};

								var duration = await Utils.GetDurationAsync(data.Date);
								if (duration != default(string))
								{
									embed.AddField(field =>
									{
										field.Name = "Duration";
										field.Value = duration;
									});
								}
								if ((data.DemoExists)
								|| (data.VideoExists))
								{
									embed.AddField(field =>
									{
										field.Name = "Links";
										var output = string.Empty;
										if (data.DemoExists)
											output += $"[Demo Download]({data.DemoLink})";
										if (data.VideoExists)
											output += $"{((data.DemoExists) ? "\n" : string.Empty)}[YouTube Video]({data.VideoLink})";
										field.Value = output;
									});
								}
								if (data.CommentExists)
								{
									embed.AddField(async field =>
									{
										field.Name = "Comment";
										field.Value = await Utils.AsRawText(data.Comment);
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
					.Description($"Compares profiles of players. Try `{Configuration.Default.PrefixCmd + c} <map_name> <players>` to compare to a specific map (you have to write the map name in one word).")
					.Parameter("map_name", ParameterType.Optional)
					.Parameter("players", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						const uint minimum = 2;
						const uint maximum = 3;
						var map = await Portal2.GetMapByName(e.GetArg("map_name"));
						if (map == null)
						{
							var players = e.GetArg("players").Split(' ');
							if ((players.Length < minimum - 1)
							|| (players.Length > maximum - 1))
								await e.Channel.SendMessage("A minimum of two (maximum three) player names are required for a comparison.");
							else
							{
								var profiles = new List<ProfileData> { (await _client.GetProfileAsync(e.GetArg("map_name")))?.Data };
								foreach (var player in players)
									profiles.Add((await _client.GetProfileAsync(player))?.Data);
								profiles.RemoveAll(profile => profile == null);
								if (profiles.Count < 2)
									await e.Channel.SendMessage("Couldn't parse enough player profiles for a comparison.");
								else
								{
									var users = new List<UserData>();
									foreach (var profile in profiles)
										users.Add((UserData)profile);
									var embed = new Embed
									{
										Color = Data.BoardColor.RawValue,
										Title = "Player Profile Comparison",
										Description = await Utils.AsRawText(await Utils.CollectionToList(users.Select(user => user.DisplayName), delimiter: " vs ")),
										Url = "https://board.iverb.me",
										Footer = new EmbedFooter("board.iverb.me", Data.Portal2IconUrl)
									};
									foreach (var user in users)
									{
										embed.AddField(field =>
										{
											field.Name = "Rank";
											field.Value = $"Single Player • {user.Points.SinglePlayer.PlayerRank.FormatRankToString()}\n" +
														  $"Cooperative • {user.Points.Cooperative.PlayerRank.FormatRankToString()}\n" +
														  $"Overall • {user.Points.Global.PlayerRank.FormatRankToString()}";
											field.Inline = true;
										});
										embed.AddField(field =>
										{
											field.Name = "Average Rank";
											field.Value = $"Single Player • {user.Times.SinglePlayer.Chapters.AveragePlace.FormatAveragePlaceToString()}\n" +
														  $"Cooperative • {user.Times.Cooperative.Chapters.AveragePlace.FormatAveragePlaceToString()}\n" +
														  $"Overall • {user.Times.GlobalAveragePlace.FormatAveragePlaceToString()}";
											field.Inline = true;
										});
										embed.AddField(field =>
										{
											field.Name = "Points";
											field.Value = $"Single Player • {user.Points.SinglePlayer.Score.FormatPointsToString()}\n" +
														  $"Cooperative • {user.Points.Cooperative.Score.FormatPointsToString()}\n" +
														  $"Overall • {user.Points.Global.Score.FormatPointsToString()}";
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
								var profiles = new List<ProfileData> { (await _client.GetProfileAsync(e.GetArg("map_name")))?.Data };
								foreach (var player in players)
									profiles.Add((await _client.GetProfileAsync(player))?.Data);
								profiles.RemoveAll(profile => profile == null);
								if (profiles.Count < 2)
									await e.Channel.SendMessage("Couldn't parse enough player profiles for a comparison.");
								else
								{
									var users = new List<UserData>();
									foreach (var profile in profiles)
										users.Add((UserData)profile);
									var embed = new Embed
									{
										Color = Data.BoardColor.RawValue,
										Title = "Player Rank Comparison",
										Description = $"{await Utils.AsRawText(await Utils.CollectionToList(users.Select(user => user.DisplayName), delimiter: " vs "))}\non {map.Alias}",
										Url = "https://board.iverb.me",
										Footer = new EmbedFooter("board.iverb.me", Data.Portal2IconUrl)
									};
									foreach (var user in users)
									{
										var data = await user.Times.GetMapData(map);
										if (data == null)
											break;

										embed.AddField(field =>
										{
											field.Name = "Rank";
											field.Value = data.PlayerRank.ToString();
											field.Inline = true;
										});
										embed.AddField(field =>
										{
											field.Name = "Time";
											field.Value = data.Score.AsTimeToString();
											field.Inline = true;
										});
										embed.AddField(field =>
										{
											field.Name = "Date";
											field.Value = data.Date.DateTimeToString();
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
					.Description($"Shows the current top five leaderboard. Try `{Configuration.Default.PrefixCmd + c} <count> <map_name>` to show a specific amount of entries (max. 10).")
					.Parameter("map_name", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						var mapname = e.GetArg("map_name");
						if (string.IsNullOrEmpty(mapname))
						{
							await e.Channel.SendMessage(await Utils.GetDescription(e.Command));
							return;
						}

						// Parsing
						var count = 5U;
						if (uint.TryParse(mapname.Split(' ')[0], out var parsed))
						{
							count = (parsed >= 10) ? 10 : parsed;
							var index = mapname.IndexOf(' ');
							if (index != -1)
								mapname = mapname.Substring(index + 1);
							else
							{
								await e.Channel.SendMessage("You didn't define a map.");
								return;
							}
						}

						var map = await Portal2.GetMapByName(mapname);
						if (map == null)
						{
							await e.Channel.SendMessage("Couldn't find that map.");
							return;
						}

						var board = await _client.GetLeaderboardAsync(map);
						if (board != null)
						{
							var data = board.Take((int)count);
							await Bot.SendAsync(CustomRequest.SendMessage(e.Channel.Id), new CustomMessage(new Embed
							{
								Color = Data.BoardColor.RawValue,
								Title = $"Portal 2 Top {data.Count()}",
								Url = map.Link,
								Image = new EmbedImage(map.ImageLinkFull),
								Footer = new EmbedFooter("board.iverb.me", Data.Portal2IconUrl)
							}
							.AddField(async field =>
							{
								field.Name = map.Alias;
								var output = string.Empty;
								foreach (var chamber in data)
								{
									var temp = $"\n**{chamber.PlayerRank.FormatRankToString()}** {chamber.Score.AsTimeToString()} by {await Utils.AsRawText(chamber.Player.Name)} ({chamber.Date.DateTimeToString()})";
									if ((output.Length + temp.Length) <= DiscordConstants.MaximumCharsPerEmbedField)
										output += temp;
								}
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

		private static Task GetDemoInformation(string c)
		{
			CService.CreateCommand(c)
					.Alias("demoinfo")
					.Description("Returns information about an entry demo with a given changelog id.")
					.Parameter("changelog_id", ParameterType.Required)
					.Do(async e =>
					{
						if (uint.TryParse(e.GetArg("changelog_id"), out var id))
						{
							var content = await _client.GetDemoContentAsync(id);
							if (content != default(string))
							{
								var demo = await SourceDemo.ParseContentAsync(Encoding.UTF8.GetBytes(content));
								await Bot.SendAsync(CustomRequest.SendMessage(e.Channel.Id), new CustomMessage(await Utils.GenerateDemoEmbed(demo)));
							}
							else
								await e.Channel.SendMessage("Couldn't get any information about this id.");
						}
						else
							await e.Channel.SendMessage("Changelog id should be an unsigned integer.");
					});
			return Task.FromResult(0);
		}
	}
}