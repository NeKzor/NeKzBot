using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using HtmlAgilityPack;
using NeKzBot.Server;
using NeKzBot.Classes;
using NeKzBot.Resources;

namespace NeKzBot.Tasks
{
	public static class Steam
	{
		private static string _cacheKey;

		public static async Task InitAsync()
		{
			await Logger.SendAsync("Initializing Steam", LogColor.Init);
			_cacheKey = _cacheKey ?? "steam";
			await Caching.CApplication.SaveCacheAsync(_cacheKey, new Dictionary<string, HtmlDocument>());
		}

		// Downloads Steam workshop item image
		public static async Task<SteamWorkshop> GetSteamWorkshopAsync(Uri link)
		{
			try
			{
				var path = link.GetLeftPart(UriPartial.Path);
				if ((path != "http://steamcommunity.com/sharedfiles/filedetails/")
				&& (path != "https://steamcommunity.com/sharedfiles/filedetails/"))
					return null;

				// Get cache
				var uri = link.AbsoluteUri;
				var doc = await GetCacheAsync(uri);
				if (doc == null)
					return null;

				const string cut = "ShowEnlargedImagePreview( '";
				var picture = (doc.DocumentNode.SelectSingleNode("//div[@class='workshopItemPreviewImageMain']")
					!= null)
					? doc.DocumentNode.SelectSingleNode("//div[@class='workshopItemPreviewImageMain']//a").Attributes["onclick"].Value
					: (doc.DocumentNode.SelectSingleNode("//div[@id='highlight_player_area']")
						!= null)
						? doc.DocumentNode.SelectSingleNode("//div[@id='highlight_player_area']//a").Attributes["onclick"].Value
						: (doc.DocumentNode.SelectSingleNode("//div[@class='collectionBackgroundImageContainer']")
							!= null)
							? $"\n{doc.DocumentNode.SelectSingleNode("//div[@class='collectionBackgroundImageContainer']//img").Attributes["src"].Value}"
							: string.Empty;
				picture = ((picture != string.Empty)
				&& (picture.Contains(cut)))
						   ? $"\n{picture.Substring(cut.Length, picture.LastIndexOf("'") - cut.Length)}"
						   : picture;

				return new SteamWorkshop()
				{
					UserLink = doc.DocumentNode.SelectSingleNode("//div[@class='creatorsBlock']//div//a[@class='friendBlockLinkOverlay']").Attributes["href"].Value,
					UserAvatar = doc.DocumentNode.SelectSingleNode("//div[@class='creatorsBlock']//div//div//img").Attributes["src"].Value,
					GameName = doc.DocumentNode.SelectSingleNode("//div[@class='apphub_AppName ellipsis']").InnerHtml,
					UserName = (doc.DocumentNode.SelectSingleNode("//div[@class='friendBlockContent']")
						!= null)
						? doc.DocumentNode.SelectSingleNode("//div[@class='friendBlockContent']").InnerHtml.Substring(0, doc.DocumentNode.SelectSingleNode("//div[@class='friendBlockContent']").InnerHtml.LastIndexOf("<br>")).Replace("\n", string.Empty).Replace("\r", string.Empty).Replace("\t", string.Empty)
						: doc.DocumentNode.SelectSingleNode("//div[@class='linkAuthor']//a").InnerHtml,
					ItemTitle = doc.DocumentNode.SelectSingleNode("//div[@class='workshopItemTitle']").InnerHtml,
					ItemImage = picture,
					ItemLink = uri
				};
			}
			catch (Exception e)
			{
				await Logger.SendAsync("Steam.WorkshopAsync Error", e);
			}
			return null;
		}

		// Copy of Leaderboard.Cache
		internal static async Task<HtmlDocument> GetCacheAsync(string url)
		{
			// Get cache
			var cache = await Caching.CApplication.GetCacheAsync(_cacheKey);

			// Search and find cached data
			if (cache != null)
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
				return await Logger.SendAsync("Fetching.GetDocumentAsync Error (Steam.GetCacheAsync)", e) as HtmlDocument;
			}

			// Save cache
			await Logger.SendAsync($"Steam.GetCacheAsync Caching -> {await Utils.StringInBytesAsync(url, doc.DocumentNode.InnerText)} bytes", LogColor.Caching);
			await Caching.CApplication.AddCache(_cacheKey, new Dictionary<string, HtmlDocument> { [url] = doc });
			return doc;
		}
	}
}