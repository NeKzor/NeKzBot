using System;
using System.Linq;
using System.Threading.Tasks;
using NeKzBot.Resources;
using NeKzBot.Server;

namespace NeKzBot.Modules.Leaderboard
{
	public partial class Leaderboard
	{
		private const int maxytid = 11;     // Maximum length of a YouTube ID

		public static async Task<string> GetLatestEntry(string url)
		{
			var doc = await Cache.GetCache(url);
			if (doc == null)
				return "**Error**";

			try
			{
				var map = doc.DocumentNode.SelectSingleNode("//div[@class='map']//a")?.FirstChild?.InnerHtml ?? string.Empty;
				var ranking = doc.DocumentNode.SelectSingleNode("//div[@class='newscore']//div[@class='rank']")?.FirstChild?.InnerHtml ?? string.Empty;
				var time = doc.DocumentNode.SelectSingleNode("//div[@class='newscore']//a[@class='time']")?.FirstChild?.InnerHtml ?? string.Empty;
				var player = doc.DocumentNode.SelectSingleNode("//div[@class='boardname']//a")?.FirstChild?.InnerHtml ?? string.Empty;
				var date = doc.DocumentNode.SelectSingleNode("//div[@class='datatable page-entries active']//div[@class='entry']//div[@class='date']")?.Attributes["date"]?.Value?.ToString() ?? string.Empty;

				if (new System.Collections.Generic.List<string>() { map, ranking, time, player, date }.Contains(string.Empty))
				{
					await Logging.CHA("GetLatestEntry document node error", ConsoleColor.Red);
					return null;
				}

				// Only add when it exists
				var demo = doc.DocumentNode.SelectSingleNode("//div[@class='entry']//div[@class='demo-url']").Descendants("a").Any()
					? $"\nDemo **-** http://board.iverb.me{doc.DocumentNode.SelectSingleNode("//div[@class='entry']//div[@class='demo-url']//a").Attributes["href"].Value.ToString()}"
					: string.Empty;
				var youtube = doc.DocumentNode.SelectSingleNode("//div[@class='entry']//div[@class='youtube']").Descendants("i").Any()
					? $"\nVideo **-** http://youtu.be/{doc.DocumentNode.SelectSingleNode("//div[@class='youtube']//i").Attributes["onclick"].Value.ToString().Substring("embedOnBody('".Length, maxytid)}"
					: string.Empty;
				var comment = doc.DocumentNode.SelectSingleNode("//div[@class='entry']//div[@class='comment']").Descendants("i").Any()
					? $"\nComment **- *{doc.DocumentNode.SelectSingleNode("//div[@class='entry']//div[@class='comment']//i").Attributes["data-content"].Value.ToString()}***"
					: string.Empty;

				ranking = ranking == "1" ? string.Empty : $"Rank **-** {TopTenFormat(ranking, true)}\n";
				var title = ranking == string.Empty ? "**[World Record - *" : "**[Entry - *";

				return $"{title + map}*]**\n"
					+ $"Time **- {time}**\n"
					+ $"Player **- {player}**\n"
					+ ranking
					+ $"Date **- {date}**"
					+ demo + youtube + comment;
			}
			catch (Exception ex)
			{
				await Logging.CHA("Lb GetLatestEntry error", ex);
				doc?.Save(await Caching.CFile.GetPathAndSave("lb"));
				return "**Error**";
			}
		}

