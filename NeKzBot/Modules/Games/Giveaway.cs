using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NeKzBot.Server;

namespace NeKzBot
{
	// TODO: make this somehow more interesting?
	public class GiveawayGame : Commands
	{
		private static Stopwatch nextReset = new Stopwatch();
		private static CancellationTokenSource cancelResetSource;
		private static CancellationToken cancelToken;
		private static string cacheKey;

		private const int N = 9;

		public static void Load()
		{
			Logging.CON("Loading giveaway game", ConsoleColor.DarkYellow);
			var c = "giveaway";
			cacheKey = "gg";
			GetGiveaway(c);
			GetGiveawayCommands(c);
		}

		private static void GetGiveaway(string c)
		{
			cmd.CreateCommand(c)
			.Description($"**-** `{Settings.Default.PrefixCmd + c}` could give you a prize.\n**-** Algorithm: User requests random number, bot checks if number equals value[index], if it does index++, tries--, try again, if index == value.length, puzzle solved.\n**-** Solve status and value (code to solve) are hidden.\n**-** Good luck!\n**-** This isn't always available.")
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				if (Settings.Default.GiveawayEnabled)
				{
					if (HasTriesLeft(e.User))
					{
						if (IsSolved())
						{
							await e.Channel.SendMessage("Ayy, congrats :grinning:");
							await e.User.SendMessage($"Your prize: {Credentials.Default.GiveawayPrizeKey}");
							if (!Reset().IsCompleted)
								cancelResetSource.Cancel();
							Logging.CON("giveaway solved", ConsoleColor.DarkCyan);
						}
						else
						{
							await e.Channel.SendMessage("Try again.");
							Logging.CON($"game giveaway status : {GetPuzzleStatus()}", ConsoleColor.DarkCyan);
						}
					}
					else
						await e.Channel.SendMessage($"Out of attempts. Try in {((int)((double)Settings.Default.GiveawayResetTime / (double)3600000) - nextReset.Elapsed.Hours).ToString()}h again.");
				}
				else
					await e.Channel.SendMessage("Giveaway is not available at the moment or the puzzle has been solved.");
			});
		}

		private static void GetGiveawayCommands(string c)
		{
			cmd.CreateGroup(c, g =>
			{
				g.CreateCommand("resetwhen")
				.Alias("when")
				.Description($"**-** `{Settings.Default.PrefixCmd + c} resetwhen` shows you when the bot will forget your attempts.")
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					if (Settings.Default.GiveawayEnabled)
						await e.Channel.SendMessage(NextResetStatus());
					else
						await e.Channel.SendMessage("There's currently no giveaway available.");
				});

				#region ADMIN ONLY
				g.CreateCommand("resettime")
				.Alias("time")
				.Description($"**-** `{Settings.Default.PrefixCmd + c} resettime` will set a new reset time for the giveaway.")
				.Parameter("p", Discord.Commands.ParameterType.Required)
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					if (Utils.RoleCheck(e.User, Settings.Default.AllowedRoles))
					{
						if (!Settings.Default.GiveawayEnabled)
							await e.Channel.SendMessage("There's currently no giveaway available.");
						else if (SetNewResetTime(Convert.ToInt16(e.Args[0])))
							await e.Channel.SendMessage($"New reset time is set to: **{e.Args[0]}min**");
						else
							await e.Channel.SendMessage("Invalid paramter. Time is in minutes.");
					}
					else
						await e.Channel.SendMessage(Data.rolesMsg);
				});

				g.CreateCommand("maxtries")
				.Alias("tries")
				.Description($"**-** `{Settings.Default.PrefixCmd + c} maxtries` will set the amount of tries for each user.")
				.Parameter("p", Discord.Commands.ParameterType.Required)
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					if (Utils.RoleCheck(e.User, Settings.Default.AllowedRoles))
					{
						if (!Settings.Default.GiveawayEnabled)
							await e.Channel.SendMessage("There's currently no giveaway available.");
						else if (SetNewMaxTries(Convert.ToInt16(e.Args[0])))
							await e.Channel.SendMessage($"New max tries is set to: *{e.Args[0]} tries**");
						else
							await e.Channel.SendMessage("Invalid paramter.");
					}
					else
						await e.Channel.SendMessage(Data.rolesMsg);
				});

				g.CreateCommand("resetnow")
				.Alias("reset")
				.Description($"**-** `{Settings.Default.PrefixCmd + c} resetnow` resets the waiting time when you're out of attempts of the give away.")
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					if (Utils.RoleCheck(e.User, Settings.Default.AllowedRoles))
					{
						if (!Settings.Default.GiveawayEnabled)
							await e.Channel.SendMessage("There's currently no giveaway available.");
						else
							await e.Channel.SendMessage(await ResetNow());
					}
					else
						await e.Channel.SendMessage(Data.rolesMsg);
				});

				g.CreateCommand("togglereset")
				.Alias("toggle")
				.Description($"**-** `{Settings.Default.PrefixCmd + c} togglereset` toggles the reset timer for the give away attempts.")
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					if (Utils.RoleCheck(e.User, Settings.Default.AllowedRoles))
						await e.Channel.SendMessage(await ToggleTimeReset());
					else
						await e.Channel.SendMessage(Data.rolesMsg);
				});

				g.CreateCommand("setstate")
				.Alias("state")
				.Description($"**-** `{Settings.Default.PrefixCmd + c} setstate <state>` enables or disables the giveaway game.")
				.Parameter("p", Discord.Commands.ParameterType.Required)
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					if (Utils.RoleCheck(e.User, Settings.Default.AllowedRoles))
						await e.Channel.SendMessage(SetGiveawayState(e.Args[0]));
					else
						await e.Channel.SendMessage(Data.rolesMsg);
				});

				g.CreateCommand("status")
				.Alias("debug")
				.Description($"**-** `{Settings.Default.PrefixCmd + c} status` will log some information about the giveaway.")
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					if (Utils.RoleCheck(e.User, Settings.Default.AllowedRoles))
					{
						Logging.CON(
							$"\ngiveaway status : {Settings.Default.GiveawayEnabled.ToString()}"
							+ $"\nmax tries : {Settings.Default.GiveawayMaxTries.ToString()}"
							//+ $"\nunlocked index : {Settings.Default.UnlockedIndex.ToString()}"
							+ $"\ncode length : {Settings.Default.GiveawayCode.Length.ToString()}"
							+ $"\nreset watch : {nextReset.Elapsed.Milliseconds.ToString()}"
							+ $"\nreset time : {Settings.Default.GiveawayResetTime.ToString()}\n", ConsoleColor.DarkCyan);
						await e.Channel.SendMessage("Status about giveaway has been logged (server-side).");
					}
					else
						await e.Channel.SendMessage(Data.rolesMsg);
				});
				#endregion
			});
		}

		#region ACTIONS
		public static async Task Reset()
		{
			try
			{
				cancelResetSource = new CancellationTokenSource();
				cancelToken = cancelResetSource.Token;

				while (Settings.Default.GiveawayEnabled)
				{
					Caching.CApplication.Save(cacheKey, new Dictionary<Discord.User, int>());
					//people = null;
					nextReset = Stopwatch.StartNew();
					Logging.CON("Giveaway reset", ConsoleColor.DarkCyan);
					await Task.Delay((int)Settings.Default.GiveawayResetTime, cancelToken);
				}
			}
			catch
			{
				Logging.CON("Giveaway ended", ConsoleColor.DarkCyan);
			}
		}

		public static string SetGiveawayState(string state)
		{
			if (state.ToLower() == "toggle")
				Settings.Default.GiveawayEnabled = !Settings.Default.GiveawayEnabled;
			else if (state.ToLower() == "true")
				Settings.Default.GiveawayEnabled = true;
			else if (state.ToLower() == "false")
				Settings.Default.GiveawayEnabled = false;
			else
				return "Invalid state.";
			Settings.Default.Save();
			return $"Giveaway state is set to: {Settings.Default.GiveawayEnabled.ToString()}";
		}

		public static async Task<string> ToggleTimeReset()
		{
			Logging.CON("Requested giveaway reset change...", ConsoleColor.DarkCyan);
			if (cancelResetSource.IsCancellationRequested || Reset().IsCompleted)
			{
				await Task.Factory.StartNew(async () =>
				{
					await Reset();
				});
				return "TimeReset started.";
			}
			cancelResetSource.Cancel();
			return "TimeReset cancelled.";
		}

		public static bool SetNewResetTime(int t)
		{
			// In minutes
			if (t < 1 || t > 1440)
				return false;
			Settings.Default.RefreshTime = (uint)(t * 1000 * 60);   // Why did I set it to ms lol
			Settings.Default.Save();
			return true;
		}

		public static bool SetNewMaxTries(int t)
		{
			if (t < 1 || t > 999)
				return false;
			Settings.Default.GiveawayMaxTries = (uint)t;
			Settings.Default.Save();
			return true;
		}

		public static async Task<string> ResetNow()
		{
			Logging.CON("Requested giveaway reset", ConsoleColor.DarkCyan);
			if (!cancelResetSource.IsCancellationRequested && !Reset().IsCompleted)
			{
				cancelResetSource.Cancel();
				await Task.Factory.StartNew(async () =>
				{
					await Reset();
				});
				return "Done.";
			}
			return "Cannot reset.";
		}
		#endregion

		#region PUZZLE ALGORITHM
		private static bool HasTriesLeft(Discord.User u)
		{
			// Get cache
			var cache = (Dictionary<Discord.User, int>)Caching.CApplication.Get(cacheKey)[0];

			// Check if new user
			bool yes = true;
			bool found = false;

			// Find user
			foreach (var item in cache)
			{
				if (item.Key.Id != u.Id)
					continue;
				found = true;
				break;
			}

			if (found)
			{
				// Check if user has tries left
				if (cache[u] > 0)
				{
					cache[u] = cache[u] - 1;
					Logging.CHA($"{u.Name} : {cache[u].ToString()}/{Settings.Default.GiveawayMaxTries.ToString()}", ConsoleColor.DarkCyan);
				}
				else
					yes = false;
			}
			else
			{
				cache.Add(u, (int)Settings.Default.GiveawayMaxTries - 1);
				Logging.CHA($"new : {u.Name} : {cache[u].ToString()}/{Settings.Default.GiveawayMaxTries.ToString()}", ConsoleColor.DarkCyan);
			}

			// Save cache
			Caching.CApplication.Save(cacheKey, cache);
			cache = null;
			return yes;
		}

		private static bool IsSolved()
		{
			// Get cache
			var cache = Convert.ToUInt16(Caching.CFile.Get(cacheKey));

			// New try
			if (Utils.RNG(N) == Convert.ToInt16(Settings.Default.GiveawayCode[(int)cache].ToString()))
				cache++;
			else
				return false;

			// Don't forget to save
			Caching.CFile.Save(cacheKey, cache.ToString());

			// Check if solved
			if (cache != (uint)Settings.Default.GiveawayCode.Length)
				return false;

			// Solved and disable
			Settings.Default.GiveawayEnabled = false;
			Settings.Default.Save();
			return true;
		}
		#endregion

		private static string GetPuzzleStatus() =>
			$"{((((double)Convert.ToUInt16(Caching.CFile.Get(cacheKey)) / (double)Settings.Default.GiveawayCode.Length) * 100)).ToString()}%";

		private static string NextResetStatus()
		{
			var h = (int)((double)Settings.Default.GiveawayResetTime / (double)3600000) - nextReset.Elapsed.Hours;
			if (h < 1)
				return "Will reset soon.";
			if (h == 1)
				return "Will reset in 1 hour.";
			return $"Will reset in {h.ToString()} hours.";
		}
	}
}