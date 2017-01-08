using System.Linq;
using SpeedrunComSharp;

namespace NeKzBot
{
	public partial class SpeedrunCom
	{
		private static SpeedrunComClient srcClient;

		private const int maxnfcount = 5;

		public static void Init()
		{
			Logging.CON("Initializing speedruncom client", System.ConsoleColor.DarkYellow);
			srcClient = new SpeedrunComClient($"{Properties.Settings.Default.AppName}/{Properties.Settings.Default.AppVersion}", Properties.Settings.Default.SpeedruncomToken, 5);

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
				+ $"**-** Game might have a level leaderboard instead. Try `{Properties.Settings.Default.PrefixCmd}{Properties.Settings.Default.PrefixCmd}il <game>`\n"
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
				var country = $":flag_{wr.Player.User.Location.Country.Code.ToLower()}:";   // Thanks for not making this null, when user doesn't have set a location
				// Proof
				var video = wr.Videos.Links.First().OriginalString;
				// Stats
				var platform = wr.Platform.Name;
				var date = wr.Date.Value.Date.ToString("dd.MM.yyyy");
				var sdate = wr.DateSubmitted.Value.Date.ToString("dd.MM.yyyy");
				// Verfied status
				var status = "";
				if (wr.Status.Type == RunStatusType.Verified && game.Ruleset.RequiresVerification)
					status = $"Verfied by {wr.Status.Examiner.Name} on {wr.Status.VerifyDate.Value.Date.ToString("dd.MM.yyyy")}";
				var comment = string.IsNullOrEmpty(wr.Comment) ? "" : $"\n*{wr.Comment}*";
				return $"**[World Record - *{name}*]**\n"
					+ $"{category} in **{FormatTime(time)}** by {player} {country}\n"
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

				var output = $"**[Personal Records - *{player.Name}* :flag_{ player.Location.Country.Code.ToLower()}:]**\n";	// RIP
				foreach (var item in pbs)
				{
					var game = "";
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
			var game = srcClient.Games.SearchGame(gName);
			if (game == null)
				return "Unknown game.";

			try
			{
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
					var country = $":flag_{wr.Player.User.Location.Country.Code.ToLower()}:";   // Still RIP
					// Stats
					var platform = wr.Platform.Name;
					var date = wr.Date.Value.Date.ToString("dd.MM.yyyy");
					output += $"{category} in **{FormatTime(time)}** by {player} {country} (Played on {platform} on {date})\n";
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
			User player = null;
			if (srcClient.Users.GetUsers(pName).Any())
				player = srcClient.Users.GetUsers(pName).First();
			else
				return "Unknown name.";

			try
			{
				var id = player.ID;
				var name = player.Name;
				var mods = player.ModeratedGames.Count().ToString();
				var pbs = player.PersonalBests.Count().ToString();
				var role = player.Role.ToString();
				var runs = player.Runs.Count();
				var sudate = player.SignUpDate.Value.ToString();
				var yt = player.YoutubeProfile != null ? "\n**YouTube -**" + player.YoutubeProfile.OriginalString : string.Empty;
				var twitch = player.TwitchProfile != null ? "\n**Twitch -**" + player.TwitchProfile.OriginalString : string.Empty;
				var twitter = player.TwitterProfile != null ? "\n**Twitch -**" + player.TwitterProfile.OriginalString : string.Empty;
				var web = "\n" + player.WebLink.OriginalString;
				return $"**[Player Info - *{name}*]**\n"
					+ $"**ID -** {id}\n"
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
			var game = srcClient.Games.SearchGame(gName);
			if (game == null)
				return "Unknown game.";

			try
			{
				// Title
				var name = game.Name;
				// Info
				var id = game.ID;
				var abbr = game.Abbreviation;
				var rdate = game.YearOfRelease.ToString();
				var cdate = "";
				if (game.CreationDate != null)
					cdate = game.CreationDate.Value.ToString();
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
					+ $"**Creation Date -** {cdate}\n"
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
			var output = "";
			var game = srcClient.Games.SearchGame(gName);
			if (game == null)
				return "Unknown game.";
			foreach (var item in game.Moderators.ToArray())
				output += item.Name + "\n";
			return output.Substring(0, output.Length - 1);
		}

		public static string PlayerHasWorldRecord(string pName)
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

		public static string GetTopTen(string gName)
		{
			var game = srcClient.Games.SearchGame(gName);
			if (game == null)
				return "Unknown game.";

			try
			{
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
						output += $"{TopTenFormat(rank.ToString(), false)} **{runs[i].Player.Name}** in {FormatTime(runs[i].Times.Primary.Value)}\n";
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

		public static string GetLastNotification(string scount = null, string type = null, bool update = false)
		{
			// Check parameter <count>
			if (scount == null)
				scount = maxnfcount.ToString();
			else if (scount.ToLower() == "x")
				scount = maxnfcount.ToString();
			else if (!new System.Text.RegularExpressions.Regex("^[1-9]").IsMatch(scount))
				return "Invalid notifcation count. Use numbers 1-9 only.";

			if (type == null)
				type = "any";

			// Check parameter <type>
			NotificationType nftype;
			switch (type.ToLower())
			{
				case "game":
					nftype = NotificationType.Game;
					break;
				case "guide":
					nftype = NotificationType.Guide;
					break;
				case "post":
					nftype = NotificationType.Post;
					break;
				case "run":
					nftype = NotificationType.Run;
					break;
				case "any":
					nftype = (NotificationType)4;
					break;
				default:
					return "Unkown notification type. Try `game`, `guide`, `post`, `run` or `any`.";
			}

			try
			{
				var count = System.Convert.ToInt16(scount);
				var nfs = new System.Collections.Generic.List<Notification>();
				
				for (int j = 0; j < maxnfcount; j++)
					nfs.Add(srcClient.Notifications.GetNotifications(null, NotificationsOrdering.NewestToOldest).ElementAt(j));

				// Auto notification updater has a different format
				if (update)
				{
					var text = nfs[0].Text;
					var player = text.Substring(0, text.IndexOf(" "));
					var msg = text.Substring(player.Length, text.Length - player.Length);
					if (msg.Contains("The new WR is") && nfs[0].Type == NotificationType.Run)
						player = "@here " + player;
					return $"**[{nfs[0].TimeCreated}]**"
						+ $"\n{player}{msg}\n{nfs[0].WebLink}";
				}
				else
				{
					var title = $"**[Latest Notifications (Type: {type})]**\n";
					var output = title;

					for (int i = 0, c = 0; i < maxnfcount; i++)
					{
						if (nfs[i].Type == nftype || nftype == (NotificationType)4)
						{
							c++;
							if (c <= count)
								output += $"{FormatMessage(nfs[i].Status.ToString())} | {nfs[i].TimeCreated} | {nfs[i].Text}\n";
							else
								break;
						}
					}
					return output == title ? "Couldn't find any notifications." : output.Substring(0, output.Length - 1);
				}
			}
			catch
			{
				return "**Error**";
			}
		}

		public static string GetGameRules(string gName, bool il = false)
		{
			var game = srcClient.Games.SearchGame(gName);
			if (game == null)
				return "Unknown game.";

			try
			{
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
				return rules == string.Empty ? "No rules have been defined." : $"*{rules}*";
			}
			catch
			{
				return $"**Error.** Try `{Properties.Settings.Default.PrefixCmd}{Properties.Settings.Default.PrefixCmd}ilrules <game>` if you haven't already.";
			}
		}

		// Formatting
		private static string FormatTime(System.TimeSpan time)
		{
			var h = time.Hours.ToString();
			var m = time.Minutes.ToString();
			var s = time.Seconds.ToString();
			var ms = time.Milliseconds.ToString();
			var output = "";
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

		private static string FormatMessage(string msg) => msg == "Unread" ? "New" : "Old";

		// Others
		private static bool IsTied(System.TimeSpan t1, System.TimeSpan t2) => t1.TotalMilliseconds == t2.TotalMilliseconds;
	}
}