using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using NeKzBot.Server;
using NeKzBot.Resources;

namespace NeKzBot.Modules.Games
{
	// TODO: make this somehow more interesting?
	public class GiveawayGame : Commands
	{
		public static bool isRunning = false;
		private static Stopwatch nextReset = new Stopwatch();
		private static CancellationTokenSource cancelResetSource;
		private static CancellationToken cancelToken;
		private static string cacheKey;

		private const int N = 9;

		public static async Task Load()
		{
			await Logging.CON("Loading giveaway game", ConsoleColor.DarkYellow);
			var c = "giveaway";
			cacheKey = "gg";
			await GetGiveaway(c);
			await GetGiveawayCommands(c);
		}

		private static Task GetGiveaway(string c)
		{
			cmd.CreateCommand(c)
			.Description($"**-** `{Settings.Default.PrefixCmd + c}` could give you a prize.\n**-** Algorithm: User requests random number, bot checks if number equals value[index], if it does index++, tries--, try again, if index == value.length, puzzle solved.\n**-** Solve status and value (code to solve) are hidden.\n**-** Good luck!\n**-** This isn't always available.")
			.AddCheck(Permission.MainServerOnly)
			.Do(async (e) =>
			{
				await e.Channel.SendIsTyping();
				if (Settings.Default.GiveawayEnabled)
				{
					if (await HasTriesLeft(e.User))
					{
						if (await IsSolved())
						{
							await e.Channel.SendMessage("Ayy, congrats :grinning:");
							await e.User.SendMessage($"Your prize: {Credentials.Default.GiveawayPrizeKey}");
							if (!Reset().IsCompleted)
								cancelResetSource.Cancel();
							await Logging.CON("giveaway solved", ConsoleColor.DarkCyan);
						}
						else
						{
							await e.Channel.SendMessage("Try again.");
							await Logging.CON($"game giveaway status : {await GetPuzzleStatus()}", ConsoleColor.DarkCyan);
						}
					}
					else
						await e.Channel.SendMessage($"Out of attempts. Try in {((int)((double)Settings.Default.GiveawayResetTime / (double)3600000) - nextReset.Elapsed.Hours).ToString()}h again.");
				}
				else
					await e.Channel.SendMessage("Giveaway is not available at the moment or the puzzle has been solved.");
			});
			return Task.FromResult(0);
		}

