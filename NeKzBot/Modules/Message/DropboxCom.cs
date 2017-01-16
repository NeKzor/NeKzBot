using System.IO;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;

namespace NeKzBot
{
	// This is a mess because I wrote this very quickly
	// Didn't test everything but the thing I wanted works
	public class DropboxCom
	{
		protected static DropboxClient dbClient;
		private static DropboxClientConfig dbClientConfig;

		public static void Init()
		{
			Logging.CON("Initialazing Dropbox client", System.ConsoleColor.DarkYellow);
			dbClientConfig = new DropboxClientConfig($"{Server.Settings.Default.AppName}/{Server.Settings.Default.AppVersion}", 3);
			dbClient = new DropboxClient(Server.Credentials.Default.DropboxToken, dbClientConfig);
		}

		public static async Task<string> Upload(string folderName, string fileName, string cacheFolder)
		{
			try
			{
				Logging.CON("Dropbox uploading file", System.ConsoleColor.Blue);
				using (var stream = new FileStream(cacheFolder, FileMode.Open, FileAccess.Read))
				{
					var response = await dbClient.Files.UploadAsync($"{folderName}/{fileName}", WriteMode.Overwrite.Instance, body: stream);
					Logging.CON($"Dropbox Id {response.Id} Rev {response.Rev}", System.ConsoleColor.Blue);
					return $"Uploaded {fileName} to Dropbox.";
				}
			}
			catch (System.Exception ex)
			{
				Logging.CHA($"Dropbox failed to upload.\n{ex.ToString()}", System.ConsoleColor.Blue);
				return "**Error**";
			}
		}

		public static async Task<string> ListFiles(string folderName)
		{
			try
			{
				Logging.CON("Dropbox reading folder", System.ConsoleColor.Blue);
				var files = await dbClient.Files.ListFolderAsync($"/{folderName}");
				var output = string.Empty;
				foreach (var item in files.Entries)
					if (item.IsFile)
						output += item.Name + "\n";
				return output != string.Empty ? output.Substring(0, output.Length - 1) : "No files found.";
			}
			catch
			{
				Logging.CON("Dropbox failed to list files", System.ConsoleColor.Blue);
				return "**Error**";
			}
		}

		public static async Task<string> ListFolders(string folderName)
		{
			try
			{
				Logging.CON("Dropbox reading folder", System.ConsoleColor.Blue);
				var files = await dbClient.Files.ListFolderAsync("/" + folderName);
				var output = string.Empty;
				foreach (var item in files.Entries)
					if (item.IsFolder)
						output += item.Name + "\n";
				return output != string.Empty ? output.Substring(0, output.Length - 1) : "No folders found.";
			}
			catch
			{
				Logging.CON("Dropbox failed to list folders", System.ConsoleColor.Blue);
				return "**Error**";
			}
		}

		public static async Task<bool> CreateFolder(string folderName)
		{
			try
			{
				Logging.CON("Dropbox creating folder", System.ConsoleColor.Blue);
				await dbClient.Files.CreateFolderAsync("/" + folderName);
				return true;
			}
			catch
			{
				Logging.CON("Dropbox failed to create folder", System.ConsoleColor.Blue);
				return false;
			}
		}

		public static async Task<string> CreateLink(string fileName)
		{
			try
			{
				Logging.CON("Dropbox creating link", System.ConsoleColor.Blue);
				var link = await dbClient.Sharing.CreateSharedLinkWithSettingsAsync("/" + fileName);
				return link.Url;
			}
			catch
			{
				Logging.CON("Dropbox failed to create link", System.ConsoleColor.Blue);
				return "**Error**";
			}
		}

		public static async Task<string> DeleteFile(string folderName, string fileName)
		{
			try
			{
				Logging.CON("Dropbox deleting file", System.ConsoleColor.Blue);

				// Check if there's a folder
				var folder = await dbClient.Files.GetMetadataAsync($"/{folderName}");
				if (folder.IsDeleted)
					return "Folder doesn't exist.";

				// Check if filename doesn't contain invalid characters
				if (Utils.ValidFileName(fileName))
				{
					var link = await dbClient.Files.DeleteAsync($"/{folderName}/{fileName}");
					if (link.IsDeleted)
						return "File deleted.";
				}
				return "Failed to delete file. Are you sure you spelled it right?";
			}
			catch
			{
				Logging.CON("Dropbox failed to delete file", System.ConsoleColor.Blue);
				return "**Error**";
			}
		}
	}
}