using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using NeKzBot.Classes;
using NeKzBot.Resources;
using NeKzBot.Tasks;

namespace NeKzBot.Modules
{
	public class Speedrun : ModuleBase
	{
		[Command("wr")]
		public async Task GetSpeedrunWorldRecord([Remainder]string game)
		{
			var record = await SpeedrunCom.GetGameWorldRecordAsync(game);
			if (record == null)
				return;

			await Message.EditAsync(Context.Message, new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					Name = record.PlayerName,
					Url = $"https://speedrun.com/{record.PlayerName}",
					IconUrl = $"https://speedrun.com/themes/user/{record.PlayerName}/image.png"
				},
				Color = Data.SpeedruncomColor,
				Title = $"{record.Game.Name} World Record",
				Url = record.Game.Link,
				ThumbnailUrl = record.Game.CoverLink,
				Footer = new EmbedFooterBuilder
				{
					Text = "Data provided by speedrun.com",
					IconUrl = "https://www.speedrun.com/themes/default/favicon.png"
				}
			}
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Category";
				field.Value = record.CategoryName;
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Player";
				field.Value = $"{record.PlayerName.Replace("_", "\\_")}{(record.PlayerCountry != string.Empty ? $" {record.PlayerCountry}" : string.Empty)}";
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Time";
				field.Value = record.EntryTime;
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Date";
				field.Value = record.EntryDate;
			})
			.AddField(async field =>
			{
				field.Name = "Duration";
				field.Value = await Utils.GetDuration(record.EntryDateTime.DateTime);
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Video";
				field.Value = $"[Link]({record.EntryVideo})";
			})
			.AddField(field =>
			{
				field.Name = "Comment";
				field.Value = (record.PlayerComment != string.Empty)
													? record.PlayerComment
													: "No comment.";
			})
			.AddField(field =>
			{
				field.Name = "Status";
				field.Value = record.EntryStatus;
			}));
		}

		[Command("wrs")]
		public async Task GetSpeedrunWorldRecords([Remainder]string game)
		{
			var records = await SpeedrunCom.GetGameWorldRecordsAsync(game);
			if (records == null)
				return;

			var embed = new EmbedBuilder
			{
				Color = Data.SpeedruncomColor,
				Title = $"{records.Game.Name} World Records",
				Url = records.Game.Link,
				ThumbnailUrl = records.Game.CoverLink,
				Footer = new EmbedFooterBuilder
				{
					Text = "Data provided by speedrun.com",
					IconUrl = "https://www.speedrun.com/themes/default/favicon.png"
				}
			};

			foreach (var wr in records.WorldRecords.Take((int)DiscordConstants.MaximumFieldsInEmbed))
			{
				embed.AddField(field =>
				{
					field.Name = wr.CategoryName;
					field.Value = $"{wr.EntryTime} by {wr.PlayerName.Replace("_", "\\_")}{(wr.PlayerCountry != string.Empty ? $" {wr.PlayerCountry}" : string.Empty)}";
				});
			}
			await Message.EditAsync(Context.Message, embed);
		}

		[Command("pbs"), Alias("prs")]
		public async Task GetPlayerPersonalBests(string player)
		{
			var profile = await SpeedrunCom.GetPersonalBestOfPlayerAsync(player);
			if (profile == null)
				return;

			var name = profile.PlayerName;
			await Message.EditAsync(Context.Message, new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					Name = name,
					Url = $"https://speedrun.com/{name}",
					IconUrl = $"https://speedrun.com/themes/user/{name}/image.png"
				},
				Color = Data.SpeedruncomColor,
				Title = "Personal Records",
				Url = $"https://speedrun.com/{name}",
				Footer = new EmbedFooterBuilder
				{
					Text = "Data provided by speedrun.com",
					IconUrl = "https://www.speedrun.com/themes/default/favicon.png"
				}
			}
			.AddField(field =>
			{
				field.Name = "Full Game Runs";
				var output = string.Empty;
				foreach (var item in profile.PersonalBests)
					if (item.LevelName == null)
						output += $"\n{item.PlayerRank} • {item.Game.Name} • {item.CategoryName} in {item.EntryTime}";
				field.Value = (output != string.Empty)
									  ? output
									  : "None.";
			})
			.AddField(field =>
			{
				field.Name = "Level Runs";
				var output = string.Empty;
				foreach (var item in profile.PersonalBests)
					if (item.LevelName != null)
						output += $"\n{item.PlayerRank} • {item.Game.Name} • {item.CategoryName} • {item.LevelName} in {item.EntryTime}";
				field.Value = (output != string.Empty)
									  ? output
									  : "None.";
			}));
		}

		[Command("top10")]
		public async Task GetGameTopTen([Remainder]string game)
		{
			var leaderboard = await SpeedrunCom.GetTopTenAsync(game);
			if (leaderboard == null)
				return;

			await Message.EditAsync(Context.Message, new EmbedBuilder
			{
				Color = Data.SpeedruncomColor,
				Title = $"{leaderboard.Game.Name} Top Ten",
				Url = leaderboard.Game.Link,
				ThumbnailUrl = leaderboard.Game.CoverLink,
				Footer = new EmbedFooterBuilder
				{
					Text = "Data provided by speedrun.com",
					IconUrl = "https://www.speedrun.com/themes/default/favicon.png"
				}
			}
			.AddField(field =>
			{
				field.Name = leaderboard.Entries[0].CategoryName;
				var output = string.Empty;
				foreach (var item in leaderboard.Entries)
					output += $"\n{item.PlayerRank} • {item.PlayerName.Replace("_", "\\_")}{(item.PlayerLocation != string.Empty ? $" {item.PlayerLocation}" : string.Empty)} with {item.EntryTime}";
				field.Value = (output != string.Empty)
									  ? output
									  : "Empty.";
			}));
		}

		[Command("rules")]
		public async Task GetSpeedrunRules([Remainder]string game)
		{
			var rules = await SpeedrunCom.GetGameRulesAsync(game);
			if (rules == null)
				return;

			await Message.EditAsync(Context.Message, new EmbedBuilder
			{
				Color = Data.SpeedruncomColor,
				Title = "Speedrun Rules",
				Url = rules.Game.Link,
				ThumbnailUrl = rules.Game.CoverLink,
				Footer = new EmbedFooterBuilder
				{
					Text = "Data provided by speedrun.com",
					IconUrl = "https://www.speedrun.com/themes/default/favicon.png"
				}
			}
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Game";
				field.Value = rules.Game.Name;
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Category";
				field.Value = rules.CategoryName;
			})
			.AddField(async field =>
			{
				field.Name = "Rules";
				field.Value = (rules.ContentRules != string.Empty)
												  ? await Utils.CutMessage(rules.ContentRules.Replace("*", "•").Replace("_", "\\_"), (int)DiscordConstants.MaximumCharsPerEmbedField - "...".Length, "...")
												  : "No rules have been defined.";
			}));
		}

		[Command("latestwr")]
		public async Task GetLatestWorldRecordNotification()
		{
			var notification = await SpeedrunCom.GetNotificationUpdateAsync();
			if (notification == null)
				return;

			var player = notification.ContentText.Split(' ')[0];
			await Message.EditAsync(Context.Message, new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					Name = player,
					Url = $"https://speedrun.com/{player}",
					IconUrl = $"https://speedrun.com/themes/user/{player}/image.png"
				},
				Color = Data.SpeedruncomColor,
				Title = "Latest Notification",
				Url = notification.ContentLink,
				Footer = new EmbedFooterBuilder
				{
					Text = "Data provided by speedrun.com",
					IconUrl = "https://www.speedrun.com/themes/default/favicon.png"
				}
			}
			.AddField(field =>
			{
				field.Name = notification.CreationDate;
				field.Value = notification.ContentText.Replace("_", "\\_");
			}));
		}
	}
}