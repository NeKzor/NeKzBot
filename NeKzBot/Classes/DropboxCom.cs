using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;
using Dropbox.Api.Sharing;
using NeKzBot.Server;
using NeKzBot.Utilities;

namespace NeKzBot.Classes
{
	public sealed class DropboxComFile
	{
		public string Name;
		public DateTime ModifiedDate;
	}

	public static class DropboxCom
	{
		private static DropboxClient _client;

		public static async Task InitAsync()
		{
			await Logger.SendAsync("Initializing Dropbox Client", LogColor.Init);
			_client = new DropboxClient(Credentials.Default.DropboxToken, new DropboxClientConfig
			{
				UserAgent = $"{Configuration.Default.AppName}/{Configuration.Default.AppVersion}",
				MaxRetriesOnError = 3
			});
		}

		public static async Task<bool> UploadAsync(string folder, string file, string localpath)
		{
			try
			{
				await Logger.SendAsync("Dropbox Uploading File", LogColor.Dropbox);
				using (var stream = new FileStream(localpath, FileMode.Open, FileAccess.Read))
					await _client.Files.UploadAsync($"/{folder}/{file}", WriteMode.Overwrite.Instance, body: stream);
				return true;
			}
			catch (ApiException<UploadError> api)
			{
				await Logger.SendAsync("DropboxCom.UploadAsync API Error", api);
			}
			catch (Exception e)
			{
				await Logger.SendAsync("DropboxCom.UploadAsync Error", e);
			}
			return false;
		}

		public static async Task<List<DropboxComFile>> GetFilesAsync(string folder)
		{
			await Logger.SendAsync("Dropbox Reading Folder", LogColor.Dropbox);
			if (await DataExists(folder))
			{
				try
				{
					var response = await _client.Files.ListFolderAsync($"/{folder}");
					var list = new List<DropboxComFile>();
					foreach (var item in response?.Entries.Where(entry => entry.IsFile))
					{
						list.Add(new DropboxComFile
						{
							Name = item.Name,
							ModifiedDate = item.AsFile.ServerModified
						});
					}
					return list;
				}
				catch (ApiException<ListFolderError> api)
				{
					await Logger.SendAsync("DropboxCom.ListFilesAsync API Error", api);
				}
				catch (Exception e)
				{
					await Logger.SendAsync("DropboxCom.ListFilesAsync Error", e);
				}
			}
			return default(List<DropboxComFile>);
		}

		public static async Task<string> DeleteFileAsync(string folder, string file, bool asadmin = false)
		{
			await Logger.SendAsync("Dropbox Deleting File", LogColor.Dropbox);
			// Fail safe
			if (!(await Utils.ValidPathName(folder)))
				return "File name has invalid characters.";
			if (!(await DataExists(folder)))
				return $"{((asadmin) ? "This" : "Your")} folder does not exist.";
			if (!(await Utils.ValidFileName(file)))
				return "File name has invalid characters.";
			if (!(await DataExists($"{folder}/{file}")))
				return $"{((asadmin) ? "This" : "Your")} file does not exist.";

			try
			{
				await _client.Files.DeleteAsync($"/{folder}/{file}");
				return $"File {await Utils.AsRawText(file)} has been deleted.";
			}
			catch (ApiException<DeleteError> api)
			{
				await Logger.SendAsync("DropboxCom.DeleteFileAsync API Error", api);
			}
			catch (Exception e)
			{
				await Logger.SendAsync("DropboxCom.DeleteFileAsync Error", e);
			}
			return "**Error.**";
		}

		public static async Task<string> CreateLinkAsync(string fullpath)
		{
			await Logger.SendAsync("Dropbox Creating Link", LogColor.Dropbox);
			try
			{
				var data = await _client.Sharing.CreateSharedLinkWithSettingsAsync($"/{fullpath}");
				return data.Url;
			}
			catch (ApiException<CreateSharedLinkWithSettingsError> api)	// I don't like this library... actually, it's decent I guess
			{
				await Logger.SendAsync("DropboxCom.CreateLinkAsync API Error", api);
				if (api.ErrorResponse.IsSharedLinkAlreadyExists)
				{
					try
					{
						var data = await _client.Sharing.ListSharedLinksAsync($"/{fullpath}", directOnly: true);
						return data.Links.FirstOrDefault().Url;
					}
					catch (Exception e)
					{
						await Logger.SendAsync("DropboxCom.CreateLinkAsync Error 0", e);
					}
				}
			}
			catch (Exception e)
			{
				await Logger.SendAsync("DropboxCom.CreateLinkAsync Error 1", e);
			}
			return null;
		}

		public static async Task<bool> DataExists(string path)
		{
			try
			{
				await _client.Files.GetMetadataAsync($"/{path}");
				return true;
			}
			catch
			{
				// Doesn't count as a real error
			}
			return false;
		}
	}
}