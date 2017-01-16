using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using HtmlAgilityPack;
using NeKzBot.Server;

namespace NeKzBot
{
	public partial class Leaderboard
	{
		internal class Cache
		{
			private static Stopwatch cacheWatch;
			private static string cacheKey;

			public static void Init()
			{
				// Reserve cache memory
				cacheKey = cacheKey ?? "lb";
				Caching.CApplication.Save(cacheKey, new Dictionary<string, HtmlDocument>());
			}

			// Get new or cached document
			public static async Task<HtmlDocument> GetCache(string url, bool ignore = false)
			{
				// Get cache
				var cache = Caching.CApplication.Get(cacheKey);

				// Search and find cached data
				if (!ignore)
					if (cache != null)
						foreach (Dictionary<string, HtmlDocument> item in cache)
							if (item.ContainsKey(url))
								return item.Values.First();

				// Download data
				var doc = new HtmlDocument();
				try
				{
					doc = await Fetching.GetDocument(url);
				}
				catch (Exception ex)
				{
					Logging.CHA($"Fetching error\n{ex.ToString()}");
				}
				if (doc == null)
					return null;

				// Parse url + webpage
				var temp = new Dictionary<string, HtmlDocument>();
				temp.Add(url, doc);
				
				// Save cache
				Logging.CON($"CACHING DATA WITH {Utils.StringInBytes(url, doc.DocumentNode.InnerText)} bytes");
				Caching.CApplication.Add(cacheKey, temp);
				temp = null;
				return doc;
			}

			// Timer
			public static async Task Reset()
			{
				try
				{
					Logging.CON("Lb caching reset started", ConsoleColor.DarkBlue);

					for (;;)
					{
						Logging.CON("Lb caching reset cleared", ConsoleColor.DarkBlue);
						cacheWatch = new Stopwatch();
						cacheWatch.Start();
						Caching.CApplication.ClearData(cacheKey);
						await Task.Delay((int)Settings.Default.CachingTime * 60000);
					}
				}
				catch
				{
					Logging.CON("Leaderboard caching error");
				}
			}

			// Get time when cache will be cleared
			public static string GetCleanCacheTime()
			{
				var min = Convert.ToInt16(Settings.Default.CachingTime) - cacheWatch.Elapsed.Minutes;
				if (min < 1)
					return "Leaderboard cache will be cleared soon.";
				if (min == 1)
					return "Leaderboard cache will be cleared in 1 minute.";
				return $"Leaderboard cache will be cleared in {min.ToString()} minutes.";
			}

			// Set time when to clear cache
			public static string SetCleanCacheTime(string t)
			{
				if (!Utils.ValidateString(t, "^[1-9]", 4))
					return "Invalid paramter.";
				var time = Convert.ToInt16(t);
				if (time < 1 || time > 1440)
					return "Invalid paramter. Time is in minutes.";
				Settings.Default.CachingTime = (uint)time;
				Settings.Default.Save();
				return $"New clean cache time is set to **{t}min**.";
			}
		}
	}
}