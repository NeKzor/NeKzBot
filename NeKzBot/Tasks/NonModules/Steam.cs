using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using HtmlAgilityPack;
using NeKzBot.Server;
using NeKzBot.Resources;

namespace NeKzBot.Tasks.NonModules
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
		public static async Task<bool> CheckWorkshopAsync(Discord.MessageEventArgs args)
		{
			try
			{
				var msg = args.Message.Text;
				// Check for the link
				if (!(Uri.TryCreate(msg, UriKind.Absolute, out Uri uri)))
					return false;
				var path = uri.GetLeftPart(UriPartial.Authority);
				if ((path != "http://steamcommunity.com/sharedfiles/filedetails/")
				&& (path != "https://steamcommunity.com/sharedfiles/filedetails/"))
					return false;

				// Get cache
				var doc = await GetCacheAsync(msg);
				if (doc == null)
					return true;

				// Name of game
				var game = doc.DocumentNode.SelectSingleNode("//div[@class='apphub_AppName ellipsis']").InnerHtml;

				// Name of user
				var user = string.Empty;
				if (doc.DocumentNode.SelectSingleNode("//div[@class='friendBlockContent']") != null)
				{
					user = doc.DocumentNode.SelectSingleNode("//div[@class='friendBlockContent']").InnerHtml;
					user = user.Substring(0, user.LastIndexOf("<br>")).Replace("\n", string.Empty).Replace("\r", string.Empty).Replace("\t", string.Empty);
				}
				else
					user = doc.DocumentNode.SelectSingleNode("//div[@class='linkAuthor']//a").InnerHtml;

				// Title of item
				var item = doc.DocumentNode.SelectSingleNode("//div[@class='workshopItemTitle']").InnerHtml;

				// Workshop preview image
				var picture = (doc.DocumentNode.SelectSingleNode("//div[@class='workshopItemPreviewImageMain']") != null)
					? doc.DocumentNode.SelectSingleNode("//div[@class='workshopItemPreviewImageMain']//a").Attributes["onclick"].Value
					: (doc.DocumentNode.SelectSingleNode("//div[@id='highlight_player_area']") != null)
						? doc.DocumentNode.SelectSingleNode("//div[@id='highlight_player_area']//a").Attributes["onclick"].Value
						: (doc.DocumentNode.SelectSingleNode("//div[@class='collectionBackgroundImageContainer']") != null)
							? $"\n{doc.DocumentNode.SelectSingleNode("//div[@class='collectionBackgroundImageContainer']//img").Attributes["src"].Value}"
							: string.Empty;
				const string cut = "ShowEnlargedImagePreview( '";
				picture = ((picture != string.Empty)
				&& (picture.Contains(cut)))
						   ? "\n" + picture.Substring(cut.Length, picture.LastIndexOf("'") - cut.Length)
						   : picture;

				await args.Channel.SendMessage($"**[Steam Workshop - *{game}*]**\n{item} made by {user}{picture}");
			}
			catch (Exception e)
			{
				await Logger.SendToChannelAsync("Steam.CheckWorkshopAsync Error", e);
				await args.Channel.SendMessage("**Error**");
			}
			return true;
		}

		// Copy of Leaderboard.Cache
		public static async Task<HtmlDocument> GetCacheAsync(string url)
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
				doc = await Fetching.GetDocumentAsync(url);
				if (doc == null)
					return null;
			}
			catch (Exception e)
			{
				return await Logger.SendToChannelAsync("Fetching.GetDocumentAsync Error (Steam.GetCacheAsync)", e) as HtmlDocument;
			}

			// Save cache
			await Logger.SendAsync($"Steam.GetCacheAsync Caching -> {await Utils.StringInBytes(url, doc.DocumentNode.InnerText)} bytes", LogColor.Caching);
			await Caching.CApplication.AddCache(_cacheKey, new Dictionary<string, HtmlDocument> { [url] = doc });
			return doc;
		}
	}
}