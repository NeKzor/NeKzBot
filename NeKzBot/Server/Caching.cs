using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NeKzBot
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
		public static void Init()
		{
			Logging.CON("Initializing caching systems", System.ConsoleColor.DarkYellow);
			CFile.Init();
			CApplication.Init();
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
			public static void Init()
			{
				fileCache = fileCache ?? new List<string>();
				fileName = "c4ch3";
				tempPath = Path.Combine(Utils.GetPath(), "Resources/cache/");
				fileExtension = ".tmp";
			}

			/// <summary>Saves data to a file with the key as a name</summary>
			/// <param name="key">Name of requester</param>
			/// <param name="data">Data to store</param>
			public static void Save(string key, string data)
			{
				AddKey(key);
				File.WriteAllText(GetPath(key), data);
			}

			/// <summary>Adds the key to the cache list</summary>
			/// <param name="key">Name of requester</param>
			public static void AddKey(string key)
			{
				ClearKey(key);
				fileCache.Add(key);
			}

			/// <summary>Clears the cache before adding a new key to the cache list</summary>
			/// <param name="key">Name of requester</param>
			public static void ClearAndAddKey(string key)
			{
				ClearKeyAndFile(key);
				fileCache.Add(key);
			}

			/// <summary>Returns the path of the key</summary>
			/// <param name="key">Name of requester</param>
			public static string GetPathAndSave(string key)
			{
				AddKey(key);
				return GetPath(key);
			}

			/// <summary>Gets the data of a key</summary>
			/// <param name="key">Name of requester</param>
			public static string Get(string key) =>
				(bool)Exists(key) ?
				GetFile(key) : null;

			/// <summary>Gets the data of a file</summary>
			/// <param name="key">Name of requester</param>
			public static string GetFile(string key) =>
				FileExists(key) ?
				File.ReadAllText(GetPath(key), System.Text.Encoding.UTF8) : null;

			/// <summary>Deletes the given key from the cache list</summary>
			/// <param name="key">Name of requester</param>
			public static void ClearKey(string key)
			{
				if ((bool)Exists(key))
					fileCache.Remove(key);
			}

			/// <summary>Deletes the file of the give key</summary>
			/// <param name="key">Name of requester</param>
			public static void ClearFile(string key)
			{
				if (FileExists(key))
					File.Delete(GetPath(key));
			}

			/// <summary>Deletes the key and the file</summary>
			/// <param name="key">Name of requester</param>
			public static void ClearKeyAndFile(string key)
			{
				ClearKey(key);
				ClearFile(key);
			}

			/// <summary>Deletes current files of each key in the cache list</summary>
			public static void ClearFiles()
			{
				foreach (var item in fileCache)
					if (FileExists(item))
						File.Delete(GetPath(item));
			}

			/// <summary>Deletes all files in the cache folder</summary>
			public static void ClearAllFiles()
			{
				foreach (var item in new DirectoryInfo(tempPath).GetFiles())
					if (item.Extension == fileExtension)
						item.Delete();
			}

			/// <summary>Clears the cache list</summary>
			public static void ClearKeys() =>
				fileCache = new List<string>();

			/// <summary>Global reset of cache after a certain time</summary>
			/// <param name="delay">Time (in minutes) to wait until reset</param>
			public static async Task ResetTimer(int delay)
			{
				for (;;)
				{
					await Task.Delay(delay * 60000);
					ClearKeys();
					Logging.CON("File cache reset");
				}
			}

			/// <summary>Checks if the key exists in the cache list</summary>
			/// <param name="key">Name of requester</param>
			private static bool? Exists(string key) =>
				fileCache?.Contains(key);

			/// <summary>Checks if the file exists in the cache list</summary>
			/// <param name="key">Name of requester</param>
			private static bool FileExists(string key) =>
				File.Exists(GetPath(key));

			/// <summary>Builds the file path for the cache file</summary>
			/// <param name="key">Name of requester</param>
			private static string GetPath(string key) =>
				Path.Combine(tempPath, fileName + key + fileExtension);
		}

		/// <summary>Caching system of internal application</summary>
		internal static class CApplication
		{
			/// <summary>Contains the names of the requesters (methods) with the data to store</summary>
			private static Dictionary<string, List<object>> appCache;

			/// <summary>Used for class initialization</summary>
			public static void Init() =>
				appCache = appCache ?? new Dictionary<string, List<object>>();

			/// <summary>Used to reset cache</summary>
			public static void Reset() =>
				appCache = new Dictionary<string, List<object>>();

			/// <summary>Save new data to cache</summary>
			/// <param name="key">Name of requester</param>
			/// <param name="data">Data to store</param>
			public static void Save(string key, object data)
			{
				ClearKey(key);
				appCache.Add(key, ToList(data));
			}

			/// <summary>Add data to existing cache</summary>
			/// <param name="key">Name of requester</param>
			/// <param name="data">Data to store</param>
			public static void Add(string key, object data) =>
				appCache[key]?.Add(data);

			/// <summary>Returns data if the key exists in the cache list</summary>
			/// <param name="key">Name of requester</param>
			public static List<object> Get(string key) =>
				(bool)Exists(key) ?
				appCache[key] : null;

			/// <summary>Removes key from the cache list</summary>
			/// <param name="key">Name of requester</param>
			public static void ClearKey(string key)
			{
				if ((bool)Exists(key))
					appCache.Remove(key);
			}

			/// <summary>Clears data cache of specific requester</summary>
			/// <param name="key">Name of requester</param>
			public static void ClearData(string key)
			{
				if ((bool)Exists(key))
					appCache[key] = new List<object>();
			}

			/// <summary>Clears specific data cache of specific requester</summary>
			/// <param name="key">Name of requester</param>
			/// <param name="data">Data to delete</param>
			public static void ClearValue(string key, string data)
			{
				if ((bool)Exists(key))
					appCache[key].Remove(data);
			}

			/// <summary>Global reset of cache after a certain time</summary>
			/// <param name="delay">Time (in minutes) to wait until reset</param>
			public static async Task ResetTimer(int delay)
			{
				for (;;)
				{
					await Task.Delay(delay * 60000);
					Reset();
					Logging.CON("Application cache reset");
				}
			}

			/// <summary>Checks if cache contains the given key already</summary>
			/// <param name="key">Name of requester in the cache list</param>
			public static bool? Exists(string key) =>
				appCache?.ContainsKey(key);

			private static List<object> ToList(object data)
			{
				var temp = new List<object>();
				temp.Add(data);
				return temp;
			}
		}
	}
}