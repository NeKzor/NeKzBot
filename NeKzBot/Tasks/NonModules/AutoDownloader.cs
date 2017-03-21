using System;
using System.Threading.Tasks;
using Discord;
using NeKzBot.Classes;
using NeKzBot.Server;
using NeKzBot.Utilities;

namespace NeKzBot.Tasks.NonModules
{
	public static class AutoDownloader
	{
		private static string _cacheKey;

		public const uint MaxFilesPerFolder = 20;
		private const uint _maxfilesize = 5000 * 1024;   // 5KB

		// Upload to Dropbox.com
		public static async Task<bool> CheckDropboxAsync(MessageEventArgs args)
		{
			try
			{
				if (args.Message.Attachments.Length > 0)
				{
					foreach (var file in args.Message.Attachments)
					{
						// Check file extension
						var filename = file.Filename;
						var extension = filename.Substring(filename.Length - 4, 4);

						// Only allow demos and saves because Source Engine games :^)
						if ((extension == ".dem")
						|| (extension == ".sav"))
						{
							// Maximum 5000KB
							if (file.Size > _maxfilesize)
							{
								await args.Channel.SendMessage($"File {await Utils.AsRawText(filename)} is too large for an upload (max. {_maxfilesize}KB).");
								continue;
							}
							else if (file.Size == 0)
							{
								await args.Channel.SendMessage($"File {await Utils.AsRawText(filename)} seems to be broken.");
								continue;
							}

							// Download data
							_cacheKey = _cacheKey ?? "dropbox";
							try
							{
								await Fetching.GetFileAndCacheAsync(file.Url, _cacheKey);
							}
							catch (Exception e)
							{
								await Logger.SendToChannelAsync("Fetching.GetFileAndCacheAsync Error (AutoDownloader.CheckDropboxAsync)", e);
								break;
							}

							// Get file
							var cacheFile = await Caching.CFile.GetPathAndSaveAsync(_cacheKey);
							if (string.IsNullOrEmpty(cacheFile))
							{
								await Logger.SendToChannelAsync("Caching.CFile.GetPathAndSaveAsync Error (AutoDownloader.CheckDropboxAsync)", LogColor.Error);
								break;
							}

							// Every user has his own folder
							var path = $"{Configuration.Default.DropboxFolderName}/{args.User.Id}";

							// Check if folder is full
							var files = await DropboxCom.ListFilesAsync(path);
							if ((files != "No files found.")
							&& (files != "**Error.**")
							&& (files.Split('\n').Length > MaxFilesPerFolder))
							{
								await args.Channel.SendMessage($"Your folder is full. Try to list all files with `{Configuration.Default.PrefixCmd}dbfolder` and delete one with `{Configuration.Default.PrefixCmd}dbdelete <filename>`.");
								continue;
							}

							// Send file to Dropbox
							var msg = await args.Channel.SendMessage("Uploading...");
							if (await DropboxCom.UploadAsync(path, filename, cacheFile))
								await msg.Edit($"Uploaded {await Utils.AsRawText(filename)} to Dropbox.");
							else
								await msg.Edit("**Upload error.**");

							var link = await DropboxCom.CreateLinkAsync($"{path}/{filename}");
							if (!(string.IsNullOrEmpty(link)))
								await msg.Edit($"{msg.RawText}\nDownload: <{link}>");
						}
					}
				}
				else
					return false;
			}
			catch (Exception e)
			{
				await Logger.SendToChannelAsync("AutoDownloader.CheckDropboxAsync Error", e);
				await args.Channel.SendMessage("**Error.**");
			}
			return true;
		}
	}
}