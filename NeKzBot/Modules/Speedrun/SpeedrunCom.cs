using System.Net;
using System.Linq;
using SpeedrunComSharp;
using System.Threading.Tasks;

namespace NeKzBot
{
	public partial class SpeedrunCom
	{
		private static SpeedrunComClient srcClient;
		private static WebHeaderCollection srcHeaders;
		private const int maxnfcount = 10;
		private const int maxnffetchcount = 100;

		public static void Init()
		{
			Logging.CON("Initializing speedruncom client", System.ConsoleColor.DarkYellow);
			srcClient = new SpeedrunComClient($"{Server.Settings.Default.AppName}/{Server.Settings.Default.AppVersion}", Server.Credentials.Default.SpeedruncomToken, 5);
			
			// Make custom header
			srcHeaders = new WebHeaderCollection();
			srcHeaders["Host"] = "www.speedrun.com";
			srcHeaders["Accept"] = "application/json";
			srcHeaders["X-API-Key"] = Server.Credentials.Default.SpeedruncomToken;
			srcHeaders["User-Agent"] = $"{Server.Settings.Default.AppName}/{Server.Settings.Default.AppVersion}";

			if (!srcClient.IsAccessTokenValid)
				Logging.CON("invalid token");
		}

		public static string GetGameWorldRecord(string gName)
		{
			var game = srcClient.Games.SearchGame(gName);
			if (game == null)
				return "Unknown game.";

			Category anyPercent;
			try
			{
				anyPercent = game.Categories.First(x => x.Type == CategoryType.PerGame && x.Runs.Any());
			}
			catch
			{
				return "**Failed** to find the world record\n"
					+ $"**-** Game might have a level leaderboard instead. Try `{Server.Settings.Default.PrefixCmd}{Server.Settings.Default.PrefixCmd}il <game>`\n"
					+ "**-** Game doesn't have a world record yet.";
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
				var country = wr.Player.User.Location?.Country?.Code?.ToLower() ?? string.Empty;
				if (country != string.Empty)
					country = $" :flag_{country}:";
				// Proof
				var video = wr.Videos.Links.First().OriginalString;
				// Stats
				var platform = wr.Platform.Name;
				var date = wr.Date.Value.Date.ToString("dd.MM.yyyy");
				var sdate = wr.DateSubmitted.Value.Date.ToString("dd.MM.yyyy");
				// Verfied status
				var status = string.Empty;
				if (wr.Status.Type == RunStatusType.Verified && game.Ruleset.RequiresVerification)
					status = $"Verfied by {wr.Status.Examiner.Name} on {wr.Status.VerifyDate.Value.Date.ToString("dd.MM.yyyy")}";
				var comment = string.IsNullOrEmpty(wr.Comment) ? "" : $"\n*{wr.Comment}*";
				return $"**[World Record - *{name}*]**\n"
					+ $"{category} in **{FormatTime(time)}** by {player}{country}\n"
					+ $"{video}\n"
					+ $"Played on {platform} on {date}\n"
					+ $"Submitted on {sdate}\n"
					+ $"{status}"
					+ $"{comment}";
			}
			catch
			{
				return "**Error**";
			}
		}

		public static string GetPersonalBestOfPlayer(string pName)
		{
			try
			{
				var player = srcClient.Users.GetUsers(pName).First();
				if (player == null)
					return "Player profile doesn't exist.";

				Record[] pbs;
				if (player.PersonalBests.Any())
					pbs = player.PersonalBests.ToArray();
				else
					return "Player doesn't have any personal records.";
				var country = player.Location?.Country?.Code?.ToLower() ?? string.Empty;
				if (country != string.Empty)
					country = $" :flag_{country}:";
				var output = $"**[Personal Records - *{player.Name}*{country}]**\n";
				foreach (var item in pbs)
				{
					var game = string.Empty;
					if (item.Category.Type == CategoryType.PerGame)
						game = $"**{item.Game.Name}** {item.Category.Name}";
					else
						game = $"**{item.Game.Name}** {item.Category.Name} **-** {item.Level.Name}";
					output += $"{TopTenFormat(item.Rank.ToString())} **-** {game} in {FormatTime(item.Times.Primary.Value)}\n";
				}
				return output.Substring(0, output.Length - 1);
			}
			catch
			{
				return "**Error**";
			}
		}

