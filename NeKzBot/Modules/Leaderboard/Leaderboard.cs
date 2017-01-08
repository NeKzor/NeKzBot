using System;
using System.Linq;
using System.Threading.Tasks;

namespace NeKzBot
{
	public partial class Leaderboard
	{
		private const int maxytid = 11;     // Maximum length of a YouTube ID

		public static async Task<string> GetLatestEntry(string url, bool autoupdate = false)
		{
			Logging.CON("Requesting new entry", ConsoleColor.DarkBlue);

			// Don't look into cache when the request comes from the auto updater
			HtmlAgilityPack.HtmlDocument doc;
			if (autoupdate)
			{
				doc = await Cache.GetCache(url, true);
				if (doc == null)
					return null;
			}
			else
			{
				doc = await Cache.GetCache(url);
				if (doc == null)
					return "**Error**";
			}

			try
			{
				var map = doc.DocumentNode.SelectSingleNode("//div[@class='map']//a").FirstChild.InnerHtml;
				var ranking = doc.DocumentNode.SelectSingleNode("//div[@class='newscore']//div[@class='rank']").FirstChild.InnerHtml;
				var time = doc.DocumentNode.SelectSingleNode("//div[@class='newscore']//a[@class='time']").FirstChild.InnerHtml;
				var player = doc.DocumentNode.SelectSingleNode("//div[@class='boardname']//a").FirstChild.InnerHtml;
				var date = doc.DocumentNode.SelectSingleNode("//div[@class='datatable page-entries active']//div[@class='entry']//div[@class='date']").Attributes["date"].Value.ToString();

				// Only add when it exists
				var demo = doc.DocumentNode.SelectSingleNode("//div[@class='entry']//div[@class='demo-url']").Descendants("a").Any()
					? $"\nDemo **-** http://board.iverb.me{doc.DocumentNode.SelectSingleNode("//div[@class='entry']//div[@class='demo-url']//a").Attributes["href"].Value.ToString()}"
					: string.Empty;
				var youtube = doc.DocumentNode.SelectSingleNode("//div[@class='entry']//div[@class='youtube']").Descendants("i").Any()
					? $"\nVideo **-** https://youtu.be/{doc.DocumentNode.SelectSingleNode("//div[@class='youtube']//i").Attributes["onclick"].Value.ToString().Substring("embedOnBody('".Length, maxytid)}"
					: string.Empty;
				var comment = doc.DocumentNode.SelectSingleNode("//div[@class='entry']//div[@class='comment']").Descendants("i").Any()
					? $"\nComment **- *{doc.DocumentNode.SelectSingleNode("//div[@class='entry']//div[@class='comment']//i").Attributes["data-content"].Value.ToString()}***"
					: string.Empty;

				doc = null;
				var title = string.Empty;
				if (ranking == "1")
				{
					// If call comes from auto updater
					if (autoupdate)
						title = "**[New World Record - *";
					title = "**[World Record - *";
					ranking = string.Empty;
				}
				else
				{
					if (autoupdate)
						title = "**[New Entry - *";
					else
						title = "**[Entry - *";
					ranking = $"Rank **-** {TopTenFormat(ranking, true)}\n";
				}

				return
					$"{title + map}*]**\n" +
					$"Time **- {time}**\n" +
					$"Player **- {player}**\n" +
					$"{ranking}" +
					$"Date **- {date}**" +
					$"{demo + youtube + comment}";
			}
			catch (Exception ex)
			{
				Logging.CHA($"Lb GetLatestEntry error\n{ex.ToString()}");
				// Some caching for debug purposes
				doc?.Save(Caching.CFile.GetPathAndSave("lb"));
				// ^ just in case only execute when it's not null
				if (autoupdate)
					return null;
				return "**Error**";
			}
		}

		public static async Task<string> GetUserStats(string url)
		{
			Logging.CON("Requesting new user stats", ConsoleColor.DarkBlue);

			// Download website
			var doc = await Cache.GetCache(url);
			if (doc == null)
				return "**Error**";

			// Check if profile does Portal 2
			if (doc.DocumentNode.Descendants("div").Any(x => x.GetAttributeValue("class", string.Empty) == "user-noexist"))
				return "Player profile doesn't exist.";
			Logging.CON("Profile found.", ConsoleColor.DarkBlue);

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

				doc = null;
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

				return
					$"**[Global Statistics For *{player}*]**\n" +
					$"__*Ranking*__\n" +
					$"Single Player **-** {TopTenFormat(sprank)}\n" +
					$"	(Average - {AddDecimalPlace(spaverage)})\n" +
					$"Cooperative **-** {TopTenFormat(cooprank)}\n" +
					$"	(Average - {AddDecimalPlace(coopaverage)})\n" +
					$"Overall **-** {TopTenFormat(rank)}\n" +
					$"	(Average - {AddDecimalPlace(average)})" +
					$"{fastpeopleonly}";
			}
			catch
			{
				Logging.CON("Lb GetUserStats error");
				doc?.Save(Caching.CFile.GetPathAndSave("lb"));
				return "**Error**";
			}
		}

		public static async Task<string> GetUserRank(string url, int index)
		{
			Logging.CON("Requesting new user rank", ConsoleColor.DarkBlue);

			// Check if map has a leaderboard
			var mapid = Data.portal2Maps[index, 0];
			if (mapid == string.Empty)
				return "Map is not supported.";

			// Download website
			var doc = await Cache.GetCache(url);
			if (doc == null)
				return "**Error**";

			try
			{
				// Check if profile is actually a Portal 2 challenge mode runner
				if (doc.DocumentNode.Descendants("div").Any(x => x.GetAttributeValue("class", string.Empty) == "user-noexist"))
					return "Player profile doesn't exist.";
				Logging.CON("Profile found", ConsoleColor.DarkBlue);

				// First get all maps
				var maps = doc.DocumentNode.SelectNodes("//div[@class='cell title']//a");
				var map = Data.portal2Maps[index, 2];

				// Find index map of all nodes
				int idx = 0;
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
					? $"\nVideo **-** https://youtu.be/{doc.DocumentNode.SelectNodes("//div[@class='cell youtube']//i")[idx].Attributes["onclick"].Value.ToString().Substring("embedOnBody('".Length, maxytid)}"
					: string.Empty;
				var comment = doc.DocumentNode.SelectNodes("//div[@class='cell comment']//i")[idx].Attributes["data-content"].Value != string.Empty
					? $"\nComment **- *{doc.DocumentNode.SelectNodes("//div[@class='cell comment']//i")[idx].Attributes["data-content"].Value.ToString()}***"
					: string.Empty;

				doc = null;
				return
					$"{GetTitle(ranking) + map}*]**\n" +
					$"Player **- {player}**\n" +
					$"Time **- {time}**\n" +
					$"{RankFromat(ranking)}" +
					$"Date **- {date}**\n" +
					$"{demo + youtube + comment}";
			}
			catch
			{
				Logging.CON("Lb GetUserRank error");
				doc?.Save(Caching.CFile.GetPathAndSave("lb"));
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
			!s.Contains(".") ?
				$"{s}.0" : s; 
	}
}