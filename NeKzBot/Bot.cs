using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using NeKzBot.Server;
using NeKzBot.Resources;
using NeKzBot.Modules;
using NeKzBot.Modules.Games;
using NeKzBot.Modules.Twitch;
using NeKzBot.Modules.Message;
using NeKzBot.Modules.Speedrun;
using NeKzBot.Modules.Leaderboard;

namespace NeKzBot
{
	public class Bot
	{
		public static DiscordClient dClient;

		public async Task Start()
		{
			await Create();
			await Init();
			await Load();
			await Connect();
		}

		private static async Task Create()
		{
			await Logging.CON("Creating client", ConsoleColor.White);

			dClient = new DiscordClient(x =>
			{
				x.LogLevel = LogSeverity.Error;
				x.LogHandler = Logging.CON;
				x.AppName = Settings.Default.AppName;
				x.AppVersion = Settings.Default.AppVersion;
				x.AppUrl = Settings.Default.AppUrl;
			});
		}

		private static async Task Init()
		{
			try
			{
				await Audio.Init();
				await Logging.Init();
				await Commands.Init();
				await Data.Init();
				await Caching.Init();
				await DataManager.Init();
				await Leaderboard.Cache.Init();
				await Leaderboard.AutoUpdater.Init();
				await SpeedrunCom.Init();
				await DropboxCom.Init();
			}
			catch (Exception ex)
			{
				await Logging.CON("Initialization failed", ex);
			}
		}

		private static async Task Load()
		{
			try
			{
				await DataManager.LoadModules();
			}
			catch (Exception ex)
			{
				await Logging.CON("Module error", ex);
			}
		}

		private static async Task Connect()
		{
			try
			{
				dClient.ExecuteAndWait(async () =>
				{
					await dClient.Connect(Credentials.Default.DiscordToken, TokenType.Bot);
					dClient.SetGame(Data.randomGames[Utils.RNG(0, Data.randomGames.Count())]);
					await LoadModulesAndWait();
				});
			}
			catch (Exception ex)
			{
				await Logging.CON("Something bad happened", ex);
			}
		}

		private static async Task LoadModulesAndWait()
		{
			Task.WaitAll(
				// Leaderboard
				Leaderboard.Cache.Reset(),
				Leaderboard.AutoUpdater.Start(),
				// Game
				GiveawayGame.Reset(),
				// SpeedrunCom
				SpeedrunCom.AutoNotification.Start(),
				// TwitchTv
				TwitchTv.Start()
			);
			await Task.Delay(-1);
		}
	}
}