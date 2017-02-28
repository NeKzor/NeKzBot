using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SpeedrunComSharp;
using NeKzBot.Classes;
using NeKzBot.Resources;
using NeKzBot.Server;

namespace NeKzBot.Tasks.Speedrun
{
	public static partial class SpeedrunCom
	{
		private static SpeedrunComClient _client;
		private static WebHeaderCollection _headers;

		private const int _maxnfcount = 10;
		private const int _maxnffetchcount = 100;

		public static async Task InitAsync()
		{
			await Logger.SendAsync("Initializing SpeedrunCom Client", LogColor.Init);
			_client = new SpeedrunComClient($"{Configuration.Default.AppName}/{Configuration.Default.AppVersion}", Credentials.Default.SpeedruncomToken, 5);

			// Make custom header
			_headers = new WebHeaderCollection
			{
				["Host"] = "www.speedrun.com",
				["Accept"] = "application/json",
				["X-API-Key"] = Credentials.Default.SpeedruncomToken,
				["User-Agent"] = $"{Configuration.Default.AppName}/{Configuration.Default.AppVersion}"
			};
			if (!_client.IsAccessTokenValid)
				await Logger.SendAsync("Invalid Token", LogColor.Error);
		}

		public static async Task<string> GetGameWorldRecordAsync(string gamename)
		{
			var game = _client.Games.SearchGame(gamename);
			if (game == null)
				return "Unknown game.";

			var anyPercent = default(Category);
			try
			{
				anyPercent = game.Categories.FirstOrDefault(category => (category.Type == CategoryType.PerGame) && (category.Runs.Any()));
			}
			catch
			{
				return "**Failed** to find the world record.\n• Game might have a level leaderboard instead.\n• Game doesn't have a world record yet.";
			}

			try
			{
				var wr = anyPercent.WorldRecord;
				var name = game.Name;
				var category = wr.Category.Name;
				// Time
				var time = wr.Times.Primary.Value;
				// Player stats
				var player = wr.Player.Name;
				var country = string.Empty;
				if (wr.Player.IsUser)
				{
					country = wr.Player.User.Location?.Country?.Code?.ToLower() ?? string.Empty;
					if (country != string.Empty)
						country = $" :flag_{country}:";
				}
				// Proof
				var video = $"{wr.Videos.Links.FirstOrDefault().OriginalString}\n";
				// Stats
				var platform = wr.Platform.Name;
				var date = (wr.Date == null)
									? string.Empty
									: $" on {wr.Date.Value.Date.ToString("dd.MM.yyyy")}";
				var sdate = (wr.DateSubmitted == null)
											  ? string.Empty
											  : $"Submitted on {wr.DateSubmitted.Value.Date.ToString("dd.MM.yyyy")}\n";
				// Verified status
				var status = string.Empty;
				if ((wr.Status.Type == RunStatusType.Verified)
				&& (game.Ruleset.RequiresVerification))
				{
					var vdate = (wr.Status.VerifyDate == null)
													  ? string.Empty
													  : " " + wr.Status.VerifyDate.Value.Date.ToString("dd.MM.yyyy");
					var examiner = (wr.Status.ExaminerUserID == null)
															 ? string.Empty
															 : $"Verified by {wr.Status.Examiner.Name}";
					status = examiner + vdate;
				}
				var comment = (string.IsNullOrEmpty(wr.Comment))
									 ? string.Empty
									 : $"\n*{wr.Comment}*";
				return $"**[World Record - *{name}*]**\n"
					+ $"{category} in **{await FormatTime(time)}** by {player + country}\n"
					+ video
					+ $"Played on {platform + date}\n"
					+ sdate
					+ status
					+ comment;
			}
			catch
			{
				return "**Error**";
			}
		}

