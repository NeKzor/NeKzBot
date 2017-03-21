using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NeKzBot.Server;

namespace NeKzBot.Classes
{
	public sealed class TwitchGame
	{
		public string Name { get; set; }
		public string BoxArt { get; set; }
	}

	public sealed class TwitchStream
	{
		public string ChannelName { get; set; }
		public TwitchGame Game { get; set; }
		public string StreamTitle { get; set; }
		public string StreamLink { get; set; }
		public string AvatarLink { get; set; }
		public string PreviewLink { get; set; }
		public uint ChannelViewers { get; set; }
	}

	public static class TwitchTv
	{
		private static readonly string _twitchApi = "https://api.twitch.tv";

		public static async Task<dynamic> GetStreamAsync(string channel)
		{
			// Twitch API
			var json = string.Empty;
			try
			{
				// Download
				json = await Fetching.GetStringAsync($"{_twitchApi}/kraken/streams/{channel}?client_id={Credentials.Default.TwitchClientId}");
			}
			catch (Exception e)
			{
				return await Logger.SendAsync("Fetching.GetStringAsync Error (TwitchTv.GetStreamAsync)", e);
			}

			// Read
			if (string.IsNullOrEmpty(json))
				return await Logger.SendAsync("TwitchTv.GetStreamAsync Is Empty", LogColor.Error);

			// Read json string
			dynamic streamer = JsonConvert.DeserializeObject(json);
			return (string.IsNullOrEmpty(streamer?.ToString()))
						  ? await Logger.SendAsync("TwitchTv.GetStreamAsync JSON Error", LogColor.Error)
						  : streamer;
		}

		public static async Task<dynamic> GetStreamerAsync(string channel)
		{
			// Twitch API
			var json = string.Empty;
			try
			{
				// Download
				json = await Fetching.GetStringAsync($"{_twitchApi}/kraken/channels/{channel}?client_id={Credentials.Default.TwitchClientId}");
			}
			catch (Exception e)
			{
				return await Logger.SendAsync("Fetching.GetStringAsync Error (TwitchTv.GetStreamerAsync)", e);
			}

			// Read
			if (string.IsNullOrEmpty(json))
				return await Logger.SendAsync("TwitchTv.GetStreamerAsync Is Empty", LogColor.Error);

			// Read json string
			dynamic streamer = JsonConvert.DeserializeObject(json);
			return (string.IsNullOrEmpty(streamer?.ToString()))
						  ? await Logger.SendAsync("TwitchTv.GetStreamerAsync JSON Error", LogColor.Error)
						  : streamer;
		}

		public static async Task<dynamic> GetGameAsync(string game)
		{
			// Twitch API
			var json = string.Empty;
			try
			{
				// Download
				json = await Fetching.GetStringAsync($"{_twitchApi}/kraken/search/games?query={WebUtility.UrlEncode(game)}&client_id={Credentials.Default.TwitchClientId}&type=suggest");
			}
			catch (Exception e)
			{
				return await Logger.SendAsync("Fetching.GetGameAsync Error (TwitchTv.GetStreamerAsync)", e);
			}

			// Read
			if (string.IsNullOrEmpty(json))
				return await Logger.SendAsync("TwitchTv.GetGameAsync Is Empty", LogColor.Error);

			// Read json string
			dynamic streamer = JsonConvert.DeserializeObject(json);
			return (string.IsNullOrEmpty(streamer?.ToString()))
						  ? await Logger.SendAsync("TwitchTv.GetGameAsync JSON Error", LogColor.Error)
						  : streamer;
		}
	}
}