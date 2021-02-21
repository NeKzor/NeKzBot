using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NeKzBot.API
{
    public class PistonApiClient : IDisposable
    {
        public static readonly string ApiUri = "https://emkc.org/api/v1/piston";

        private readonly WebClient _client;

        public PistonApiClient(string userAgent)
        {
            _client = new WebClient(userAgent);
        }

        public async Task<IReadOnlyCollection<PistonVersion>?> GetVersions()
        {
            var (success, obj) = await _client.GetJsonObjectAsync<List<PistonVersion>>($"{ApiUri}/versions");
            return (success && obj is not null) ? obj : default(List<PistonVersion>);
        }

        public async Task<PistonResult?> Execute(PistonCode code)
        {
            var (success, obj) = await _client.PostJsonObjectAsync<PistonResult, PistonCode>($"{ApiUri}/execute", code);
            return (success && obj is not null) ? obj : default(PistonResult);
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
