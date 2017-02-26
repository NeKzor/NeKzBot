using System;
using System.Collections.Generic;

namespace NeKzBot.Classes
{
	public sealed class SpeedrunGameLeaderboard
	{
		public List<SpeedrunWorldRecord> WorldRecords { get; set; }
		public List<SpeedrunPlayerPersonalBest> Entries { get; set; }
		public SpeedrunGame Game { get; set; }
	}

	public sealed class SpeedrunPlayerProfile
	{
		public List<SpeedrunPlayerPersonalBest> PersonalBests { get; set; }
		public string PlayerName { get; set; }
		public string PlayerLocation { get; set; }
	}

	public sealed class SpeedrunGameRules
	{
		public SpeedrunGame Game { get; set; }
		public string CategoryName { get; set; }
		public string ContentRules { get; set; }
	}

	public sealed class SpeedrunNotification
	{
		public string ContentText { get; set; }
		public string CreationDate { get; set; }
		public string ContentLink { get; set; }
		public string NotificationType { get; set; }
	}

	public class SpeedrunPlayerPersonalBest
	{
		public SpeedrunGame Game { get; set; }
		public string PlayerName { get; set; }
		public string CategoryName { get; set; }
		public string LevelName { get; set; }
		public string PlayerRank { get; set; }
		public string EntryTime { get; set; }
		public string PlayerLocation { get; set; }
	}

	public class SpeedrunWorldRecord
	{
		public SpeedrunGame Game { get; set; }
		public string CategoryName { get; set; }
		public string EntryTime { get; set; }
		public string PlayerName { get; set; }
		public string PlayerCountry { get; set; }
		public string EntryVideo { get; set; }
		public string Platform { get; set; }
		public string EntryDate { get; set; }
		public string EntryStatus { get; set; }
		public string PlayerComment { get; set; }
		public DateTimeOffset EntryDateTime { get; set; }
	}

	public class SpeedrunGame
	{
		public string Name { get; set; }
		public string Link { get; set; }
		public string CoverLink { get; set; }
	}
}