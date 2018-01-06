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
			var obj = await _client.GetJsonObjectAsync<SpeedrunData<SpeedrunNotification>>(ApiUri + $"/notifications?max={max}");
			return (obj?.Data != null) ? obj.Data : throw new Exception($"[{nameof(SpeedrunComApiClient)}] Failed to fetch notifications!");
		}

		public void Dispose()
		{
			_client.Dispose();
		}
	}
}