using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

// Modified version from Portal2Boards.Net
namespace NeKzBot.API
{
	public sealed class WebClient : IDisposable
	{
		private readonly HttpClient _client;

		public WebClient(string userAgent)
		{
			_client = new HttpClient();
			_client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
		}

		public async Task<T> GetJsonObjectAsync<T>(string url)
		{
			var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Get, url), HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
			response.EnsureSuccessStatusCode();
			return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
		}

		public async Task<(bool Success, T Result)> TryGetJsonObjectAsync<T>(string url)
		{
			try
			{
				var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Get, url), HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
				response.EnsureSuccessStatusCode();
				return (true, JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync().ConfigureAwait(false)));
			}
			catch
			{
			}
			return (default, default);
		}

		public async Task<byte[]> GetBytesAsync(string url)
		{
			var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Get, url), HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
			response.EnsureSuccessStatusCode();
			return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
		}

		public async Task<(bool Sucess, byte[] Result)> TryGetBytesAsync(string url)
		{
			try
			{
				var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Get, url), HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
				response.EnsureSuccessStatusCode();
				return (true, await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false));
			}
			catch
			{
			}
			return (default, default);
		}

		public void Dispose()
		{
			_client.Dispose();
		}
	}
}