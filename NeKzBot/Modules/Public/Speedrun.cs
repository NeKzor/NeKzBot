using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using NeKzBot.Classes;
using NeKzBot.Extensions;
using NeKzBot.Resources;
using NeKzBot.Server;
using NeKzBot.Tasks.Speedrun;

namespace NeKzBot.Modules.Public
{
	public class Speedrun : Commands
	{
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Speedrun Module", LogColor.Init);
			await FindGameWorldRecord($"{Configuration.Default.PrefixCmd}wr");
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
								await Bot.SendAsync(CustomRequest.SendMessage(e.Channel.Id), new CustomMessage(new Embed
								{
									Author = new EmbedAuthor(result.Player.Name, result.Player.PlayerLink, result.Player.PlayerAvatar),
									Color = Data.SpeedruncomColor.RawValue,
									Title = $"{result.Game.Name} World Record",
									Url = result.Game.Link,
									Thumbnail = new EmbedThumbnail(result.Game.CoverLink),
									Footer = new EmbedFooter("speedrun.com", Data.SpeedrunComIconUrl),
									Fields = new EmbedField[]
									{
										new EmbedField("Category", result.CategoryName, true),
										new EmbedField("Player", $"{result.Player.Name.Replace("_", "\\_")}{(result.Player.Location != string.Empty ? $" {result.Player.Location}" : string.Empty)}", true),
										new EmbedField("Time", result.EntryTime, true),
										new EmbedField("Date", result.EntryDate, true),
										new EmbedField("Duration", await Utils.GetDuration(result.EntryDateTime.DateTime)),
										new EmbedField("Video", $"[Link]({result.EntryVideo})", true),
										new EmbedField("Comment", (result.EntryComment != string.Empty) ? result.EntryComment : "No comment.", true),
										new EmbedField("Status", result.EntryStatus, true)
									}
								}));
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
					.Description("Shows you the personal bests of a player.")
					.Parameter("player", ParameterType.Required)
					.Do(async e =>
					{
						if (!(string.IsNullOrEmpty(e.Args[0])))
						{
							await e.Channel.SendIsTyping();
							var result = await SpeedrunCom.GetPersonalBestOfPlayerAsync(e.Args[0]);
							if (result == null)
								await e.Channel.SendMessage("Couldn't parse a profile with this name.");
							else if (result.PersonalBests == null)
								await e.Channel.SendMessage("Player doesn't have any personal records.");
							else
							{
								var fullgameruns = string.Empty;
								foreach (var item in result.PersonalBests)
								{
									var temp = $"\n{item.PlayerRank} • {item.Game.Name} • {item.CategoryName} in {item.EntryTime}";
									if ((item.LevelName == null)
									&& (fullgameruns.Length + temp.Length <= DiscordConstants.MaximumCharsPerEmbedField))
										fullgameruns += temp;
								}

								var levelruns = string.Empty;
								foreach (var item in result.PersonalBests)
								{
									var temp = $"\n{item.PlayerRank} • {item.Game.Name} • {item.CategoryName} • {item.LevelName} in {item.EntryTime}";
									if ((item.LevelName == null)
									&& (levelruns.Length + temp.Length <= DiscordConstants.MaximumCharsPerEmbedField))
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
										new EmbedField(result.Name, $"**Id** {result.Id}\n"
																  + $"**Abbreviation** {result.Abbreviation}\n"
																  + $"**Creation Date** {result.CreationDate}\n"
																  + $"**Release Date** {result.ReleaseDate}\n"
																  + $"**Moderator Count** {result.Moderators.Count}\n"
																  + $"**Is Rom Hack?** {result.IsRom}\n"
																  + $"**Default Timing Method** {result.DefaultTimingMethod}\n"
																  + $"**Emulators Allowed?** {result.EmulatorsAllowed}\n"
																  + $"**Requires Verification?** {result.RequiresVerification}\n"
																  + $"**Requires Video?** {result.RequiresVideoProof}\n"
																  + $"**Show Milliseconds?** {result.ShowMilliseconds}")
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
										new EmbedField($"{result.Name}", $"**Id** • {result.Id}\n"
																	   + $"**Location** {(result.Location != string.Empty ? result.Location : "Unknown")}\n"
																	   + $"{(result.Region != string.Empty ? $"**Region** {result.Region}\n" : string.Empty)}"
																	   + $"**Moderator** {result.Mods}\n"
																	   + $"**Personal Records** {result.PersonalBests.Count}\n"
																	   + $"**Role** {result.Role}\n"
																	   + $"**Runs** {result.Runs}\n"
																	   + $"**Join Date** {result.SignUpDate}"
																	   + (result.YouTubeLink != string.Empty ? $"\n[YouTube]({result.YouTubeLink})" : string.Empty)
																	   + (result.TwitchLink != string.Empty ? $"\n[Twitch]({result.TwitchLink})" : string.Empty)
																	   + (result.TwitterLink != string.Empty ? $"\n[Twitter]({result.TwitterLink})" : string.Empty)
																	   + (result.WebsiteLink != string.Empty ? $"\n[Website]({result.WebsiteLink})" : string.Empty))
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
									fields[i] = new EmbedField(wr.CategoryName, $"{wr.EntryTime} by {wr.Player.Name.Replace("_", "\\_")}{(wr.Player.Location != string.Empty ? $" {wr.Player.Location}" : string.Empty)}");
								}

								await Bot.SendAsync(CustomRequest.SendMessage(e.Channel.Id), new CustomMessage(new Embed
								{
									Color = Data.SpeedruncomColor.RawValue,
									Title = $"{result.Game.Name} World Records",
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
						if (!(string.IsNullOrEmpty(e.Args[0])))
						{
							await e.Channel.SendIsTyping();
							var result = await SpeedrunCom.GetTopTenAsync(e.Args[0]);
							if (result == null)
								await e.Channel.SendMessage("Couldn't parse a leaderboard with this name.");
							else
							{
								var entries = string.Empty;
								foreach (var item in result.Entries)
								{
									var temp = $"\n{item.PlayerRank} • {item.PlayerName.Replace("_", "\\_")}{(item.PlayerLocation != string.Empty ? $" {item.PlayerLocation}" : string.Empty)} with {item.EntryTime}";
									if (entries.Length + temp.Length <= DiscordConstants.MaximumCharsPerEmbedField)
										entries += temp;
								}

								await Bot.SendAsync(CustomRequest.SendMessage(e.Channel.Id), new CustomMessage(new Embed
								{
									Color = Data.SpeedruncomColor.RawValue,
									Title = $"{result.Game.Name} Top Ten",
									Url = result.Game.Link,
									Thumbnail = new EmbedThumbnail(result.Game.CoverLink),
									Footer = new EmbedFooter("speedrun.com", Data.SpeedrunComIconUrl),
									Fields = new EmbedField[]
									{
										new EmbedField(result.Entries[0].CategoryName, (entries != string.Empty) ? entries : "None.")
									}
								}));
							}
						}
						else
							await e.User.SendMessage(await Utils.GetDescription(e.Command));
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
									var temp = $"\n{(moderator.Location != string.Empty ? $"{moderator.Location}" : string.Empty)}{moderator.Name.Replace("_", "\\_")}";
									if (output.Length + temp.Length <= DiscordConstants.MaximumCharsPerEmbedField)
										output += temp;
								}

								await Bot.SendAsync(CustomRequest.SendMessage(e.Channel.Id), new CustomMessage(new Embed
								{
									Color = Data.SpeedruncomColor.RawValue,
									Title = $"{result.Name} Moderators",
									Url = result.Link,
									Thumbnail = new EmbedThumbnail(result.CoverLink),
									Footer = new EmbedFooter("speedrun.com", Data.SpeedrunComIconUrl),
									Fields = new EmbedField[]
									{
										new EmbedField("Sorted By Name", output)
									}
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
								var temp = $"\n{nf.Status} | {nf.CreationDate}\n[{nf.Author.Name}]({nf.Author.PlayerLink}) {nf.FormattedText.Replace("\n", " ")}";
								if (output.Length + temp.Length <= DiscordConstants.MaximumCharsPerEmbedField)
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
					await e.Channel.SendMessage($"Couldn't parse a game with this name.{((isil) ? string.Empty : "Game might have a level leaderboard instead: Try `{Configuration.Default.PrefixCmd}{Configuration.Default.PrefixCmd}ilrules <game>` if you haven't already.")}");
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
							new EmbedField("Game", result.Game.Name),
							new EmbedField("Category", result.CategoryName),
							new EmbedField("Rules", (result.ContentRules != string.Empty)
																		 ? await Utils.CutMessage(result.ContentRules.Replace("*", "•").Replace("_", "\\_"), (int)DiscordConstants.MaximumCharsPerEmbedField - "...".Length, "...")
																		 : "No rules have been defined.")
						}
					}));
				}
			}
			else
				await e.Channel.SendMessage(await Utils.GetDescription(e.Command));
		}
	}
}