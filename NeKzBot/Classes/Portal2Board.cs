using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NeKzBot.Classes
{
	public enum MapFilter
	{
		Any,
		SinglePlayer,
		MultiPlayer
	}

	public sealed class Portal2Maps
	{
		public List<Portal2Map> Maps { get; set; }

		public Portal2Maps()
			=> Maps = new List<Portal2Map>();

		public Portal2Maps(List<Portal2Map> list)
			=> Maps = list;

		public Task<Portal2Map> Search(string value)
		{
			var index = -1;
			if ((index = Maps.FindIndex(map => string.Equals(map.BestTimeId, value, StringComparison.CurrentCultureIgnoreCase))) == -1)
				if ((index = Maps.FindIndex(map => string.Equals(map.BestPortalsId, value, StringComparison.CurrentCultureIgnoreCase))) == -1)
					if ((index = Maps.FindIndex(map => string.Equals(map.Name, value, StringComparison.CurrentCultureIgnoreCase))) == -1)
						if ((index = Maps.FindIndex(map => string.Equals(map.ChallengeModeName, value, StringComparison.CurrentCultureIgnoreCase))) == -1)
							if ((index = Maps.FindIndex(map => string.Equals(map.ThreeLetterCode, value, StringComparison.CurrentCultureIgnoreCase))) == -1)
								return null;
			return Task.FromResult(Maps[index]);
		}
	}

	public sealed class Portal2Map
	{
		public string Name { get; set; }
		public string ChallengeModeName { get; set; }
		public string BestTimeId { get; set; }
		public string BestPortalsId { get; set; }
		public string ThreeLetterCode { get; set; }
		public string ElevatorTiming { get; set; }
		public MapFilter Filter { get; set; }
	}

	public sealed class Portal2Leaderboard
	{
		public List<Portal2Entry> Entries { get; set; }
		public string MapName { get; set; }
		public string MapId { get; set; }
		public string MapPreview { get; set; }
	}

	public sealed class Portal2EntryUpdate
	{
		public Portal2Entry Entry { get; set; }
		public string CacheFormat { get; set; }
		public Portal2TweetUpdate Tweet { get; set; }
	}

	public sealed class Portal2TweetUpdate
	{
		public string Message { get; set; }
		public string Location { get; set; }
		public string CommentMessage { get; set; }
	}

	public sealed class Portal2Entry
	{
		public string Map { get; set; }
		public string Time { get; set; }
		public Portal2User Player { get; set; }
		public string Ranking { get; set; }
		public string Date
		{
			get => (_date != string.Empty)
						 ? $"{_date} UTC"
						 : "_Unknown._";
			set { _date = value; }
		}
		public DateTime DateTime    // Useful for duration
		{
			get => (DateTime.TryParse(_date, out var result))
							? result
							: default(DateTime);
		}
		public string Demo { get; set; }
		public string YouTube { get; set; }
		public string Comment { get; set; }
		public string MapId { get; set; }
		private string _date;
	}

	public sealed class Portal2User
	{
		public string Name { get; set; }
		public string SteamId { get; set; }
		public string SteamAvatar { get; set; }
		public string SteamLink
		{
			get => $"https://steamcommunity.com/profiles/{SteamId}";
		}
		public string BoardLink
		{
			get => $"https://board.iverb.me/profile/{SteamId}";
		}
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
}