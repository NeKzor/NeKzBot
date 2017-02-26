using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using NeKzBot.Resources;

namespace NeKzBot.Server
{
	/// <summary>Used to cache data.</summary>
	public static class Caching
	{
		/// <summary>Used to initialize all caching methods.</summary>
		public static async Task InitAsync()
		{
			await Logger.SendAsync("Initializing Caching Systems", LogColor.Init);
			await CFile.InitAsync();
			await CApplication.Init();
		}

		/// <summary>Caching system with files.</summary>
		internal static class CFile
		{
			/// <summary>Contains the names of the requesters (methods).</summary>
			private static List<string> _fileCache;
			private static string _fileName;
			private static string _cachePath;
			private static string _fileExtension;

			/// <summary>Used for class initialization.</summary>
			public static async Task InitAsync()
			{
				_fileCache = _fileCache ?? new List<string>();
				_fileName = "c4ch3";
				_cachePath = await Utils.GetPath() + "/Data/Cache/";
				_fileExtension = ".tmp";
			}

			/// <summary>Saves data to a file with the key as a name.</summary>
			/// <param name="key">Name of requester.</param>
			/// <param name="data">Data to store.</param>
			public static async Task SaveCacheAsync(string key, string data)
			{
				await AddKeyAsync(key);
				File.WriteAllText(await GetPath(key), data);
			}

			/// <summary>Adds the key to the cache list.</summary>
			/// <param name="key">Name of requester.</param>
			public static async Task AddKeyAsync(string key)
			{
				await ClearKeyAsync(key);
				_fileCache.Add(key);
			}

			/// <summary>Clears the cache before adding a new key to the cache list.</summary>
			/// <param name="key">Name of requester.</param>
			public static async Task ClearAndAddKeyAsync(string key)
			{
				await ClearCacheAsync(key);
				_fileCache.Add(key);
			}

			/// <summary>Returns the path of the key.</summary>
			/// <param name="key">Name of requester.</param>
			public static async Task<string> GetPathAndSaveAsync(string key)
			{
				await AddKeyAsync(key);
				return await GetPath(key);
			}

			/// <summary>Gets the data of a key.</summary>
			/// <param name="key">Name of requester.</param>
			public static async Task<string> GetCacheAsync(string key)
				=> ((bool)(await KeyExists(key)))
									? await GetFileAsync(key)
									: null;

			/// <summary>Gets the data of a file.</summary>
			/// <param name="key">Name of requester.</param>
			public static async Task<string> GetFileAsync(string key)
				=> (await FileExistsAsync(key))
								? File.ReadAllText(await GetPath(key), Encoding.UTF8)
								: null;

			/// <summary>Deletes the given key from the cache list.</summary>
			/// <param name="key">Name of requester.</param>
			public static async Task ClearKeyAsync(string key)
			{
				if ((bool)await KeyExists(key))
					_fileCache.Remove(key);
			}

			/// <summary>Deletes the file of the give key.</summary>
			/// <param name="key">Name of requester.</param>
			public static async Task ClearFileAsync(string key)
			{
				if (await FileExistsAsync(key))
					File.Delete(await GetPath(key));
			}

			/// <summary>Deletes the key and the file.</summary>
			/// <param name="key">Name of requester.</param>
			public static async Task ClearCacheAsync(string key)
			{
				await ClearKeyAsync(key);
				await ClearFileAsync(key);
			}

			/// <summary>Deletes current files of each key in the cache list.</summary>
			public static async Task ClearFilesAsync()
			{
				foreach (var item in _fileCache)
					if (await FileExistsAsync(item))
						File.Delete(await GetPath(item));
			}

			/// <summary>Deletes all files in the cache folder.</summary>
			public static Task ClearAllFiles()
			{
				foreach (var item in new DirectoryInfo(_cachePath).GetFiles())
					if (item.Extension == _fileExtension)
						item.Delete();
				return Task.FromResult(0);
			}

			/// <summary>Clears the cache list.</summary>
			public static Task ClearKeys()
			{
				_fileCache = new List<string>();
				return Task.FromResult(0);
			}

			/// <summary>Checks if the key exists in the cache list.</summary>
			/// <param name="key">Name of requester.</param>
			internal static Task<bool?> KeyExists(string key)
				=> Task.FromResult(_fileCache?.Contains(key));

			/// <summary>Checks if the file exists in the cache list.</summary>
			/// <param name="key">Name of requester.</param>
			internal static async Task<bool> FileExistsAsync(string key)
				=> File.Exists(await GetPath(key));

			/// <summary>Builds the file path for the cache file.</summary>
			/// <param name="key">Name of requester.</param>
			internal static Task<string> GetPath(string key)
				=> Task.FromResult(Path.Combine(_cachePath, _fileName + key + _fileExtension));
		}

		/// <summary>Caching system of internal application.</summary>
		internal static class CApplication
		{
			/// <summary>Contains the names of the requesters (methods) with the data to store.</summary>
			private static Dictionary<string, List<object>> _appCache;

			/// <summary>Used for class initialization.</summary>
			public static Task Init()
			{
				_appCache = _appCache ?? new Dictionary<string, List<object>>();
				return Task.FromResult(0);
			}

			/// <summary>Used to reset cache.</summary>
			public static Task ResetCache()
			{
				_appCache = new Dictionary<string, List<object>>();
				return Task.FromResult(0);
			}

			/// <summary>Save new data to cache.</summary>
			/// <param name="key">Name of requester.</param>
			/// <param name="data">Data to store.</param>
			public static async Task SaveCacheAsync(string key, object data)
			{
				await ClearKeyAsync(key);
				_appCache.Add(key, await ToList(data));
			}

			/// <summary>Add data to existing cache.</summary>
			/// <param name="key">Name of requester.</param>
			/// <param name="data">Data to store.</param>
			public static Task AddCache(string key, object data)
			{
				_appCache[key]?.Add(data);
				return Task.FromResult(0);
			}

			/// <summary>Returns data if the key exists in the cache list.</summary>
			/// <param name="key">Name of requester.</param>
			public static async Task<List<object>> GetCacheAsync(string key)
				=> ((bool)(await KeyExists(key)))
									? _appCache[key]
									: null;

			/// <summary>Removes key from the cache list.</summary>
			/// <param name="key">Name of requester.</param>
			public static async Task ClearKeyAsync(string key)
			{
				if ((bool)(await KeyExists(key)))
					_appCache.Remove(key);
			}

			/// <summary>Clears data cache of specific requester.</summary>
			/// <param name="key">Name of requester.</param>
			public static async Task ClearDataAsync(string key)
			{
				if ((bool)(await KeyExists(key)))
					_appCache[key] = new List<object>();
			}

			/// <summary>Clears specific data cache of specific requester.</summary>
			/// <param name="key">Name of requester.</param>
			/// <param name="data">Data to delete.</param>
			public static async Task ClearValueAsync(string key, string data)
			{
				if ((bool)(await KeyExists(key)))
					_appCache[key].Remove(data);
			}

			/// <summary>Checks if cache contains the given key already.</summary>
			/// <param name="key">Name of requester in the cache list.</param>
			public static Task<bool?> KeyExists(string key)
				=> Task.FromResult(_appCache?.ContainsKey(key));

			private static Task<List<object>> ToList(object data)
				=> Task.FromResult(new List<object> { data });
		}
	}
}