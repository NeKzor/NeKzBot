using System;
using System.Threading.Tasks;

namespace NeKzBot.API
{
    public class CvarsApiClient : IDisposable
    {
        public static readonly string ApiUri = "https://raw.githubusercontent.com/NeKzor/cvars/api";

        private readonly WebClient _client;

        public CvarsApiClient(string userAgent)
        {
            _client = new WebClient(userAgent);
        }

        public async Task<Cvars?> GetCvarsAsync(string game)
        {
            var (success, obj) = await _client.GetJsonObjectAsync<Cvars>($"{ApiUri}/{game}");
            return (success && obj?.Data != null) ? obj : default(Cvars);
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
