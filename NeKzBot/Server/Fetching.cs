using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using NeKzBot.Classes;

namespace NeKzBot.Server
{
	/// <summary>Used to download webpages.</summary>
	public static class Fetching
	{
		/// <summary>Downloads the webpage as file to the give path.</summary>
		/// <param name="uri">Web address to download from.</param>
		/// <param name="path">Folder path to store the file.</param>
		public static async Task GetFileAsync(string uri, string path)
		{
			using (var client = await CreateHttpClient())
			using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
			using (var content = await (await client.SendAsync(request)).Content.ReadAsStreamAsync())
			using (var stream = new FileStream(path, FileMode.Create))
				await content.CopyToAsync(stream);
		}

		/// <summary>Downloads the webpage as file and caches it as file.</summary>
		/// <param name="uri">Web address to download from.</param>
		/// <param name="key">Name of requester.</param>
		public static async Task GetFileAndCacheAsync(string uri, string key)
			=> await GetFileAsync(uri, await Caching.CFile.GetPathAndSaveAsync(key));

		/// <summary>Downloads the webpage as string.</summary>
		/// <param name="uri">Web address to download from.</param>
		/// <param name="headers">Headers for the http client.</param>
		public static async Task<string> GetStringAsync(string uri, List<WebHeader> headers = default(List<WebHeader>))
		{
			using (var client = await CreateHttpClient(headers))
			using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
				return await (await client.SendAsync(request)).Content.ReadAsStringAsync();
		}

		/// <summary>Downloads the webpage as HtmlDocument (HtmlAgilityPack).</summary>
		/// <param name="uri">Web address to download from.</param>
		public static async Task<HtmlDocument> GetDocumentAsync(string uri)
		{
			var doc = new HtmlDocument();
			using (var ms = new MemoryStream(Encoding.Default.GetBytes(await GetStringAsync(uri))))
				doc.Load(ms);
			return doc;
		}

		/// <summary>Creates a new client for downloading multiple things at the same time.</summary>
		/// <param name="headers">Optional headers for the http client.</param>
		public static Task<HttpClient> CreateHttpClient(List<WebHeader> headers = default(List<WebHeader>))
		{
			var client = new HttpClient();
			if (headers == default(List<WebHeader>))
				client.DefaultRequestHeaders.UserAgent.ParseAdd($"{Configuration.Default.AppName}/{Configuration.Default.AppVersion}");
			else
				foreach (var header in headers)
					client.DefaultRequestHeaders.Add(header.GetHeader().Item1, header.GetHeader().Item2);
			return Task.FromResult(client);
		}

		/// <summary>Returns data cache.</summary>
		/// <param name="key">Name of requester.</param>
		/// <param name="cachingtype">Caching type class to choose.</param>
		public static async Task<object> GetStringFromCacheAsync(string key, Type cachingtype)
			=> (cachingtype == typeof(Caching.CFile))
							? await Caching.CFile.GetCacheAsync(key)
							: (cachingtype == typeof(Caching.CApplication))
										   ? await Caching.CApplication.GetCache(key)
										   : null as object;
	}
}