using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NeKzBot.Classes;
using NeKzBot.Server;

namespace NeKzBot.Tasks
{
	public static class TwitchTv
	{
		public static async Task<TwitchStream> GetPreviewAsync(string channel)
		{
			var streamer = await GetStreamerAsync(channel);
			return new TwitchStream()
			{
				ChannelName = streamer?.stream?.channel?.display_name?.ToString() ?? string.Empty,
				GameName = (streamer?.stream?.channel?.game != null)
															? $"playing {streamer.stream.channel.game.ToString()}"
															: "streaming",
				ChannelViewers = streamer?.stream?.viewers ?? 0,
				StreamTitle = System.Text.Encoding.UTF8.GetString(Encoding.Unicode.GetBytes(streamer?.stream?.channel?.status?.ToString())) ?? string.Empty,
				StreamLink = streamer?.stream?.channel?.url?.ToString() ?? string.Empty,
				PreviewLink = streamer?.stream?.preview?.large.ToString() ?? string.Empty,
				AvatarLink = streamer?.stream?.channel?.logo?.ToString() ?? string.Empty
			};
		}

		internal static async Task<dynamic> GetStreamerAsync(string channel)
		{
			// Twitch API
			var json = string.Empty;
			try
			{
				// Download
				if (string.IsNullOrEmpty(json = await Fetching.GetStringAsync($"https://api.twitch.tv/kraken/streams/{channel}?client_id={Credentials.Default.TwitchClientId}")))
					return await Logger.SendAsync("TwitchTv.GetStreamerAsync Web Client Error", LogColor.Error);
			}
			catch (Exception e)
			{
				return await Logger.SendAsync("Fetching.GetStringAsync Error (TwitchTv.GetStreamerAsync)", e);
			}

			// Read
			dynamic streamer = JsonConvert.DeserializeObject(json);
			return (string.IsNullOrEmpty(streamer?.ToString()))
						  ? await Logger.SendAsync("TwitchTv.GetStreamerAsync Json Error", LogColor.Error)
						  : streamer;
		}
	}
}