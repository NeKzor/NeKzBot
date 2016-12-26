using System;
using System.Linq;

namespace NeKzBot
{
	public class Leaderboard
	{
		private const int maxytid = 11;     // Maximum length of a YouTube ID

		public static string GetLatestEntry(string url, bool isupdate = false)
		{
			Logging.CON("Requesting new entry", ConsoleColor.DarkBlue);

			// Don't look into cache when the request comes from the auto updater
			HtmlAgilityPack.HtmlDocument doc;
			if (isupdate)
				doc = new HtmlAgilityPack.HtmlWeb().Load(url);
			else
				doc = Caching.GetCache(url);

			var map = doc.DocumentNode.SelectNodes("//div[@class='map']//a")[0].ChildNodes[0].InnerHtml;
			var ranking = doc.DocumentNode.SelectNodes("//div[@class='newscore']//div[@class='rank']")[0].ChildNodes[0].InnerHtml;
			var time = doc.DocumentNode.SelectNodes("//div[@class='newscore']//a[@class='time']")[0].ChildNodes[0].InnerHtml;
			var player = doc.DocumentNode.SelectNodes("//div[@class='boardname']//a")[0].ChildNodes[0].InnerHtml;
			var date = doc.DocumentNode.SelectNodes("//div[@class='datatable page-entries active']//div[@class='entry']//div[@class='date']")[0].Attributes["date"].Value.ToString();

			// Only add when it exists
			var demo = doc.DocumentNode.SelectNodes("//div[@class='entry']//div[@class='demo-url']")[0].Descendants("a").Any()
				? $"\nDemo **-** http://board.iverb.me{doc.DocumentNode.SelectNodes("//div[@class='entry']//div[@class='demo-url']//a")[0].Attributes["href"].Value.ToString()}"
				: string.Empty;
			var youtube = doc.DocumentNode.SelectNodes("//div[@class='entry']//div[@class='youtube']")[0].Descendants("i").Any()
				? $"\nVideo **-** https://youtu.be/{doc.DocumentNode.SelectNodes("//div[@class='youtube']//i")[0].Attributes["onclick"].Value.ToString().Substring("embedOnBody('".Length, maxytid)}"
				: string.Empty;
			var comment = doc.DocumentNode.SelectNodes("//div[@class='entry']//div[@class='comment']")[0].Descendants("i").Any()
				? $"\nComment **- *{doc.DocumentNode.SelectNodes("//div[@class='entry']//div[@class='comment']//i")[0].Attributes["data-content"].Value.ToString()}***"
				: string.Empty;

			doc = null;
			var title = string.Empty;
			if (ranking == "1")
			{
				// If call comes from auto updater
				if (isupdate)
					title = "**[New World Record - *";
				title = "**[World Record - *";
				ranking = string.Empty;
			}
			else
			{
				if (isupdate)
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

		public static string GetUserStats(string url)
		{
			Logging.CON("Requesting new user stats", ConsoleColor.DarkBlue);

			// Download website
			var doc = Caching.GetCache(url);

			// Check if profile does Portal 2
			if (doc.DocumentNode.Descendants("div").Any(a => a.GetAttributeValue("class", string.Empty) == "user-noexist"))
				return "Player profile doesn't exist.";
			Logging.CON("Profile found.", ConsoleColor.DarkBlue);

			// Nickname of profile
			var player = doc.DocumentNode.SelectNodes("//head//title")[0].ChildNodes[0].InnerHtml;

			// Ranking + average ranking
			var sprank = doc.DocumentNode.SelectNodes("//div[@class='block-container ranks']//div[@class='block']//div[@class='block-inner']//div[@class='number']")[0].ChildNodes[0].InnerHtml;
			var cooprank = doc.DocumentNode.SelectNodes("//div[@class='block-container ranks']//div[@class='block']//div[@class='block-inner']//div[@class='number']")[1].ChildNodes[0].InnerHtml;
			var rank = doc.DocumentNode.SelectNodes("//div[@class='block-container ranks']//div[@class='block']//div[@class='block-inner']//div[@class='number']")[2].ChildNodes[0].InnerHtml;
			var spaverage = doc.DocumentNode.SelectNodes("//div[@class='block-container average']//div[@class='block']//div[@class='block-inner']//div[@class='number']")[0].ChildNodes[0].InnerHtml.Replace(" ", string.Empty).Replace("\n", string.Empty);
			var coopaverage = doc.DocumentNode.SelectNodes("//div[@class='block-container average']//div[@class='block']//div[@class='block-inner']//div[@class='number']")[1].ChildNodes[0].InnerHtml.Replace(" ", string.Empty).Replace("\n", string.Empty);
			var average = doc.DocumentNode.SelectNodes("//div[@class='block-container average']//div[@class='block']//div[@class='block-inner']//div[@class='number']")[2].ChildNodes[0].InnerHtml.Replace(" ", string.Empty).Replace("\n", string.Empty);

			// WR count
			var spwrs = doc.DocumentNode.SelectNodes("//div[@class='block-container wr']//div[@class='block']//div[@class='block-inner']//div[@class='number']")[0].ChildNodes[0].InnerHtml;
			var coopwrs = doc.DocumentNode.SelectNodes("//div[@class='block-container wr']//div[@class='block']//div[@class='block-inner']//div[@class='number']")[1].ChildNodes[0].InnerHtml;
			var wrs = doc.DocumentNode.SelectNodes("//div[@class='block-container wr']//div[@class='block']//div[@class='block-inner']//div[@class='number']")[2].ChildNodes[0].InnerHtml;

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

		public static string GetUserRank(string url, int index)
		{
			Logging.CON("Requesting new user rank", ConsoleColor.DarkBlue);

			// Check if map has a leaderboard
			var mapid = Data.portal2Maps[index, 0];
			if (mapid == string.Empty)
				return "Map is not supported.";

			// Download website
			var doc = Caching.GetCache(url);

			// Check if profile is actually a Portal 2 challenge mode runner
			if (doc.DocumentNode.Descendants("div").Any(a => a.GetAttributeValue("class", string.Empty) == "user-noexist"))
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

			var player = doc.DocumentNode.SelectNodes("//head//title")[0].ChildNodes[0].InnerHtml;
			var ranking = doc.DocumentNode.SelectNodes("//div[@class='cell rank']")[idx].ChildNodes[0].InnerHtml;
			var time = doc.DocumentNode.SelectNodes("//a[@class='cell score']")[idx].ChildNodes[0].InnerHtml.Replace("\n", string.Empty).Replace(" ", string.Empty);
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
				if (allbold)
					return $"**#{r}**";
				else
					return $"#{r}";
			if (r == "1")
				return "**1st** :trophy:";
			if (r == "2")
				return "**2nd**";
			if (r == "3")
				return "**3rd**";
			return $"**{r}th**";
		}

		private static string AddDecimalPlace(string s) => s.Contains(".") ? s : $"{s}.0";
	}
}