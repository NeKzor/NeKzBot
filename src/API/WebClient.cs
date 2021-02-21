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

        public Task WithHeaders(params string[] headerAndValue)
        {
            if ((headerAndValue.Length < 2) || ((headerAndValue.Length % 2) != 0))
                throw new InvalidOperationException();

            for (int i = 0; i < headerAndValue.Length; i += 2)
                _client.DefaultRequestHeaders.Add(headerAndValue[i], headerAndValue[i + 1]);
            return Task.CompletedTask;
        }

        // GET
        public async Task<(bool, T?)> GetJsonObjectAsync<T>(string url)
        {
            var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Get, url), HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
            return (response.IsSuccessStatusCode)
                ? (true, JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync().ConfigureAwait(false)))
                : (false, default);
        }
        public async Task<(bool, byte[]?)> GetBytesAsync(string url)
        {
            var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Get, url), HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
            return (response.IsSuccessStatusCode)
                ? (true, await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false))
                : (false, default);
        }
        public async Task<(bool, System.IO.Stream?)> GetStreamAsync(string url)
        {
            var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Get, url), HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
            return (response.IsSuccessStatusCode)
                ? (true, await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                : (false, default);
        }
        public async Task<(bool, System.Net.HttpStatusCode)> Ping(string url)
        {
            var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Get, url), HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
            return (response.IsSuccessStatusCode, response.StatusCode);
        }

        // POST
        public async Task<(bool, T?)> PostJsonObjectAsync<T, U>(string url, U data)
        {
            var body = new StringContent(JsonConvert.SerializeObject(data), System.Text.Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = body };
            var response = await _client.SendAsync(request, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
            return (response.IsSuccessStatusCode)
                ? (true, JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync().ConfigureAwait(false)))
                : (false, default);
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
