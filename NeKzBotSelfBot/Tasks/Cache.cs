using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using NeKzBot.Resources;
using NeKzBot.Server;

namespace NeKzBot.Tasks
{
	public static partial class Leaderboard
	{
		internal static class Cache
		{
			private static string _cacheKey;

			public static async Task InitAsync()
			{
				await Logger.SendAsync("Initializing Leaderboard Cache", LogColor.Init);
				// Reserve cache memory
				_cacheKey = _cacheKey ?? "lb";
				await Caching.CApplication.SaveCacheAsync(_cacheKey, new Dictionary<string, HtmlDocument>());
			}

			// Get new or cached document
			internal static async Task<HtmlDocument> GetCacheAsync(string url, bool ignore = false)
			{
				// Get cache
				var cache = await Caching.CApplication.GetCacheAsync(_cacheKey);

				// Search and find cached data
				if (!(ignore && cache != null))
					foreach (Dictionary<string, HtmlDocument> item in cache)
						if (item.ContainsKey(url))
							return item.Values.FirstOrDefault();

				// Download data
				var doc = new HtmlDocument();
				try
				{
					if ((doc = await Fetching.GetDocumentAsync(url)) == null)
						return null;
				}
				catch (Exception e)
				{
					return await Logger.SendAsync("Fetching.GetDocumentAsync Error (Leaderboard.Cache.GetCacheAsync)", e) as HtmlDocument;
				}

				// Save cache
				await Logger.SendAsync($"Leaderboard.Cache.GetCacheAsync Caching -> {await Utils.StringInBytesAsync(url, doc.DocumentNode.InnerText)} bytes", LogColor.Caching);
				await Caching.CApplication.AddCache(_cacheKey, new Dictionary<string, HtmlDocument> { [url] = doc });
				return doc;
			}

			internal static async Task ResetAsync()
			{
				await Logger.SendAsync("Leaderboard.Cache.ResetAsync Started", LogColor.Leaderboard);
				try
				{
					for (;;)
					{
						// Clear every 10 minutes
						await Task.Delay(10 * 60000);
						await Caching.CApplication.ClearDataAsync(_cacheKey);
					}
				}
				catch
				{
					await Logger.SendAsync("Leaderboard.ResetAsync Error", LogColor.Error);
				}
			}
		}
	}
}