using System;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using HtmlAgilityPack;
using NeKzBot.Classes;

namespace NeKzBot.Server
{
	/// <summary>Class used to download webpages.</summary>
	public static class Fetching
	{
		/// <summary>Downloads the webpage as file to the given path.</summary>
		/// <param name="uri">Web address to download from.</param>
		/// <param name="path">Folder path to store the file.</param>
		public static async Task GetFile(string uri, string path)
		{
			using (var fileStream = File.Create(path))
				(await (await CreateClient()).GetStreamAsync(new Uri(uri))).CopyTo(fileStream);
		}

		/// <summary>Downloads the wepbage as file and caches it as file.</summary>
		/// <param name="uri">Web address to download from.</param>
		/// <param name="key">Name of requested.</param>
		public static async Task GetFileAndCacheAsync(string uri, string key)
		{
			using (var fileStream = File.Create(await Caching.CFile.GetPathAndSaveAsync(key)))
				(await (await CreateClient()).GetStreamAsync(new Uri(uri))).CopyTo(fileStream);
		}

		/// <summary>Downloads the wepbage as string.</summary>
		/// <param name="uri">Web address to download from.</param>
		public static async Task<string> GetStringAsync(string uri)
			=>  await (await CreateClient()).GetStringAsync(new Uri(uri));

		/// <summary>Downloads the wepbage as string.</summary>
		/// <param name="uri">Web address to download from.</param>
		/// /// <param name="headers">Http header for the http client.</param>
		public static async Task<string> GetStringAsync(string uri, List<WebHeader> headers)
			=> await (await CreateClient(headers)).GetStringAsync(new Uri(uri));

		/// <summary>Downloads the wepbage as HtmlDocument (HtmlAgilityPack).</summary>
		/// <param name="uri">Web address to download from.</param>
		public static async Task<HtmlDocument> GetDocumentAsync(string uri)
		{
			var doc = new HtmlDocument();
			using (var ms = new MemoryStream(Encoding.GetEncoding(0).GetBytes(await GetStringAsync(uri))))
				doc.Load(ms);
			return doc;
		}

		/// <summary>Returns data cache.</summary>
		/// <param name="key">Name of requested.</param>
		/// <param name="cachingtype">Caching system to choose.</param>
		public static async Task<object> GetStringAsync(string key, Type cachingtype)
			=> (cachingtype == typeof(Caching.CFile))
							? await Caching.CFile.GetCacheAsync(key)
							: (cachingtype == typeof(Caching.CApplication))
										   ? await Caching.CApplication.GetCacheAsync(key)
										   : null as object;

		/// <summary>Creates a new client for downloading multiple things at the same time.</summary>
		private static Task<HttpClient> CreateClient()
		{
			var client = new HttpClient();
			client.DefaultRequestHeaders.UserAgent.ParseAdd($"{Configuration.Default.AppName}/{Configuration.Default.AppVersion}");
			return Task.FromResult(client);
		}

		/// <summary>Creates a new client for downloading multiple things at the same time.</summary>
		/// /// /// <param name="headers">Http header for the http client.</param>
		private static Task<HttpClient> CreateClient(List<WebHeader> headers)
		{
			var client = new HttpClient();
			headers.ForEach(header => client.DefaultRequestHeaders.Add(header.GetHeader().Item1, header.GetHeader().Item2));
			return Task.FromResult(client);
		}
	}
}