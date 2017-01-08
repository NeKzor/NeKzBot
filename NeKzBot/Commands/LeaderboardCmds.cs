using System.Linq;
using Discord.Commands;
using NeKzBot.Properties;

namespace NeKzBot
{
	public class LeaderboardCmds : Commands
	{
		public static void Load()
		{
			Logging.CON("Loading leaderboard commands", System.ConsoleColor.DarkYellow);
			WorldRecordUpdate("latestwr");          // !lastestwr <mapname>
			GetCurrentWorldRecord("wr");            // !wr <yt, demo>
			GetOwnRank("rank", "me", "pb");         // !rank, !me <mapname>
			GetUserRank("player");                  // !player <name> <mapname>
			LeaderboardUpdate("latestentry");       // !latestentry, !entry

			// Special roles only !lb <cmd>
			LeaderboardCommands(Settings.Default.LeaderboardCmd);
		}

		private static void WorldRecordUpdate(string c)
		{
			cmd.CreateCommand(c)
			.Alias("wrupdate")
			.Description($"**-** `{Settings.Default.PrefixCmd + c}` gives you the most recent world record.\n**-** `{Settings.Default.PrefixCmd + c} yt` filters wrs by videos only.\n**-** `{Settings.Default.PrefixCmd + c} demo` filters wrs by demos only.\n**-** Data is from board.iverb.me.")
			.Parameter("p", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				if (e.Args[0] == string.Empty)
					await e.Channel.SendMessage(await Leaderboard.GetLatestEntry("http://board.iverb.me/changelog?wr=1"));
				else if (e.Args[0] == "yt")
					await e.Channel.SendMessage(await Leaderboard.GetLatestEntry("http://board.iverb.me/changelog?wr=1&yt=1"));
				else if (e.Args[0] == "demo")
					await e.Channel.SendMessage(await Leaderboard.GetLatestEntry("http://board.iverb.me/changelog?wr=1&demo=1"));
				else
					await e.Channel.SendMessage($"Unknow parameter. Try `{Settings.Default.PrefixCmd + c} yt` or `{Settings.Default.PrefixCmd + c} demo`");
			});
		}