		public static string GetGameWorldRecords(string gName)
		{
			try
			{
				var game = srcClient.Games.SearchGame(gName);
				if (game == null)
					return "Unknown game.";

				var all = game.Categories.ToArray();
				var name = game.Name;
				var output = $"**[World Records - *{name}*]**\n";
				foreach (var item in all)
				{
					var wr = item.WorldRecord;
					// Skip when there is no record
					if (wr == null)
						continue;
					var category = wr.Category.Name;
					// Time
					var time = wr.Times.Primary.Value;
					// Player stats
					var player = wr.Player.Name;
					var country = wr.Player.User.Location?.Country?.Code?.ToLower() ?? string.Empty;
					if (country != string.Empty)
						country = $" :flag_{country}:";
					// Stats
					var platform = wr.Platform.Name;
					var date = wr.Date.Value.Date.ToString("dd.MM.yyyy");
					output += $"{category} in **{FormatTime(time)}** by {player}{country} (Played on {platform} on {date})\n";
				}
				return output.Substring(0, output.Length - 1);
			}
			catch
			{
				return "**Error**";
			}
		}

		public static string GetPlayerInfo(string pName)
		{
			try
			{
				User player = null;
				if (srcClient.Users.GetUsers(pName).Any())
					player = srcClient.Users.GetUsers(pName).First();
				else
					return "Unknown name.";

				var id = player.ID;
				var name = player.Name;
				var countrycode = player.Location?.Country?.Code?.ToLower() ?? string.Empty;
				var country = string.Empty;
				country = countrycode != string.Empty ? $" :flag_{countrycode}:" : string.Empty; ;
				countrycode = countrycode != string.Empty ? $"**Country Code -** {countrycode}\n" : string.Empty;
				var region = player.Location?.Region?.Code?.ToLower() ?? string.Empty;
				region = region != string.Empty ? $"**Region Code -** {region}\n" : string.Empty;
				var mods = player.ModeratedGames.Count().ToString();
				var pbs = player.PersonalBests.Count().ToString();
				var role = player.Role.ToString();
				var runs = player.Runs.Count();
				var sudate = player.SignUpDate.Value.ToString();
				var yt = player.YoutubeProfile != null ? "\n**YouTube -**" + player.YoutubeProfile.OriginalString : string.Empty;
				var twitch = player.TwitchProfile != null ? "\n**Twitch -**" + player.TwitchProfile.OriginalString : string.Empty;
				var twitter = player.TwitterProfile != null ? "\n**Twitch -**" + player.TwitterProfile.OriginalString : string.Empty;
				var web = "\n" + player.WebLink.OriginalString;
				return $"**[Player Info - *{name}*{country}]**\n"
					+ $"**ID -** {id}\n"
					+ $"{countrycode}"
					+ $"{region}"
					+ $"**Moderator -** {mods}\n"
					+ $"**Personal Records -** {pbs}\n"
					+ $"**Role -** {role}\n"
					+ $"**Runs -** {runs}\n"
					+ $"**Join Date -** {sudate}"
					+ $"{yt}"
					+ $"{twitch}"
					+ $"{twitter}"
					+ $"{web}";
			}
			catch
			{
				return "**Error**";
			}
		}

		public static string GetGameInfo(string gName)
		{
			try
			{
				var game = srcClient.Games.SearchGame(gName);
				if (game == null)
					return "Unknown game.";

				// Title
				var name = game.Name;
				// Info
				var id = game.ID;
				var abbr = game.Abbreviation;
				var rdate = game.YearOfRelease.ToString();
				var cdate = string.Empty;
				if (game.CreationDate != null)
					cdate = $"**Creation Date -** {game.CreationDate.Value.ToString()}\n";
				var mods = game.Moderators.Count.ToString();
				var isrom = game.IsRomHack.ToString();
				// Rules
				var deftiming = game.Ruleset.DefaultTimingMethod.ToString();
				var emus = game.Ruleset.EmulatorsAllowed.ToString();
				var verification = game.Ruleset.RequiresVerification.ToString();
				var vproof = game.Ruleset.RequiresVideo.ToString();
				var showms = game.Ruleset.ShowMilliseconds.ToString();

				return $"**[Game Info - *{name}*]**\n"
					+ $"**ID -** {id}\n"
					+ $"**Abbreviation -** {abbr}\n"
					+ cdate
					+ $"**Release Date -** {rdate}\n"
					+ $"**Moderator Count -** {mods}\n"
					+ $"**Is Romhack? -** {isrom}\n"
					+ $"**Default Timing Method -** {deftiming}\n"
					+ $"**Emulators Allowed? -** {emus}\n"
					+ $"**Requires Verification? -** {verification}\n"
					+ $"**Requires Video? -** {vproof}\n"
					+ $"**Show Milliseconds? -** {showms}";
			}
			catch
			{
				return "**Error**";
			}
		}

