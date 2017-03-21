using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SpeedrunComSharp;
using NeKzBot.Classes;
using NeKzBot.Server;

namespace NeKzBot.Tasks.Speedrun
{
	public static partial class SpeedrunCom
	{
		private static SpeedrunComClient _client;
		private static List<WebHeader> _headers;

		private const int _maxnfcount = 10;
		private const int _maxnffetchcount = 100;

		public static async Task InitAsync()
		{
			await Logger.SendAsync("Initializing SpeedrunCom Client", LogColor.Init);
			_client = new SpeedrunComClient($"{Configuration.Default.AppName}/{Configuration.Default.AppVersion}", Credentials.Default.SpeedruncomToken, 5);

			// Make custom header
			_headers = new List<WebHeader>()
			{
				new WebHeader("Host", "www.speedrun.com"),
				new WebHeader("Accept", "application/json"),
				new WebHeader("X-API-Key", Credentials.Default.SpeedruncomToken),
				new WebHeader("User-Agent", $"{Configuration.Default.AppName}/{Configuration.Default.AppVersion}")
			};
			if (!_client.IsAccessTokenValid)
				await Logger.SendAsync("Invalid Token", LogColor.Default);
		}

		public static async Task<SpeedrunWorldRecord> GetGameWorldRecordAsync(string gamename, string categoryname = default(string))
		{
			var game = _client.Games.SearchGame(gamename);
			if (game != null)
			{
				var category = (categoryname == null)
									   ? game.Categories.FirstOrDefault(cat => (cat.Type == CategoryType.PerGame)
																			&& (cat.Runs.Any()))
									   : game.Categories.FirstOrDefault(cat => (cat.Type == CategoryType.PerGame)
																			&& (cat.Runs.Any())
																			&& (string.Equals(cat.Name, categoryname, StringComparison.CurrentCultureIgnoreCase)));
				var wr = category?.WorldRecord;
				if (wr == null)
					return new SpeedrunWorldRecord();

				try
				{
					var players = new List<SpeedrunPlayerProfile>();
					foreach (var player in wr.Players)
					{
						players.Add(new SpeedrunPlayerProfile
						{
							Name = player.Name,
							CountryCode = (player.IsUser)
												 ? player.User.Location?.Country?.Code?.ToLower() ?? string.Empty
												 : string.Empty
						});
					}

					var variables = new List<SpeedrunVariable>();
					foreach (var variable in wr.VariableValues)
					{
						variables.Add(new SpeedrunVariable
						{
							Name = variable.Name,
							Value = variable.Value
						});
					}

					return new SpeedrunWorldRecord()
					{
						Game = new SpeedrunGame
						{
							Name = game.Name,
							Link = game.WebLink.AbsoluteUri,
							CoverLink = game.Assets.CoverMedium.Uri.AbsoluteUri
						},
						Players = players,
						Variables = variables,
						CategoryName = wr.Category.Name,
						EntryId = wr.ID,
						EntryTime = await FormatTime(wr.Times.Primary.Value),
						EntryVideo = wr.Videos?.Links?.FirstOrDefault()?.OriginalString,
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
						EntryComment = (string.IsNullOrEmpty(wr.Comment))
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
						return new SpeedrunPlayerProfile();

					var profile = new SpeedrunPlayerProfile()
					{
						Name = player.Name,
						Location = (player.Location?.Country?.Code?.ToLower() != null)
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

		public static async Task<SpeedrunGameLeaderboard> GetGameWorldRecordsAsync(string gamename)
		{
			var game = _client.Games.SearchGame(gamename);
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

						var players = new List<SpeedrunPlayerProfile>();
						foreach (var player in wr.Players)
						{
							players.Add(new SpeedrunPlayerProfile
							{
								Name = player.Name,
								CountryCode = (player.IsUser)
													 ? player.User.Location?.Country?.Code?.ToLower() ?? string.Empty
													 : string.Empty
							});
						}

						records.Add(new SpeedrunWorldRecord()
						{
							CategoryName = wr.Category.Name,
							EntryId = wr.ID,
							EntryTime = await FormatTime(wr.Times.Primary.Value),
							Players =  players,
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

		public static async Task<SpeedrunPlayerProfile> GetPlayerInfoAsync(string playername)
		{
			try
			{
				var player = default(User);
				if (_client.Users.GetUsers(playername).Any())
					player = _client.Users.GetUsers(playername).FirstOrDefault();
				else
					return null;

				return new SpeedrunPlayerProfile
				{
					Id = player.ID,
					Name = player.Name,
					CountryCode = player.Location?.Country?.Code?.ToLower() ?? string.Empty,
					Region = player.Location?.Region?.Code?.ToLower() ?? string.Empty,
					Mods = player.ModeratedGames.Count(),
					PersonalBests = new List<SpeedrunPlayerPersonalBest>(player.PersonalBests.Count),
					Role = player.Role.ToString(),
					Runs = player.Runs.Count(),
					SignUpDateTime = player.SignUpDate.Value,
					YouTubeLink = player.YoutubeProfile?.OriginalString ?? string.Empty,
					TwitchLink = player.TwitchProfile?.OriginalString ?? string.Empty,
					TwitterLink = player.TwitterProfile?.OriginalString ?? string.Empty,
					WebsiteLink = player.WebLink?.OriginalString ?? string.Empty
				};
			}
			catch (Exception e)
			{
				return await Logger.SendToChannelAsync("SpeedrunCom.GetPlayerInfoAsync Error", e) as SpeedrunPlayerProfile;
			}
		}

		public static async Task<SpeedrunGame> GetGameInfoAsync(string gamename)
		{
			try
			{
				var game = _client.Games.SearchGame(gamename);
				if (game == null)
					return null;

				return new SpeedrunGame
				{
					// Title
					Name = game.Name,
					// Info
					Id = game.ID,
					Link = game.WebLink.AbsoluteUri,
					CoverLink = game.Assets.CoverMedium.Uri.AbsoluteUri,
					Abbreviation = game.Abbreviation,
					ReleaseDate = game.YearOfRelease,
					CreationDateTime = game.CreationDate,
					Moderators = new List<SpeedrunPlayerProfile>(game.Moderators.Count),
					IsRom = game.IsRomHack,
					// Rules
					DefaultTimingMethod = game.Ruleset.DefaultTimingMethod.ToString(),
					EmulatorsAllowed = game.Ruleset.EmulatorsAllowed,
					RequiresVerification = game.Ruleset.RequiresVerification,
					RequiresVideoProof = game.Ruleset.RequiresVideo,
					ShowMilliseconds = game.Ruleset.ShowMilliseconds
				};
			}
			catch (Exception e)
			{
				return await Logger.SendToChannelAsync("SpeedrunCom.GetGameInfoAsync Error", e) as SpeedrunGame;
			}
		}

		public static async Task<SpeedrunGame> GetModeratorsAsync(string gamename)
		{
			try
			{
				var output = string.Empty;
				var game = _client.Games.SearchGame(gamename);
				if (game == null)
					return null;

				var moderators = new List<SpeedrunPlayerProfile>();
				foreach (var item in game.Moderators.ToArray())
				{
					moderators.Add(new SpeedrunPlayerProfile
					{
						Name = item.Name,
						Id = item.UserID,
						CountryCode = item.User.Location?.Country?.Code?.ToLower() ?? string.Empty
					});
				}

				return new SpeedrunGame
				{
					Name = game.Name,
					Id = game.ID,
					Link = game.WebLink.AbsoluteUri,
					CoverLink = game.Assets.CoverMedium.Uri.AbsoluteUri,
					Moderators = moderators
				};
			}
			catch (Exception e)
			{
				return await Logger.SendToChannelAsync("SpeedrunCom.GetModeratorsAsync Error", e) as SpeedrunGame;
			}
		}

		public static async Task<SpeedrunGame> GetCategoriesAsync(string gamename)
		{
			try
			{
				var output = string.Empty;
				var game = _client.Games.SearchGame(gamename);
				if (game == null)
					return null;

				var categories = new List<SpeedrunGameCategory>();
				foreach (var item in game.Categories.ToArray())
				{
					categories.Add(new SpeedrunGameCategory
					{
						Name = item.Name,
						Id = item.ID,
						Type = (SpeedrunCategoryType)item.Type
					});
				}

				return new SpeedrunGame
				{
					Name = game.Name,
					Id = game.ID,
					Link = game.WebLink.AbsoluteUri,
					CoverLink = game.Assets.CoverMedium.Uri.AbsoluteUri,
					Categories = categories
				};
			}
			catch (Exception e)
			{
				return await Logger.SendToChannelAsync("SpeedrunCom.GetModeratorsAsync Error", e) as SpeedrunGame;
			}
		}

		public static Task<string> PlayerHasWorldRecord(string playername)
		{
			try
			{
				var player = default(User);
				if (_client.Users.GetUsers(playername).Any())
					player = _client.Users.GetUsers(playername).FirstOrDefault();
				else
					return Task.FromResult("Unknown name.");

				var pbs = default(Record[]);
				if (player.PersonalBests.Count > 0)
					pbs = player.PersonalBests.ToArray();
				else
					return Task.FromResult("Player doesn't have any personal records.");

				foreach (var record in pbs)
				{
					if (record.Rank != 1)
						continue;
					return Task.FromResult("Yes.");
				}
				return Task.FromResult("No.");
			}
			catch
			{
				return Task.FromResult("**Error**");
			}
		}

		public static async Task<SpeedrunGameLeaderboard> GetTopTenAsync(string name)
		{
			var game = _client.Games.SearchGame(name);
			if (game != null)
			{
				try
				{
					var category = game.FullGameCategories.FirstOrDefault(cat => (cat.Type == CategoryType.PerGame) && (cat.Runs.Any()));
					var temp = category.Leaderboard.Records;
					// I don't know how to make this faster...
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
						if (!(names.Contains(runs[i].Player.Name)))
						{
							rank++;
							if ((rank >= 2)
							&& (await IsTied(runs[i].Times.Primary.Value, runs[i - 1].Times.Primary.Value)))
								rank--;
							names.Add(runs[i].Player.Name);

							records.Add(new SpeedrunPlayerPersonalBest()
							{
								CategoryName = runs[i].Category.Name,
								PlayerName = runs[i].Player.Name,
								PlayerLocation = ((runs[i].Player.IsUser)
											  && (runs[i].Player.User.Location?.Country?.Code?.ToLower() != null))
													? $":flag_{runs[i].Player.User.Location.Country.Code.ToLower()}:"
													: string.Empty,
								PlayerRank = await TopTenFormat(rank.ToString(), false),
								EntryTime = await FormatTime(runs[i].Times.Primary.Value)
							});
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

		public static async Task<List<SpeedrunNotification>> GetLastNotificationAsync(string scount = "x", string nftype = null)
		{
			try
			{
				if (string.Equals(scount, "x", StringComparison.CurrentCultureIgnoreCase))
					scount = _maxnfcount.ToString();

				if (!(uint.TryParse(scount, out var count)))
					return null;

				nftype = nftype ?? "any";

				var json = await Fetching.GetStringAsync($"https://www.speedrun.com/api/v1/notifications?max={_maxnffetchcount}", _headers);

				// Read
				if (string.IsNullOrEmpty(json))
					return null;

				// Read json string
				dynamic api = JsonConvert.DeserializeObject(json);
				if (string.IsNullOrEmpty(api?.ToString()))
					return null;

				// Parse data
				var updates = new List<SpeedrunNotification>();
				foreach (var data in api.data)
				{
					if (updates.Count == count)
						break;

					var notification = new SpeedrunNotification()
					{
						CreationDate = data?.created.ToString() ?? string.Empty,
						ContentText = data?.text.ToString() ?? string.Empty,
						Type = (SpeedrunNotificationType)data?.item?.rel,
						ContentLink = data?.item?.uri?.ToString() ?? string.Empty,
						Status = data?.item?.status == "read"
													? SpeedrunNotificationStatus.Read
													: SpeedrunNotificationStatus.Unread
					};

					// Filtering
					if ((string.Equals(notification.Type.ToString(), nftype, StringComparison.CurrentCultureIgnoreCase))
					|| (nftype == "any"))
					{
						await notification.BuildGame();
						var game = _client.Games.SearchGame(notification.Game.Name);
						if (game != null)
						{
							notification.Game.Link = game?.WebLink?.AbsoluteUri ?? string.Empty;
							notification.Game.CoverLink = game?.Assets?.CoverSmall?.Uri?.AbsoluteUri ?? string.Empty;
						}
						await notification.BuildCache();
						updates.Add(notification);
					}
				}
				return updates;
			}
			catch (Exception e)
			{
				return await Logger.SendToChannelAsync("SpeedrunCom.GetLastNotificationAsync Error", e) as List<SpeedrunNotification>;
			}
		}

		public static async Task<List<SpeedrunNotification>> GetNotificationUpdatesAsync(uint count)
		{
			try
			{
				var json = await Fetching.GetStringAsync($"https://www.speedrun.com/api/v1/notifications?max={_maxnffetchcount}", _headers);

				// Read
				if (string.IsNullOrEmpty(json))
					return await Logger.SendToChannelAsync("SpeedrunCom.GetNotificationUpdatesAsync JSON Error", LogColor.Error) as List<SpeedrunNotification>;

				// Read json string
				dynamic api = JsonConvert.DeserializeObject(json);
				if (string.IsNullOrEmpty(api?.ToString()))
					return await Logger.SendToChannelAsync("SpeedrunCom.GetNotificationUpdatesAsync API Error", LogColor.Error) as List<SpeedrunNotification>;

				// Parse data
				var updates = new List<SpeedrunNotification>();
				foreach (var data in api.data)
				{
					if (updates.Count == count)
						break;

					var notification = new SpeedrunNotification()
					{
						CreationDate = data?.created.ToString() ?? string.Empty,
						ContentText = data?.text.ToString() ?? string.Empty,
						Type = (SpeedrunNotificationType)data?.item?.rel,
						ContentLink = data?.item?.uri?.ToString() ?? string.Empty,
						Status = (data?.item?.status == "read") ? SpeedrunNotificationStatus.Read : SpeedrunNotificationStatus.Unread
					};

					// Filtering
					//if (notification.Type != SpeedrunNotificationType.Post)
					//{
						// Eh, blame the guy who designed the website/api :c
						await notification.BuildGame();
						var game = _client.Games.SearchGame(notification.Game.Name);
						if (game != null)
						{
							notification.Game.Link = game?.WebLink?.AbsoluteUri ?? string.Empty;
							notification.Game.CoverLink = game?.Assets?.CoverSmall?.Uri?.AbsoluteUri ?? string.Empty;
						}
						await notification.BuildCache();
						updates.Add(notification);
					//}
				}
				return updates;
			}
			catch (Exception e)
			{
				return await Logger.SendToChannelAsync("SpeedrunCom.GetNotificationUpdatesAsync Error", e) as List<SpeedrunNotification>;
			}
		}

		public static async Task<SpeedrunGameRules> GetGameRulesAsync(string gamename, bool il = false)
		{
			var game = _client.Games.SearchGame(gamename);
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
						else
							return new SpeedrunGameRules();
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
							: output = $"#{rank}";
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