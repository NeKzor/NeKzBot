using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using NeKzBot.Internals;
using NeKzBot.Internals.Entities;
using NeKzBot.Resources;
using NeKzBot.Server;
using NeKzBot.Utilities;

namespace NeKzBot.Tasks.Leaderboard
{
	public static partial class Portal2
	{
		internal static class Cache
		{
			public static bool IsRunning { get; private set; } = false;
			public static InternalWatch Watch { get; } = new InternalWatch();
			private static Stopwatch _cacheWatch;
			private static string _cacheKey;
			private static Fetcher _fetchClient;

			public static async Task InitAsync()
			{
				await Logger.SendAsync("Initializing Portal2 Cache", LogColor.Init);
				_fetchClient = new Fetcher(15);
				// Reserve cache memory
				_cacheKey = "lb";
				await Caching.CApplication.ReserverMemoryAsync<Tuple<string, HtmlDocument>>(_cacheKey);
			}

			// Get new or cached document
			public static async Task<HtmlDocument> GetAsync(string url, bool ignore = false)
			{
				// Get cache
				var cache = (await Caching.CApplication.GetCache(_cacheKey))?.Cast<Tuple<string, HtmlDocument>>().ToList();
				var index = cache?.FindIndex(c => c.Item1 == url) ?? -1;

				// Search and find cached data
				if ((!(ignore)) && (index != -1))
					return cache[index].Item2;

				// Download data
				var doc = new HtmlDocument();
				try
				{
					doc = await _fetchClient.GetDocumentAsync(url);
					if (doc == null)
						return null;
				}
				catch (Exception e)
				{
					return await Logger.SendAsync("Fetching.GetDocumentAsync Error (Portal2.Cache.GetAsync)", e) as HtmlDocument;
				}

				// Add if not found or replace if new was requested
				if (index == -1)
					cache.Add(new Tuple<string, HtmlDocument>(url, doc));
				else
					cache[index] = new Tuple<string, HtmlDocument>(url, doc);

				// Save cache
				await Caching.CApplication.AddOrUpdateCache(_cacheKey, cache);
				cache = null;
				return doc;
			}

			// Timer
			public static async Task ResetAsync()
			{
				await Logger.SendAsync("Portal2 Reset Cache Started", LogColor.Leaderboard);
				_cacheWatch = new Stopwatch();
				_cacheWatch.Start();
				IsRunning = true;
				try
				{
					for (;;)
					{
						var delay = (int)(Configuration.Default.CachingTime * 60 * 1000) - await Watch.GetElapsedTime(debugmsg: "Portal2.Cache.ResetAsync Delay Took -> ");
						await Task.Delay((delay > 0) ? delay : 0);
						await Watch.RestartAsync();

						// Cache stuff
						await Task.Factory.StartNew(() => _cacheWatch.Restart());
						await Caching.CApplication.ClearDataAsync(_cacheKey);

						// Use cache reset to set new game and status
						await Task.Factory.StartNew(async () => Bot.Client.SetGame(await Utils.RngAsync((await Data.Get<Simple>("games")).Value)));
						await Task.Factory.StartNew(async () => Bot.Client.SetStatus(await Utils.RngAsync(Data.BotStatus)));
					}
				}
				catch
				{
					await Logger.SendAsync("Portal2.Cache.ResetAsync Error", LogColor.Error);
				}
				IsRunning = false;
			}

			// Get time when cache will be cleared
			public static Task<string> GetCleanCacheTime()
			{
				var min = Convert.ToInt16(Configuration.Default.CachingTime) - _cacheWatch.Elapsed.Minutes;
				return Task.FromResult((min < 1)
											? "Leaderboard cache will be cleared soon."
											: (min == 1)
												   ? "Leaderboard cache will be cleared in 1 minute."
												   : $"Leaderboard cache will be cleared in {min} minutes.");
			}

			// Set time when to clear cache
			public static async Task<string> SetCleanCacheTimeAsync(string t)
			{
				if (!(await Utils.ValidateString(t, "^[1-9]", 4)))
					return "Invalid parameter.";
				var time = Convert.ToInt16(t);
				if ((time < 1)
				|| (time > 1440))
					return "Invalid parameter. Time is in minutes.";
				Configuration.Default.CachingTime = (uint)time;
				Configuration.Default.Save();
				return $"New clean cache time is set to **{t}min**.";
			}
		}
	}
}