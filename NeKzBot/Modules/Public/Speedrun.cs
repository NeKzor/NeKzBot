using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using NeKzBot.Classes;
using NeKzBot.Extensions;
using NeKzBot.Resources;
using NeKzBot.Server;
using NeKzBot.Tasks.Speedrun;
using NeKzBot.Utilities;

namespace NeKzBot.Modules.Public
{
	public class Speedrun : CommandModule
	{
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Speedrun Module", LogColor.Init);
			await FindGameWorldRecord($"{Configuration.Default.PrefixCmd}wr");
			await FindGameWorldRecordWithCategory($"{Configuration.Default.PrefixCmd}category");
			await FindPlayerPersonalBests($"{Configuration.Default.PrefixCmd}pbs");
			await FindGame($"{Configuration.Default.PrefixCmd}game");
			await FindPlayer($"{Configuration.Default.PrefixCmd}player");
			await FindModerators($"{Configuration.Default.PrefixCmd}moderators");
			await GetTopTen($"{Configuration.Default.PrefixCmd}top");
			await GetWorldRecordStatus($"{Configuration.Default.PrefixCmd}haswr");
			await GetAllGameWorldRecords($"{Configuration.Default.PrefixCmd}wrs");
			await GetFullGameRules($"{Configuration.Default.PrefixCmd}rules");
			await GetIndividualLevelRules($"{Configuration.Default.PrefixCmd}ilrules");
			await GetNotification($"{Configuration.Default.PrefixCmd}notification");
			await GetCategories($"{Configuration.Default.PrefixCmd}categories");
		}

