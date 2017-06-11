using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using HtmlAgilityPack;
using NeKzBot.Classes;
using NeKzBot.Extensions;
using NeKzBot.Resources;
using NeKzBot.Server;
using NeKzBot.Utilities;

namespace NeKzBot.Tasks.NonModules
{
	public static class Steam
	{
		private static Fetcher _fetchClient;
		private static string _cacheKey;

		public static async Task InitAsync()
		{
			await Logger.SendAsync("Initializing Steam", LogColor.Init);
			_fetchClient = new Fetcher();
			_cacheKey = "steam";
			await Caching.CApplication.ReserverMemoryAsync<Tuple<string, HtmlDocument>>(_cacheKey);
		}

		// Downloads Steam workshop item image
		public static async Task<bool> CheckWorkshopAsync(MessageEventArgs args)
		{
			try
			{
				// Check for the link
				if (!(Uri.TryCreate(args.Message.Text, UriKind.Absolute, out Uri uri)))
					return false;
				var result = await GetSteamWorkshopAsync(uri);
				if (result != null)
				{
					await Bot.SendAsync(CustomRequest.SendMessage(args.Channel.Id), new CustomMessage(new Embed
					{
						Author = new EmbedAuthor(result.UserName, result.UserLink, result.UserAvatar),
						Color = Data.SteamColor.RawValue,
						Title = $"{await Utils.AsRawText(result.GameName)} Workshop Item",
						Description = $"{await Utils.AsRawText(result.ItemTitle)} made by [{await Utils.AsRawText(result.UserName)}]({result.UserLink})",
						Url = result.ItemLink,
						Image = new EmbedImage(result.ItemImage),
						Footer = new EmbedFooter("steamcommunity.com", Data.SteamCommunityIconUrl)
					}));
					result = null;
				}
			}
			catch (Exception e)
			{
				await Logger.SendToChannelAsync("Steam.CheckWorkshopAsync Error", e);
			}
			return true;
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

				// Not the best check, but this should only support workshop items and not all the other shared files stuff
				var temp = doc.DocumentNode.SelectSingleNode("//title").InnerHtml;
				if (temp.Substring(0, "Steam Workshop".Length) != "Steam Workshop")
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

				var workshop = new SteamWorkshop
				{
					UserLink = doc.DocumentNode.SelectSingleNode("//div[@class='creatorsBlock']//div//a[@class='friendBlockLinkOverlay']").Attributes["href"].Value,
					UserAvatar = doc.DocumentNode.SelectSingleNode("//div[@class='creatorsBlock']//div//div//img").Attributes["src"].Value,
					GameName = doc.DocumentNode.SelectSingleNode("//div[@class='apphub_AppName ellipsis']").InnerHtml,
					UserName = doc.DocumentNode.SelectSingleNode("//div[@class='friendBlockContent']").InnerHtml.Substring(0, doc.DocumentNode.SelectSingleNode("//div[@class='friendBlockContent']").InnerHtml.IndexOf("<br>")).Replace("\n", string.Empty).Replace("\r", string.Empty).Replace("\t", string.Empty),
					ItemTitle = doc.DocumentNode.SelectSingleNode("//div[@class='workshopItemTitle']").InnerHtml,
					ItemImage = picture,
					ItemLink = uri
				};
				doc = null;
				return workshop;
			}
			catch (Exception e)
			{
				await Logger.SendToChannelAsync("Steam.GetSteamWorkshopAsync Error", e);
			}
			return default(SteamWorkshop);
		}

		// Copy of Leaderboard.Cache
		public static async Task<HtmlDocument> GetCacheAsync(string url)
		{
			// Get cache
			var cache = (await Caching.CApplication.GetCache(_cacheKey))?.Cast<Tuple<string, HtmlDocument>>().ToList();

			// Search cached data and return if it exists
			var index = cache?.FindIndex(c => c.Item1 == url) ?? -1;
			if (index != -1)
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
				return await Logger.SendToChannelAsync("Fetching.GetDocumentAsync Error (Steam.GetCacheAsync)", e) as HtmlDocument;
			}

			// Save cache
			await Logger.SendAsync($"Steam.GetCacheAsync Caching -> {await Utils.StringInBytes(url, doc.DocumentNode.InnerText)} bytes", LogColor.Caching);
			await Caching.CApplication.AddOrUpdateCache(_cacheKey, cache);
			cache = null;
			return doc;
		}
	}
}