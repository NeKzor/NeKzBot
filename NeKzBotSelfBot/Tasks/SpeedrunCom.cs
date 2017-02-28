using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NeKzBot.Classes;
using NeKzBot.Server;
using Newtonsoft.Json;
using SpeedrunComSharp;

namespace NeKzBot.Tasks
{
	public static class SpeedrunCom
	{
		private static SpeedrunComClient _client;
		private static List<WebHeader> _headers;

		private const int _maxnfcount = 10;
		private const int _maxnffetchcount = 100;

		public static async Task InitAsync()
		{
			await Logger.SendAsync("Initializing SpeedrunCom", LogColor.Init);
			_client = new SpeedrunComClient($"{Configuration.Default.AppName}/{Configuration.Default.AppVersion}", Credentials.Default.SpeedruncomToken, 5);
			// Make custom header
			_headers = new List<WebHeader>
			{
				new WebHeader("Host", "www.speedrun.com"),
				new WebHeader("Accept", "application/json"),
				new WebHeader("X-API-Key", Credentials.Default.SpeedruncomToken),
				new WebHeader("User-Agent", $"{Configuration.Default.AppName}/{Configuration.Default.AppVersion}")
			};
			if (!_client.IsAccessTokenValid)
				await Logger.SendAsync("Invalid Token", LogColor.Error);
		}

		public static async Task<SpeedrunWorldRecord> GetGameWorldRecordAsync(string name)
		{
			var game = _client.Games.SearchGame(name);
			if (game != null)
			{
				var anyPercent = default(Category);
				try
				{
					anyPercent = game.Categories.FirstOrDefault(category => (category.Type == CategoryType.PerGame) && (category.Runs.Any()));
				}
				catch (Exception e)
				{
					return await Logger.SendAsync("SpeedrunCom.GetGameWorldRecordAsync Category Error", e) as SpeedrunWorldRecord;
				}

				try
				{
					var wr = anyPercent.WorldRecord;
					return new SpeedrunWorldRecord()
					{
						Game = new SpeedrunGame
						{
							Name = game.Name,
							Link = game.WebLink.AbsoluteUri,
							CoverLink = game.Assets.CoverMedium.Uri.AbsoluteUri
						},
						CategoryName = wr.Category.Name,
						EntryTime = await FormatTime(wr.Times.Primary.Value),
						PlayerName = wr.Player.Name,
						PlayerCountry = ((wr.Player.IsUser)
									 && (wr.Player.User.Location?.Country?.Code?.ToLower() != null))
											? $":flag_{wr.Player.User.Location.Country.Code.ToLower()}:"
											: string.Empty,
						EntryVideo = wr.Videos.Links.FirstOrDefault().OriginalString,
						Platform = wr.Platform.Name,
						EntryDateTime = wr.DateSubmitted.Value,
						EntryDate = (wr.DateSubmitted == null)
													  ? string.Empty
													  : wr.DateSubmitted.Value.ToString(@"yyyy\-MM\-dd hh\:mm\:ss"),
						EntryStatus = ((wr.Status.Type == RunStatusType.Verified)
								   && (game.Ruleset.RequiresVerification)
								   && (wr.Status.ExaminerUserID != null))
																? $"Verified by {wr.Status.Examiner.Name}" + $"{(wr.Status.VerifyDate == null ? string.Empty : $" on {wr.Status.VerifyDate.Value.ToString(@"yyyy\-MM\-dd hh\:mm\:ss")}")}"
																: string.Empty,
						PlayerComment = (string.IsNullOrEmpty(wr.Comment))
											   ? string.Empty
											   : wr.Comment
					};
				}
				catch (Exception e)
				{
					await Logger.SendAsync("SpeedrunCom.GetGameWorldRecordAsync Error", e);
				}
			}
			return null;
		}

