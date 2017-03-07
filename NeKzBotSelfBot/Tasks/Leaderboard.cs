using System;
using System.Collections.Generic;
using System.Compat.Web;
using System.Linq;
using System.Threading.Tasks;
using NeKzBot.Classes;
using NeKzBot.Server;

namespace NeKzBot.Tasks
{
	public static partial class Leaderboard
	{
		private const int _maxytid = 11;     // Maximum length of a YouTube ID

		public static async Task<Portal2Entry> GetLatestEntryAsync(string url)
		{
			var doc = await Cache.GetCacheAsync(url);
			if (doc != null)
			{
				try
				{
					var temp = doc.DocumentNode.SelectSingleNode("//div[@class='map']//a")?.Attributes["href"].Value;
					var entry = new Portal2Entry()
					{
						MapId = temp.Substring("/chamber/".Length, temp.Length - "/chamber/".Length),
						Map = doc.DocumentNode.SelectSingleNode("//div[@class='map']//a")?.FirstChild?.InnerHtml ?? string.Empty,
						Ranking = await FormatRank(doc.DocumentNode.SelectSingleNode("//div[@class='newscore']//div[@class='rank']")?.FirstChild?.InnerHtml ?? string.Empty),
						Time = doc.DocumentNode.SelectSingleNode("//div[@class='newscore']//a[@class='time']")?.FirstChild?.InnerHtml ?? string.Empty,
						Player = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//div[@class='boardname']//a")?.FirstChild?.InnerHtml) ?? string.Empty,
						Date = doc.DocumentNode.SelectSingleNode("//div[@class='datatable page-entries active']//div[@class='entry']//div[@class='date']")?.Attributes["date"]?.Value?.ToString() ?? string.Empty,
						Demo = (doc.DocumentNode.SelectSingleNode("//div[@class='entry']//div[@class='demo-url']").Descendants("a")
							.Any())
							? $"http://board.iverb.me{doc.DocumentNode.SelectSingleNode("//div[@class='entry']//div[@class='demo-url']//a").Attributes["href"].Value}"
							: string.Empty,
						YouTube = (doc.DocumentNode.SelectSingleNode("//div[@class='entry']//div[@class='youtube']").Descendants("i")
							.Any())
							? $"https://youtu.be/{doc.DocumentNode.SelectSingleNode("//div[@class='youtube']//i").Attributes["onclick"].Value.Substring("embedOnBody('".Length, _maxytid)}"
							: string.Empty,
						Comment = (doc.DocumentNode.SelectSingleNode("//div[@class='entry']//div[@class='comment']").Descendants("i")
							.Any())
							? $"{HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//div[@class='entry']//div[@class='comment']//i").Attributes["data-content"].Value)}"
							: string.Empty
					};
					return (new List<string>() { entry.Map, entry.Ranking, entry.Time, entry.Player, entry.Date }
						.Contains(string.Empty))
						? await Logger.SendAsync("Leaderboard.GetLatestEntry Node Empty", LogColor.Error) as Portal2Entry
						: entry;
				}
				catch (Exception e)
				{
					await Logger.SendAsync("Leaderboard.GetLatestEntryAsync Error", e);
				}
			}
			return null;
		}

		public static async Task<Portal2User> GetUserStatsAsync(string url)
		{
			var doc = await Cache.GetCacheAsync(url);
			if ((doc != null)
			&& !((bool)(doc?.DocumentNode.Descendants("div").Any(node => node.GetAttributeValue("class", string.Empty) == "user-noexist"))))
			{
				try
				{
					return new Portal2User()
					{
						PlayerSteamLink = doc.DocumentNode.SelectNodes("//div[@class='usericons']//a").Last().Attributes["href"].Value,
						PlayerSteamAvatar = doc.DocumentNode.SelectSingleNode("//div[@class='general-wrapper']//img").Attributes["src"].Value,
						BestPlaceMap = (doc.DocumentNode.SelectNodes("//div[@class='block-container bestworst']//div[@class='block']//div[@class='block-inner']//div[@class='title']//span")[0].Descendants("a")
							.Any())
							? doc.DocumentNode.SelectNodes("//div[@class='block-container bestworst']//div[@class='block']//div[@class='block-inner']//div[@class='title']//span//a")[0].FirstChild.InnerHtml
							: "too many.",
						WorstPlaceMap = (doc.DocumentNode.SelectNodes("//div[@class='block-container bestworst']//div[@class='block']//div[@class='block-inner']//div[@class='title']//span//a")?
							.Count() == 2)
							? doc.DocumentNode.SelectNodes("//div[@class='block-container bestworst']//div[@class='block']//div[@class='block-inner']//div[@class='title']//span//a")[1].FirstChild.InnerHtml
							: (doc.DocumentNode.SelectNodes("//div[@class='block-container bestworst']//div[@class='block']//div[@class='block-inner']//div[@class='title']//span//a")?
								.Count() == 1)
								? doc.DocumentNode.SelectNodes("//div[@class='block-container bestworst']//div[@class='block']//div[@class='block-inner']//div[@class='title']//span//a")[0].FirstChild.InnerHtml
								: "too many.",
						BestPlaceRank = await FormatRank(doc.DocumentNode.SelectNodes("//div[@class='block-container bestworst']//div[@class='block']//div[@class='block-inner']//div[@class='number']")[0].FirstChild.InnerHtml),
						WorstPlaceRank = await FormatRank(doc.DocumentNode.SelectNodes("//div[@class='block-container bestworst']//div[@class='block']//div[@class='block-inner']//div[@class='number']")[1].FirstChild.InnerHtml),
						PlayerName = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//head//title").FirstChild.InnerHtml),
						SinglePlayerRank = await FormatRank(doc.DocumentNode.SelectNodes("//div[@class='block-container ranks']//div[@class='block']//div[@class='block-inner']//div[@class='number']")[0].FirstChild.InnerHtml),
						CooperativeRank = await FormatRank(doc.DocumentNode.SelectNodes("//div[@class='block-container ranks']//div[@class='block']//div[@class='block-inner']//div[@class='number']")[1].FirstChild.InnerHtml),
						OverallRank = await FormatRank(doc.DocumentNode.SelectNodes("//div[@class='block-container ranks']//div[@class='block']//div[@class='block-inner']//div[@class='number']")[2].FirstChild.InnerHtml),
						AverageSinglePlayerRank = await AddDecimalPlace(doc.DocumentNode.SelectNodes("//div[@class='block-container average']//div[@class='block']//div[@class='block-inner']//div[@class='number']")[0].FirstChild.InnerHtml.Replace(" ", string.Empty).Replace("\n", string.Empty)),
						AverageCooperativeRank = await AddDecimalPlace(doc.DocumentNode.SelectNodes("//div[@class='block-container average']//div[@class='block']//div[@class='block-inner']//div[@class='number']")[1].FirstChild.InnerHtml.Replace(" ", string.Empty).Replace("\n", string.Empty)),
						AverageOverallRank = await AddDecimalPlace(doc.DocumentNode.SelectNodes("//div[@class='block-container average']//div[@class='block']//div[@class='block-inner']//div[@class='number']")[2].FirstChild.InnerHtml.Replace(" ", string.Empty).Replace("\n", string.Empty)),
						SinglePlayerPoints = await FormatPoints(doc.DocumentNode.SelectNodes("//div[@class='block-container points']//div[@class='block']//div[@class='block-inner']//div[@class='number']")[0].FirstChild.InnerHtml.Replace(" ", string.Empty).Replace("\n", string.Empty)),
						CooperativePoints = await FormatPoints(doc.DocumentNode.SelectNodes("//div[@class='block-container points']//div[@class='block']//div[@class='block-inner']//div[@class='number']")[1].FirstChild.InnerHtml.Replace(" ", string.Empty).Replace("\n", string.Empty)),
						OverallPoints = await FormatPoints(doc.DocumentNode.SelectNodes("//div[@class='block-container points']//div[@class='block']//div[@class='block-inner']//div[@class='number']")[2].FirstChild.InnerHtml.Replace(" ", string.Empty).Replace("\n", string.Empty)),
						SinglePlayerWorldRecords = doc.DocumentNode.SelectNodes("//div[@class='block-container wr']//div[@class='block']//div[@class='block-inner']//div[@class='number']")[0].FirstChild.InnerHtml,
						CooperativeWorldRecords = doc.DocumentNode.SelectNodes("//div[@class='block-container wr']//div[@class='block']//div[@class='block-inner']//div[@class='number']")[1].FirstChild.InnerHtml,
						OverallWorldRecords = doc.DocumentNode.SelectNodes("//div[@class='block-container wr']//div[@class='block']//div[@class='block-inner']//div[@class='number']")[2].FirstChild.InnerHtml
					};
				}
				catch (Exception e)
				{
					await Logger.SendAsync("Leaderboard.GetUserStatsAsync Error", e);
				}
			}
			return null;
		}

		public static async Task<Portal2Leaderboard> GetMapEntriesAsync(string url, int starting = 0, int count = 10)
		{
			var doc = await Cache.GetCacheAsync(url);
			if (doc != null)
			{
				try
				{
					var entries = new List<Portal2Entry>();
					for (int i = starting; i < count; i++)
					{
						var entry = new Portal2Entry()
						{
							Player = HttpUtility.HtmlDecode(doc.DocumentNode.SelectNodes("//div[@class='datatable page-entries active']//div//div[@class='boardname']//a")?[i].FirstChild.InnerHtml) ?? string.Empty,
							Ranking = await FormatRank(doc.DocumentNode.SelectNodes("//div[@class='datatable page-entries active']//div//div[@class='place']")?[i].FirstChild.InnerHtml ?? string.Empty),
							Time = doc.DocumentNode.SelectNodes("//div[@class='datatable page-entries active']//div//a[@class='score']")?[i].FirstChild.InnerHtml ?? string.Empty,
							Date = doc.DocumentNode.SelectNodes("//div[@class='datatable page-entries active']//div//div[@class='date']")?[i].Attributes["date"]?.Value?.ToString() ?? string.Empty
						};

						// Don't check date because this can sometimes be unknown
						if (new List<string>() { entry.Ranking, entry.Time, entry.Player }.Contains(string.Empty))
							return await Logger.SendAsync("Leaderboard.GetMapEntriesAsync Node Empty", LogColor.Error) as Portal2Leaderboard;
						entries.Add(entry);
					}

					return new Portal2Leaderboard()
					{
						// This isn't really needed anymore
						//MapId = null,
						//MapPreview = null,
						MapName = doc.DocumentNode.SelectSingleNode("//head//title").InnerHtml,
						Entries = entries
					};
				}
				catch (Exception e)
				{
					await Logger.SendAsync("Leaderboard.GetMapEntriesAsync Error", e);
				}
			}
			return null;
		}

		internal static Task<string> FormatRank(string s)
		{
			if (s == "NO")
				return Task.FromResult(s);

			var output = $"{s}th";
			if ((s == "11")
			|| (s == "12")
			|| (s == "13"))
				return Task.FromResult(output);
			else if (s[s.Length - 1] == '1')
				output = $"{s}st";
			else if (s[s.Length - 1] == '2')
				output = $"{s}nd";
			else if (s[s.Length - 1] == '3')
				output = $"{s}rd";
			return Task.FromResult(output);
		}

		private static Task<string> AddDecimalPlace(string s)
			=> Task.FromResult((s.Contains("."))
							|| (s == "NO")
								? s
								: $"{s}.0");

		private static Task<string> FormatPoints(string s)
			=> Task.FromResult((s != "0")
								  ? int.Parse(s).ToString("#,###,###.##")
								  : s);
	}
}