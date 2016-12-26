using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using HtmlAgilityPack;
using NeKzBot.Properties;

namespace NeKzBot
{
	// This class prevents users requesting too many downloads from board.iverb.me 
	public class Caching
	{
		private static Stopwatch cacheWatch;
		private static List<Dictionary<string, HtmlDocument>> dataCache;

		public static HtmlDocument GetCache(string url)
		{
			if (dataCache == null)
				dataCache = new List<Dictionary<string, HtmlDocument>>();
			else
			{
				// Search and find cached data
				foreach (var item in dataCache)
					if (item.ContainsKey(url))
						return item.Values.First();
			}

			// Download data
			var doc = new HtmlWeb().Load(url);

			// Cache url + webpage
			var temp = new Dictionary<string, HtmlDocument>();
			temp.Add(url, doc);
			dataCache.Add(temp);
			temp = null;
			Logging.CON($"CACHING DATA WITH {Utils.StringInBytes(url, doc.DocumentNode.InnerText)} bytes");
			return doc;
		}

		// Timer
		public static async Task ResetDataCache()
		{
			try
			{
				Logging.CON("ResetDataCache started", ConsoleColor.DarkBlue);

				// Caching is important so I guess it should always be on...
				while (Settings.Default.DataCaching)
				{
					Logging.CON("Resetting data cache", ConsoleColor.DarkBlue);
					cacheWatch = new Stopwatch();
					cacheWatch.Start();
					dataCache = null;	// Better than .Clear(), right?
					Logging.CON("ResetDataCache is now sleeping", ConsoleColor.DarkBlue);
					await Task.Delay((int)Settings.Default.CachingTime * 60000);
				}
			}
			catch
			{
				// ...and never be cancelled
				Logging.CON("ResetDataCache has been cancelled", ConsoleColor.DarkBlue);
			}
			finally
			{
				Logging.CON("ResetDataCache has ended", ConsoleColor.DarkBlue);
			}
		}

		// Get time when cache will be cleared
		public static string GetCleanCacheTime()
		{
			int min = Convert.ToInt16(Settings.Default.CachingTime) - cacheWatch.Elapsed.Minutes;
			if (min < 1)
				return "Leaderboard cache will be cleared soon.";
			if (min == 1)
				return "Leaderboard cache will be cleared in 1 minute.";
			return $"Leaderboard cache will be cleared in {min.ToString()} minutes.";
		}

		// Set time when to clear cache
		public static string SetCleanCacheTime(string t)
		{
			if (!Utils.ValidateString(t, "^[0-9]", 4))
				return "Invalid paramter.";
			int time = Convert.ToInt16(t);
			if (time < 1 || time > 1440)
				return "Invalid paramter. Time is in minutes.";
			Settings.Default.CachingTime = (uint)time;
			Settings.Default.Save();
			return $"New clean cache time is set to **{t}min**";
		}
	}
}