		public static async Task<SpeedrunPlayerProfile> GetPersonalBestOfPlayerAsync(string name)
		{
			var player = _client.Users.GetUsers(name)?.FirstOrDefault();
			if (player != null)
			{
				try
				{
					var pbs = default(Record[]);
					if (player.PersonalBests.Count > 0)
						pbs = player.PersonalBests.ToArray();
					else
						return null;

					var profile = new SpeedrunPlayerProfile()
					{
						PlayerName = player.Name,
						PlayerLocation = (player.Location?.Country?.Code?.ToLower() != null)
											? $":flag_{player.Location.Country.Code.ToLower()}:"
											: string.Empty
					};

					var records = new List<SpeedrunPlayerPersonalBest>();
					foreach (var pb in pbs)
					{
						var record = new SpeedrunPlayerPersonalBest()
						{
							CategoryName = pb.Category.Name,
							LevelName = pb.Level?.Name,
							Game = new SpeedrunGame
							{
								Name = pb.Game.Name,
								Link = pb.Game.WebLink.AbsoluteUri,
								CoverLink = pb.Game.Assets.CoverMedium.Uri.AbsoluteUri
							},
							PlayerRank = await TopTenFormat(pb.Rank.ToString()),
							EntryTime = await FormatTime(pb.Times.Primary.Value)
						};
						records.Add(record);
					}
					profile.PersonalBests = records;
					return profile;
				}
				catch (Exception e)
				{
					await Logger.SendAsync("SpeedrunCom.GetPersonalBestOfPlayerAsync Error", e);
				}
			}
			return null;
		}

		public static async Task<SpeedrunGameLeaderboard> GetGameWorldRecordsAsync(string name)
		{
			var game = _client.Games.SearchGame(name);
			if (game != null)
			{
				try
				{
					var records = new List<SpeedrunWorldRecord>();
					foreach (var category in game.Categories.Where(category => category.Type == CategoryType.PerGame))
					{
						var wr = category.WorldRecord;
						// Skip when there is no record
						if (wr == null)
							continue;

						records.Add(new SpeedrunWorldRecord()
						{
							CategoryName = wr.Category.Name,
							EntryTime = await FormatTime(wr.Times.Primary.Value),
							PlayerName = wr.Player.Name,
							PlayerCountry = ((wr.Player.IsUser)
										 && (wr.Player.User.Location?.Country?.Code?.ToLower() != null))
												? $":flag_{wr.Player.User.Location.Country.Code.ToLower()}:"
												: string.Empty,
							Platform = wr.Platform.Name,
							EntryDate = (wr.Date == null)
												 ? string.Empty
												 : wr.Date.Value.ToString(@"yyyy\-MM\-dd hh\:mm\:ss")
						});
					}

					return new SpeedrunGameLeaderboard()
					{
						Game = new SpeedrunGame
						{
							Name = game.Name,
							Link = game.WebLink.AbsoluteUri,
							CoverLink = game.Assets.CoverMedium.Uri.AbsoluteUri
						},
						WorldRecords = records
					};
				}
				catch (Exception e)
				{
					await Logger.SendAsync("SpeedrunCom.GetGameWorldRecordsAsync Error", e);
				}
			}
			return null;
		}

		public static async Task<SpeedrunGameLeaderboard> GetTopTenAsync(string name)
		{
			var game = _client.Games.SearchGame(name);
			if (game != null)
			{
				try
				{
					var category = game.FullGameCategories.FirstOrDefault(cat => (cat.Type == CategoryType.PerGame) && (cat.Runs.Any()));
					var runs = category.Runs.Where(run => run.Status.Type == RunStatusType.Verified)
											.OrderBy(run => run.Times.Primary.Value.TotalMilliseconds)
											.ToArray();
					var rankcount = (runs.Length >= 10)
												 ? 10
												 : runs.Length;
					var names = new List<string>();
					var records = new List<SpeedrunPlayerPersonalBest>();
					for (int i = 0, rank = 0; i < rankcount; i++)
					{
						if (!names.Contains(runs[i].Player.Name))
						{
							rank++;
							if ((rank >= 2)
							&& (await IsTied(runs[i].Times.Primary.Value, runs[i - 1].Times.Primary.Value)))
								rank--;
							names.Add(runs[i].Player.Name);

							var profile = new SpeedrunPlayerPersonalBest()
							{
								CategoryName = runs[i].Category.Name,
								PlayerName = runs[i].Player.Name,
								PlayerLocation = ((runs[i].Player.IsUser)
											  && (runs[i].Player.User.Location?.Country?.Code?.ToLower() != null))
													? $":flag_{runs[i].Player.User.Location.Country.Code.ToLower()}:"
													: string.Empty,
								PlayerRank = await TopTenFormat(rank.ToString(), false),
								EntryTime = await FormatTime(runs[i].Times.Primary.Value)
							};
							records.Add(profile);
						}
						else
							rankcount++;
					}

					return new SpeedrunGameLeaderboard()
					{
						Game = new SpeedrunGame
						{
							Name = game.Name,
							Link = game.WebLink.AbsoluteUri,
							CoverLink = game.Assets.CoverMedium.Uri.AbsoluteUri
						},
						Entries = records
					};
				}
				catch (Exception e)
				{
					await Logger.SendAsync("SpeedrunCom.GetTopTenAsync Error", e);
				}
			}
			return null;
		}