		private static Task GetGiveawayCommands(string c)
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
						await e.Channel.SendMessage(await NextResetStatus());
					else
						await e.Channel.SendMessage("There's currently no giveaway available.");
				});

				#region BOT OWNER ONLY
				g.CreateCommand("resettime")
				.Alias("time")
				.Description($"**-** `{Settings.Default.PrefixCmd + c} resettime` will set a new reset time for the giveaway.")
				.Parameter("p", Discord.Commands.ParameterType.Required)
				.AddCheck(Permission.BotOwnerOnly).Hide()
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					if (!Settings.Default.GiveawayEnabled)
						await e.Channel.SendMessage("There's currently no giveaway available.");
					else if (SetNewResetTime(Convert.ToInt16(e.Args[0])))
						await e.Channel.SendMessage($"New reset time is set to: **{e.Args[0]}min**");
					else
						await e.Channel.SendMessage("Invalid paramter. Time is in minutes.");
				});

				g.CreateCommand("maxtries")
				.Alias("tries")
				.Description($"**-** `{Settings.Default.PrefixCmd + c} maxtries` will set the amount of tries for each user.")
				.Parameter("p", Discord.Commands.ParameterType.Required)
				.AddCheck(Permission.BotOwnerOnly).Hide()
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					if (!Settings.Default.GiveawayEnabled)
						await e.Channel.SendMessage("There's currently no giveaway available.");
					else if (SetNewMaxTries(Convert.ToInt16(e.Args[0])))
						await e.Channel.SendMessage($"New max tries is set to: *{e.Args[0]} tries**");
					else
						await e.Channel.SendMessage("Invalid paramter.");
				});

				g.CreateCommand("resetnow")
				.Alias("reset")
				.Description($"**-** `{Settings.Default.PrefixCmd + c} resetnow` resets the waiting time when you're out of attempts of the give away.")
				.AddCheck(Permission.BotOwnerOnly).Hide()
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					if (!Settings.Default.GiveawayEnabled)
						await e.Channel.SendMessage("There's currently no giveaway available.");
					else
						await e.Channel.SendMessage(await ResetNow());
				});

				g.CreateCommand("togglereset")
				.Alias("toggle")
				.Description($"**-** `{Settings.Default.PrefixCmd + c} togglereset` toggles the reset timer for the give away attempts.")
				.AddCheck(Permission.BotOwnerOnly).Hide()
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					await e.Channel.SendMessage(await ToggleTimeReset());
				});

				g.CreateCommand("setstate")
				.Alias("state")
				.Description($"**-** `{Settings.Default.PrefixCmd + c} setstate <state>` enables or disables the giveaway game.")
				.Parameter("p", Discord.Commands.ParameterType.Required)
				.AddCheck(Permission.BotOwnerOnly).Hide()
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					await e.Channel.SendMessage(SetGiveawayState(e.Args[0]));
				});

				g.CreateCommand("status")
				.Alias("debug")
				.Description($"**-** `{Settings.Default.PrefixCmd + c} status` will log some information about the giveaway.")
				.AddCheck(Permission.BotOwnerOnly).Hide()
				.Do(async (e) =>
				{
					await e.Channel.SendIsTyping();
					await Logging.CON(
						$"\ngiveaway status : {Settings.Default.GiveawayEnabled.ToString()}"
						+ $"\nmax tries : {Settings.Default.GiveawayMaxTries.ToString()}"
						//+ $"\nunlocked index : {Settings.Default.UnlockedIndex.ToString()}"
						+ $"\ncode length : {Settings.Default.GiveawayCode.Length.ToString()}"
						+ $"\nreset watch : {nextReset.Elapsed.Milliseconds.ToString()}"
						+ $"\nreset time : {Settings.Default.GiveawayResetTime.ToString()}\n", ConsoleColor.DarkCyan);
					await e.Channel.SendMessage("Status about giveaway has been logged (server-side).");
				});
				#endregion
			});
			return Task.FromResult(0);
		}

		#region ACTIONS
		public static async Task Reset()
		{
			isRunning = true;
			try
			{
				cancelResetSource = new CancellationTokenSource();
				cancelToken = cancelResetSource.Token;

				while (Settings.Default.GiveawayEnabled)
				{
					await Caching.CApplication.Save(cacheKey, new Dictionary<Discord.User, int>());
					//people = null;
					nextReset = Stopwatch.StartNew();
					await Logging.CON("Giveaway reset", ConsoleColor.DarkCyan);
					await Task.Delay((int)Settings.Default.GiveawayResetTime, cancelToken);
				}
			}
			catch
			{
				await Logging.CON("Giveaway ended", ConsoleColor.DarkCyan);
			}
			isRunning = false;
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
			await Logging.CON("Requested giveaway reset change...", ConsoleColor.DarkCyan);
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
			await Logging.CON("Requested giveaway reset", ConsoleColor.DarkCyan);
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
		private static async Task<bool> HasTriesLeft(Discord.User u)
		{
			// Get cache
			var cache = (Dictionary<Discord.User, int>)((await Caching.CApplication.Get(cacheKey))[0]);

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
					await Logging.CHA($"{u.Name} : {cache[u].ToString()}/{Settings.Default.GiveawayMaxTries.ToString()}", ConsoleColor.DarkCyan);
				}
				else
					yes = false;
			}
			else
			{
				cache.Add(u, (int)Settings.Default.GiveawayMaxTries - 1);
				await Logging.CHA($"new : {u.Name} : {cache[u].ToString()}/{Settings.Default.GiveawayMaxTries.ToString()}", ConsoleColor.DarkCyan);
			}

			// Save cache
			await Caching.CApplication.Save(cacheKey, cache);
			cache = null;
			return yes;
		}

		private static async Task<bool> IsSolved()
		{
			// Get cache
			var cache = Convert.ToUInt16(await Caching.CFile.Get(cacheKey));

			// New try
			if (Utils.RNG(N) == Convert.ToInt16(Settings.Default.GiveawayCode[(int)cache].ToString()))
				cache++;
			else
				return false;

			// Don't forget to save
			await Caching.CFile.Save(cacheKey, cache.ToString());

			// Check if solved
			if (cache != (uint)Settings.Default.GiveawayCode.Length)
				return false;

			// Solved and disable
			Settings.Default.GiveawayEnabled = false;
			Settings.Default.Save();
			return true;
		}
		#endregion

		private static async Task<string> GetPuzzleStatus() =>
			$"{((((double)Convert.ToUInt16(await Caching.CFile.Get(cacheKey)) / (double)Settings.Default.GiveawayCode.Length) * 100)).ToString()}%";

		private static Task<string> NextResetStatus()
		{
			var h = (int)((double)Settings.Default.GiveawayResetTime / (double)3600000) - nextReset.Elapsed.Hours;
			if (h < 1)
				return Task.FromResult("Will reset soon.");
			if (h == 1)
				return Task.FromResult("Will reset in 1 hour.");
			return Task.FromResult($"Will reset in {h.ToString()} hours.");
		}
	}
}