		public static async Task<Tuple<string, string, string>> GetEntryUpdate(string url)
		{
			await Logging.CON("Requesting new entry", ConsoleColor.DarkBlue);

			var doc = await Cache.GetCache(url, true);
			if (doc == null)
				return null;

			try
			{
				var map = doc.DocumentNode.SelectSingleNode("//div[@class='map']//a")?.FirstChild?.InnerHtml ?? string.Empty;
				var ranking = doc.DocumentNode.SelectSingleNode("//div[@class='newscore']//div[@class='rank']")?.FirstChild?.InnerHtml ?? string.Empty;
				var time = doc.DocumentNode.SelectSingleNode("//div[@class='newscore']//a[@class='time']")?.FirstChild?.InnerHtml ?? string.Empty;
				var player = doc.DocumentNode.SelectSingleNode("//div[@class='boardname']//a")?.FirstChild?.InnerHtml ?? string.Empty;
				var date = doc.DocumentNode.SelectSingleNode("//div[@class='datatable page-entries active']//div[@class='entry']//div[@class='date']")?.Attributes["date"]?.Value?.ToString() ?? string.Empty;

				if (new System.Collections.Generic.List<string>() { map, ranking, time, player, date }.Contains(string.Empty))
				{
					await Logging.CHA("GetEntryUpdate document node error", ConsoleColor.Red);
					return null;
				}

				// Only add when it exists
				var demo = doc.DocumentNode.SelectSingleNode("//div[@class='entry']//div[@class='demo-url']").Descendants("a").Any()
					? $"http://board.iverb.me{doc.DocumentNode.SelectSingleNode("//div[@class='entry']//div[@class='demo-url']//a").Attributes["href"].Value.ToString()}"
					: string.Empty;
				var youtube = doc.DocumentNode.SelectSingleNode("//div[@class='entry']//div[@class='youtube']").Descendants("i").Any()
					? $"http://youtu.be/{doc.DocumentNode.SelectSingleNode("//div[@class='youtube']//i").Attributes["onclick"].Value.ToString().Substring("embedOnBody('".Length, maxytid)}"
					: string.Empty;
				var comment = doc.DocumentNode.SelectSingleNode("//div[@class='entry']//div[@class='comment']").Descendants("i").Any()
					? doc.DocumentNode.SelectSingleNode("//div[@class='entry']//div[@class='comment']//i").Attributes["data-content"].Value.ToString()
					: string.Empty;

				// Might change this to wr only (it's currently possible to change the board parameter)
				ranking = ranking == "1" ? string.Empty : $"Rank **-** {TopTenFormat(ranking, true)}\n";
				var title = ranking == string.Empty ? "**[New World Record - *" : "**[New Entry - *";

				return new Tuple<string, string, string>(
					// Channel message format
					$"{title + map}*]**\n"
					+ $"Time **- {time}**\n"
					+ $"Player **- {player}**\n"
					+ ranking
					+ $"Date **- {date}**"
					+ $"\nDemo **-** {demo}"
					+ $"\nVideo **-** {youtube}"
					+ $"\nComment **- *{comment}***",
					// Tweet format
					await FormatTweet(
						$"New World Record in {map}\n{time} by {player}\n{date}",
						demo,
						youtube,
						comment
					),
					// Cache format
					map + time + player // <- this will also detect player name changes (pls don't) and ties
				);
			}
			catch (Exception ex)
			{
				await Logging.CHA("Lb GetEntryUpdate error", ex);
				doc?.Save(await Caching.CFile.GetPathAndSave("lb"));
				return null;
			}
		}

