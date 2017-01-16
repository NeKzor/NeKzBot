using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace NeKzBot
{
	public class AutoDownloader : DropboxCom
	{
		private static string dropboxCacheKey;
		private static string steamCacheKey;

		private const int maxfilesperfolder = 20;
		private const int maxfilesize = 5000 * 1024;	// 5KB

		public static async Task Check(Discord.MessageEventArgs args)
		{
			await UserCloud(args);
			await SteamWorkshop(args);
		}

		public static void Load()
		{
			dropboxCacheKey = dropboxCacheKey ?? "dropbox";
			steamCacheKey = steamCacheKey ?? "steam";
			Caching.CApplication.Save(steamCacheKey, new Dictionary<string, HtmlDocument>());
		}
		
		// Uploads to Dropbox.com
		private static async Task UserCloud(Discord.MessageEventArgs args)
		{
			try
			{
				Load();
				if (args.Message.Attachments.Length > 0)
				{
					foreach (var file in args.Message.Attachments)
					{
						// Check file extension
						var filename = file.Filename;
						var extension = filename.Substring(filename.Length - 4, 4);

						// Only allow demos and saves because we like Source Engine games :^)
						if (extension == ".dem" || extension == ".sav")
						{
							// Maximum 5000KB
							if (file.Size > maxfilesize)
							{
								await args.Channel.SendMessage($"File {file.Filename} is too big to upload (max. {maxfilesize}KB).");
								continue;
							}
							else if (file.Size == 0)
							{
								await args.Channel.SendMessage($"File {file.Filename} seems to be broken.");
								continue;
							}

							// Download data
							try
							{
								await Fetching.GetFileAndCache(file.Url, dropboxCacheKey);
							}
							catch (System.Exception ex)
							{
								Logging.CHA($"Fetching error\n{ex.ToString()}");
							}

							// Get file
							var cacheFile = Caching.CFile.GetPathAndSave(dropboxCacheKey);
							if (string.IsNullOrEmpty(cacheFile))
							{
								await args.Channel.SendMessage("**Caching Error**");
								Logging.CON("AutoDownloader caching error");
								return;
							}

							// Every user has its on folder
							var username = args.User.Nickname;
							var path = Server.Settings.Default.DropboxFolderName;

							//// Don't create a folder with bad characters
							//if (!Utils.ValidateString(username, "^[a-zA-Z0-9_-]"))
							//	path += username;
							//else
								path += args.User.Id;	// Unique folder ids for safety of course

							// Check if folder full
							var files = await ListFiles(path);
							if (files != "No files found." && files != "**Error**")
							{
								if (files.Split('\n').Length > maxfilesperfolder)
								{
									await args.Channel.SendMessage($"Your folder is full. Try to list all files with {Server.Settings.Default.PrefixCmd}dbfolder and delete one with {Server.Settings.Default.PrefixCmd}dbdelete <filename>");
									continue;
								}
							}

							// Send file to Dropbox
							await args.Channel.SendMessage("Uploading...");
							await args.Channel.SendMessage(await Upload(path, filename, cacheFile));
						}
					}
				}
			}
			catch (System.Exception ex)
			{
				Logging.CHA($"AutoDownloader dropbox error\n{ex.ToString()}");
				await args.Channel.SendMessage("**Error**");
			}
		}

		// Downloads Steam workshop item image
		private static async Task SteamWorkshop(Discord.MessageEventArgs args)
		{
			try
			{
				var msg = args.Message.Text;

				// Check for the link
				var scan = "http://steamcommunity.com/sharedfiles/filedetails/?id=";
				if (msg.Length < scan.Length)
					return;

				// Only the link is allowed
				if (msg.Substring(0, scan.Length) == scan && !msg.Contains(" "))
				{
					// Get cache
					var doc = await GetCache(msg);
					if (doc == null)
						return;

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
					var picture = doc.DocumentNode.SelectSingleNode("//div[@class='workshopItemPreviewImageMain']") != null
						? doc.DocumentNode.SelectSingleNode("//div[@class='workshopItemPreviewImageMain']//a").Attributes["onclick"].Value.ToString()
						: doc.DocumentNode.SelectSingleNode("//div[@id='highlight_player_area']") != null
						? doc.DocumentNode.SelectSingleNode("//div[@id='highlight_player_area']//a").Attributes["onclick"].Value.ToString()
						: doc.DocumentNode.SelectSingleNode("//div[@class='collectionBackgroundImageContainer']") != null
						? "\n" + doc.DocumentNode.SelectSingleNode("//div[@class='collectionBackgroundImageContainer']//img").Attributes["src"].Value.ToString()
						: string.Empty;
					var cut = "ShowEnlargedImagePreview( '";
					picture = picture != string.Empty && picture.Contains(cut)
						? "\n" + picture.Substring(cut.Length, picture.LastIndexOf("'") - cut.Length)
						: picture;

					await args.Channel.SendMessage($"**[Steam Workshop - *{game}*]**\n{item} made by {user}{picture}");
				}
			}
			catch
			{
				Logging.CON("AutoDownloader steam error");
				await args.Channel.SendMessage("**Error**");
			}
		}

		// Copy of Leaderboard.Cache
		public static async Task<HtmlDocument> GetCache(string url)
		{
			// Get cache
			var cache = Caching.CApplication.Get(steamCacheKey);

			// Search and find cached data
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
			catch (System.Exception ex)
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
			Caching.CApplication.Add(steamCacheKey, temp);
			temp = null;
			return doc;
		}
	}
}