		public static async Task<string> GetPersonalBestOfPlayerAsync(string name)
		{
			try
			{
				var player = _client.Users.GetUsers(name)?.FirstOrDefault();
				if (player == null)
					return "Player profile doesn't exist.";

				var pbs = default(Record[]);
				if (player.PersonalBests.Count > 0)
					pbs = player.PersonalBests.ToArray();
				else
					return "Player doesn't have any personal records.";

				var country = player.Location?.Country?.Code?.ToLower() ?? string.Empty;
				if (country != string.Empty)
					country = $" :flag_{country}:";
				var output = $"**[Personal Records - *{player.Name}*{country}]**\n";
				foreach (var record in pbs)
				{
					var game = (record.Category.Type == CategoryType.PerGame)
													 ? $"**{record.Game.Name}** {record.Category.Name}"
													 : $"**{record.Game.Name}** {record.Category.Name} • {record.Level.Name}";
					output += $"{await TopTenFormat(record.Rank.ToString())} • {game} in {await FormatTime(record.Times.Primary.Value)}\n";
				}
				return output.Substring(0, output.Length - 1);
			}
			catch
			{
				return "**Error**";
			}
		}

		public static async Task<string> GetGameWorldRecordsAsync(string gamename)
		{
			try
			{
				var game = _client.Games.SearchGame(gamename);
				if (game == null)
					return "Unknown game.";

				var categories = game.Categories.Where(category => category.Type == CategoryType.PerGame);
				var name = game.Name;
				var title = $"**[World Records - *{name}*]**\n";
				var output = title;
				foreach (var cat in categories)
				{
					var wr = cat.WorldRecord;
					// Skip when there is no record
					if (wr == null)
						continue;
					var category = wr.Category.Name;
					// Time
					var time = wr.Times.Primary.Value;
					// Player stats
					var player = wr.Player.Name;
					var country = string.Empty;
					if (wr.Player.IsUser)
					{
						country = wr.Player.User.Location?.Country?.Code?.ToLower() ?? string.Empty;
						if (country != string.Empty)
							country = $" :flag_{country}:";
					}
					// Stats
					var platform = wr.Platform.Name;
					var date = (wr.Date == null)
										? string.Empty
										: $" on {wr.Date.Value.Date.ToString("dd.MM.yyyy")}";
					output += $"{category} in **{await FormatTime(time)}** by {player + country} (Played on {platform + date})\n";
				}
				return (output == title)
							   ? "**Failed** to find the world record.\n• Game might have a level leaderboard instead.\n• Game doesn't have a world record yet."
							   : output.Substring(0, output.Length - 1);
			}
			catch
			{
				return "**Error**";
			}
		}

		public static Task<string> GetPlayerInfo(string playername)
		{
			try
			{
				var player = default(User);
				if (_client.Users.GetUsers(playername).Any())
					player = _client.Users.GetUsers(playername).FirstOrDefault();
				else
					return Task.FromResult("Unknown name.");

				var id = player.ID;
				var name = player.Name;
				var countrycode = player.Location?.Country?.Code?.ToLower() ?? string.Empty;
				var country = string.Empty;
				country = (countrycode != string.Empty)
									   ? $" :flag_{countrycode}:"
									   : string.Empty;
				countrycode = (countrycode != string.Empty)
										   ? $"**Country Code** • {countrycode}\n"
										   : string.Empty;
				var region = player.Location?.Region?.Code?.ToLower() ?? string.Empty;
				region = (region != string.Empty)
								 ? $"**Region Code** • {region}\n"
								 : string.Empty;
				var mods = player.ModeratedGames.Count();
				var pbs = player.PersonalBests.Count;
				var role = player.Role;
				var runs = player.Runs.Count();
				var sudate = player.SignUpDate.Value;
				var yt = (player.YoutubeProfile != null)
												? $"\n**YouTube •<{player.YoutubeProfile.OriginalString}> "
												: string.Empty;
				var twitch = (player.TwitchProfile != null)
												   ? $"\n**Twitch •<{player.TwitchProfile.OriginalString}>"
												   : string.Empty;
				var twitter = (player.TwitterProfile != null)
													 ? $"\n**Twitch •<{player.TwitterProfile.OriginalString}>"
													 : string.Empty;
				var web = $"\n<{player.WebLink.OriginalString}>";
				return Task.FromResult($"**[Player Info - *{name}*{country}]**\n"
									+ $"**Id** • {id}\n"
									+ countrycode
									+ region
									+ $"**Moderator** • {mods}\n"
									+ $"**Personal Records** • {pbs}\n"
									+ $"**Role** • {role}\n"
									+ $"**Runs** • {runs}\n"
									+ $"**Join Date** • {sudate}"
									+ yt
									+ twitch
									+ twitter
									+ web);
			}
			catch
			{
				return Task.FromResult("**Error**");
			}
		}

