using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeKzBot.Server;
using NeKzBot.Utilities;

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
		public string Name { get; set; }
		public string Id { get; set; }
		public string CountryCode { get; set; }
		public string Location
		{
			get => (CountryCode != string.Empty)
								? $" :flag_{CountryCode}:"
								: string.Empty;
			set { }
		}
		public string Region { get; set; }
		public int Mods { get; set; }
		public string Role { get; set; }
		public int Runs { get; set; }
		public string SignUpDate
			=> SignUpDateTime.ToString("yyyy-MM-dd HH:mm:ss");
		public DateTime SignUpDateTime { get; set; }
		public string YouTubeLink { get; set; }
		public string TwitchLink { get; set; }
		public string TwitterLink { get; set; }
		public string WebsiteLink { get; set; }
		public string PlayerLink
			=> $"https://speedrun.com/{Name}";
		public string PlayerAvatar
			=> $"https://speedrun.com/themes/user/{Name}/image.png";
	}

	public sealed class SpeedrunGameRules
	{
		public SpeedrunGame Game { get; set; }
		public string CategoryName { get; set; }
		public string ContentRules { get; set; }
	}

	public sealed class SpeedrunNotification
	{
		public string Id { get; set; }
		public string ContentText { get; set; }
		public string CreationDate { get; set; }
		public string ContentLink { get; set; }
		public SpeedrunNotificationType Type { get; set; }
		public SpeedrunGame Game { get; private set; }
		public SpeedrunNotificationStatus Status { get; set; }
		public SpeedrunPlayerProfile Author
			=> (Type != SpeedrunNotificationType.Resource)
					 ? new SpeedrunPlayerProfile { Name = ContentText.Split(' ')[0] }
					 : null;

		// Tasks
		public Task BuildGame()
		{
			Game = new SpeedrunGame(ParsedGame);
			return Task.FromResult(0);
		}

		// Extensions
		public string FormattedText
		{
			get
			{
				try
				{
					// NOTE: notification type "guide" is missing
					if (Type == SpeedrunNotificationType.Thread)
						return $"New thread post:\n_[{ContentText.Substring(ContentText.LastIndexOf(" forum: ") + " forum: ".Length)}]({Utils.AsRawText(ContentLink).GetAwaiter().GetResult()})_";
					if (Type == SpeedrunNotificationType.Post)
						return $"New thread response:\n_[{ContentText.Substring(ContentText.IndexOf("'") + 1, ContentText.LastIndexOf("'") - ContentText.IndexOf("'") - 1)}]({Utils.AsRawText(ContentLink).GetAwaiter().GetResult()})_";
					if (Type == SpeedrunNotificationType.Resource)
					{
						if (ContentText.EndsWith("updated. "))
							return $"The resource _{ContentText.Substring(ContentText.IndexOf("The tool resource '") + "The tool resource '".Length + 1, ContentText.LastIndexOf("' for ") - ContentText.IndexOf("The tool resource '") - "The tool resource '".Length - 1)}_ has been updated.";
						if (ContentText.EndsWith("added."))
							return $"The resource _{ContentText.Substring(ContentText.IndexOf("A new tool resource, ") + "A new tool resource, ".Length + 1, ContentText.LastIndexOf(", has been added to ") - ContentText.IndexOf("A new tool resource, ") - "A new tool resource, ".Length - 1)}_ has been added.";
					}
					if (Type == SpeedrunNotificationType.Run)
						return $"New {(ContentText.Contains(" beat the WR in ") ? "world record" : "personal best")} in [{Utils.AsRawText(Game.Name).GetAwaiter().GetResult()}]({((string.IsNullOrEmpty(Game.Link)) ? Utils.AsRawText(ContentLink).GetAwaiter().GetResult() : Utils.AsRawText(Game.Link).GetAwaiter().GetResult())})\nwith a time of {ContentText.Substring(ContentText.LastIndexOf(". The new WR is ") + ". The new WR is ".Length)}";
					if (Type == SpeedrunNotificationType.Moderator)
						return "A new moderator has been chosen.\nCongrats.";
				}
				catch (Exception ex)
				{
					Logger.SendAsync("SpeedrunNotification.FormattedText Error", ex).GetAwaiter().GetResult();
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
				catch (Exception ex)
				{
					Logger.SendAsync("SpeedrunNotification.ParsedGame Error", ex).GetAwaiter().GetResult();
				}
				return string.Empty;
			}
		}
	}

	public sealed class SpeedrunPlayerPersonalBest
	{
		public SpeedrunGame Game { get; set; }
		public string PlayerName { get; set; }
		public string CategoryName { get; set; }
		public string LevelName { get; set; }
		public string PlayerRank { get; set; }
		public string EntryTime { get; set; }
		public string PlayerLocation { get; set; }
	}

	public sealed class SpeedrunVariable
	{
		public string Name { get; set; }
		public string Value { get; set; }
	}

	public enum PersonalBestFilter
	{
		DontCare,
		Best,
		Worst,
		Oldest
	}

	public sealed class SpeedrunWorldRecord
	{
		public IEnumerable<SpeedrunPlayerProfile> Players { get; set; }
		public SpeedrunGame Game { get; set; }
		public string CategoryName { get; set; }
		public string Platform { get; set; }
		public string EntryId { get; set; }
		public string EntryLink
			=> $"https://www.speedrun.com/run/{EntryId}";
		public string EntryVideo { get; set; }
		public string EntryTime { get; set; }
		public string EntryDate { get; set; }
		public string EntryStatus { get; set; }
		public string EntryComment { get; set; }
		public DateTimeOffset EntryDateTime { get; set; }
		public IEnumerable<SpeedrunVariable> Variables { get; set; }
	}

	public sealed class SpeedrunGame
	{
		public string Name { get; set; }
		public string Id { get; set; }
		public string Link { get; set; }
		public string CoverLink { get; set; }
		public string Abbreviation { get; set; }
		public int? ReleaseDate { get; set; }
		public string CreationDate
		{
			get => (CreationDateTime != null)
									 ? CreationDateTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Unknown"
									 : "Unknown";
			set { }
		}
		public DateTime? CreationDateTime { get; set; }
		public List<SpeedrunPlayerProfile> Moderators { get; set; }
		public List<SpeedrunGameCategory> Categories { get; set; }
		public bool IsRom { get; set; }
		public string DefaultTimingMethod { get; set; }
		public bool EmulatorsAllowed { get; set; }
		public bool RequiresVerification { get; set; }
		public bool RequiresVideoProof { get; set; }
		public bool ShowMilliseconds { get; set; }

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

	public sealed class SpeedrunGameCategory
	{
		public string Name { get; set; }
		public string Id { get; set; }
		public SpeedrunCategoryType Type { get; set; }
	}

	public enum SpeedrunNotificationType
	{
		Post,
		Thread,
		Moderator,
		Run,
		Game,
		Guide,
		Resource,
		Any			// Custom type
	}

	public enum SpeedrunNotificationStatus
	{
		Read,
		Unread
	}

	public enum SpeedrunCategoryType
	{
		FullGame,
		Game
	}
}