		public static async Task<SpeedrunNotification> GetNotificationUpdateAsync()
		{
			try
			{
				var json = await Fetching.GetStringAsync("http://www.speedrun.com/api/v1/notifications", _headers);
				if (string.IsNullOrEmpty(json))
					return null;

				// Read
				dynamic api = JsonConvert.DeserializeObject(json);
				if (string.IsNullOrEmpty(api?.ToString()))
					return null;

				// Parse data
				foreach (var item in api.data)
				{
					var nf = new SpeedrunNotification()
					{
						CreationDate = item?.created?.ToString(),
						ContentText = item?.text?.ToString(),
						NotificationType = item?.item?.rel?.ToString(),
						ContentLink = item?.item?.uri?.ToString()
					};

					// Filter out stuff
					//if (((nf.ContentText.Contains("posted a new thread")) && (nf.NotificationType == "thread")) || (nf.NotificationType == "resource"))
					//	return nf;
					if ((nf.ContentText.Contains("beat the WR"))
					&& (nf.NotificationType == "run"))
						return nf;
				}
			}
			catch (Exception e)
			{
				await Logger.SendAsync("SpeedrunCom.GetNotificationUpdateAsync Error", e);
			}
			return null;
		}

		public static async Task<SpeedrunGameRules> GetGameRulesAsync(string name, bool il = false)
		{
			var game = _client.Games.SearchGame(name);
			if (game != null)
			{
				try
				{
					foreach (var category in game.Categories.ToArray())
					{
						if (il)
						{
							if ((string.IsNullOrEmpty(category.Rules))
							|| (category.Type != CategoryType.PerLevel))
								continue;
						}
						else
						{
							if ((string.IsNullOrEmpty(category.Rules))
							|| (category.Type != CategoryType.PerGame))
								continue;
						}

						if (category.Rules != string.Empty)
						{
							return new SpeedrunGameRules()
							{
								ContentRules = category.Rules,
								CategoryName = category.Name,
								Game = new SpeedrunGame
								{
									Name = game.Name,
									Link = game.WebLink.AbsoluteUri,
									CoverLink = game.Assets.CoverMedium.Uri.AbsoluteUri
								}
							};
						}
					}
				}
				catch (Exception e)
				{
					await Logger.SendAsync("SpeedrunCom.GetGameRulesAsync Error", e);
				}
			}
			return null;
		}

		// Formatting
		private static Task<string> FormatTime(TimeSpan time)
		{
			var h = time.Hours.ToString();
			var m = time.Minutes.ToString();
			var s = time.Seconds.ToString();
			var ms = time.Milliseconds.ToString();
			var output = (h == "0")
							? string.Empty
							: $"{h}:";
			output += ((m == "0")
			&& (output == string.Empty))
					   ? string.Empty
					   : (m.Length == 1)
								   ? $"0{m}:"
								   : $"{m}:";
			output += (s.Length == 1)
								? (output == string.Empty)
										  ? s
										  : $"0{s}"
								: s;
			output += (ms == "0")
						  ? (output.Length <= 2)
										   ? "s"
										   : string.Empty
						  : $".{ms}";
			return Task.FromResult(output);
		}

		private static Task<string> TopTenFormat(string rank, bool trohpy = true, bool allbold = false)
		{
			var output = $"**{rank}th**";
			if (Convert.ToInt16(rank) > 10)
			{
				output = (allbold)
							? $"**#{rank}**"
							: $"#{rank}";
			}
			else if (rank == "1")
			{
				output = (trohpy)
							? "**1st** :trophy:"
							: "**1st**";
			}
			else if (rank == "2")
				output = "**2nd**";
			else if (rank == "3")
				output = "**3rd**";
			return Task.FromResult(output);
		}

		// Others
		private static Task<bool> IsTied(TimeSpan t1, TimeSpan t2)
			=> Task.FromResult(t1.TotalMilliseconds == t2.TotalMilliseconds);
	}
}