		private static void LeaderboardUpdate(string c)
		{
			cmd.CreateCommand(c)
			.Alias("entry")
			.Description($"**-** `{Settings.Default.PrefixCmd + c}` gives you the most recent leaderboard entry.\n**-** `{Settings.Default.PrefixCmd + c} yt` filters entries by videos only.\n**-** `{Settings.Default.PrefixCmd + c} demo` filters entries by demos only.\n**-** Data is from board.iverb.me.")
			.Parameter("p", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				if (e.Args[0] == string.Empty)
					await e.Channel.SendMessage(await Leaderboard.GetLatestEntry("http://board.iverb.me/changelog"));
				else if (e.Args[0] == "yt")
					await e.Channel.SendMessage(await Leaderboard.GetLatestEntry("http://board.iverb.me/changelog?yt=1"));
				else if (e.Args[0] == "demo")
					await e.Channel.SendMessage(await Leaderboard.GetLatestEntry("http://board.iverb.me/changelog?demo=1"));
				else
					await e.Channel.SendMessage($"Unknow parameter. Try `{Settings.Default.PrefixCmd + c} yt` or `{Settings.Default.PrefixCmd + c} demo`");
			});
		}

		private static void GetCurrentWorldRecord(string c)
		{
			cmd.CreateCommand(c)
			.Description($"**-** `{Settings.Default.PrefixCmd + c} <mapname>` shows you the latest world record of a map.\n**-** `{Settings.Default.PrefixCmd + c}` shows you a random wr entry.\n**-** Data is from board.iverb.me.")
			.Parameter("p", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				int index;
				if (e.Args[0] == string.Empty)
					await e.Channel.SendMessage(await Leaderboard.GetLatestEntry("http://board.iverb.me/changelog?wr=1&chamber=" + Data.portal2Maps[Utils.RNG(Data.portal2Maps.GetLength(0)), 0]));
				else if (Utils.SearchArray(Data.portal2Maps, 2, e.Args[0], out index))
					await e.Channel.SendMessage(await Leaderboard.GetLatestEntry("http://board.iverb.me/changelog?wr=1&chamber=" + Data.portal2Maps[index, 0]));
				else if (Utils.SearchArray(Data.portal2Maps, 3, e.Args[0], out index))
					await e.Channel.SendMessage(await Leaderboard.GetLatestEntry("http://board.iverb.me/changelog?wr=1&chamber=" + Data.portal2Maps[index, 0]));
				else if (Utils.SearchArray(Data.portal2Maps, 5, e.Args[0], out index))
					await e.Channel.SendMessage(await Leaderboard.GetLatestEntry("http://board.iverb.me/changelog?wr=1&chamber=" + Data.portal2Maps[index, 0]));
				else
					await e.Channel.SendMessage($"Couldn't find that map. Try `{Settings.Default.PrefixCmd + c}` with one of these:\n{Utils.ArrayToList(Data.portal2Maps, 5)}");    // List all maps lmao
			});
		}

		private static void GetOwnRank(string c, params string[] a)
		{
			cmd.CreateCommand(c)
			.Alias(a)
			.Description($"**-** `{Settings.Default.PrefixCmd + c}` shows your leaderboard stats.\n**-** `" + c + " <mapname>` shows your personal best of a map.\n**-** Data is from board.iverb.me.")
			.Parameter("p", ParameterType.Unparsed)
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				int index;
				if (e.Args[0] == string.Empty)
					await e.Channel.SendMessage(await Leaderboard.GetUserStats("http://board.iverb.me/profile/" + e.User.Name));
				else if (Utils.SearchArray(Data.portal2Maps, 2, e.Args[0], out index))
					await e.Channel.SendMessage(await Leaderboard.GetUserRank("http://board.iverb.me/profile/" + e.User.Name, index));
				else if (Utils.SearchArray(Data.portal2Maps, 3, e.Args[0], out index))
					await e.Channel.SendMessage(await Leaderboard.GetUserRank("http://board.iverb.me/profile/" + e.User.Name, index));
				else if (Utils.SearchArray(Data.portal2Maps, 5, e.Args[0], out index))
					await e.Channel.SendMessage(await Leaderboard.GetUserRank("http://board.iverb.me/profile/" + e.User.Name, index));
				else
					await e.Channel.SendMessage($"Couldn't find that map. Try `{Settings.Default.PrefixCmd + c} <mapname>` with one of these:\n{Utils.ArrayToList(Data.portal2Maps, 5)}");
			});
		}

		private static void GetUserRank(string c)
		{
			cmd.CreateCommand(c)
			.Description($"**-** `{Settings.Default.PrefixCmd + c} <playername>` shows leaderboard stats about that player.\n**-** `{Settings.Default.PrefixCmd + c} <playername> <mapname>` shows the ranking of a specific map.\n**-** `{Settings.Default.PrefixCmd + c} <steamid>` would also work as parameter.\n**-** Data is from board.iverb.me.")
			.Parameter("p", ParameterType.Multiple)
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				int index;
				try
				{
					if (e.Args.Count() > 1)
						if (Utils.SearchArray(Data.portal2Maps, 2, Utils.GetRest(e.Args, 1, e.Args.Count()), out index))
							await e.Channel.SendMessage(await Leaderboard.GetUserRank("http://board.iverb.me/profile/" + e.Args[0], index));
						else if (Utils.SearchArray(Data.portal2Maps, 3, Utils.GetRest(e.Args, 1, e.Args.Count()), out index))
							await e.Channel.SendMessage(await Leaderboard.GetUserRank("http://board.iverb.me/profile/" + e.Args[0], index));
						else if (Utils.SearchArray(Data.portal2Maps, 5, Utils.GetRest(e.Args, 1, e.Args.Count()), out index))
							await e.Channel.SendMessage(await Leaderboard.GetUserRank("http://board.iverb.me/profile/" + e.Args[0], index));
						else
							await e.Channel.SendMessage($"Couldn't find that map. Try `{Settings.Default.PrefixCmd + c} <mapname>` with one of these:\n{Utils.ArrayToList(Data.portal2Maps, 5)}");
					else
						await e.Channel.SendMessage(await Leaderboard.GetUserStats("http://board.iverb.me/profile/" + e.Args[0]));
				}
				catch
				{
					await e.Channel.SendMessage($"Invalid parameter. Try `{Settings.Default.PrefixCmd + c} <playername>` or `{Settings.Default.PrefixCmd}help {c}` for more information.");
				}
			});
		}

		private static void LeaderboardCommands(string c)
		{
			cmd.CreateGroup(c, g =>
			{
				g.CreateCommand("refreshtime")
				.Alias("rt")
				.Description($"**-** `{Settings.Default.PrefixCmd + c} refreshtime` shows when the bot will refresh the channel.")
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					if (Utils.RoleCheck(e.User, Settings.Default.AllowedRoles))
						await e.Channel.SendMessage(Leaderboard.AutoUpdater.GetRefreshTime());
					else
						await e.Channel.SendMessage(Data.rolesMsg);
				});

				#region SETTINGS
				g.CreateCommand("setrefreshtime")
				.Alias("setrt")
				.Description($"**-** `{Settings.Default.PrefixCmd + c} setrefreshtime <time>` sets the refresh update time of the *" + Settings.Default.UpdateChannelName + "* channel.")
				.Parameter("p", ParameterType.Required)
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					if (Utils.RoleCheck(e.User, Settings.Default.AllowedRoles))
						await e.Channel.SendMessage(Leaderboard.AutoUpdater.SetResfreshTime(e.Args[0]));
					else
						await e.Channel.SendMessage(Data.rolesMsg);
				});

				g.CreateCommand("setchannel")
				.Alias("sc")
				.Description($"**-** `{Settings.Default.PrefixCmd + c} setchannel <name>` sets a new name for the channel where the bot writes his updates.\n**-** Current channel is: *" + Settings.Default.UpdateChannelName + "*.")
				.Parameter("p", ParameterType.Required)
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					if (Utils.RoleCheck(e.User, Settings.Default.AllowedRoles))
						await e.Channel.SendMessage(Leaderboard.AutoUpdater.SetUpdateChannel(e.Args[0]));
					else
						await e.Channel.SendMessage(Data.rolesMsg);
				});

				g.CreateCommand("updatestate")
				.Alias("us")
				.Parameter("p", ParameterType.Required)
				.Description($"**-** `{Settings.Default.PrefixCmd + c} updatestate <state>` sets the updating state of the leaderboard auto updater.")
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					if (Utils.RoleCheck(e.User, Settings.Default.AllowedRoles))
						await e.Channel.SendMessage(Leaderboard.AutoUpdater.SetAutoUpdateState(e.Args[0]));
					else
						await e.Channel.SendMessage(Data.rolesMsg);
				});

				g.CreateCommand("boardparameter")
				.Alias("bp")
				.Description($"**-** `{Settings.Default.PrefixCmd + c} boardparameter <name>` sets a new parameter for the leaderboard auto updater.")
				.Parameter("p", ParameterType.Required)
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					if (Utils.RoleCheck(e.User, Settings.Default.AllowedRoles))
						await e.Channel.SendMessage(Leaderboard.AutoUpdater.SetNewBoardParameter(e.Args[0]));
					else
						await e.Channel.SendMessage(Data.rolesMsg);
				});
				#endregion

				#region ACTIONS
				g.CreateCommand("toggleupdate")
				.Alias("tu")
				.Description($"**-** `{Settings.Default.PrefixCmd + c} update` enables update channel where the bot writes his updates.")
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					if (Utils.RoleCheck(e.User, Settings.Default.AllowedRoles))
						await e.Channel.SendMessage(Leaderboard.AutoUpdater.ToggleUpdate());
					else
						await e.Channel.SendMessage(Data.rolesMsg);
				});

				g.CreateCommand("refreshnow")
				.Alias("refresh")
				.Description($"**-** `{Settings.Default.PrefixCmd + c} refreshnow` refreshes the update channel where the bot writes his updates.")
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					if (Utils.RoleCheck(e.User, Settings.Default.AllowedRoles))
						await e.Channel.SendMessage(Leaderboard.AutoUpdater.RefreshNow());
					else
						await e.Channel.SendMessage(Data.rolesMsg);
				});

				g.CreateCommand("cleanentrycache")
				.Alias("cleanentry")
				.Description($"**-** `{Settings.Default.PrefixCmd + c} cleanentrycache` clears entry cache which is used to compare old data with new one when checking for an update.")
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					if (Utils.RoleCheck(e.User, Settings.Default.AllowedRoles))
						await e.Channel.SendMessage(Leaderboard.AutoUpdater.CleanEntryCache());
					else
						await e.Channel.SendMessage(Data.rolesMsg);
				});
				#endregion

				#region CACHING
				g.CreateCommand("cachetime")
				.Alias("ct")
				.Description($"**-** `{Settings.Default.PrefixCmd + c} cachetime` shows you when the bot clears all data about leaderboard entries and stats.")
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					if (Utils.RoleCheck(e.User, Settings.Default.AllowedRoles))
						await e.Channel.SendMessage(Leaderboard.Cache.GetCleanCacheTime());
					else
						await e.Channel.SendMessage(Data.rolesMsg);
				});

				g.CreateCommand("setcachetime")
				.Alias("setct")
				.Description($"**-** `{Settings.Default.PrefixCmd + c} setcachetime <value>` sets a new time when the bot will clean the leaderboard cache.")
				.Parameter("p", ParameterType.Required)
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					if (Utils.RoleCheck(e.User, Settings.Default.AllowedRoles))
						await e.Channel.SendMessage(Leaderboard.Cache.SetCleanCacheTime(e.Args[0]));
					else
						await e.Channel.SendMessage(Data.rolesMsg);
				});
				#endregion
			});
		}
	}
}