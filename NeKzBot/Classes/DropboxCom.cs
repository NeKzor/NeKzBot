using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;
using NeKzBot.Server;
using NeKzBot.Resources;
using Dropbox.Api.Sharing;

namespace NeKzBot.Classes
{
	public static class DropboxCom
	{
		private static DropboxClient _client;
		private static DropboxClientConfig _clientConfig;

		public static async Task InitAsync()
		{
			await Logger.SendAsync("Initialazing Dropbox Client", LogColor.Init);
			_clientConfig = new DropboxClientConfig($"{Configuration.Default.AppName}/{Configuration.Default.AppVersion}", 3);
			_client = new DropboxClient(Credentials.Default.DropboxToken, _clientConfig);
		}

		public static async Task<bool> UploadAsync(string folder, string file, string filepath)
		{
			try
			{
				await Logger.SendAsync("Dropbox Uploading File", LogColor.Dropbox);
				using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
					await _client.Files.UploadAsync($"/{folder}/{file}", WriteMode.Overwrite.Instance, body: stream);
				return true;
			}
			catch (Exception e)
			{
				await Logger.SendToChannelAsync("Dropbox.UploadAsync Error", e);
			}
			return false;
		}

		public static async Task<string> ListFilesAsync(string folderName)
		{
			await Logger.SendAsync("Dropbox Reading Folder", LogColor.Dropbox);
			try
			{
				var output = string.Empty;
				foreach (var item in (await _client.Files.ListFolderAsync($"/{folderName}")).Entries)
					if (item.IsFile)
						output += $"{item.Name}\n";
				return (output != string.Empty)
							   ? output.Substring(0, output.Length - 1)
							   : "No files found.";
			}
			catch
			{
				await Logger.SendAsync("Dropbox.ListFilesAsync Error", LogColor.Dropbox);
			}
			return "**Error**";
		}

		public static async Task<string> DeleteFileAsync(string folderName, string fileName)
		{
			await Logger.SendAsync("Dropbox Deleting File", LogColor.Dropbox);
			try
			{
				// Check if there's a folder
				if ((await _client.Files.GetMetadataAsync($"/{folderName}")).IsDeleted)
					return "Folder doesn't exist.";

				// Check if filename doesn't contain invalid characters
				if (await Utils.ValidFileName(fileName))
					if ((await _client.Files.DeleteAsync($"/{folderName}/{fileName}")).IsDeleted)
						return "File deleted.";

				return "Failed to delete file. Are you sure you spelled it right?";
			}
			catch
			{
				await Logger.SendAsync("Dropbox.DeleteFileAsync Error", LogColor.Dropbox);
			}
			return "**Error**";
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