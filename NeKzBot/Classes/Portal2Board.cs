using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NeKzBot.Classes
{
	[JsonObject("map")]
	public sealed class Portal2Map
	{
		[JsonProperty("name")]
		public string Name { get; set; }
		[JsonProperty("cm_name")]
		public string ChallengeModeName { get; set; }
		[JsonProperty("best_time_id")]
		public string BestTimeId { get; set; }
		[JsonProperty("best_portals_id")]
		public string BestPortalsId { get; set; }
		[JsonProperty("three_letter_code")]
		public string ThreeLetterCode { get; set; }
		[JsonProperty("elevator_timing")]
		public string ElevatorTiming { get; set; }
		[JsonProperty("map_filter")]
		public Portal2MapFilter Filter { get; set; }
	}

	public sealed class Portal2Leaderboard
	{
		public List<Portal2Entry> Entries { get; set; }
		public Portal2Map Map { get; set; }
	}

	public sealed class Portal2EntryUpdate
	{
		public Portal2Entry Entry { get; set; }
		public Portal2TweetUpdate Tweet { get; set; }
		public string CacheFormat { get; set; }
	}

	public sealed class Portal2TweetUpdate
	{
		public string Location { get; set; }
		public string CommentMessage { get; set; }
	}

	public sealed class Portal2Entry
	{
		public Portal2Map Map { get; set; }
		public string Time { get; set; }
		public Portal2User Player { get; set; }
		public string Ranking { get; set; }
		public string Date
		{
			get => (_date != string.Empty)
						  ? $"{_date} (UTC)"
						  : "_Unknown._";
			set => _date = value;
		}
		// Useful for duration
		public DateTime DateTime
			=> (DateTime.TryParse(_date, out var result))
						? result
						: default(DateTime);
		public string Demo { get; set; }
		public string YouTube { get; set; }
		public string Comment { get; set; }
		public string ImageLink
			=> $"https://board.iverb.me/images/chambers_full/{Map.BestTimeId}.jpg";
		private string _date;
	}

	public sealed class Portal2User
	{
		public string Name { get; set; }
		public string SteamId { get; set; }
		public string SteamAvatar { get; set; }
		public string SteamLink
			=> $"https://steamcommunity.com/profiles/{SteamId}";
		public string BoardLink
			=> $"https://board.iverb.me/profile/{SteamId}";
		public string SinglePlayerPoints { get; set; }
		public string CooperativePoints { get; set; }
		public string OverallPoints { get; set; }
		public string SinglePlayerRank { get; set; }
		public string CooperativeRank { get; set; }
		public string OverallRank { get; set; }
		public string AverageSinglePlayerRank { get; set; }
		public string AverageCooperativeRank { get; set; }
		public string AverageOverallRank { get; set; }
		public string SinglePlayerWorldRecords { get; set; }
		public string CooperativeWorldRecords { get; set; }
		public string OverallWorldRecords { get; set; }
		public string BestPlaceRank { get; set; }
		public string BestPlaceMap { get; set; }
		public string WorstPlaceRank { get; set; }
		public string WorstPlaceMap { get; set; }
	}

	public enum Portal2MapFilter
	{
		Any,
		SinglePlayer,
		MultiPlayer,
		Workshop		// Workshop has two other modes again but it means custom map for now
	}
}