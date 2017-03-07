using System;
using System.Collections.Generic;
using System.Compat.Web;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using NeKzBot.Classes;
using NeKzBot.Resources;
using NeKzBot.Server;

namespace NeKzBot.Tasks.Leaderboard
{
	public static partial class Portal2
	{
		private const int _maxytid = 11;     // Maximum length of a YouTube id

		public static async Task<Portal2Entry> GetLatestEntryAsync(string url)
		{
			var doc = await Cache.GetAsync(url);
			if (doc != null)
			{
				try
				{
					const string root = "//div[@class='datatable page-entries active']";
					var map = doc.DocumentNode.SelectSingleNode($"{root}//div[@class='map']//a")?.FirstChild?.InnerHtml ?? string.Empty;
					var ranking = doc.DocumentNode.SelectSingleNode($"{root}//div[@class='newscore']//div[@class='rank']")?.FirstChild?.InnerHtml ?? string.Empty;
					var time = doc.DocumentNode.SelectSingleNode($"{root}//div[@class='newscore']//a[@class='time']")?.FirstChild?.InnerHtml ?? string.Empty;
					var player = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode($"{root}//div[@class='boardname']//a")?.FirstChild?.InnerHtml) ?? string.Empty;
					var date = doc.DocumentNode.SelectSingleNode($"{root}//div[@class='entry']//div[@class='date']")?.Attributes["date"]?.Value?.ToString() ?? string.Empty;

					if (new List<string>() { map, ranking, time, player, date }.Contains(string.Empty))
					{
						await Logger.SendToChannelAsync("Portal2.GetLatestEntryAsync Node Is Empty", LogColor.Error);
						return null;
					}

					// Only add when it exists
					var demo = (doc.DocumentNode.SelectSingleNode($"{root}//div[@class='demo-url']").Descendants("a")
						.Any())
						? $"http://board.iverb.me{doc.DocumentNode.SelectSingleNode($"{root}//div[@class='demo-url']//a").Attributes["href"].Value}"
						: string.Empty;
					var youtube = (doc.DocumentNode.SelectSingleNode($"{root}//div[@class='youtube']").Descendants("i")
						.Any())
						? $"http://youtu.be/{doc.DocumentNode.SelectSingleNode($"{root}//div[@class='youtube']//i").Attributes["onclick"].Value.Substring("embedOnBody('".Length, _maxytid)}"
						: string.Empty;
					var comment = (doc.DocumentNode.SelectSingleNode($"{root}//div[@class='comment']").Descendants("i")
						.Any())
						? HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode($"{root}//div[@class='comment']//i").Attributes["data-content"].Value)
						: string.Empty;

					return new Portal2Entry
					{
						Comment = comment,
						YouTube = youtube,
						Demo = demo,
						Date = date,
						Map = map,
						MapId = map,
						Player = await GetUserStatsAsync($"https://board.iverb.me/profile/{player}"), // This makes it slower :c
						Ranking = ranking,
						Time = time
					};
				}
				catch (Exception e)
				{
					await Logger.SendToChannelAsync("Portal2.GetLatestEntryAsync Error", e);
					doc?.Save(await Caching.CFile.GetPathAndSaveAsync("lb"));
				}
			}
			return null;
		}

		public static async Task<List<Portal2EntryUpdate>> GetEntryUpdateAsync(string url, uint count)
		{
			await Logger.SendAsync("Portal 2 Entry Update Request", LogColor.Leaderboard);

			var doc = await Cache.GetAsync(url, true);
			if (doc != null)
			{
				try
				{
					var entries = new List<Portal2EntryUpdate>();
					for (int i = 0; i < count; i++)
					{
						var node = HtmlNode.CreateNode(doc.DocumentNode.SelectNodes("//div[@class='datatable page-entries active']//div[@class='entry']")[i].InnerHtml);
						var map = node.SelectSingleNode("//div[@class='map']//a")?.FirstChild?.InnerHtml ?? string.Empty;
						var ranking = node.SelectSingleNode("//div[@class='newscore']//div[@class='rank']")?.FirstChild?.InnerHtml ?? string.Empty;
						var time = node.SelectSingleNode("//div[@class='newscore']//a[@class='time']")?.FirstChild?.InnerHtml ?? string.Empty;
						var player = HttpUtility.HtmlDecode(node.SelectSingleNode("//div[@class='boardname']//a")?.FirstChild?.InnerHtml) ?? string.Empty;
						var date = node.SelectSingleNode("//div[@class='date']")?.Attributes["date"]?.Value?.ToString() ?? string.Empty;

						if (new List<string>() { map, ranking, time, player, date }.Contains(string.Empty))
						{
							await Logger.SendToChannelAsync("Portal2.GetEntryUpdateAsync Node Is Empty", LogColor.Error);
							return null;
						}

						// Only add when it exists
						var demo = (node.SelectSingleNode("//div[@class='demo-url']").Descendants("a")
							.Any())
							? $"http://board.iverb.me{node.SelectSingleNode("//div[@class='demo-url']//a").Attributes["href"].Value}"
							: string.Empty;
						var youtube = (node.SelectSingleNode("//div[@class='youtube']").Descendants("i")
							.Any())
							? $"http://youtu.be/{node.SelectSingleNode("//div[@class='youtube']//i").Attributes["onclick"].Value.Substring("embedOnBody('".Length, _maxytid)}"
							: string.Empty;
						var comment = (node.SelectSingleNode("//div[@class='comment']").Descendants("i")
							.Any())
							? HttpUtility.HtmlDecode(node.SelectSingleNode("//div[@class='comment']//i").Attributes["data-content"].Value)
							: string.Empty;

						var temp = node.SelectSingleNode("//div[@class='map']//a")?.Attributes["href"].Value;
						var mapid = temp.Substring("/chamber/".Length, temp.Length - "/chamber/".Length);
						var profile = node.SelectSingleNode("//div[@class='profileIcon']//a").Attributes["href"].Value;
						var avatar = node.SelectSingleNode("//div[@class='profileIcon']//a//img").Attributes["src"].Value;

						entries.Add(new Portal2EntryUpdate()
						{
							// Webhooks <3
							Entry = new Portal2Entry
							{
								Comment = comment,
								Date = date,
								Demo = demo,
								Map = map,
								MapId = mapid,
								Player = new Portal2User
								{
									Name = player,
									SteamLink = profile,
									SteamAvatar = avatar
								},
								Ranking = ranking,
								Time = time,
								YouTube = youtube
							},
							// This will also detect player name changes (pls don't) and ties
							CacheFormat = $"{map}{time}{player}",
							// Tweet
							Tweet = new Portal2TweetUpdate
							{
								Message =  await FormatMainTweetAsync($"New World Record in {map}\n{time} by {player}\n{date} (UTC)", demo, youtube),
								Location = player,
								CommentMessage = await FormatReplyTweetAsync(player, comment)
							}
						});
					}
					return entries;
				}
				catch (Exception e)
				{
					await Logger.SendToChannelAsync("Portal2.GetEntryUpdateAsync Error", e);
					doc?.Save(await Caching.CFile.GetPathAndSaveAsync("lb"));
				}
			}
			return null;
		}

		public static async Task<Portal2User> GetUserStatsAsync(string url)
		{
			var doc = await Cache.GetAsync(url);
			if ((doc != null)
			&& !((bool)(doc?.DocumentNode.Descendants("div").Any(node => node.GetAttributeValue("class", string.Empty) == "user-noexist"))))
			{
				try
				{
					return new Portal2User()
					{
						SteamLink = doc.DocumentNode.SelectNodes("//div[@class='usericons']//a").Last().Attributes["href"].Value,
						SteamAvatar = doc.DocumentNode.SelectSingleNode("//div[@class='general-wrapper']//img").Attributes["src"].Value,
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
						Name = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//head//title").FirstChild.InnerHtml),
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

		public static async Task<Portal2Entry> GetUserRankAsync(string url, int index)
		{
			// Check if map has a leaderboard
			var mapid = Data.Portal2Maps[index, 0];
			if (mapid == string.Empty)
				return null;	// "Map is not supported.";

			var doc = await Cache.GetAsync(url);
			if (doc != null)
			{
				try
				{
					// Check if profile is actually a Portal 2 challenge mode runner
					if (doc.DocumentNode.Descendants("div").Any(n => n.GetAttributeValue("class", string.Empty) == "user-noexist"))
						return null;	// "Player profile doesn't exist.";

					// First get all maps
					var maps = doc.DocumentNode.SelectNodes("//div[@class='cell title']//a");
					var map = Data.Portal2Maps[index, 2];

					// Find index map of all nodes
					var idx = 0;
					for (; idx < maps.Count; idx++)
						if (maps[idx].Attributes["href"].Value == "/chamber/" + mapid)
							break;

					var player = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//head//title").FirstChild.InnerHtml);
					// This should fix it
					var node = HtmlNode.CreateNode(doc.DocumentNode.SelectNodes("//div[@class='chamberScoreInfo']")[idx].InnerHtml);
					var ranking = node.SelectSingleNode("//div[@class='cell rank']").FirstChild.InnerHtml;

					// Check if the rank doesn't exist
					if (ranking == "-")
						return null; // "Player doesn't have a ranking for this map.";

					var time = node.SelectSingleNode("//a[@class='cell score']").FirstChild.InnerHtml.Replace("\n", string.Empty).Replace(" ", string.Empty);
					// Difference shouldn't exist since everything is UTC, parsing this one because the new created node doesn't come with the date attribute
					var date = node.SelectSingleNode("//div[@class='cell dateDifferenceColor']").Attributes["date"].Value;

					// Only add when it exists
					var demo = (node.SelectSingleNode("//div[@class='cell demo-url']//a").Attributes["style"]
						== null)
						? $"http://board.iverb.me{node.SelectSingleNode("//div[@class='cell demo-url']//a").Attributes["href"].Value}"
						: string.Empty;
					var youtube = (node.SelectSingleNode("//div[@class='cell youtube']//i").Attributes["onclick"]
						!= null)
						? $"http://youtu.be/{node.SelectSingleNode("//div[@class='cell youtube']//i").Attributes["onclick"].Value.Substring("embedOnBody('".Length, _maxytid)}"
						: string.Empty;
					var comment = (node.SelectSingleNode("//div[@class='cell comment']//i").Attributes["data-content"].Value
						!= string.Empty)
						? HttpUtility.HtmlDecode(node.SelectSingleNode("//div[@class='cell comment']//i").Attributes["data-content"].Value)
						: string.Empty;

					var steamlink = doc.DocumentNode.SelectNodes("//div[@class='usericons']//a").Last().Attributes["href"].Value;
					var steamavatar = doc.DocumentNode.SelectSingleNode("//div[@class='general-wrapper']//img").Attributes["src"].Value;

					return new Portal2Entry
					{
						Player = new Portal2User
						{
							Name = player,
							SteamLink = steamlink,
							SteamAvatar = steamavatar
						},
						Date = date,
						Map = map,
						MapId = mapid,
						Ranking = ranking,
						Time = time,
						Demo = demo,
						YouTube = youtube,
						Comment = comment
					};
				}
				catch
				{
					await Logger.SendAsync("Portal2.GetUserRankAsync Error", LogColor.Error);
					doc?.Save(await Caching.CFile.GetPathAndSaveAsync("lb"));
				}
			}
			return null;
		}

		public static async Task<bool> CheckIfUserHasWorldRecordAsync(string url)
		{
			var doc = await Cache.GetAsync(url);
			if (doc != null)
			{
				try
				{
					return doc.DocumentNode.SelectNodes("//div[@id='changelog']//div[@class='activity']") != null;
				}
				catch (Exception e)
				{
					await Logger.SendToChannelAsync("Portal2.CheckIfUserHasWorldRecordAsync Error", e);
					doc?.Save(await Caching.CFile.GetPathAndSaveAsync("lb"));
				}
			}
			return false;
		}

		private static Task<string> RankFromat(string s)
		{
			var output = $"Rank • **{s}th**";
			if (Convert.ToInt16(s) > 10)
				output = $"Rank • **#{s}**";
			else if (s == "1")
				output = string.Empty;  // Trophy maybe?
			else if (s == "2")
				output = "Rank • **2nd**";
			else if (s == "3")
				output = "Rank • **3rd**";
			return Task.FromResult(output);
		}

		private static Task<string> AddDecimalPlace(string s)
			=> Task.FromResult((s.Contains(".")
							|| (s == "NO"))
								  ? s
								  : $"{s}.0");

		private static Task<string> FormatRank(string s)
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

		private static Task<string> FormatPoints(string s)
			=> Task.FromResult((s != "0")
								  ? int.Parse(s).ToString("#,###,###.##")
								  : s);

		private static async Task<string> FormatMainTweetAsync(string msg, params string[] stuff)
		{
			var output = msg;
			try
			{
				// Check if this (somehow) exceed the character limit
				if (Twitter.TweetLimit - output.Length < 0)
					return string.Empty;

				// Only append stuff if it fits
				foreach (var item in stuff)
				{
					if (item == string.Empty)
						continue;
					if (Twitter.TweetLimit - output.Length - item.Length < 0)
						return output;
					output += "\n" + item;
				}
				return output;
			}
			catch (Exception e)
			{
				await Logger.SendToChannelAsync("Portal2.FormatMainTweetAsync Error", e);
			}
			return string.Empty;
		}

		private static async Task<string> FormatReplyTweetAsync(string player, string comment)
		{
			var newline = $"@Portal2Records {player}: ";
			var output = string.Empty;
			try
			{
				// There isn't always a comment
				if (comment == string.Empty)
					return comment;

				// Check what's left
				const string cut = "...";
				var left = Twitter.TweetLimit - output.Length - newline.Length - comment.Length;
				if ((-left + cut.Length == comment.Length)
				|| (newline.Length >= Twitter.TweetLimit - output.Length))
					return output;

				// It's safe
				if (left >= 0)
					return newline + comment;

				// Cut comment and append "..." at the end
				return newline + comment.Substring(0, Twitter.TweetLimit - output.Length - newline.Length - cut.Length) + cut;
			}
			catch (Exception e)
			{
				await Logger.SendToChannelAsync("Portal2.FormatReplyTweetAsync Error", e);
			}
			return string.Empty;
		}
	}
}