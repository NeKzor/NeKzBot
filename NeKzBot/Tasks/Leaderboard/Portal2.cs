using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using NeKzBot.Server;
using NeKzBot.Classes;
using NeKzBot.Resources;

namespace NeKzBot.Tasks.Leaderboard
{
	public static partial class Portal2
	{
		private const int _maxytid = 11;     // Maximum length of a YouTube id

		public static async Task<string> GetLatestEntryAsync(string url)
		{
			var doc = await Cache.GetCacheAsync(url);
			if (doc != null)
			{
				try
				{
					const string root = "//div[@class='datatable page-entries active']";
					var map = doc.DocumentNode.SelectSingleNode($"{root}//div[@class='map']//a")?.FirstChild?.InnerHtml ?? string.Empty;
					var ranking = doc.DocumentNode.SelectSingleNode($"{root}//div[@class='newscore']//div[@class='rank']")?.FirstChild?.InnerHtml ?? string.Empty;
					var time = doc.DocumentNode.SelectSingleNode($"{root}//div[@class='newscore']//a[@class='time']")?.FirstChild?.InnerHtml ?? string.Empty;
					var player = doc.DocumentNode.SelectSingleNode($"{root}//div[@class='boardname']//a")?.FirstChild?.InnerHtml ?? string.Empty;
					var date = doc.DocumentNode.SelectSingleNode($"{root}//div[@class='entry']//div[@class='date']")?.Attributes["date"]?.Value?.ToString() ?? string.Empty;

					if (new List<string>() { map, ranking, time, player, date }.Contains(string.Empty))
					{
						await Logger.SendToChannelAsync("Portal2.GetLatestEntryAsync Node Is Empty", LogColor.Error);
						return null;
					}

					// Only add when it exists
					var demo = (doc.DocumentNode.SelectSingleNode($"{root}//div[@class='demo-url']").Descendants("a")
						.Any())
						? $"\nDemo • http://board.iverb.me{doc.DocumentNode.SelectSingleNode($"{root}//div[@class='demo-url']//a").Attributes["href"].Value}"
						: string.Empty;
					var youtube = (doc.DocumentNode.SelectSingleNode($"{root}//div[@class='youtube']").Descendants("i")
						.Any())
						? $"\nVideo • http://youtu.be/{doc.DocumentNode.SelectSingleNode($"{root}//div[@class='youtube']//i").Attributes["onclick"].Value.Substring("embedOnBody('".Length, _maxytid)}"
						: string.Empty;
					var comment = (doc.DocumentNode.SelectSingleNode($"{root}//div[@class='comment']").Descendants("i")
						.Any())
						? $"\nComment • ***{doc.DocumentNode.SelectSingleNode($"{root}//div[@class='comment']//i").Attributes["data-content"].Value}***"
						: string.Empty;

					ranking = (ranking == "1")
									   ? string.Empty
									   : $"Rank • {await TopTenFormat(ranking, true)}\n";
					var title = (ranking == string.Empty)
										 ? "**[World Record - *"
										 : "**[Entry - *";

					return	  $"{title + map}*]**\n"
							+ $"Time • **{time}**\n"
							+ $"Player • **{player}**\n"
							+ ranking
							+ $"Date • **{date}**"
							+ demo
							+ youtube
							+ comment;
				}
				catch (Exception e)
				{
					await Logger.SendToChannelAsync("Portal2.GetLatestEntryAsync Error", e);
					doc?.Save(await Caching.CFile.GetPathAndSaveAsync("lb"));
				}
			}
			return "**Error**";
		}

