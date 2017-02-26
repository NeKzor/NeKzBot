namespace NeKzBot.Classes
{
	public class Portal2EntryUpdate
	{
		public string ChannelMessage { get; set; }
		public string TweetMessage { get; set; }
		public string CacheFormat { get; set; }
		public Cache TweetCache { get; set; }
		public Portal2Entry Global { get; set; }

		public class Cache
		{
			public string Location { get; set; }
			public string CommentMessage { get; set; }
		}
	}

	public sealed class Portal2Entry
	{
		public string Map { get; set; }
		public string Time { get; set; }
		public Portal2User Player { get; set; }
		public string Ranking { get; set; }
		public string Date { get; set; }
		public string Demo { get; set; }
		public string YouTube { get; set; }
		public string Comment { get; set; }
		public string MapID { get; set; }
	}

	public sealed class Portal2User
	{
		public string Name { get; set; }
		public string ProfileLink { get; set; }
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
		public string SteamAvatar { get; set; }
		public string BestPlaceRank { get; set; }
		public string BestPlaceMap { get; set; }
		public string WorstPlaceRank { get; set; }
		public string WorstPlaceMap { get; set; }
	}
}