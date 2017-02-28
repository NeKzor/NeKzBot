using System.Threading.Tasks;
using Discord.Commands;
using NeKzBot.Resources;
using NeKzBot.Server;
using NeKzBot.Tasks.Leaderboard;

namespace NeKzBot.Modules.Public
{
	public class Leaderboard : Commands
	{
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Leaderboard Module", LogColor.Init);
			await GetLatestWorldRecord("latestwr");
			await GetCurrentWorldRecord("wr");
			await GetOwnRank("rank", "me", "pb");
			await GetUserRank("player");
			await GetLatestLeaderboardEntry("latestentry");
			await LeaderboardCommands(Configuration.Default.LeaderboardCmd);
		}

		private static Task GetLatestWorldRecord(string c)
		{
			CService.CreateCommand(c)
					.Alias("wrupdate")
					.Description($"Gives you the most recent world record. Try `{Configuration.Default.PrefixCmd + c} yt` to filter wrs by videos only or `{Configuration.Default.PrefixCmd + c} demo` to filter them by demos only.")
					.Parameter("filter", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (e.Args[0] == string.Empty)
							await e.Channel.SendMessage(await Portal2.GetLatestEntryAsync("http://board.iverb.me/changelog?wr=1"));
						else if (e.Args[0] == "yt")
							await e.Channel.SendMessage(await Portal2.GetLatestEntryAsync("http://board.iverb.me/changelog?wr=1&yt=1"));
						else if (e.Args[0] == "demo")
							await e.Channel.SendMessage(await Portal2.GetLatestEntryAsync("http://board.iverb.me/changelog?wr=1&demo=1"));
						else
							await e.Channel.SendMessage($"Unknown parameter. Try `{Configuration.Default.PrefixCmd + c} yt` or `{Configuration.Default.PrefixCmd + c} demo`.");
					});
			return Task.FromResult(0);
		}

		private static Task GetLatestLeaderboardEntry(string c)
		{
			CService.CreateCommand(c)
					.Alias("entry")
					.Description($"Gives you the most recent leaderboard entry. Try `{Configuration.Default.PrefixCmd + c} yt` to filter entries by videos only or `{Configuration.Default.PrefixCmd + c} demo` to filter by demos only.")
					.Parameter("filter", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (e.Args[0] == string.Empty)
							await e.Channel.SendMessage(await Portal2.GetLatestEntryAsync("http://board.iverb.me/changelog"));
						else if (e.Args[0] == "yt")
							await e.Channel.SendMessage(await Portal2.GetLatestEntryAsync("http://board.iverb.me/changelog?yt=1"));
						else if (e.Args[0] == "demo")
							await e.Channel.SendMessage(await Portal2.GetLatestEntryAsync("http://board.iverb.me/changelog?demo=1"));
						else
							await e.Channel.SendMessage($"Unknown parameter. Try `{Configuration.Default.PrefixCmd + c} yt` or `{Configuration.Default.PrefixCmd + c} demo`.");
					});
			return Task.FromResult(0);
		}

		private static Task GetCurrentWorldRecord(string c)
		{
			CService.CreateCommand(c)
					.Description($"Shows you the latest world record of a map. Try `{Configuration.Default.PrefixCmd + c}` to show a random wr entry.")
					.Parameter("mapname", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (e.Args[0] == string.Empty)
							await e.Channel.SendMessage(await Portal2.GetLatestEntryAsync("http://board.iverb.me/changelog?wr=1&chamber=" + await Utils.RNGAsync(Data.Portal2Maps, 0)));
						else if (await Utils.SearchArray(Data.Portal2Maps, 2, e.Args[0], out var index))
							await e.Channel.SendMessage(await Portal2.GetLatestEntryAsync("http://board.iverb.me/changelog?wr=1&chamber=" + Data.Portal2Maps[index, 0]));
						else if (await Utils.SearchArray(Data.Portal2Maps, 3, e.Args[0], out index))
							await e.Channel.SendMessage(await Portal2.GetLatestEntryAsync("http://board.iverb.me/changelog?wr=1&chamber=" + Data.Portal2Maps[index, 0]));
						else if (await Utils.SearchArray(Data.Portal2Maps, 5, e.Args[0], out index))
							await e.Channel.SendMessage(await Portal2.GetLatestEntryAsync("http://board.iverb.me/changelog?wr=1&chamber=" + Data.Portal2Maps[index, 0]));
						else
							await e.Channel.SendMessage($"Couldn't find that map. Try `{Configuration.Default.PrefixCmd + c}` with one of these:\n{await Utils.ArrayToList(Data.Portal2Maps, 5)}");    // List all maps lmao
					});
			return Task.FromResult(0);
		}

		private static Task GetOwnRank(string c, params string[] a)
		{
			CService.CreateCommand(c)
					.Alias(a)
					.Description($"Shows your leaderboard stats. Try `{Configuration.Default.PrefixCmd + c} <mapname>` to show your personal best of a map.")
					.Parameter("mapname", ParameterType.Unparsed)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (e.Args[0] == string.Empty)
							await e.Channel.SendMessage(await Portal2.GetUserStatsAsync("http://board.iverb.me/profile/" + e.User.Name));
						else if (await Utils.SearchArray(Data.Portal2Maps, 2, e.Args[0], out var index))
							await e.Channel.SendMessage(await Portal2.GetUserRankAsync("http://board.iverb.me/profile/" + e.User.Name, index));
						else if (await Utils.SearchArray(Data.Portal2Maps, 3, e.Args[0], out index))
							await e.Channel.SendMessage(await Portal2.GetUserRankAsync("http://board.iverb.me/profile/" + e.User.Name, index));
						else if (await Utils.SearchArray(Data.Portal2Maps, 5, e.Args[0], out index))
							await e.Channel.SendMessage(await Portal2.GetUserRankAsync("http://board.iverb.me/profile/" + e.User.Name, index));
						else
							await e.Channel.SendMessage($"Couldn't find that map. Try `{Configuration.Default.PrefixCmd + c} <mapname>` with one of these:\n{await Utils.ArrayToList(Data.Portal2Maps, 5)}");
					});
			return Task.FromResult(0);
		}

		private static Task GetUserRank(string c)
		{
			CService.CreateCommand(c)
					.Description($"Shows leaderboard stats about that player. Try `{Configuration.Default.PrefixCmd + c} <playername> <mapname>` to show the ranking of a specific map. This `{Configuration.Default.PrefixCmd + c} <steamid>` would also work.")
					.Parameter("playername", ParameterType.Multiple)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (e.Args.Length == 0)
							await e.Channel.SendMessage($"Invalid parameter. Try `{Configuration.Default.PrefixCmd}{c} <playername>` or `{Configuration.Default.PrefixCmd}help {c}` for more information.");
						else if (e.Args.Length > 1)
						{
							if (await Utils.SearchArray(Data.Portal2Maps, 2, await Utils.GetRest(e.Args, 1, e.Args.Length), out var index))
								await e.Channel.SendMessage(await Portal2.GetUserRankAsync("http://board.iverb.me/profile/" + e.Args[0], index));
							else if (await Utils.SearchArray(Data.Portal2Maps, 3, await Utils.GetRest(e.Args, 1, e.Args.Length), out index))
								await e.Channel.SendMessage(await Portal2.GetUserRankAsync("http://board.iverb.me/profile/" + e.Args[0], index));
							else if (await Utils.SearchArray(Data.Portal2Maps, 5, await Utils.GetRest(e.Args, 1, e.Args.Length), out index))
								await e.Channel.SendMessage(await Portal2.GetUserRankAsync("http://board.iverb.me/profile/" + e.Args[0], index));
							else
								await e.Channel.SendMessage($"Couldn't find that map. Try `{Configuration.Default.PrefixCmd + c} <mapname>` with one of these:\n{await Utils.ArrayToList(Data.Portal2Maps, 5)}");
						}
						else
							await e.Channel.SendMessage(await Portal2.GetUserStatsAsync("http://board.iverb.me/profile/" + e.Args[0]));
					});
			return Task.FromResult(0);
		}

		private static Task LeaderboardCommands(string c)
		{
			CService.CreateGroup(c, GBuilder =>
			{
				GBuilder.CreateCommand("refreshtime")
						.Alias("rt")
						.Description("Shows when the bot will check for the next update.")
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage(await Portal2.AutoUpdater.GetRefreshTime());
						});

				#region SETTINGS
				GBuilder.CreateCommand("setrefreshtime")
						.Alias("setrt")
						.Description("Sets the refresh update time of leaderboard updater.")
						.Parameter("time", ParameterType.Required)
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage(await Portal2.AutoUpdater.SetResfreshTimeAsync(e.Args[0]));
						});

				GBuilder.CreateCommand("updatestate")
						.Alias("us")
						.Description("Sets the updating state of the leaderboard auto updater.")
						.Parameter("state", ParameterType.Required)
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage(await Portal2.AutoUpdater.SetAutoUpdateState(e.Args[0]));
						});

				GBuilder.CreateCommand("boardparameter")
						.Alias("bp")
						.Description("Sets a new parameter for the leaderboard auto updater.")
						.Parameter("name", ParameterType.Required)
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage(await Portal2.AutoUpdater.SetNewBoardParameterAsync(e.Args[0]));
						});
				#endregion

				#region ACTIONS
				GBuilder.CreateCommand("toggleupdate")
						.Alias("tu")
						.Description("Enables or disables the update channel where the bot writes his updates.")
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage(await Portal2.AutoUpdater.ToggleUpdateAsync());
						});

				GBuilder.CreateCommand("refreshnow")
						.Alias("refresh")
						.Description("Stops the auto updater task and starts a new one.")
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage(await Portal2.AutoUpdater.RefreshNowAsync());
						});

				GBuilder.CreateCommand("cleanentrycache")
						.Alias("cleanentry")
						.Description("Clears entry cache which is used to compare old data with new one when checking for an update.")
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage(await Portal2.AutoUpdater.CleanEntryCacheAsync());
						});
				#endregion

				#region CACHING
				GBuilder.CreateCommand("cachetime")
						.Alias("ct")
						.Description("Shows you when the bot clears all data about leaderboard entries and stats.")
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage(await Portal2.Cache.GetCleanCacheTime());
						});

				GBuilder.CreateCommand("setcachetime")
						.Alias("setct")
						.Description("Sets a new time when the bot will clean the leaderboard cache.")
						.Parameter("value", ParameterType.Required)
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage(await Portal2.Cache.SetCleanCacheTimeAsync(e.Args[0]));
						});
				#endregion
			});
			return Task.FromResult(0);
		}
	}
}