		public static Task<string> GetGameInfo(string gamename)
		{
			try
			{
				var game = _client.Games.SearchGame(gamename);
				if (game == null)
					return Task.FromResult("Unknown game.");

				// Title
				var name = game.Name;
				// Info
				var id = game.ID;
				var abbr = game.Abbreviation;
				var rdate = game.YearOfRelease;
				var cdate = string.Empty;
				if (game.CreationDate != null)
					cdate = $"**Creation Date** • {game.CreationDate.Value}\n";
				var mods = game.Moderators.Count;
				var isrom = game.IsRomHack;
				// Rules
				var deftiming = game.Ruleset.DefaultTimingMethod;
				var emus = game.Ruleset.EmulatorsAllowed;
				var verification = game.Ruleset.RequiresVerification;
				var vproof = game.Ruleset.RequiresVideo;
				var showms = game.Ruleset.ShowMilliseconds;

				return Task.FromResult($"**[Game Info - *{name}*]**\n"
									 + $"**Id** • {id}\n"
									 + $"**Abbreviation** • {abbr}\n"
									 + cdate
									 + $"**Release Date** • {rdate}\n"
									 + $"**Moderator Count** • {mods}\n"
									 + $"**Is Rom Hack?** • {isrom}\n"
									 + $"**Default Timing Method** • {deftiming}\n"
									 + $"**Emulators Allowed?** • {emus}\n"
									 + $"**Requires Verification?** • {verification}\n"
									 + $"**Requires Video?** • {vproof}\n"
									 + $"**Show Milliseconds?** • {showms}");
			}
			catch
			{
				return Task.FromResult("**Error**");
			}
		}

		public static Task<string> GetModerators(string gamename)
		{
			try
			{
				var output = string.Empty;
				var game = _client.Games.SearchGame(gamename);
				if (game == null)
					return Task.FromResult("Unknown game.");
				foreach (var item in game.Moderators.ToArray())
				{
					var country = item.User.Location?.Country?.Code?.ToLower() ?? string.Empty;
					if (country != string.Empty)
						country = $" :flag_{country}:";
					output += $"{item.Name}{country}\n";
				}
				return Task.FromResult(output.Substring(0, output.Length - 1));
			}
			catch
			{
				return Task.FromResult("**Error**");
			}
		}

		public static Task<string> PlayerHasWorldRecord(string playername)
		{
			try
			{
				var player = default(User);
				if (_client.Users.GetUsers(playername).Any())
					player = _client.Users.GetUsers(playername).FirstOrDefault();
				else
					return Task.FromResult("Unknown name.");

				var pbs = default(Record[]);
				if (player.PersonalBests.Count > 0)
					pbs = player.PersonalBests.ToArray();
				else
					return Task.FromResult("Player doesn't have any personal records.");

				foreach (var record in pbs)
				{
					if (record.Rank != 1)
						continue;
					return Task.FromResult("Yes.");
				}
				return Task.FromResult("No.");
			}
			catch
			{
				return Task.FromResult("**Error**");
			}
		}