		private static Task FindGameWorldRecord(string c)
		{
			CService.CreateCommand(c)
					.Description("Returns the latest world record of the fastest category.")
					.Parameter("game", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (!(string.IsNullOrEmpty(e.Args[0])))
						{
							var result = await SpeedrunCom.GetGameWorldRecordAsync(e.Args[0]);
							if (result == null)
								await e.Channel.SendMessage("Couldn't parse a game with this name.");
							else if (result.Game == null)
								await e.Channel.SendMessage("Couldn't find the world record:\n• Game might have a level leaderboard instead.\n• Game doesn't have a world record yet.");
							else
							{
								var embed = new Embed
								{
									Color = Data.SpeedruncomColor.RawValue,
									Title = $"{await Utils.AsRawText(result.Game.Name)} World Record",
									Url = result.Game.Link,
									Thumbnail = new EmbedThumbnail(result.Game.CoverLink),
									Footer = new EmbedFooter("speedrun.com", Data.SpeedrunComIconUrl)
								};
								var partners = result.Players.Count();
								if (partners == 1)
									embed.WithAuthor(new EmbedAuthor(result.Players.First().Name, result.Players.First().PlayerLink, result.Players.First().PlayerAvatar));
								embed.AddField(async field =>
								{
									field.Name = "Category";
									field.Value = await Utils.AsRawText(result.CategoryName);
									field.Inline = true;
								});
								embed.AddField(async field =>
								{
									if (partners > 1)
									{
										field.Name = "Players";
										var output = string.Empty;
										foreach (var player in result.Players.Take(2))
											output += $"\n{await Utils.AsRawText(player.Name)}{((player.Location != string.Empty) ? $" {player.Location}" : string.Empty)}";
										field.Value = (partners > 2)
																? $"{output}\n[and {partners - 2} other{((partners - 2 == 1) ? string.Empty : "s")}]({result.EntryLink})"
																: output;
									}
									else if (partners == 1)
									{
										field.Name = "Player";
										field.Value = $"{await Utils.AsRawText(result.Players.First().Name)}{((result.Players.First().Location != string.Empty) ? $" {result.Players.First().Location}" : string.Empty)}";
									}
									else
									{
										field.Name = "Error";
										field.Name = "Sry :(";
									}
									field.Inline = true;
								});
								embed.AddField(field =>
								{
									field.Name = "Time";
									field.Value = result.EntryTime;
									field.Inline = true;
								});
								embed.AddField(field =>
								{
									field.Name = "Date";
									field.Value = result.EntryDate;
									field.Inline = true;
								});
								embed.AddField(async field =>
								{
									field.Name = "Duration";
									field.Value = await Utils.GetDurationAsync(result.EntryDateTime.DateTime);
								});
								embed.AddField(field =>
								{
									field.Name = "Video";
									field.Value = (string.IsNullOrEmpty(result.EntryVideo))
														 ? "_Not available._"
														 : $"[Link]({result.EntryVideo})";
									field.Inline = true;
								});
								embed.AddField(async field =>
								{
									field.Name = "Comment";
									field.Value = (result.EntryComment != string.Empty)
																	   ? await Utils.AsRawText(result.EntryComment)
																	   : "_No comment._";
									field.Inline = true;
								});
								embed.AddField(async field =>
								{
									field.Name = "Status";
									field.Value = await Utils.AsRawText(result.EntryStatus);
									field.Inline = true;
								});

								await Bot.SendAsync(CustomRequest.SendMessage(e.Channel.Id), new CustomMessage(embed));
							}
						}
						else
							await e.Channel.SendMessage(await Utils.GetDescription(e.Command));
					});
			return Task.FromResult(0);
		}

		private static Task FindGameWorldRecordWithCategory(string c)
		{
			CService.CreateCommand(c)
					.Alias($"{Configuration.Default.PrefixCmd}cat")
					.Description("Returns the latest world record of the specified category.")
					.Parameter("game_abbreviation", ParameterType.Required)
					.Parameter("category", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (!(string.IsNullOrEmpty(e.Args[0])))
						{
							var result = await SpeedrunCom.GetGameWorldRecordAsync(e.GetArg("game_abbreviation"), e.GetArg("category"));
							if ((result == null)
							|| (result?.Game == null))
							{
								await e.Channel.SendMessage("Couldn't parse a game or category with these parameters. " +
															$"If you don't know the exact category name try `{Configuration.Default.PrefixCmd}{Configuration.Default.PrefixCmd}categories <game>` to list all categories.");
							}
							else
							{
								var embed = new Embed
								{
									Color = Data.SpeedruncomColor.RawValue,
									Title = $"{await Utils.AsRawText(result.Game.Name)} World Record",
									Url = result.Game.Link,
									Thumbnail = new EmbedThumbnail(result.Game.CoverLink),
									Footer = new EmbedFooter("speedrun.com", Data.SpeedrunComIconUrl)
								};
								var partners = result.Players.Count();
								if (partners == 1)
									embed.WithAuthor(new EmbedAuthor(result.Players.First().Name, result.Players.First().PlayerLink, result.Players.First().PlayerAvatar));
								embed.AddField(async field =>
								{
									field.Name = "Category";
									field.Value = await Utils.AsRawText(result.CategoryName);
									field.Inline = true;
								});
								embed.AddField(async field =>
								{
									if (partners > 1)
									{
										field.Name = "Players";
										var output = string.Empty;
										foreach (var player in result.Players.Take(2))
											output += $"\n{await Utils.AsRawText(player.Name)}{((player.Location != string.Empty) ? $" {player.Location}" : string.Empty)}";
										field.Value = (partners > 2)
																? $"{output}\n[and {partners - 2} other{((partners - 2 == 1) ? string.Empty : "s")}]({result.EntryLink})"
																: output;
									}
									else if (partners == 1)
									{
										field.Name = "Player";
										field.Value = $"{await Utils.AsRawText(result.Players.First().Name)}{(result.Players.First().Location != string.Empty ? $" {result.Players.First().Location}" : string.Empty)}";
									}
									else
									{
										field.Name = "Error";
										field.Name = "Sry :(";
									}
									field.Inline = true;
								});
								embed.AddField(field =>
								{
									field.Name = "Time";
									field.Value = result.EntryTime;
									field.Inline = true;
								});
								embed.AddField(field =>
								{
									field.Name = "Date";
									field.Value = result.EntryDate;
									field.Inline = true;
								});
								embed.AddField(async field =>
								{
									field.Name = "Duration";
									field.Value = await Utils.GetDurationAsync(result.EntryDateTime.DateTime);
								});
								embed.AddField(field =>
								{
									field.Name = "Video";
									field.Value = (string.IsNullOrEmpty(result.EntryVideo))
														 ? "_Not available._"
														 : $"[Link]({result.EntryVideo})";
									field.Inline = true;
								});
								embed.AddField(async field =>
								{
									field.Name = "Comment";
									field.Value = (result.EntryComment != string.Empty)
																	   ? await Utils.AsRawText(result.EntryComment)
																	   : "_No comment._";
									field.Inline = true;
								});
								embed.AddField(async field =>
								{
									field.Name = "Status";
									field.Value = await Utils.AsRawText(result.EntryStatus);
									field.Inline = true;
								});

								await Bot.SendAsync(CustomRequest.SendMessage(e.Channel.Id), new CustomMessage(embed));
							}
						}
						else
							await e.Channel.SendMessage(await Utils.GetDescription(e.Command));
					});
			return Task.FromResult(0);
		}

		private static Task FindPlayerPersonalBests(string c)
		{
			CService.CreateCommand(c)
					.Alias($"{Configuration.Default.PrefixCmd}pb")
					.Description("Shows you the personal bests of a player (best five of full game runs and level runs). You can filter your records with these keywords: `best`, `worst`, `oldest`.")
					.Parameter("player", ParameterType.Required)
					.Parameter("filter", ParameterType.Optional)
					.Do(async e =>
					{
						if (!(string.IsNullOrEmpty(e.Args[0])))
						{
							await e.Channel.SendIsTyping();
							var result = default(SpeedrunPlayerProfile);
							var player = e.GetArg("player");
							var filter = e.GetArg("filter");
							if (!(string.IsNullOrEmpty(filter)))
							{
								if (string.Equals(filter, "best", StringComparison.CurrentCultureIgnoreCase))
									result = await SpeedrunCom.GetPersonalBestOfPlayerAsync(player, PersonalBestFilter.Best);
								else if (string.Equals(filter, "worst", StringComparison.CurrentCultureIgnoreCase))
									result = await SpeedrunCom.GetPersonalBestOfPlayerAsync(player, PersonalBestFilter.Worst);
								else if (string.Equals(filter, "oldest", StringComparison.CurrentCultureIgnoreCase))
									result = await SpeedrunCom.GetPersonalBestOfPlayerAsync(player, PersonalBestFilter.Oldest);
								else
								{
									await e.Channel.SendMessage("This filter name does not exist. Try one of these: `best`, `worst`, `oldest`.");
									return;
								}
							}
							else
								result = await SpeedrunCom.GetPersonalBestOfPlayerAsync(player);

							if (result == null)
								await e.Channel.SendMessage("Couldn't parse a profile with this name.");
							else if (result.PersonalBests == null)
								await e.Channel.SendMessage("Player doesn't have any personal records.");
							else
							{
								var fullgameruns = string.Empty;
								foreach (var item in result.PersonalBests)
								{
									var temp = $"\n{item.PlayerRank} • {await Utils.AsRawText(item.Game.Name)} • {await Utils.AsRawText(item.CategoryName)} in {item.EntryTime}";
									if ((item.LevelName == null)
									&& ((fullgameruns.Length + temp.Length) <= DiscordConstants.MaximumCharsPerEmbedField))
										fullgameruns += temp;
								}

								var levelruns = string.Empty;
								foreach (var item in result.PersonalBests)
								{
									var temp = $"\n{item.PlayerRank} • {await Utils.AsRawText(item.Game.Name)} • {await Utils.AsRawText(item.CategoryName)} • {await Utils.AsRawText(item.LevelName)} in {item.EntryTime}";
									if ((item.LevelName != null)
									&& ((levelruns.Length + temp.Length) <= DiscordConstants.MaximumCharsPerEmbedField))
										levelruns += temp;
								}

								await Bot.SendAsync(CustomRequest.SendMessage(e.Channel.Id), new CustomMessage(new Embed
								{
									Author = new EmbedAuthor(result.Name, result.PlayerLink, result.PlayerAvatar),
									Color = Data.SpeedruncomColor.RawValue,
									Title = "Personal Records",
									Url = result.PlayerLink,
									Footer = new EmbedFooter("speedrun.com", Data.SpeedrunComIconUrl),
									Fields = new EmbedField[]
									{
										new EmbedField("Full Game Runs", (fullgameruns != string.Empty) ? fullgameruns : "None."),
										new EmbedField("Level Runs", (levelruns != string.Empty) ? levelruns : "None.")
									}
								}));
							}
						}
						else
							await e.Channel.SendMessage(await Utils.GetDescription(e.Command));
					});
			return Task.FromResult(0);
		}

		private static Task FindGame(string c)
		{
			CService.CreateCommand(c)
					.Description("Returns some info about the game.")
					.Parameter("game", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (!(string.IsNullOrEmpty(e.Args[0])))
						{
							var result = await SpeedrunCom.GetGameInfoAsync(e.Args[0]);
							if (result == null)
								await e.Channel.SendMessage("Couldn't parse game information with this name.");
							else
							{
								await Bot.SendAsync(CustomRequest.SendMessage(e.Channel.Id), new CustomMessage(new Embed
								{
									Color = Data.SpeedruncomColor.RawValue,
									Title = "Game Info",
									Url = result.Link,
									Thumbnail = new EmbedThumbnail(result.CoverLink),
									Footer = new EmbedFooter("speedrun.com", Data.SpeedrunComIconUrl),
									Fields = new EmbedField[]
									{
										new EmbedField(await Utils.AsRawText(result.Name), $"**Id** {result.Id}\n"
																						 + $"**Abbreviation** {await Utils.AsRawText(result.Abbreviation)}\n"
																						 + $"**Creation Date** {result.CreationDate}\n"
																						 + $"**Release Date** {result.ReleaseDate}\n"
																						 + $"**Moderator Count** {result.Moderators.Count}\n"
																						 + $"**Is Rom Hack?** {(result.IsRom ? "Yes" : "No")}\n"
																						 + $"**Default Timing Method** {result.DefaultTimingMethod}\n"
																						 + $"**Emulators Allowed?** {(result.EmulatorsAllowed ? "Yes" : "No")}\n"
																						 + $"**Requires Verification?** {(result.RequiresVerification ? "Yes" : "No")}\n"
																						 + $"**Requires Video?** {(result.RequiresVideoProof ? "Yes" : "No")}\n"
																						 + $"**Show Milliseconds?** {(result.ShowMilliseconds ? "Yes" : "No")}")
									}
								}));
							}
						}
						else
							await e.Channel.SendMessage(await Utils.GetDescription(e.Command));
					});
			return Task.FromResult(0);
		}

		private static Task FindPlayer(string c)
		{
			CService.CreateCommand(c)
					.Description("Returns some info about a player.")
					.Parameter("player", ParameterType.Required)
					.Do(async e =>
					{
						if (!(string.IsNullOrEmpty(e.Args[0])))
						{
							await e.Channel.SendIsTyping();
							var result = await SpeedrunCom.GetPlayerInfoAsync(e.Args[0]);
							if (result == null)
								await e.Channel.SendMessage("Couldn't parse a profile with this name.");
							else
							{
								await Bot.SendAsync(CustomRequest.SendMessage(e.Channel.Id), new CustomMessage(new Embed
								{
									Author = new EmbedAuthor(result.Name, result.PlayerLink, result.PlayerAvatar),
									Color = Data.SpeedruncomColor.RawValue,
									Title = "Player Profile",
									Url = result.PlayerLink,
									Footer = new EmbedFooter("speedrun.com", Data.SpeedrunComIconUrl),
									Fields = new EmbedField[]
									{
										new EmbedField(await Utils.AsRawText(result.Name), $"**Id** • {result.Id}\n"
																						 + $"**Location** {((result.Location != string.Empty) ? result.Location : "Unknown")}\n"
																						 + $"{((result.Region != string.Empty) ? $"**Region** {result.Region}\n" : string.Empty)}"
																						 + $"**Moderator Of** {result.Mods} Games\n"
																						 + $"**Personal Records** {result.PersonalBests.Count}\n"
																						 + $"**Role** {result.Role}\n"
																						 + $"**Runs** {result.Runs}\n"
																						 + $"**Join Date** {result.SignUpDate}"
																						 + ((result.YouTubeLink != string.Empty) ? $"\n[YouTube]({result.YouTubeLink})" : string.Empty)
																						 + ((result.TwitchLink != string.Empty) ? $"\n[Twitch]({result.TwitchLink})" : string.Empty)
																						 + ((result.TwitterLink != string.Empty) ? $"\n[Twitter]({result.TwitterLink})" : string.Empty)
																						 + ((result.WebsiteLink != string.Empty) ? $"\n[Website]({result.WebsiteLink})" : string.Empty))
									}
								}));
							}
						}
						else
							await e.Channel.SendMessage(await Utils.GetDescription(e.Command));
					});
			return Task.FromResult(0);
		}

		private static Task GetAllGameWorldRecords(string c)
		{
			CService.CreateCommand(c)
					.Description("Returns all world record of each category.")
					.Parameter("game", ParameterType.Unparsed)
					.Do(async e =>
					{
						if (!(string.IsNullOrEmpty(e.Args[0])))
						{
							await e.Channel.SendIsTyping();
							var result = await SpeedrunCom.GetGameWorldRecordsAsync(e.Args[0]);
							if (result == null)
								await e.Channel.SendMessage("Couldn't parse a game with this name.");
							else if (result.WorldRecords?.Count < 1)
								await e.Channel.SendMessage("There are no world records for this game.");
							else
							{
								var list = result.WorldRecords.Take((int)DiscordConstants.MaximumFieldsInEmbed);
								var fields = new EmbedField[list.Count()];
								for (int i = 0; i < list.Count(); i++)
								{
									var wr = list.ElementAt(i);
									var output = string.Empty;
									var partners = wr.Players.Count();
									if (partners == 1)
										output = $"{wr.EntryTime} {await Utils.AsRawText(wr.Players.First().Name)}{((wr.Players.First().Location != string.Empty) ? $" {wr.Players.First().Location}" : string.Empty)}";
									else if (partners > 1)
									{
										output += wr.EntryTime;
										foreach (var player in wr.Players.Take(2))
											output += $"  {await Utils.AsRawText(player.Name)}{((player.Location != string.Empty) ? $" {player.Location}" : string.Empty)}";
										output += (partners > 2)
															? $"  [and {partners - 2} other{((partners - 2 == 1) ? string.Empty : "s")}]({wr.EntryLink})"
															: string.Empty;
									}
									fields[i] = new EmbedField(await Utils.AsRawText(wr.CategoryName), (output != string.Empty)
																											   ? output
																											   : "Error\nSry :(");
								}

								await Bot.SendAsync(CustomRequest.SendMessage(e.Channel.Id), new CustomMessage(new Embed
								{
									Color = Data.SpeedruncomColor.RawValue,
									Title = $"{await Utils.AsRawText(result.Game.Name)} World Records",
									Url = result.Game.Link,
									Thumbnail = new EmbedThumbnail(result.Game.CoverLink),
									Footer = new EmbedFooter("speedrun.com", Data.SpeedrunComIconUrl),
									Fields = fields
								}));
							}
						}
						else
							await e.Channel.SendMessage(await Utils.GetDescription(e.Command));
					});
			return Task.FromResult(0);
		}

		private static Task GetTopTen(string c)
		{
			CService.CreateCommand(c)
					.Alias($"{Configuration.Default.PrefixCmd}top10", $"{Configuration.Default.PrefixCmd}topten", $"{Configuration.Default.PrefixCmd}10")
					.Description("Returns the top ten ranking of a game.")
					.Parameter("game", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (!(string.IsNullOrEmpty(e.Args[0])))
						{
							var msg = await e.Channel.SendMessage("This might take a while.");
							var result = await SpeedrunCom.GetTopTenAsync(e.Args[0]);
							if (result == null)
								await msg.Edit("Couldn't parse a leaderboard with this name.");
							else
							{
								var entries = string.Empty;
								foreach (var item in result.Entries)
								{
									var temp = $"\n{item.PlayerRank} • {await Utils.AsRawText(item.PlayerName)}{((item.PlayerLocation != string.Empty) ? $" {item.PlayerLocation}" : string.Empty)} with {item.EntryTime}";
									if ((entries.Length + temp.Length) <= DiscordConstants.MaximumCharsPerEmbedField)
										entries += temp;
								}
								await msg.Delete();
								await Bot.SendAsync(CustomRequest.SendMessage(e.Channel.Id), new CustomMessage(new Embed
								{
									Color = Data.SpeedruncomColor.RawValue,
									Title = $"{await Utils.AsRawText(result.Game.Name)} Top Ten",
									Url = result.Game.Link,
									Thumbnail = new EmbedThumbnail(result.Game.CoverLink),
									Footer = new EmbedFooter("speedrun.com", Data.SpeedrunComIconUrl),
									Fields = new EmbedField[] { new EmbedField(await Utils.AsRawText(result.Entries[0].CategoryName), (entries != string.Empty) ? entries : "None.") }
								}));
							}
						}
						else
							await e.Channel.SendMessage(await Utils.GetDescription(e.Command));
					});
			return Task.FromResult(0);
		}

		private static Task FindModerators(string c)
		{
			CService.CreateCommand(c)
					.Alias($"{Configuration.Default.PrefixCmd}mods")
					.Description("Returns the moderator list of a game.")
					.Parameter("game", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (!(string.IsNullOrEmpty(e.Args[0])))
						{
							var result = await SpeedrunCom.GetModeratorsAsync(e.Args[0]);
							if (result == null)
								await e.Channel.SendMessage("Couldn't parse a leaderboard with this name.");
							else
							{
								var output = string.Empty;
								foreach (var moderator in result.Moderators.OrderBy(mod => mod.Name))
								{
									var temp = $"\n{((moderator.Location != string.Empty) ? $"{moderator.Location}  " : string.Empty)}{await Utils.AsRawText(moderator.Name)}";
									if ((output.Length + temp.Length) <= DiscordConstants.MaximumCharsPerEmbedField)
										output += temp;
								}

								await Bot.SendAsync(CustomRequest.SendMessage(e.Channel.Id), new CustomMessage(new Embed
								{
									Color = Data.SpeedruncomColor.RawValue,
									Title = $"{await Utils.AsRawText(result.Name)} Moderators",
									Url = result.Link,
									Thumbnail = new EmbedThumbnail(result.CoverLink),
									Footer = new EmbedFooter("speedrun.com", Data.SpeedrunComIconUrl),
									Fields = new EmbedField[] { new EmbedField("Sorted By Name", output) }
								}));
							}
						}
						else
							await e.Channel.SendMessage(await Utils.GetDescription(e.Command));
					});
			return Task.FromResult(0);
		}

		private static Task GetWorldRecordStatus(string c)
		{
			CService.CreateCommand(c)
					.Alias($"{Configuration.Default.PrefixCmd}haswr?", $"{Configuration.Default.PrefixCmd}wr?", $"{Configuration.Default.PrefixCmd}isfast?")
					.Description("Checks if a player has a world record.")
					.Parameter("player", ParameterType.Required)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"**{await SpeedrunCom.PlayerHasWorldRecord(e.Args[0])}**");
					});
			return Task.FromResult(0);
		}

		private static Task GetNotification(string c)
		{
			CService.CreateCommand(c)
					.Alias($"{Configuration.Default.PrefixCmd}nf", $"{Configuration.Default.PrefixCmd}news")
					.Description("Returns latest notifications. Enter the keyword _x_ to skip the count parameter.")
					.Parameter("count", ParameterType.Required)
					.Parameter("type", ParameterType.Optional)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						var type = e.GetArg("type");
						var result = new List<SpeedrunNotification>();
						if (!(string.IsNullOrEmpty(type)))
							result = await SpeedrunCom.GetLastNotificationAsync(e.GetArg("count"), type);
						else
							result = await SpeedrunCom.GetLastNotificationAsync(e.GetArg("count"));

						if ((result == null)
						|| (result?.Count < 1))
							await e.Channel.SendMessage("Couldn't get any notifications.");
						else
						{
							var output = string.Empty;
							foreach (var nf in result)
							{
								var temp = $"\n{nf.Status} | {nf.CreationDate}\n[{await Utils.AsRawText(nf.Author.Name)}]({nf.Author.PlayerLink}) {nf.FormattedText.Replace("\n", " ")}";
								if ((output.Length + temp.Length) <= DiscordConstants.MaximumCharsPerEmbedField)
									output += temp;
							}

							await Bot.SendAsync(CustomRequest.SendMessage(e.Channel.Id), new CustomMessage(new Embed
							{
								Color = Data.SpeedruncomColor.RawValue,
								Title = "Latest Notifications",
								Url = "https://speedrun.com",
								Footer = new EmbedFooter("speedrun.com", Data.SpeedrunComIconUrl),
								Fields = new EmbedField[] { new EmbedField($"Type: {(string.IsNullOrEmpty(type) ? "any" : type.ToLower())}", output) }
							}));
						}
					});
			return Task.FromResult(0);
		}

		private static Task GetFullGameRules(string c)
		{
			CService.CreateCommand(c)
					.Alias($"{Configuration.Default.PrefixCmd}rules?")
					.Description("Returns the main rules of a game.")
					.Parameter("game", ParameterType.Unparsed)
					.Do(async e => await GetRules(e));
			return Task.FromResult(0);
		}

		private static Task GetIndividualLevelRules(string c)
		{
			CService.CreateCommand(c)
					.Alias($"{Configuration.Default.PrefixCmd}ilrules?")
					.Description("Returns the main IL rules of a game.")
					.Parameter("game", ParameterType.Unparsed)
					.Do(async e => await GetRules(e, true));
			return Task.FromResult(0);
		}

		private static async Task GetRules(CommandEventArgs e, bool isil = false)
		{
			if (!(string.IsNullOrEmpty(e.Args[0])))
			{
				await e.Channel.SendIsTyping();
				var result = await SpeedrunCom.GetGameRulesAsync(e.Args[0], isil);
				if (result == null)
					await e.Channel.SendMessage($"Couldn't parse a game with this name.{((isil) ? string.Empty : " Game might have a level leaderboard instead: Try `{Configuration.Default.PrefixCmd}{Configuration.Default.PrefixCmd}ilrules <game>` if you haven't already.")}");
				else if (result.ContentRules == null)
					await e.Channel.SendMessage("No rules have been defined.");
				else
				{
					await Bot.SendAsync(CustomRequest.SendMessage(e.Channel.Id), new CustomMessage(new Embed
					{
						Color = Data.SpeedruncomColor.RawValue,
						Title = "Speedrun Rules",
						Url = result.Game.Link,
						Thumbnail = new EmbedThumbnail(result.Game.CoverLink),
						Footer = new EmbedFooter("speedrun.com", Data.SpeedrunComIconUrl),
						Fields = new EmbedField[]
						{
							new EmbedField("Game", await Utils.AsRawText(result.Game.Name)),
							new EmbedField("Category", await Utils.AsRawText(result.CategoryName)),
							new EmbedField("Rules", (result.ContentRules != string.Empty)
																		 ? await Utils.CutMessageAsync(result.ContentRules, (int)DiscordConstants.MaximumCharsPerEmbedField - "...".Length, "...")
																		 : "No rules have been defined.")
						}
					}));
				}
			}
			else
				await e.Channel.SendMessage(await Utils.GetDescription(e.Command));
		}

		private static Task GetCategories(string c)
		{
			CService.CreateCommand(c)
					.Alias($"{Configuration.Default.PrefixCmd}cats")
					.Description("Returns the category list of a game.")
					.Parameter("game", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (!(string.IsNullOrEmpty(e.Args[0])))
						{
							var result = await SpeedrunCom.GetCategoriesAsync(e.Args[0]);
							if (result == null)
								await e.Channel.SendMessage("Couldn't parse a leaderboard with this name.");
							else
							{
								var output = string.Empty;
								foreach (var category in result.Categories.OrderBy(cat => cat.Name))
								{
									var temp = $"\n{await Utils.AsRawText(category.Name)}";
									if ((output.Length + temp.Length) <= DiscordConstants.MaximumCharsPerEmbedField)
										output += temp;
								}

								await Bot.SendAsync(CustomRequest.SendMessage(e.Channel.Id), new CustomMessage(new Embed
								{
									Color = Data.SpeedruncomColor.RawValue,
									Title = $"{await Utils.AsRawText(result.Name)} Categories",
									Url = result.Link,
									Thumbnail = new EmbedThumbnail(result.CoverLink),
									Footer = new EmbedFooter("speedrun.com", Data.SpeedrunComIconUrl),
									Fields = new EmbedField[] { new EmbedField("Sorted By Name", output) }
								}));
							}
						}
						else
							await e.Channel.SendMessage(await Utils.GetDescription(e.Command));
					});
			return Task.FromResult(0);
		}
	}
}