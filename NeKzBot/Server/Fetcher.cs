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
	/// <summary>Used to fetch data from webpages.</summary>
	public sealed class Fetcher
	{
		private readonly HttpClient _client;

		/// <summary>Creates a new fetching instance.</summary>
		/// <param name="timeout">Waiting request timeout of the client in minutes.</param>
		public Fetcher(double timeout = 5)
		{
			_client = new HttpClient { Timeout = TimeSpan.FromMinutes(timeout) };
			_client.DefaultRequestHeaders.UserAgent.ParseAdd($"{Configuration.Default.AppName}/{Configuration.Default.AppVersion}");
		}

		/// <summary>Creates a new fetching instance.</summary>
		/// <param name="headers">Collection of webheaders for the client. This should also provide a header for the user agent.</param>
		/// <param name="timeout">Waiting request timeout of the client in minutes.</param>
		public Fetcher(IEnumerable<WebHeader> headers, double timeout = 5)
		{
			_client = new HttpClient { Timeout = TimeSpan.FromMinutes(timeout) };
			foreach (var header in headers)
				_client.DefaultRequestHeaders.Add(header.GetHeader().Item1, header.GetHeader().Item2);
		}

		/// <summary>Get the current HttpClient of this instance.</summary>
		public Task<HttpClient> GetClient()
			=> Task.FromResult(_client);

		/// <summary>Downloads the webpage and saves it to the given path.</summary>
		/// <param name="uri">Web address to download from.</param>
		/// <param name="path">File path to save it as. Should also include the file extension.</param>
		public async Task<bool> GetFileAsync(string uri, string path)
		{
			try
			{
				var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Get, uri));
				if (response.IsSuccessStatusCode)
					using (var stream = new FileStream(path, FileMode.Create))
						await response.Content.CopyToAsync(stream);
				return response.IsSuccessStatusCode;
			}
			catch (Exception e)
			{
				await Logger.SendAsync("Fetcher.GetFileAsync Error", e);
			}
			return false;
		}

		/// <summary>Downloads the webpage and saves it automatically with the file caching system.</summary>
		/// <param name="uri">Web address to download from.</param>
		/// <param name="key">The key of the requester.</param>
		public async Task<bool> GetFileAndCacheAsync(string uri, string key)
			=> await GetFileAsync(uri, await Caching.CFile.GetPathAndSaveAsync(key));

		/// <summary>Downloads the webpage as a string.</summary>
		/// <param name="uri">Web address to download from.</param>
		public async Task<string> GetStringAsync(string uri)
		{
			var result = default(string);
			try
			{
				var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Get, uri));
				if (response.IsSuccessStatusCode)
					result = await response.Content.ReadAsStringAsync();
			}
			catch (Exception e)
			{
				await Logger.SendAsync("Fetcher.GetStringAsync Error", e);
			}
			return result;
		}

		/// <summary>Downloads the webpage and parses it as HtmlDocument (HtmlAgilityPack).</summary>
		/// <param name="uri">Web address to download from.</param>
		public async Task<HtmlDocument> GetDocumentAsync(string uri)
		{
			var doc = default(HtmlDocument);
			var result = await GetStringAsync(uri);
			if (result != default(string))
			{
				doc = new HtmlDocument();
				try
				{
					using (var ms = new MemoryStream(Encoding.Default.GetBytes(result)))
						doc.Load(ms);
				}
				catch (Exception e)
				{
					await Logger.SendAsync("Fetcher.GetDocumentAsync Error", e);
				}
			}
			return doc;
		}
	}
}