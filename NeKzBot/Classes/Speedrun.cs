using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
		public SpeedrunNotificationType Type { get; set; }
		public SpeedrunGame Game { get; private set; }

		public string Cache
		{
			get => CreationDate + ContentLink + ContentText;
		}

		public string Author
		{
			get => Type != SpeedrunNotificationType.Resource
						? ContentText.Split(' ')[0]
						: string.Empty;
		}

		public Task BuildGame()
		{
			Game = new SpeedrunGame(ParsedGame);
			return Task.FromResult(0);
		}

		public string FormattedText
		{
			get
			{
				try
				{
					if (Type == SpeedrunNotificationType.Thread)
						return $"Posted a new thread:\n_[{ContentText.Substring(ContentText.LastIndexOf(" forum: ") + " forum: ".Length, ContentText.Length - ContentText.IndexOf(" forum: ") - " forum: ".Length)}]({ContentLink})_";
					if (Type == SpeedrunNotificationType.Post)
						return $"Responded to the thread:\n_[{ContentText.Substring(ContentText.IndexOf("'") + 1, ContentText.LastIndexOf("'") - ContentText.IndexOf("'") - 1)}]({ContentLink})_";
					if (Type == SpeedrunNotificationType.Resource)
						return $"The resource _{ContentText.Substring(ContentText.IndexOf("'") + 1, ContentText.LastIndexOf("'") - ContentText.IndexOf("'") - 1)}_ has been updated or added.";
					if (Type == SpeedrunNotificationType.Run)
						return $"Sets a new {(ContentText.Contains(" beat the WR in ") ? "world record" : "personal best")} in [{Game.Name}]({Game.Link})\nwith a time of {ContentText.Substring(ContentText.LastIndexOf(". The new WR is ") + ". The new WR is ".Length, ContentText.Length - ContentText.LastIndexOf(". The new WR is ") - ". The new WR is ".Length)}";
					if (Type == SpeedrunNotificationType.Moderator)
						return "Is now a moderator.\nCongrats.";
				}
				catch
				{
				}
				return string.Empty;
			}
		}

		private string ParsedGame
		{
			get
			{
				try
				{
					if (Type == SpeedrunNotificationType.Thread)
						return ContentText.Substring(ContentText.LastIndexOf(" in the ") + " in the ".Length, ContentText.IndexOf(" forum:") - ContentText.IndexOf(" in the ") - "in the ".Length);
					if (Type == SpeedrunNotificationType.Post)
						return ContentText.Substring(ContentText.IndexOf(" in the ") + " in the ".Length, ContentText.IndexOf(" forum.") - ContentText.IndexOf(" in the ") - " in the ".Length);
					if (Type == SpeedrunNotificationType.Resource)
						return ContentText.Substring(ContentText.IndexOf(" for ") + " for ".Length, ContentText.LastIndexOf(" has") - ContentText.IndexOf(" for ") - "for".Length);
					// Cannot separate category from game :(
					if (Type == SpeedrunNotificationType.Run)
						return ContentText.Substring(ContentText.IndexOf("beat the WR in ") + "beat the WR in ".Length, ContentText.IndexOf(". The new WR is") - ContentText.IndexOf("beat the WR in ") - "beat the WR in ".Length);
					if (Type == SpeedrunNotificationType.Moderator)
						return ContentText.Substring(ContentText.IndexOf("has been added to ") + "has been added to ".Length, ContentText.IndexOf(" as a moderator.") - ContentText.IndexOf("has been added to ") - "has been added to ".Length);
				}
				catch
				{
				}
				return string.Empty;
			}
		}
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

		public SpeedrunGame()
		{
		}

		public SpeedrunGame(string name, string link = "", string coverlink = "")
		{
			Name = name;
			Link = link;
			CoverLink = coverlink;
		}
	}

	public enum SpeedrunNotificationType
	{
		Post,
		Thread,
		Moderator,
		Run,
		Game,
		Guide,
		Resource
	}

	public enum SpeedrunNotificationStatus
	{
		Read,
		Unread
	}
}