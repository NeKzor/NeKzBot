using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NeKzBot.Server;

namespace NeKzBot.Classes
{
	public static class TwitchApi
	{
		private static readonly string _twitchApi = "https://api.twitch.tv";
		private static readonly Fetcher _fetchClient = new Fetcher();

		public static async Task<TwitchStreamObject> GetStreamAsync(string channel)
		{
			var json = default(string);
			try
			{
				json = await _fetchClient.GetStringAsync($"{_twitchApi}/kraken/streams/{channel}?client_id={Credentials.Default.TwitchClientId}");
				if (string.IsNullOrEmpty(json))
					return await Logger.SendAsync("TwitchTv.GetStreamAsync JSON Error", LogColor.Error) as TwitchStreamObject;
			}
			catch (Exception e)
			{
				return await Logger.SendAsync("Fetching.GetStringAsync Error (TwitchTv.GetStreamAsync)", e) as TwitchStreamObject;
			}
			return JsonConvert.DeserializeObject<TwitchStreamObject>(json);
		}

		public static async Task<TwitchChannel> GetChannelAsync(string channel)
		{
			var json = default(string);
			try
			{
				json = await _fetchClient.GetStringAsync($"{_twitchApi}/kraken/channels/{channel}?client_id={Credentials.Default.TwitchClientId}");
				if (string.IsNullOrEmpty(json))
					return await Logger.SendAsync("TwitchTv.GetStreamerAsync JSON Error", LogColor.Error) as TwitchChannel;
			}
			catch (Exception e)
			{
				return await Logger.SendAsync("Fetching.GetStringAsync Error (TwitchTv.GetStreamerAsync)", e) as TwitchChannel;
			}
			return JsonConvert.DeserializeObject<TwitchChannel>(json);
		}

		public static async Task<TwitchGameSearchObject> GetGamesAsync(string game)
		{
			var json = default(string);
			try
			{
				json = await _fetchClient.GetStringAsync($"{_twitchApi}/kraken/search/games?query={WebUtility.UrlEncode(game)}&client_id={Credentials.Default.TwitchClientId}&type=suggest");
				if (string.IsNullOrEmpty(json))
					return await Logger.SendAsync("TwitchTv.GetGameAsync JSON Error", LogColor.Error) as TwitchGameSearchObject;
			}
			catch (Exception e)
			{
				return await Logger.SendAsync("Fetching.GetGameAsync Error (TwitchTv.GetStreamerAsync)", e) as TwitchGameSearchObject;
			}
			return JsonConvert.DeserializeObject<TwitchGameSearchObject>(json);
		}
	}

	public sealed class TwitchStreamObject
	{
		[JsonProperty("stream")]
		public TwitchStream Stream { get; set; }
	}

	public sealed class TwitchGameSearchObject
	{
		[JsonProperty("games")]
		public IEnumerable<TwitchGame> Games { get; set; }
	}

	public sealed class TwitchGame
	{
		[JsonProperty("name")]
		public string Name { get; set; }
		[JsonProperty("popularity")]
		public uint Popularity { get; set; }
		[JsonProperty("_id")]
		public ulong Id { get; set; }
		[JsonProperty("giantbomb_id")]
		public ulong GiantbombId { get; set; }
		[JsonProperty("box")]
		public TwitchGameBox Box { get; set; }
	}

	[JsonObject("box")]
	public sealed class TwitchGameBox
	{
		[JsonProperty("large")]
		public string Large { get; set; }
		[JsonProperty("medium")]
		public string Medium { get; set; }
		[JsonProperty("small")]
		public string Small { get; set; }
		[JsonProperty("template")]
		public string Template { get; set; }
	}

	[JsonObject("logo")]
	public sealed class TwitchGameLogo
	{
		[JsonProperty("large")]
		public string Large { get; set; }
		[JsonProperty("medium")]
		public string Medium { get; set; }
		[JsonProperty("small")]
		public string Small { get; set; }
		[JsonProperty("template")]
		public string Template { get; set; }
	}

	[JsonObject("stream")]
	public sealed class TwitchStream
	{
		[JsonProperty("_id")]
		public ulong Id { get; set; }
		[JsonProperty("game")]
		public string Game { get; set; }
		[JsonProperty("viewers")]
		public uint Viewers { get; set; }
		[JsonProperty("video_height")]
		public uint VideoHeight { get; set; }
		[JsonProperty("average_fps")]
		public float AverageFps { get; set; }
		[JsonProperty("delay")]
		public uint Delay { get; set; }
		[JsonProperty("created_at")]
		public DateTime CreatedAt { get; set; }
		[JsonProperty("is_playlist")]
		public bool IsPlaylist { get; set; }
		[JsonProperty("preview")]
		public TwitchStreamPreview Preview { get; set; }
		[JsonProperty("channel")]
		public TwitchChannel Channel { get; set; }
	}

	[JsonObject("preview")]
	public sealed class TwitchStreamPreview
	{
		[JsonProperty("small")]
		public string Small { get; set; }
		[JsonProperty("medium")]
		public string Medium { get; set; }
		[JsonProperty("large")]
		public string Large { get; set; }
		[JsonProperty("template")]
		public string Template { get; set; }
	}

	[JsonObject("channel")]
	public sealed class TwitchChannel
	{
		[JsonProperty("mature")]
		public bool IsMature { get; set; }
		[JsonProperty("status")]
		public string Status { get; set; }
		[JsonProperty("broadcaster_language")]
		public string BroadcasterLanguage { get; set; }
		[JsonProperty("display_name")]
		public string DisplayName { get; set; }
		[JsonProperty("game")]
		public string Game { get; set; }
		[JsonProperty("language")]
		public string Language { get; set; }
		[JsonProperty("_id")]
		public ulong Id { get; set; }
		[JsonProperty("name")]
		public string Name { get; set; }
		[JsonProperty("created_at")]
		public DateTime CreatedAt { get; set; }
		[JsonProperty("updated_at")]
		public DateTime UpdatedAt { get; set; }
		[JsonProperty("logo")]
		public string Logo { get; set; }
		[JsonProperty("profile_banner")]
		public string ProfileBanner { get; set; }
		[JsonProperty("partner")]
		public bool IsPartner { get; set; }
		[JsonProperty("url")]
		public string Url { get; set; }
		[JsonProperty("views")]
		public uint Views { get; set; }
		[JsonProperty("followers")]
		public uint Followers { get; set; }
	}
}