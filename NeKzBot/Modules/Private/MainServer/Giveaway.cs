using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using NeKzBot.Internals;
using NeKzBot.Resources;
using NeKzBot.Server;

namespace NeKzBot.Modules.Private.MainServer
{
	// TODO: lol I still didn't delete this
	public class Giveaway : Commands
	{
		public static bool IsRunning { get; set; } = false;
		public static InternalWatch Watch { get; } = new InternalWatch();
		private static Stopwatch _nextReset;
		private static CancellationTokenSource _cancelResetSource;
		private static CancellationToken _cancelToken;
		private static string _cacheKey;

		private const int _n = 9;

		public static async new Task InitAsync()
		{
			await Logger.SendAsync("Initializing Giveaway Game", LogColor.Init);
			_nextReset = new Stopwatch();
			_cacheKey = "gg";

			// Reserver memory
			await Caching.CFile.AddKeyAsync(_cacheKey);
			// Write data if it doesn't exist yet
			if (await Caching.CFile.GetFileAsync(_cacheKey) == null)
				await Caching.CFile.SaveCacheAsync(_cacheKey, "0");
		}

		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Giveaway Module", LogColor.Init);
			await GetGiveaway("giveaway");
			await GiveawayCommands("giveaway");
		}

		private static Task GetGiveaway(string c)
		{
			CService.CreateCommand(c)
					.Description("Could give you a prize. Algorithm:\n• User requests random number\n• Checking if number equals value[index]\n• If it does index++ and tries--\n• Try again\n• If index == value.length, puzzle solved.\n• Good luck!\n• Notes: Solve status and value (code to solve) are hidden, this game isn't always available.")
					.AddCheck(Permissions.MainServerOnly)
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						if (Configuration.Default.GiveawayEnabled)
						{
							if (await HasTriesLeftAsync(e.User))
							{
								if (await CheckIfSolvedAsync())
								{
									await e.Channel.SendMessage("Ayy, congrats :grinning:");
									await e.User.SendMessage($"Your prize: {Credentials.Default.GiveawayPrizeKey}");
									if (!(ResetAsync().IsCompleted))
										_cancelResetSource.Cancel();
									await Logger.SendAsync("giveaway solved", LogColor.Giveaway);
								}
								else
								{
									await e.Channel.SendMessage("Try again.");
									await Logger.SendAsync($"game giveaway status : {await GetPuzzleStatusAsync()}", LogColor.Giveaway);
								}
							}
							else
								await e.Channel.SendMessage($"Out of attempts. Try in {(int)((double)Configuration.Default.GiveawayResetTime / 3600000) - _nextReset.Elapsed.Hours}h again.");
						}
						else
							await e.Channel.SendMessage("Giveaway is not available at the moment or the puzzle has been solved.");
					});
			return Task.FromResult(0);
		}

		private static Task GiveawayCommands(string c)
		{
			CService.CreateGroup(c, GBuilder =>
			{
				GBuilder.CreateCommand("resetwhen")
						.Alias("when")
						.Description("Shows you when the bot will forget your attempts.")
						.AddCheck(Permissions.MainServerOnly)
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							if (Configuration.Default.GiveawayEnabled)
								await e.Channel.SendMessage(await NextResetStatus());
							else
								await e.Channel.SendMessage("There's currently no giveaway available.");
						});

				#region BOT OWNER ONLY
				GBuilder.CreateCommand("resettime")
						.Alias("time")
						.Description("Will set a new reset time for the giveaway.")
						.Parameter("time", ParameterType.Required)
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							if (!(Configuration.Default.GiveawayEnabled))
								await e.Channel.SendMessage("There's currently no giveaway available.");
							else if (await SetNewResetTime(Convert.ToInt16(e.Args[0])))
								await e.Channel.SendMessage($"New reset time is set to: **{e.Args[0]}min**");
							else
								await e.Channel.SendMessage("Invalid parameter. Time is in minutes.");
						});

				GBuilder.CreateCommand("maxtries")
						.Alias("tries")
						.Description("Will set the amount of tries for each user.")
						.Parameter("tries", ParameterType.Required)
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							if (!(Configuration.Default.GiveawayEnabled))
								await e.Channel.SendMessage("There's currently no giveaway available.");
							else if (await SetNewMaxTries(Convert.ToInt16(e.Args[0])))
								await e.Channel.SendMessage($"New max tries is set to: *{e.Args[0]} tries*");
							else
								await e.Channel.SendMessage("Invalid parameter.");
						});

				GBuilder.CreateCommand("resetnow")
						.Alias("reset")
						.Description("Resets the waiting time when you're out of attempts of the give away.")
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							if (!(Configuration.Default.GiveawayEnabled))
								await e.Channel.SendMessage("There's currently no giveaway available.");
							else
								await e.Channel.SendMessage(await ResetNowAsync());
						});

				GBuilder.CreateCommand("togglereset")
						.Alias("toggle")
						.Description("Toggles the reset timer for the give away attempts.")
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage(await ToggleTimeResetAsync());
						});

				GBuilder.CreateCommand("setstate")
						.Alias("state")
						.Description("Enables or disables the giveaway game.")
						.Parameter("state", ParameterType.Required)
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage(await SetGiveawayState(e.Args[0]));
						});

				GBuilder.CreateCommand("status")
						.Alias("debug")
						.Description("Will log some information about the giveaway.")
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await Logger.SendAsync($"\nGiveaway Status -> {Configuration.Default.GiveawayEnabled}"
												 + $"\nMax Tries -> {Configuration.Default.GiveawayMaxTries}"
												 + $"\nUnlocked index -> {Convert.ToUInt16(await Caching.CFile.GetCacheAsync(_cacheKey))}"
												 + $"\nCode Length -> {Configuration.Default.GiveawayCode.Length}"
												 + $"\nReset Watch -> {_nextReset.Elapsed.Milliseconds}"
												 + $"\nReset Time -> {Configuration.Default.GiveawayResetTime}", LogColor.Giveaway);
							await e.Channel.SendMessage("Status about giveaway has been logged (server-side).");
						});
				#endregion
			});
			return Task.FromResult(0);
		}

		#region ACTIONS
		public static async Task ResetAsync()
		{
			IsRunning = true;
			try
			{
				_cancelResetSource = new CancellationTokenSource();
				_cancelToken = _cancelResetSource.Token;

				while (Configuration.Default.GiveawayEnabled)
				{
					await Caching.CApplication.SaveCacheAsync(_cacheKey, new Dictionary<Discord.User, int>());
					_nextReset = Stopwatch.StartNew();
					await Logger.SendAsync("Giveaway.ResetAsync Reset", LogColor.Giveaway);
					await Task.Delay((int)Configuration.Default.GiveawayResetTime - await Watch.GetElapsedTimeAsync(message: "Giveaway.ResetAsync Delay Took -> "), _cancelToken);
					await Watch.RestartAsync();
				}
			}
			catch
			{
				await Logger.SendAsync("Giveaway.ResetAsync Ended", LogColor.Giveaway);
			}
			IsRunning = false;
		}

		public static Task<string> SetGiveawayState(string state)
		{
			if (string.Equals(state, "toggle", StringComparison.CurrentCultureIgnoreCase))
				Configuration.Default.GiveawayEnabled = !Configuration.Default.GiveawayEnabled;
			else if (string.Equals(state, "true", StringComparison.CurrentCultureIgnoreCase))
				Configuration.Default.GiveawayEnabled = true;
			else if (string.Equals(state, "false", StringComparison.CurrentCultureIgnoreCase))
				Configuration.Default.GiveawayEnabled = false;
			else
				return Task.FromResult("Invalid state.");
			Configuration.Default.Save();
			return Task.FromResult($"Giveaway state is set to: {Configuration.Default.GiveawayEnabled}");
		}

		public static async Task<string> ToggleTimeResetAsync()
		{
			await Logger.SendAsync("Requested Giveaway Reset Change...", LogColor.Giveaway);
			if ((_cancelResetSource.IsCancellationRequested)
			|| (ResetAsync().IsCompleted))
			{
				await Task.Factory.StartNew(async () => await ResetAsync());
				return "TimeReset started.";
			}
			_cancelResetSource.Cancel();
			return "TimeReset cancelled.";
		}

		public static Task<bool> SetNewResetTime(int t)
		{
			// In minutes
			if ((t < 1)
			|| (t > 1440))
				return Task.FromResult(false);
			Configuration.Default.RefreshTime = (uint)(t * 1000 * 60);   // Why did I set it to ms lol
			Configuration.Default.Save();
			return Task.FromResult(true);
		}

		public static Task<bool> SetNewMaxTries(int t)
		{
			if ((t < 1)
			|| (t > 999))
				return Task.FromResult(false);
			Configuration.Default.GiveawayMaxTries = (uint)t;
			Configuration.Default.Save();
			return Task.FromResult(true);
		}

		public static async Task<string> ResetNowAsync()
		{
			await Logger.SendAsync("Requested Giveaway Reset", LogColor.Giveaway);
			if (!(_cancelResetSource.IsCancellationRequested)
			&& !(ResetAsync().IsCompleted))
			{
				_cancelResetSource.Cancel();
				await Task.Factory.StartNew(async () => await ResetAsync());
				return "Done.";
			}
			return "Cannot reset.";
		}
		#endregion

		#region PUZZLE ALGORITHM
		private static async Task<bool> HasTriesLeftAsync(Discord.User u)
		{
			// Get cache
			var cache = (await Caching.CApplication.GetCacheAsync(_cacheKey))[0] as Dictionary<Discord.User, int>;

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
					cache[u]--;
					await Logger.SendToChannelAsync($"{u.Name} : {cache[u]}/{Configuration.Default.GiveawayMaxTries}", LogColor.Giveaway);
				}
				else
					yes = false;
			}
			else
			{
				cache.Add(u, (int)Configuration.Default.GiveawayMaxTries - 1);
				await Logger.SendToChannelAsync($"New : {u.Name} : {cache[u]}/{Configuration.Default.GiveawayMaxTries}", LogColor.Giveaway);
			}

			// Save cache
			await Caching.CApplication.SaveCacheAsync(_cacheKey, cache);
			return yes;
		}

		private static async Task<bool> CheckIfSolvedAsync()
		{
			// Get cache
			var cache = Convert.ToUInt16(await Caching.CFile.GetCacheAsync(_cacheKey));

			// New try
			if (await Utils.RNGAsync(_n) == Convert.ToInt16(Configuration.Default.GiveawayCode[cache].ToString()))
				cache++;
			else
				return false;

			// Don't forget to save
			await Caching.CFile.SaveCacheAsync(_cacheKey, cache.ToString());

			// Check if solved
			if (cache != (uint)Configuration.Default.GiveawayCode.Length)
				return false;

			// Solved and disable
			Configuration.Default.GiveawayEnabled = false;
			Configuration.Default.Save();
			return true;
		}
		#endregion

		private static async Task<string> GetPuzzleStatusAsync()
			=> $"{Convert.ToUInt16(await Caching.CFile.GetCacheAsync(_cacheKey)) / (double)Configuration.Default.GiveawayCode.Length * 100}%";

		private static Task<string> NextResetStatus()
		{
			var h = (int)(Configuration.Default.GiveawayResetTime / (double)3600000) - _nextReset.Elapsed.Hours;
			return Task.FromResult((h < 1)
									  ? "Will reset soon."
									  : (h == 1)
										   ? "Will reset in 1 hour."
										   : $"Will reset in {h} hours.");
		}
	}
}