using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net.Http;
using System.Collections.Generic;
using NeKzBot.Classes;

// TODO: replace webclient with httpclient
namespace NeKzBot.Server
{
	/// <summary>Used to download webpages.</summary>
	public static class Fetching
	{
		/// <summary>Downloads the webpage as file to the give path.</summary>
		/// <param name="uri">Web address to download from.</param>
		/// <param name="path">Folder path to store the file.</param>
		public static async Task GetFileAsync(string uri, string path)
			=> await (await CreateWebClient()).DownloadFileTaskAsync(new Uri(uri), path);

		/// <summary>Downloads the wepbage as file and caches it as file.</summary>
		/// <param name="uri">Web address to download from.</param>
		/// <param name="key">Name of requester..</param>
		public static async Task GetFileAndCacheAsync(string uri, string key)
			=> await (await CreateWebClient()).DownloadFileTaskAsync(new Uri(uri), await Caching.CFile.GetPathAndSaveAsync(key));

		/// <summary>Downloads the wepbage as string.</summary>
		/// <param name="uri">Web address to download from.</param>
		public static async Task<string> GetStringAsync(string uri)
			=> await (await CreateWebClient()).DownloadStringTaskAsync(new Uri(uri));

		/// <summary>Downloads the wepbage as string.</summary>
		/// <param name="uri">Web address to download from.</param>
		/// /// <param name="wc">Http header for the web client.</param>
		public static async Task<string> GetStringAsync(string uri, WebHeaderCollection wc)
			=> await (await CreateWebClient(wc)).DownloadStringTaskAsync(new Uri(uri));

		/// <summary>Downloads the wepbage as HtmlDocument (HtmlAgilityPack).</summary>
		/// <param name="uri">Web address to download from.</param>
		public static async Task<HtmlDocument> GetDocumentAsync(string uri)
		{
			var doc = new HtmlDocument();
			using (var ms = new MemoryStream(Encoding.Default.GetBytes(await GetStringAsync(uri))))
				doc.Load(ms);
			return doc;
		}

		/// <summary>Returns data cache.</summary>
		/// <param name="key">Name of requester.</param>
		/// <param name="cachingtype">Caching system to choose.</param>
		public static async Task<object> GetStringAsync(string key, Type cachingtype)
			=> (cachingtype == typeof(Caching.CFile))
							? await Caching.CFile.GetCacheAsync(key)
							: (cachingtype == typeof(Caching.CApplication))
										   ? await Caching.CApplication.GetCacheAsync(key)
										   : null as object;

		/// <summary>Creates a new client for downloading multiple things at the same time.</summary>
		public static Task<WebClient> CreateWebClient()
		{
			var client = new WebClient() { Encoding = Encoding.UTF8 };
			client.Headers["User-Agent"] = $"{Configuration.Default.AppName}/{Configuration.Default.AppVersion}";
			return Task.FromResult(client);
		}

		/// <summary>Creates a new client for downloading multiple things at the same time.</summary>
		/// /// /// <param name="wc">Http header for the web client.</param>
		public static Task<WebClient> CreateWebClient(WebHeaderCollection wc)
		{
			return Task.FromResult(new WebClient
			{
				Encoding = Encoding.UTF8,
				Headers = wc
			});
		}

		/// <summary>Creates a new client for downloading multiple things at the same time.</summary>
		public static Task<HttpClient> CreateHttpClient()
		{
			var client = new HttpClient();
			client.DefaultRequestHeaders.UserAgent.ParseAdd($"{Configuration.Default.AppName}/{Configuration.Default.AppVersion}");
			return Task.FromResult(client);
		}

		/// <summary>Creates a new client for downloading multiple things at the same time.</summary>
		/// /// /// <param name="headers">Http header for the http client.</param>
		public static Task<HttpClient> CreateHttpClient(List<WebHeader> headers)
		{
			var client = new HttpClient();
			foreach (var item in headers)
				client.DefaultRequestHeaders.Add(item.GetHeader().Item1, item.GetHeader().Item2);
			return Task.FromResult(client);
		}
	}
}