		public static async Task<string> GetTopTenAsync(string name)
		{
			try
			{
				var game = _client.Games.SearchGame(name);
				if (game == null)
					return "Unknown game.";

				var output = $"**[Top 10 - *{game.Name}*]**\n";
				var category = game.FullGameCategories.FirstOrDefault(cat => (cat.Type == CategoryType.PerGame) && (cat.Runs.Any()));
				var runs = category.Runs.Where(run => run.Status.Type == RunStatusType.Verified)
										.OrderBy(run => run.Times.Primary.Value.TotalMilliseconds)
										.ToArray();
				var rankcount = (runs.Length >= 10)
											 ? 10
											 : runs.Length;
				var names = new List<string>();
				for (int i = 0, rank = 0; i < rankcount; i++)
				{
					if (!(names.Contains(runs[i].Player.Name)))
					{
						rank++;
						if (rank >= 2)
							if (await IsTied(runs[i].Times.Primary.Value, runs[i - 1].Times.Primary.Value))
								rank--;
						names.Add(runs[i].Player.Name);
						var country = string.Empty;
						if (runs[i].Player.IsUser)
						{
							country = runs[i].Player.User.Location?.Country?.Code?.ToLower() ?? string.Empty;
							if (country != string.Empty)
								country = $" :flag_{country}:";
						}
						output += $"{await TopTenFormat(rank.ToString(), false)} **{runs[i].Player.Name}**{country} in {await FormatTime(runs[i].Times.Primary.Value)}\n";
					}
					else
						rankcount++;
				}
				return output.Substring(0, output.Length - 1);
			}
			catch
			{
				return "**Error**";
			}
		}

		public static async Task<string> GetLastNotificationAsync(string scount = null, string nftype = null)
		{
			try
			{
				// Check parameter <count>
				if (scount == null)
					scount = _maxnfcount.ToString();
				else if (string.Equals(scount, "x", StringComparison.CurrentCultureIgnoreCase))
					scount = _maxnfcount.ToString();
				else if (!(new Regex("^[1-9]").IsMatch(scount)))
					return "Invalid notification count. Use numbers 1-9 only.";
				nftype = nftype ?? "any";

				// Web request
				var json = await Fetching.GetStringAsync($"https://www.speedrun.com/api/v1/notifications?max={_maxnffetchcount}", _headers);
				if (string.IsNullOrEmpty(json))
					return null;

				// Read
				dynamic api = JsonConvert.DeserializeObject(json);
				if (string.IsNullOrEmpty(api?.ToString()))
					return null;

				// Parse data
				var output = $"**[Latest Notifications (Type: {nftype})]**\n";
				var count = Convert.ToInt16(scount);
				for (int i = 0; i < count; i++)
				{
					var data = api.data[i];
					var id = data?.id?.ToString();
					var created = data?.created?.ToString() ?? string.Empty;
					var status = data?.status?.ToString() ?? string.Empty;
					var text = data?.text?.ToString() ?? string.Empty;
					var type = data?.item?.rel.ToString() ?? string.Empty;
					var url = data?.item?.uri?.ToString() ?? string.Empty;

					// Filter
					if ((nftype == "any")
					|| (nftype == type))
						output += $"{(status == "read" ? "Read" : "Unread")} | {created} | {text}\n";
					else
						count++;
				}
				return await Utils.CutMessage(output.Replace("_", "\\_"), 1);
			}
			catch (Exception e)
			{
				await Logger.SendToChannelAsync("SpeedrunCom.GetLastNotificationAsync Error", e);
			}
			return "**Error**";
		}