		public static async Task<string> GetUserStats(string url)
		{
			var doc = await Cache.GetCache(url);
			if (doc == null)
				return "**Error**";

			if (doc.DocumentNode.Descendants("div").Any(x => x.GetAttributeValue("class", string.Empty) == "user-noexist"))
				return "Player profile doesn't exist.";

			try
			{
				// Nickname of profile
				var player = doc.DocumentNode.SelectSingleNode("//head//title").FirstChild.InnerHtml;

				// Ranking + average ranking
				var sprank = doc.DocumentNode.SelectNodes("//div[@class='block-container ranks']//div[@class='block']//div[@class='block-inner']//div[@class='number']")[0].FirstChild.InnerHtml;
				var cooprank = doc.DocumentNode.SelectNodes("//div[@class='block-container ranks']//div[@class='block']//div[@class='block-inner']//div[@class='number']")[1].FirstChild.InnerHtml;
				var rank = doc.DocumentNode.SelectNodes("//div[@class='block-container ranks']//div[@class='block']//div[@class='block-inner']//div[@class='number']")[2].FirstChild.InnerHtml;
				var spaverage = doc.DocumentNode.SelectNodes("//div[@class='block-container average']//div[@class='block']//div[@class='block-inner']//div[@class='number']")[0].FirstChild.InnerHtml.Replace(" ", string.Empty).Replace("\n", string.Empty);
				var coopaverage = doc.DocumentNode.SelectNodes("//div[@class='block-container average']//div[@class='block']//div[@class='block-inner']//div[@class='number']")[1].FirstChild.InnerHtml.Replace(" ", string.Empty).Replace("\n", string.Empty);
				var average = doc.DocumentNode.SelectNodes("//div[@class='block-container average']//div[@class='block']//div[@class='block-inner']//div[@class='number']")[2].FirstChild.InnerHtml.Replace(" ", string.Empty).Replace("\n", string.Empty);

				// WR count
				var spwrs = doc.DocumentNode.SelectNodes("//div[@class='block-container wr']//div[@class='block']//div[@class='block-inner']//div[@class='number']")[0].FirstChild.InnerHtml;
				var coopwrs = doc.DocumentNode.SelectNodes("//div[@class='block-container wr']//div[@class='block']//div[@class='block-inner']//div[@class='number']")[1].FirstChild.InnerHtml;
				var wrs = doc.DocumentNode.SelectNodes("//div[@class='block-container wr']//div[@class='block']//div[@class='block-inner']//div[@class='number']")[2].FirstChild.InnerHtml;

				// Only show when player has at least one wr
				var fastpeopleonly = "\n__*World Records*__";
				if (spwrs != "0")
					fastpeopleonly += $"\nSingle Player **- {spwrs}** :trophy:";
				if (coopwrs != "0")
					fastpeopleonly += $"\nCooperative **- {coopwrs}** :trophy:";
				if (wrs != "0")
					fastpeopleonly += $"\nOverall **- {wrs}** :trophy:";
				else
					fastpeopleonly = string.Empty;

				return $"**[Global Statistics For *{player}*]**\n"
					+ $"__*Ranking*__\n"
					+ $"Single Player **-** {TopTenFormat(sprank)}\n"
					+ $"	(Average - {AddDecimalPlace(spaverage)})\n"
					+ $"Cooperative **-** {TopTenFormat(cooprank)}\n"
					+ $"	(Average - {AddDecimalPlace(coopaverage)})\n"
					+ $"Overall **-** {TopTenFormat(rank)}\n"
					+ $"	(Average - {AddDecimalPlace(average)})"
					+ fastpeopleonly;
			}
			catch
			{
				await Logging.CON("Lb GetUserStats error", ConsoleColor.Red);
				doc?.Save(await Caching.CFile.GetPathAndSave("lb"));
				return "**Error**";
			}
		}

