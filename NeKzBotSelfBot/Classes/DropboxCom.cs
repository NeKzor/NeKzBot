using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;
using Dropbox.Api.Sharing;
using NeKzBot.Server;

namespace NeKzBot.Classes
{
	public static class DropboxCom
	{
		private static DropboxClient _client;
		private static DropboxClientConfig _clientConfig;

		public static async Task InitAsync()
		{
			await Logger.SendAsync("Initializing Dropbox Client", LogColor.Init);
			_clientConfig = new DropboxClientConfig($"{Configuration.Default.AppName}/{Configuration.Default.AppVersion}", 3);
			_client = new DropboxClient(Credentials.Default.DropboxToken, _clientConfig);
		}

		public static async Task<bool> UploadAsync(string folderpath, string file, string cachepath)
		{
			try
			{
				await Logger.SendAsync("Dropbox Uploading File", LogColor.Dropbox);
				using (var stream = new FileStream(cachepath, FileMode.Open, FileAccess.Read))
					await _client.Files.UploadAsync($"/{folderpath}/{file}", WriteMode.Overwrite.Instance, body: stream);
				return true;
			}
			catch (Exception e)
			{
				await Logger.SendAsync("Dropbox.UploadAsync Error", e);
			}
			return false;
		}

		public static async Task<string> ListFilesAsync(string fullpath)
		{
			await Logger.SendAsync("Dropbox Reading Folder", LogColor.Dropbox);
			try
			{
				var output = string.Empty;
				foreach (var item in (await _client.Files.ListFolderAsync($"/{fullpath}")).Entries)
					if (item.IsFile)
						output += $"{item.Name}\n";
				return (output != string.Empty)
							   ? output.Substring(0, output.Length - 1)
							   : "No files found.";
			}
			catch (Exception e)
			{
				await Logger.SendAsync("Dropbox.ListFilesAsync Error", e);
			}
			return "**Error**";
		}

		public static async Task<bool> CreateFolderAsync(string name)
		{
			await Logger.SendAsync("Dropbox Creating Folder", LogColor.Dropbox);
			try
			{
				await _client.Files.CreateFolderAsync($"/{name}");
				return true;
			}
			catch (Exception e)
			{
				await Logger.SendAsync("Dropbox.CreateFolderAsync Error", e);
			}
			return false;
		}

		public static async Task<string> CreateLinkAsync(string fullpath)
		{
			await Logger.SendAsync("Dropbox Creating Link", LogColor.Dropbox);
			try
			{
				return (await _client.Sharing.CreateSharedLinkWithSettingsAsync($"/{fullpath}")).Url;
			}
			catch (ApiException<CreateSharedLinkWithSettingsError> e)
			{
				if (e.ErrorResponse.IsSharedLinkAlreadyExists)
				{
					try
					{
						return (await _client.Sharing.ListSharedLinksAsync($"/{fullpath}", directOnly: true)).Links.FirstOrDefault().Url;
					}
					catch (Exception ex)
					{
						await Logger.SendAsync("Dropbox.CreateLinkAsync List Links Error", ex);
					}
				}
				else
					await Logger.SendAsync("Dropbox.CreateLinkAsync Error", e);
			}
			return null;
		}
	}
}