using System.Collections.Generic;

namespace NeKzBot.Classes
{
	public sealed class Portal2Map
	{
		public string Name { get; set; }
		public string ChallengeModeName { get; set; }
		public string BestTimeId { get; set; }
		public string BestPortalsId { get; set; }
		public string ThreeLetterCode { get; set; }
		public string ElevatorTiming { get; set; }
	}

	public sealed class Portal2Leaderboard
	{
		public List<Portal2Entry> Entries { get; set; }
		public string MapName { get; set; }
		public string MapId { get; set; }
		public string MapPreview { get; set; }
	}

	public sealed class Portal2User
	{
		public string PlayerName { get; set; }
		public string PlayerSteamLink { get; set; }
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
		public string PlayerSteamAvatar { get; set; }
		public string BestPlaceRank { get; set; }
		public string BestPlaceMap { get; set; }
		public string WorstPlaceRank { get; set; }
		public string WorstPlaceMap { get; set; }
	}

	public sealed class Portal2Entry
	{
		public string Map { get; set; }
		public string Time { get; set; }
		public string Player { get; set; }
		public string Ranking { get; set; }
		public string Date { get; set; }
		public string Demo { get; set; }
		public string YouTube { get; set; }
		public string Comment { get; set; }
		public string MapId { get; set; }
	}
}