		public static async Task<string> GetUserRank(string url, int index)
		{
			// Check if map has a leaderboard
			var mapid = Data.portal2Maps[index, 0];
			if (mapid == string.Empty)
				return "Map is not supported.";

			var doc = await Cache.GetCache(url);
			if (doc == null)
				return "**Error**";

			try
			{
				// Check if profile is actually a Portal 2 challenge mode runner
				if (doc.DocumentNode.Descendants("div").Any(x => x.GetAttributeValue("class", string.Empty) == "user-noexist"))
					return "Player profile doesn't exist.";

				// First get all maps
				var maps = doc.DocumentNode.SelectNodes("//div[@class='cell title']//a");
				var map = Data.portal2Maps[index, 2];

				// Find index map of all nodes
				var idx = 0;
				for (; idx < maps.Count; idx++)
					if (maps[idx].Attributes["href"].Value.ToString() == "/chamber/" + mapid)
						break;

				var player = doc.DocumentNode.SelectSingleNode("//head//title").FirstChild.InnerHtml;
				var ranking = doc.DocumentNode.SelectNodes("//div[@class='cell rank']")[idx].FirstChild.InnerHtml;
				var time = doc.DocumentNode.SelectNodes("//a[@class='cell score']")[idx].FirstChild.InnerHtml.Replace("\n", string.Empty).Replace(" ", string.Empty);
				var date = doc.DocumentNode.SelectNodes("//div[@class='chamberScoreInfo']")[idx].Attributes["date"].Value.ToString();

				// Only add when it exists
				var demo = doc.DocumentNode.SelectNodes("//div[@class='cell demo-url']//a")[idx].Attributes["style"] == null
					? $"\nDemo **-** http://board.iverb.me{doc.DocumentNode.SelectNodes("//div[@class='cell demo-url']//a")[idx].Attributes["href"].Value.ToString()}"
					: string.Empty;
				var youtube = doc.DocumentNode.SelectNodes("//div[@class='cell youtube']//i")[idx].Attributes["onclick"] != null
					? $"\nVideo **-** http://youtu.be/{doc.DocumentNode.SelectNodes("//div[@class='cell youtube']//i")[idx].Attributes["onclick"].Value.ToString().Substring("embedOnBody('".Length, maxytid)}"
					: string.Empty;
				var comment = doc.DocumentNode.SelectNodes("//div[@class='cell comment']//i")[idx].Attributes["data-content"].Value != string.Empty
					? $"\nComment **- *{doc.DocumentNode.SelectNodes("//div[@class='cell comment']//i")[idx].Attributes["data-content"].Value.ToString()}***"
					: string.Empty;

				return $"{GetTitle(ranking) + map}*]**\n"
					+ $"Player **- {player}**\n"
					+ $"Time **- {time}**\n"
					+ $"{RankFromat(ranking)}"
					+ $"Date **- {date}**\n"
					+ demo + youtube + comment;
			}
			catch
			{
				await Logging.CON("Lb GetUserRank error", ConsoleColor.Red);
				doc?.Save(await Caching.CFile.GetPathAndSave("lb"));
				return "**Error**";
			}
		}

		private static string RankFromat(string s)
		{
			if (Convert.ToInt16(s) > 10)
				return $"Rank **- #{s}**\n";
			if (s == "1")
				return string.Empty;  // Trophy maybe?
			if (s == "2")
				return "Rank **- 2nd**\n";
			if (s == "3")
				return "Rank **- 3rd**\n";
			return $"Rank **- {s}th**\n";
		}

		private static string GetTitle(string s)
		{
			if (Convert.ToInt16(s) > 10)
				return "**[Personal Best - *";
			if (s == "1")
				return "**[World Record - *";
			return "**[Top 10 - *";
		}

		private static string TopTenFormat(string r, bool allbold = false)
		{
			if (Convert.ToInt16(r) > 10)
			{
				if (allbold)
					return $"**#{r}**";
				else
					return $"#{r}";
			}
			if (r == "1")
				return "**1st** :trophy:";
			if (r == "2")
				return "**2nd**";
			if (r == "3")
				return "**3rd**";
			return $"**{r}th**";
		}

		private static string AddDecimalPlace(string s) =>
			!s.Contains(".") ? $"{s}.0" : s;

		private static async Task<string> FormatTweet(string msg, string demo, string youtube, string comment)
		{
			try
			{
				var cut = "...";
				var newline = "\nComment: ";
				var output = msg;

				// Check if we exceed the character limit
				if (Twitter.Twitter.tweetLimit - output.Length < 0)
					return string.Empty;

				// Only append demo link if it fits
				if (Twitter.Twitter.tweetLimit - output.Length - demo.Length < 0)
					return output;
				output += "\n" + demo;

				// Only append youtube link if it fits
				if (Twitter.Twitter.tweetLimit - output.Length - youtube.Length < 0)
					return output;
				output += "\n" + youtube;

				// There isn't always a comment
				if (comment == string.Empty)
					return output;

				// Check what we have left
				var left = Twitter.Twitter.tweetLimit - output.Length - newline.Length - comment.Length;
				if (-left + cut.Length == comment.Length || newline.Length >= Twitter.Twitter.tweetLimit - output.Length)
					return output;

				// It's safe to append comment
				if (left >= 0)
					return output + newline + comment;

				// Cut comment and append "..." at the end
				return output + newline + comment.Substring(0, Twitter.Twitter.tweetLimit - output.Length - newline.Length - cut.Length) + cut;
			}
			catch (Exception ex)
			{
				await Logging.CHA("Format tweet error", ex);
				return string.Empty;
			}
		}
	}
}