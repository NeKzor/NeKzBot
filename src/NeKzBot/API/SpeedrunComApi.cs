using System.Collections.Generic;
using Newtonsoft.Json;

namespace NeKzBot.API
{
	public interface ISpeedrunModel
	{
		string Id { get; set; }
		IEnumerable<SpeedrunLink> Links { get; set; }
	}
	[JsonObject]
	public class SpeedrunData<T>
		where T : ISpeedrunModel
	{
		[JsonProperty("data")]
		public IEnumerable<T> Data { get; set; }
		[JsonProperty("pagination")]
		public SpeedrunPagination Pagination { get; set; }
	}
	[JsonObject]
	public class SpeedrunLink
	{
		[JsonProperty("rel")]
		public string Rel { get; set; }
		[JsonProperty("uri")]
		public string Uri { get; set; }
	}
	[JsonObject]
	public class SpeedrunPagination
	{
		[JsonProperty("offset")]
		public uint Offset { get; set; }
		[JsonProperty("max")]
		public uint Max { get; set; }
		[JsonProperty("size")]
		public uint Size { get; set; }
		[JsonProperty("links")]
		public IEnumerable<SpeedrunLink> Links { get; set; }
	}
	// Models
	[JsonObject]
	public class SpeedrunNotification : ISpeedrunModel
	{
		[JsonProperty("id")]
		public string Id { get; set; }
		[JsonProperty("created")]
		public string Created { get; set; }
		[JsonProperty("status")]
		public string Status { get; set; }
		[JsonProperty("text")]
		public string Text { get; set; }
		[JsonProperty("item")]
		public SpeedrunLink Item { get; set; }
		[JsonProperty("links")]
		public IEnumerable<SpeedrunLink> Links { get; set; }
	}
	[JsonObject]
	public class SpeedrunGame : ISpeedrunModel
	{
		[JsonProperty("id")]
		public string Id { get; set; }
		[JsonProperty("names")]
		public SpeedrunNames Names { get; set; }
		[JsonProperty("abbreviation")]
		public string Abbreviation { get; set; }
		[JsonProperty("weblink")]
		public string Weblink { get; set; }
		[JsonProperty("released")]
		public int Released { get; set; }
		[JsonProperty("release-date")]
		public string ReleaseDate { get; set; }
		[JsonProperty("ruleset")]
		public SpeedrunRuleset Ruleset { get; set; }
		[JsonProperty("romhack")]
		public bool Romhack { get; set; }
		[JsonProperty("gametypes")]
		public object[] GameTypes { get; set; }
		[JsonProperty("regions")]
		public IEnumerable<string> Regions { get; set; }
		[JsonProperty("genres")]
		public object[] Genres { get; set; }
		[JsonProperty("engines")]
		public object[] Engines { get; set; }
		[JsonProperty("developers")]
		public object[] Developers { get; set; }
		[JsonProperty("publishers")]
		public object[] Publishers { get; set; }
		[JsonProperty("moderators")]
		public Dictionary<string, string> Moderators { get; set; }
		[JsonProperty("created")]
		public string Created { get; set; }
		[JsonProperty("assets")]
		public SpeedrunAssets Assets { get; set; }
		[JsonProperty("links")]
		public IEnumerable<SpeedrunLink> Links { get; set; }
	}
	// Objects
	[JsonObject]
	public class SpeedrunNames
	{
		[JsonProperty("international")]
		public string International { get; set; }
		[JsonProperty("japanese")]
		public string Japanese { get; set; }
		[JsonProperty("Twitch")]
		public string Twitch { get; set; }
	}
	[JsonObject]
	public class SpeedrunRuleset
	{
		[JsonProperty("show-milliseconds")]
		public bool ShowMilliseconds { get; set; }
		[JsonProperty("require-verification")]
		public bool RequireVerification { get; set; }
		[JsonProperty("require-video")]
		public bool RequireVideo { get; set; }
		[JsonProperty("run-times")]
		public IEnumerable<string> RunTimes { get; set; }
		[JsonProperty("default-time")]
		public string DefaultTime { get; set; }
		[JsonProperty("emulators-allowed")]
		public bool EmulatorsAllowed { get; set; }
	}
	[JsonObject]
	public class SpeedrunAssets
	{
		[JsonProperty("logo")]
		public SpeedrunAssetsItem Logo { get; set; }
		[JsonProperty("cover-tiny")]
		public SpeedrunAssetsItem CoverTiny { get; set; }
		[JsonProperty("cover-small")]
		public SpeedrunAssetsItem CoverSmall { get; set; }
		[JsonProperty("cover-medium")]
		public SpeedrunAssetsItem CoverMedium { get; set; }
		[JsonProperty("cover-large")]
		public SpeedrunAssetsItem CoverLarge { get; set; }
		[JsonProperty("icon")]
		public SpeedrunAssetsItem Icon { get; set; }
		[JsonProperty("trophy-1st")]
		public SpeedrunAssetsItem TrophyFirst { get; set; }
		[JsonProperty("trophy-2nd")]
		public SpeedrunAssetsItem TrophySecond { get; set; }
		[JsonProperty("trophy-3rd")]
		public SpeedrunAssetsItem TrophyThird { get; set; }
		[JsonProperty("trophy-4th")]
		public SpeedrunAssetsItem TrophyFourth { get; set; }
		[JsonProperty("background")]
		public SpeedrunAssetsItem Background { get; set; }
		[JsonProperty("foreground")]
		public SpeedrunAssetsItem Foreground { get; set; }
	}
	[JsonObject]
	public class SpeedrunAssetsItem
	{
		[JsonProperty("uri")]
		public string Uri { get; set; }
		[JsonProperty("width")]
		public int Width { get; set; }
		[JsonProperty("height")]
		public int Height { get; set; }
	}
}