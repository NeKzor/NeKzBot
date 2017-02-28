using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using HtmlAgilityPack;
using NeKzBot.Internals;
using NeKzBot.Resources;
using NeKzBot.Server;

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

			public static async Task InitAsync()
			{
				await Logger.SendAsync("Initializing Portal2 Cache", LogColor.Init);
				// Reserve cache memory
				_cacheKey = _cacheKey ?? "lb";
				await Caching.CApplication.SaveCacheAsync(_cacheKey, new Dictionary<string, HtmlDocument>());
			}

			// Get new or cached document
			public static async Task<HtmlDocument> GetCacheAsync(string url, bool ignore = false)
			{
				// Get cache
				var cache = await Caching.CApplication.GetCacheAsync(_cacheKey);

				// Search and find cached data
				if (!(ignore))
					if (cache != null)
						foreach (Dictionary<string, HtmlDocument> item in cache)
							if (item.ContainsKey(url))
								return item.Values.FirstOrDefault();

				// Download data
				var doc = new HtmlDocument();
				try
				{
					doc = await Fetching.GetDocumentAsync(url);
					if (doc == null)
						return null;
				}
				catch (Exception e)
				{
					return await Logger.SendToChannelAsync("Fetching.GetDocumentAsync Error (Portal2.Cache.GetCacheAsync)", e) as HtmlDocument;
				}

				// Save cache
				await Logger.SendAsync($"Portal2.Cache.GetCacheAsync Caching -> {await Utils.StringInBytes(url, doc.DocumentNode.InnerText)} bytes", LogColor.Caching);
				await Caching.CApplication.AddCache(_cacheKey, new Dictionary<string, HtmlDocument> { [url] = doc });
				return doc;
			}

			// Timer
			public static async Task ResetCacheAsync()
			{
				await Logger.SendAsync("Portal2 Reset Cache Started", LogColor.Leaderboard);
				_cacheWatch = new Stopwatch();
				_cacheWatch.Start();
				IsRunning = true;
				try
				{
					for (;;)
					{
						await Task.Delay(((int)Configuration.Default.CachingTime * 60000) - await Watch.GetElapsedTimeAsync(message: "Portal2.Cache.ResetCacheAsync Delay Took -> "));
						await Watch.RestartAsync();

						// Cache stuff
						await Task.Factory.StartNew(() => _cacheWatch.Restart());
						await Caching.CApplication.ClearDataAsync(_cacheKey);

						// Use cache reset to set new game and status
						await Task.Factory.StartNew(async() => Bot.Client.SetGame(await Utils.RNGAsync(Data.RandomGames) as string));
						await Task.Factory.StartNew(async() => Bot.Client.SetStatus(await Utils.RNGAsync(Data.BotStatus) as UserStatus));
					}
				}
				catch
				{
					await Logger.SendAsync("Portal2.Cache.ResetCacheAsync Error", LogColor.Error);
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