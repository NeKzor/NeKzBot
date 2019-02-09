using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NeKzBot.API
{
    public class SpeedrunComApiClient : IDisposable
    {
        public static readonly uint ApiVersion = 1;
        public static readonly string ApiUri = $"https://www.speedrun.com/api/v{ApiVersion}";

        private readonly WebClient _client;

        public SpeedrunComApiClient(string userAgent, string xApiKey)
        {
            _client = new WebClient(userAgent);
            _client.WithHeaders(
                "Host", "www.speedrun.com",
                "Accept", "application/json",
                "X-API-Key", xApiKey);
        }

        public async Task<IEnumerable<SpeedrunNotification>> GetNotificationsAsync(uint max = 0)
        {
            var (success, obj) = await _client.GetJsonObjectAsync<SpeedrunData<SpeedrunNotification>>($"{ApiUri}/notifications?max={max}");
            return (success && obj?.Data != null) ? obj.Data : default;
        }
        public async Task<IEnumerable<SpeedrunGame>> GetGamesAsync(string name)
        {
            var (success, obj) = await _client.GetJsonObjectAsync<SpeedrunData<SpeedrunGame>>($"{ApiUri}/games?name={name}");
            return (success && obj?.Data != null) ? obj.Data : default;
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