		public static string GetModerators(string gName)
		{
			try
			{
				var output = string.Empty;
				var game = srcClient.Games.SearchGame(gName);
				if (game == null)
					return "Unknown game.";
				foreach (var item in game.Moderators.ToArray())
				{
					var country = item.User.Location?.Country?.Code?.ToLower() ?? string.Empty;
					if (country != string.Empty)
						country = $" :flag_{country}:";
					output += item.Name + country + "\n";
				}
				return output.Substring(0, output.Length - 1);
			}
			catch
			{
				return "**Error**";
			}
		}

		public static string PlayerHasWorldRecord(string pName)
		{
			try
			{
				User player = null;
				if (srcClient.Users.GetUsers(pName).Any())
					player = srcClient.Users.GetUsers(pName).First();
				else
					return "Unknown name.";

				Record[] pbs;
				if (player.PersonalBests.Any())
					pbs = player.PersonalBests.ToArray();
				else
					return "Player doesn't have any personal records.";

				foreach (var item in pbs)
				{
					if (item.Rank != 1)
						continue;
					return "Yes.";
				}
				return "No.";
			}
			catch
			{
				return "**Error**";
			}
		}

		public static string GetTopTen(string gName)
		{
			try
			{
				var game = srcClient.Games.SearchGame(gName);
				if (game == null)
					return "Unknown game.";

				var output = $"**[Top 10 - *{game.Name}*]**\n";
				var category = game.FullGameCategories.First(x => x.Type == CategoryType.PerGame && x.Runs.Any());
				var runs = category.Runs.Where(x => x.Status.Type == RunStatusType.Verified).OrderBy(x => x.Times.Primary.Value.TotalMilliseconds).ToArray();
				var rankcount = runs.Count() >= 10 ? 10 : runs.Count();
				var names = new System.Collections.Generic.List<string>();
				for (int i = 0, rank = 0; i < rankcount; i++)
				{
					if (!names.Contains(runs[i].Player.Name))
					{
						rank++;
						if (rank >= 2)
							if (IsTied(runs[i].Times.Primary.Value, runs[i - 1].Times.Primary.Value))
								rank--;
						names.Add(runs[i].Player.Name);
						var country = runs[i].Player.User.Location?.Country?.Code?.ToLower() ?? string.Empty;
						if (country != string.Empty)
							country = $" :flag_{country}:";
						output += $"{TopTenFormat(rank.ToString(), false)} **{runs[i].Player.Name}**{country} in {FormatTime(runs[i].Times.Primary.Value)}\n";
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

		public static async Task<string> GetLastNotification(string scount = null, string nftype = null)
		{
			try
			{
				// Check parameter <count>
				if (scount == null)
					scount = maxnfcount.ToString();
				else if (scount.ToLower() == "x")
					scount = maxnfcount.ToString();
				else if (!new System.Text.RegularExpressions.Regex("^[1-9]").IsMatch(scount))
					return "Invalid notifcation count. Use numbers 1-9 only.";
				if (nftype == null)
					nftype = "any";

				// Web request
				var json = await Fetching.GetString($"http://www.speedrun.com/api/v1/notifications?max={maxnffetchcount}", srcHeaders);

				// Read
				if (string.IsNullOrEmpty(json))
					return null;

				// Read json string
				dynamic api = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
				if (string.IsNullOrEmpty(api?.ToString()))
					return null;

				// Parse data
				var title = $"**[Latest Notifications (Type: {nftype})]**\n";
				var output = title;
				var count = System.Convert.ToInt16(scount);
				for (int i = 0; i < count; i++)
				{
					var item = api.data[i];
					//var id = item.id.ToString();
					var created = item.created.ToString();
					var status = item.status.ToString();
					var text = item.text.ToString();
					var type = item.item.rel.ToString();
					//var url = item.item.uri.ToString();

					// Filter
					if (nftype == "any")
						output += $"{FormatMessage(status)} | {created} | {text}\n";
					else if (nftype == type)
						output += $"{FormatMessage(status)} | {created} | {text}\n";
					else
						count++;
				}
				output = output.Replace("_", "\\_");
				if (output.Length > 2000)
					return output.Substring(0, 2000);
				else
					return output.Substring(0, output.Length - 1);
			}
			catch (System.Exception ex)
			{
				Logging.CON(ex.ToString());
				return "**Error**";
			}
		}

		public static async Task<string> GetNotificationUpdate()
		{
			try
			{
				var json = await Fetching.GetString("http://www.speedrun.com/api/v1/notifications", srcHeaders);
				
				// Read
				if (string.IsNullOrEmpty(json))
					return null;

				// Read json string
				dynamic api = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
				if (string.IsNullOrEmpty(api?.ToString()))
					return null;

				// Parse data
				foreach (var item in api.data)
				{
					var created = item.created.ToString();
					var text = item.text.ToString();
					var type = item.item.rel.ToString();
					var url = item.item.uri.ToString();

					// Filter out stuff
					if ((text.Contains("posted a new thread") && type == "post") || type == "resource")
						return $"**[{created}]**\n{text}\n{url}";
					if (text.Contains("beat the WR") && type == "run")
						return $"**[{created}]**\n@here {text}\n{url}";
				}
				return string.Empty;
			}
			catch (System.Exception ex)
			{
				Logging.CON(ex.ToString());
				return null;
			}
		}

		public static string GetGameRules(string gName, bool il = false)
		{
			try
			{
				var game = srcClient.Games.SearchGame(gName);
				if (game == null)
					return "Unknown game.";

				var categories = game.Categories.ToArray();
				var rules = string.Empty;
				foreach (var item in categories)
				{
					if (il)
					{
						if (string.IsNullOrEmpty(item.Rules) || item.Type != CategoryType.PerLevel)
							continue;
					}
					else
					{
						if (string.IsNullOrEmpty(item.Rules) || item.Type != CategoryType.PerGame)
							continue;
					}
					rules = item.Rules;
					if (rules != string.Empty)
						break;
				}
				return rules == string.Empty ?
					"No rules have been defined." : $"*{rules}*";
			}
			catch
			{
				return $"**Error.** Try `{Server.Settings.Default.PrefixCmd}{Server.Settings.Default.PrefixCmd}ilrules <game>` if you haven't already.";
			}
		}

		// Formatting
		private static string FormatTime(System.TimeSpan time)
		{
			var h = time.Hours.ToString();
			var m = time.Minutes.ToString();
			var s = time.Seconds.ToString();
			var ms = time.Milliseconds.ToString();
			var output = string.Empty;
			output += (h == "0") ? "" : h + ":";
			output += (m == "0" && output == string.Empty) ? "" : m + ":";
			output += (s.Length == 1) ? (output == string.Empty) ? s : "0" + s : s;
			output += (ms == "0") ? (output.Length <= 2) ? "s" : "" : "." + ms;
			return output;
		}

		private static string TopTenFormat(string rank, bool trohpy = true, bool allbold = false)
		{
			if (System.Convert.ToInt16(rank) > 10)
			{
				if (allbold)
					return $"**#{rank}**";
				return $"#{rank}";
			}
			if (rank == "1")
			{
				if (trohpy)
					return "**1st** :trophy:";
				return "**1st**";
			}
			if (rank == "2")
				return "**2nd**";
			if (rank == "3")
				return "**3rd**";
			return $"**{rank}th**";
		}

		private static string FormatMessage(string msg) =>
			msg == "Unread" ?
			"New" : "Old";

		// Others
		private static bool IsTied(System.TimeSpan t1, System.TimeSpan t2) =>
			t1.TotalMilliseconds == t2.TotalMilliseconds;
	}
}