		public static async Task<List<Portal2EntryUpdate>> GetEntryUpdateAsync(string url, uint count)
		{
			await Logger.SendAsync("Portal 2 Entry Update Request", LogColor.Leaderboard);

			var doc = await Cache.GetCacheAsync(url, true);
			if (doc != null)
			{
				try
				{
					var entries = new List<Portal2EntryUpdate>();
					for (int i = 0; i < count; i++)
					{
						var node = HtmlAgilityPack.HtmlNode.CreateNode(doc.DocumentNode.SelectNodes("//div[@class='datatable page-entries active']//div[@class='entry']")[i].InnerHtml);
						var map = node.SelectSingleNode("//div[@class='map']//a")?.FirstChild?.InnerHtml ?? string.Empty;
						var ranking = node.SelectSingleNode("//div[@class='newscore']//div[@class='rank']")?.FirstChild?.InnerHtml ?? string.Empty;
						var time = node.SelectSingleNode("//div[@class='newscore']//a[@class='time']")?.FirstChild?.InnerHtml ?? string.Empty;
						var player = node.SelectSingleNode("//div[@class='boardname']//a")?.FirstChild?.InnerHtml ?? string.Empty;
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
							? node.SelectSingleNode("//div[@class='comment']//i").Attributes["data-content"].Value
							: string.Empty;

						// Might change this to wr only (it's currently possible to change the board parameter)
						ranking = (ranking == "1")
										   ? string.Empty
										   : $"Rank • {await TopTenFormat(ranking, true)}\n";
						var title = (ranking == string.Empty)
											 ? "**[New World Record - *"
											 : "**[New Entry - *";

						var temp = node.SelectSingleNode("//div[@class='map']//a")?.Attributes["href"].Value;
						var mapid = temp.Substring("/chamber/".Length, temp.Length - "/chamber/".Length);
						var profile = node.SelectSingleNode("//div[@class='profileIcon']//a").Attributes["href"].Value;
						var avatar = node.SelectSingleNode("//div[@class='profileIcon']//a//img").Attributes["src"].Value;

						entries.Add(new Portal2EntryUpdate()
						{
							ChannelMessage =  $"{title + map}*]**\n"
											+ $"Time • **{time}**\n"
											+ $"Player • **{player}**\n"
											+ ranking
											+ $"Date • **{date} (UTC)**"
											+ $"{(demo == string.Empty ? demo : $"\nDemo • {demo}")}"
											+ $"{(youtube == string.Empty ? youtube : $"\nVideo • {youtube}")}"
											+ $"{(comment == string.Empty ? comment : $"\nComment • ***{comment}***")}",
							// Tweet
							TweetMessage = await FormatMainTweetAsync($"New World Record in {map}\n{time} by {player}\n{date} (UTC)", demo, youtube),
							// This will also detect player name changes (pls don't) and ties
							CacheFormat = map + time + player,
							// Others
							TweetCache = new Portal2EntryUpdate.Cache()
							{
								Location = player,
								CommentMessage = await FormatReplyTweetAsync(player, comment)
							},
							// Webhooks <3
							Global = new Portal2Entry
							{
								Comment = comment,
								Date = date,
								Demo = demo,
								Map = map,
								MapID = mapid,
								Player = new Portal2User
								{
									Name = player,
									ProfileLink = profile,
									SteamAvatar = avatar
								},
								Ranking = ranking,
								Time = time,
								YouTube = youtube
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

		public static async Task<string> GetUserStatsAsync(string url)
		{
			var doc = await Cache.GetCacheAsync(url);
			if (doc != null)
			{
				if (doc.DocumentNode.Descendants("div").Any(node => node.GetAttributeValue("class", string.Empty) == "user-noexist"))
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
						fastpeopleonly += $"\nSingle Player • **{spwrs}** :trophy:";
					if (coopwrs != "0")
						fastpeopleonly += $"\nCooperative • **{coopwrs}** :trophy:";
					if (wrs != "0")
						fastpeopleonly += $"\nOverall • **{wrs}** :trophy:";
					else
						fastpeopleonly = string.Empty;

					return	  $"**[Global Statistics - *{player}*]**\n__*Ranking*__\n"
							+ $"Single Player • {await TopTenFormat(sprank)}\n"
							+ $"	(Average • {await AddDecimalPlace(spaverage)})\n"
							+ $"Cooperative • {await TopTenFormat(cooprank)}\n"
							+ $"	(Average • {await AddDecimalPlace(coopaverage)})\n"
							+ $"Overall • {await TopTenFormat(rank)}\n"
							+ $"	(Average • {await AddDecimalPlace(average)})"
							+ fastpeopleonly;
				}
				catch
				{
					await Logger.SendAsync("Portal.GetUserStatsAsync Error", LogColor.Error);
					doc?.Save(await Caching.CFile.GetPathAndSaveAsync("lb"));
				}
			}
			return "**Error**";
		}

		public static async Task<string> GetUserRankAsync(string url, int index)
		{
			// Check if map has a leaderboard
			var mapid = Data.Portal2Maps[index, 0];
			if (mapid == string.Empty)
				return "Map is not supported.";

			var doc = await Cache.GetCacheAsync(url);
			if (doc != null)
			{
				try
				{
					// Check if profile is actually a Portal 2 challenge mode runner
					if (doc.DocumentNode.Descendants("div").Any(node => node.GetAttributeValue("class", string.Empty) == "user-noexist"))
						return "Player profile doesn't exist.";

					// First get all maps
					var maps = doc.DocumentNode.SelectNodes("//div[@class='cell title']//a");
					var map = Data.Portal2Maps[index, 2];

					// Find index map of all nodes
					var idx = 0;
					for (; idx < maps.Count; idx++)
						if (maps[idx].Attributes["href"].Value == "/chamber/" + mapid)
							break;

					var player = doc.DocumentNode.SelectSingleNode("//head//title").FirstChild.InnerHtml;
					var ranking = doc.DocumentNode.SelectNodes("//div[@class='cell rank']")[idx].FirstChild.InnerHtml;
					var time = doc.DocumentNode.SelectNodes("//a[@class='cell score']")[idx].FirstChild.InnerHtml.Replace("\n", string.Empty).Replace(" ", string.Empty);
					var date = doc.DocumentNode.SelectNodes("//div[@class='chamberScoreInfo']")[idx].Attributes["date"].Value;

					// Only add when it exists
					var demo = (doc.DocumentNode.SelectNodes("//div[@class='cell demo-url']//a")[idx].Attributes["style"]
						== null)
						? $"\nDemo • http://board.iverb.me{doc.DocumentNode.SelectNodes("//div[@class='cell demo-url']//a")[idx].Attributes["href"].Value}"
						: string.Empty;
					var youtube = (doc.DocumentNode.SelectNodes("//div[@class='cell youtube']//i")[idx].Attributes["onclick"]
						!= null)
						? $"\nVideo • http://youtu.be/{doc.DocumentNode.SelectNodes("//div[@class='cell youtube']//i")[idx].Attributes["onclick"].Value.Substring("embedOnBody('".Length, _maxytid)}"
						: string.Empty;
					var comment = (doc.DocumentNode.SelectNodes("//div[@class='cell comment']//i")[idx].Attributes["data-content"].Value
						!= string.Empty)
						? $"\nComment • ***{doc.DocumentNode.SelectNodes("//div[@class='cell comment']//i")[idx].Attributes["data-content"].Value}***"
						: string.Empty;

					return	  $"{await GetTitle(ranking) + map}*]**\n"
							+ $"Player • **{player}**\n"
							+ $"Time • **{time}**\n"
							+ $"{await RankFromat(ranking)}"
							+ $"Date • **{date}**\n"
							+ demo
							+ youtube
							+ comment;
				}
				catch
				{
					await Logger.SendAsync("Portal2.GetUserRankAsync Error", LogColor.Error);
					doc?.Save(await Caching.CFile.GetPathAndSaveAsync("lb"));
				}
			}
			return "**Error**";
		}

		public static async Task<bool> CheckIfUserHasWorldRecordAsync(string url)
		{
			var doc = await Cache.GetCacheAsync(url);
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
			var output = $"Rank • **{s}th**\n";
			if (Convert.ToInt16(s) > 10)
				output = $"Rank • **#{s}**\n";
			else if (s == "1")
				output = string.Empty;  // Trophy maybe?
			else if (s == "2")
				output = "Rank • **2nd**\n";
			else if (s == "3")
				output = "Rank • **3rd**\n";
			return Task.FromResult(output);
		}

		private static Task<string> GetTitle(string s)
		{
			var output = "**[Top 10 - *";
			if (Convert.ToInt16(s) > 10)
				output = "**[Personal Best - *";
			if (s == "1")
				output = "**[World Record - *";
			return Task.FromResult(output);
		}

		private static Task<string> TopTenFormat(string r, bool allbold = false)
		{
			var output = $"**{r}th**";
			if (Convert.ToInt16(r) > 10)
			{
				output = (allbold)
							? $"**#{r}**"
							: $"#{r}";
			}
			else if (r == "1")
				output = "**1st** :trophy:";
			else if (r == "2")
				output = "**2nd**";
			else if (r == "3")
				output = "**3rd**";
			return Task.FromResult(output);
		}

		private static Task<string> AddDecimalPlace(string s)
			=> Task.FromResult(s.Contains(".")
								? s
								: $"{s}.0");

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