		public static async Task<List<SpeedrunNotification>> GetNotificationUpdatesAsync(uint count)
		{
			try
			{
				var json = await Fetching.GetStringAsync($"https://www.speedrun.com/api/v1/notifications?max={_maxnffetchcount}", _headers);

				// Read
				if (string.IsNullOrEmpty(json))
					return null;

				// Read json string
				dynamic api = JsonConvert.DeserializeObject(json);
				if (string.IsNullOrEmpty(api?.ToString()))
					return null;

				// Parse data
				var updates = new List<SpeedrunNotification>();
				foreach (var data in api.data)
				{
					if (updates.Count == count)
						break;

					var notification = new SpeedrunNotification()
					{
						CreationDate = data?.created.ToString() ?? string.Empty,
						ContentText = data?.text.ToString() ?? string.Empty,
						Type = (SpeedrunNotificationType)data?.item?.rel,
						ContentLink = data?.item?.uri?.ToString() ?? string.Empty
					};

					// Filtering
					if (notification.Type != SpeedrunNotificationType.Post)
					{
						// Eh, blame the guy who designed the website/api :c
						await notification.BuildGame();
						var game = _client.Games.SearchGame(notification.Game.Name);
						if (game != null)
						{
							notification.Game.Link = game?.WebLink?.AbsoluteUri ?? string.Empty;
							notification.Game.CoverLink = game?.Assets?.CoverSmall?.Uri?.AbsoluteUri ?? string.Empty;
						}
						await notification.BuildCache();
						updates.Add(notification);
					}
				}
				return updates;
			}
			catch (Exception e)
			{
				return await Logger.SendToChannelAsync("SpeedrunCom.GetNotificationUpdatesAsync Error", e) as List<SpeedrunNotification>;
			}
		}

		public static Task<string> GetGameRules(string gName, bool il = false)
		{
			try
			{
				var game = _client.Games.SearchGame(gName);
				if (game == null)
					return Task.FromResult("Unknown game.");

				var rules = string.Empty;
				foreach (var category in game.Categories.ToArray())
				{
					if (il)
					{
						if ((string.IsNullOrEmpty(category.Rules))
						|| (category.Type != CategoryType.PerLevel))
							continue;
					}
					else
					{
						if ((string.IsNullOrEmpty(category.Rules))
						|| (category.Type != CategoryType.PerGame))
							continue;
					}
					rules = category.Rules;
					if (rules != string.Empty)
						break;
				}
				return Task.FromResult((rules == string.Empty)
											  ? "No rules have been defined."
											  : $"*{rules}*");
			}
			catch
			{
				return Task.FromResult($"**Error.** Try `{Configuration.Default.PrefixCmd}{Configuration.Default.PrefixCmd}ilrules <game>` if you haven't already.");
			}
		}

		// Formatting
		private static Task<string> FormatTime(TimeSpan time)
		{
			var h = time.Hours.ToString();
			var m = time.Minutes.ToString();
			var s = time.Seconds.ToString();
			var ms = time.Milliseconds.ToString();
			var output = (h == "0")
							? string.Empty
							: $"{h}:";
			output += ((m == "0")
			&& (output == string.Empty))
					   ? string.Empty
					   : (m.Length == 1)
								   ? $"0{m}:"
								   : $"{m}:";
			output += (s.Length == 1)
								? (output == string.Empty)
										  ? s
										  : $"0{s}"
								: s;
			output += (ms == "0")
						  ? (output.Length <= 2)
										   ? "s"
										   : string.Empty
						  : $".{ms}";
			return Task.FromResult(output);
		}

		private static Task<string> TopTenFormat(string rank, bool trohpy = true, bool allbold = false)
		{
			var output = $"**{rank}th**";
			if (Convert.ToInt16(rank) > 10)
			{
				output = (allbold)
							? $"**#{rank}**"
							: output = $"#{rank}";
			}
			else if (rank == "1")
			{
				output = (trohpy)
							? "**1st** :trophy:"
							: "**1st**";
			}
			else if (rank == "2")
				output = "**2nd**";
			else if (rank == "3")
				output = "**3rd**";
			return Task.FromResult(output);
		}

		// Others
		private static Task<bool> IsTied(TimeSpan t1, TimeSpan t2)
			=> Task.FromResult(t1.TotalMilliseconds == t2.TotalMilliseconds);
	}
}