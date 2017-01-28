using System.Text;
using System.Net;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace NeKzBot.Server
{
	/// <summary>Class used to download webpages</summary>
	public class Fetching
	{
		/// <summary>Downloads the webpage as file to the give path</summary>
		/// <param name="uri">Web address to download from</param>
		/// <param name="path">Folder path to store the file</param>
		public static async Task GetFile(string uri, string path) =>
			await (await CreateClient()).DownloadFileTaskAsync(new System.Uri(uri), path);

		/// <summary>Downloads the wepbage as file and caches it as file</summary>
		/// <param name="uri">Web address to download from</param>
		public static async Task GetFileAndCache(string uri, string key) =>
			await (await CreateClient()).DownloadFileTaskAsync(new System.Uri(uri), await Caching.CFile.GetPathAndSave(key));

		/// <summary>Downloads the wepbage as string</summary>
		/// <param name="uri">Web address to download from</param>
		public static async Task<string> GetString(string uri) =>
			Encoding.UTF8.GetString(Encoding.Default.GetBytes(await (await CreateClient()).DownloadStringTaskAsync(new System.Uri(uri))));

		/// <summary>Downloads the wepbage as string</summary>
		/// <param name="uri">Web address to download from</param>
		/// /// <param name="wc">Http header for the web client</param>
		public static async Task<string> GetString(string uri, WebHeaderCollection wc) =>
			Encoding.UTF8.GetString(Encoding.Default.GetBytes(await (await CreateClient(wc)).DownloadStringTaskAsync(new System.Uri(uri))));

		/// <summary>Downloads the wepbage as HtmlDocument (HtmlAgilityPack)</summary>
		/// <param name="uri">Web address to download from</param>
		public static async Task<HtmlDocument> GetDocument(string uri)
		{
			var doc = new HtmlDocument();
			using (var ms = new System.IO.MemoryStream(Encoding.Default.GetBytes(await GetString(uri))))
				doc.Load(ms);
			return doc;
		}

		/// <summary>Returns data cache</summary>
		/// <param name="key">Name of requester</param>
		/// <param name="cachingtype">Caching system to choose</param>
		public static async Task<object> GetString(string key, System.Type cachingtype) =>
			cachingtype == typeof(Caching.CFile) ?
			await Caching.CFile.Get(key) : cachingtype == typeof(Caching.CApplication) ?
			await Caching.CApplication.Get(key) : (object)null;

		/// <summary>Creates a new client for downloading multiple things at the same time</summary>
		private static Task<WebClient> CreateClient()
		{
			var client = new WebClient();
			client.Encoding = Encoding.UTF8;
			client.Headers["User-Agent"] = $"{Settings.Default.AppName}/{Settings.Default.AppVersion}";
			return Task.FromResult(client);
		}

		/// <summary>Creates a new client for downloading multiple things at the same time</summary>
		/// /// /// <param name="wc">Http header for the web client</param>
		private static Task<WebClient> CreateClient(WebHeaderCollection wc)
		{
			var client = new WebClient();
			client.Encoding = Encoding.UTF8;
			client.Headers = wc;
			return Task.FromResult(client);
		}
	}
}