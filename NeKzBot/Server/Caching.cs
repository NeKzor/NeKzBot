using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using NeKzBot.Resources;

namespace NeKzBot.Server
{
	/*	Advanced caching system variants
	 *	 - As unique files
	 *	 - As static variables
	 *	 - As properties user scope (old method, shouldn't be used for caching)
	 *	
	 *	Module				System
	 *	-------------------------------------
	 *	Leaderboard			Files/Static ✔
	 *	SpeedrunCom			Files ✔
	 *	DropboxCom			Files ✔
	 *	Giveaway Game		Files/Static ✔
	 *	TwitchTv			Static ✔
	 *	Steam				Static ✔		*/

	/// <summary>Class used to cache data</summary>
	public class Caching
	{
		/// <summary>Used to initialize all caching methods<summary>
		public static async Task Init()
		{
			await Logging.CON("Initializing caching systems", System.ConsoleColor.DarkYellow);
			await CFile.Init();
			await CApplication.Init();
		}

		/// <summary>Caching system with files</summary>
		internal static class CFile
		{
			/// <summary>Contains the names of the requesters (methods)</summary>
			private static List<string> fileCache;
			private static string fileName;
			private static string tempPath;
			private static string fileExtension;

			/// <summary>Used for class initialization</summary>
			public static Task Init()
			{
				fileCache = fileCache ?? new List<string>();
				fileName = "c4ch3";
				tempPath = Path.Combine(Utils.GetPath(), "Resources/cache/");
				fileExtension = ".tmp";
				return Task.FromResult(0);
			}

			/// <summary>Saves data to a file with the key as a name</summary>
			/// <param name="key">Name of requester</param>
			/// <param name="data">Data to store</param>
			public static async Task Save(string key, string data)
			{
				await AddKey(key);
				File.WriteAllText(await GetPath(key), data);
			}

			/// <summary>Adds the key to the cache list</summary>
			/// <param name="key">Name of requester</param>
			public static async Task AddKey(string key)
			{
				await ClearKey(key);
				fileCache.Add(key);
			}

			/// <summary>Clears the cache before adding a new key to the cache list</summary>
			/// <param name="key">Name of requester</param>
			public static async Task ClearAndAddKey(string key)
			{
				await ClearKeyAndFile(key);
				fileCache.Add(key);
			}

			/// <summary>Returns the path of the key</summary>
			/// <param name="key">Name of requester</param>
			public static async Task<string> GetPathAndSave(string key)
			{
				await AddKey(key);
				return await GetPath(key);
			}

			/// <summary>Gets the data of a key</summary>
			/// <param name="key">Name of requester</param>
			public static async Task<string> Get(string key) =>
				(bool)(await Exists(key)) ?
				await GetFile(key) : null;

			/// <summary>Gets the data of a file</summary>
			/// <param name="key">Name of requester</param>
			public static async Task<string> GetFile(string key) =>
				await FileExists(key) ?
				File.ReadAllText(await GetPath(key), System.Text.Encoding.UTF8) : null;

			/// <summary>Deletes the given key from the cache list</summary>
			/// <param name="key">Name of requester</param>
			public static async Task ClearKey(string key)
			{
				if ((bool) await Exists(key))
					fileCache.Remove(key);
			}

			/// <summary>Deletes the file of the give key</summary>
			/// <param name="key">Name of requester</param>
			public static async Task ClearFile(string key)
			{
				if (await FileExists(key))
					File.Delete(await GetPath(key));
			}

			/// <summary>Deletes the key and the file</summary>
			/// <param name="key">Name of requester</param>
			public static async Task ClearKeyAndFile(string key)
			{
				await ClearKey(key);
				await ClearFile(key);
			}

			/// <summary>Deletes current files of each key in the cache list</summary>
			public static async Task ClearFiles()
			{
				foreach (var item in fileCache)
					if (await FileExists(item))
						File.Delete(await GetPath(item));
			}

			/// <summary>Deletes all files in the cache folder</summary>
			public static Task ClearAllFiles()
			{
				foreach (var item in new DirectoryInfo(tempPath).GetFiles())
					if (item.Extension == fileExtension)
						item.Delete();
				return Task.FromResult(0);
			}

			/// <summary>Clears the cache list</summary>
			public static Task ClearKeys()
			{
				fileCache = new List<string>();
				return Task.FromResult(0);
			}

			/// <summary>Checks if the key exists in the cache list</summary>
			/// <param name="key">Name of requester</param>
			private static Task<bool?> Exists(string key) =>
				Task.FromResult(fileCache?.Contains(key));

			/// <summary>Checks if the file exists in the cache list</summary>
			/// <param name="key">Name of requester</param>
			private static async Task<bool> FileExists(string key) =>
				File.Exists(await GetPath(key));

			/// <summary>Builds the file path for the cache file</summary>
			/// <param name="key">Name of requester</param>
			private static Task<string> GetPath(string key) =>
				Task.FromResult(Path.Combine(tempPath, fileName + key + fileExtension));
		}

		/// <summary>Caching system of internal application</summary>
		internal static class CApplication
		{
			/// <summary>Contains the names of the requesters (methods) with the data to store</summary>
			private static Dictionary<string, List<object>> appCache;

			/// <summary>Used for class initialization</summary>
			public static Task Init()
			{
				appCache = appCache ?? new Dictionary<string, List<object>>();
				return Task.FromResult(0);
			}

			/// <summary>Used to reset cache</summary>
			public static Task Reset()
			{
				appCache = new Dictionary<string, List<object>>();
				return Task.FromResult(0);
			}

			/// <summary>Save new data to cache</summary>
			/// <param name="key">Name of requester</param>
			/// <param name="data">Data to store</param>
			public static async Task Save(string key, object data)
			{
				await ClearKey(key);
				appCache.Add(key, await ToList(data));
			}

			/// <summary>Add data to existing cache</summary>
			/// <param name="key">Name of requester</param>
			/// <param name="data">Data to store</param>
			public static Task Add(string key, object data)
			{
				appCache[key]?.Add(data);
				return Task.FromResult(0);
			}

			/// <summary>Returns data if the key exists in the cache list</summary>
			/// <param name="key">Name of requester</param>
			public static async Task<List<object>> Get(string key) =>
				(bool)(await Exists(key)) ?
				appCache[key] : null;

			/// <summary>Removes key from the cache list</summary>
			/// <param name="key">Name of requester</param>
			public static async Task ClearKey(string key)
			{
				if ((bool)(await Exists(key)))
					appCache.Remove(key);
			}

			/// <summary>Clears data cache of specific requester</summary>
			/// <param name="key">Name of requester</param>
			public static async Task ClearData(string key)
			{
				if ((bool)(await Exists(key)))
					appCache[key] = new List<object>();
			}

			/// <summary>Clears specific data cache of specific requester</summary>
			/// <param name="key">Name of requester</param>
			/// <param name="data">Data to delete</param>
			public static async Task ClearValue(string key, string data)
			{
				if ((bool)(await Exists(key)))
					appCache[key].Remove(data);
			}

			/// <summary>Checks if cache contains the given key already</summary>
			/// <param name="key">Name of requester in the cache list</param>
			public static Task<bool?> Exists(string key) =>
				Task.FromResult(appCache?.ContainsKey(key));

			private static Task<List<object>> ToList(object data)
			{
				var temp = new List<object>();
				temp.Add(data);
				return Task.FromResult(temp);
			}
		}
	}
}