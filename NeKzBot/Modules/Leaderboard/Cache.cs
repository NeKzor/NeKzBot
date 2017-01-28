using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using HtmlAgilityPack;
using NeKzBot.Server;
using NeKzBot.Resources;

namespace NeKzBot.Modules.Leaderboard
{
	public partial class Leaderboard
	{
		internal class Cache
		{
			public static bool isRunning = false;
			private static Stopwatch cacheWatch;
			private static string cacheKey;

			public static async Task Init()
			{
				// Reserve cache memory
				cacheKey = cacheKey ?? "lb";
				await Caching.CApplication.Save(cacheKey, new Dictionary<string, HtmlDocument>());
			}

			// Get new or cached document
			public static async Task<HtmlDocument> GetCache(string url, bool ignore = false)
			{
				// Get cache
				var cache = await Caching.CApplication.Get(cacheKey);
				
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
					await Logging.CHA("Fetching error", ex);
					return null;
				}
				if (doc == null)
					return null;

				// Parse url + webpage
				var temp = new Dictionary<string, HtmlDocument>();
				temp.Add(url, doc);
				
				// Save cache
				await Logging.CON($"Leaderboard cache -> {Utils.StringInBytes(url, doc.DocumentNode.InnerText)} bytes", ConsoleColor.Red);
				await Caching.CApplication.Add(cacheKey, temp);
				temp = null;
				return doc;
			}

			// Timer
			public static async Task Reset()
			{
				await Logging.CON("Lb caching reset started", ConsoleColor.DarkBlue);
				isRunning = true;
				try
				{
					for (;;)
					{
						cacheWatch = new Stopwatch();
						cacheWatch.Start();
						await Caching.CApplication.ClearData(cacheKey);
						await Task.Delay((int)Settings.Default.CachingTime * 60000);
					}
				}
				catch
				{
					await Logging.CON("Leaderboard reset cache error", ConsoleColor.Red);
				}
				isRunning = false;
			}

			// Get time when cache will be cleared
			public static Task<string> GetCleanCacheTime()
			{
				var min = Convert.ToInt16(Settings.Default.CachingTime) - cacheWatch.Elapsed.Minutes;
				if (min < 1)
					return Task.FromResult("Leaderboard cache will be cleared soon.");
				if (min == 1)
					return Task.FromResult("Leaderboard cache will be cleared in 1 minute.");
				return Task.FromResult($"Leaderboard cache will be cleared in {min.ToString()} minutes.");
			}

			// Set time when to clear cache
			public static Task<string> SetCleanCacheTime(string t)
			{
				if (!Utils.ValidateString(t, "^[1-9]", 4))
					return Task.FromResult("Invalid paramter.");
				var time = Convert.ToInt16(t);
				if (time < 1 || time > 1440)
					return Task.FromResult("Invalid paramter. Time is in minutes.");
				Settings.Default.CachingTime = (uint)time;
				Settings.Default.Save();
				return Task.FromResult($"New clean cache time is set to **{t}min**.");
			}
		}
	}
}