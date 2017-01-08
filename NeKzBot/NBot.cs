using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using NeKzBot.Properties;

namespace NeKzBot
{
	public class NBot
	{
		public static DiscordClient dClient;

		public NBot()
		{
			Create();
			Init();
			Load();
			Connect();
		}

		private void Create()
		{
			Logging.CON("Creating new client", ConsoleColor.White);

			dClient = new DiscordClient(x =>
			{
				x.LogLevel = LogSeverity.Error;
				x.LogHandler = Logging.CON;
				x.AppName = Settings.Default.AppName;
				x.AppVersion = Settings.Default.AppVersion;
				x.AppUrl = Settings.Default.AppUrl;
			});
		}

		private void Init()
		{
			try
			{
				Audio.Init();
				Logging.Init();
				Commands.Init();
				Data.Init();
				Caching.Init();
				CmdManager.Init();
				Leaderboard.Cache.Init();
				Leaderboard.AutoUpdater.Init();
				SpeedrunCom.Init();
				DropboxCom.Init();
			}
			catch (Exception ex)
			{
				Logging.CON($"Initialization failed:\n{ex.ToString()}");
				Console.ReadKey();
				Environment.Exit(0);
			}
		}

		private void Load()
		{
			try
			{
				CmdManager.LoadModules();
			}
			catch (Exception ex)
			{
				Logging.CON($"Module error:\n{ex.ToString()}");
				Console.ReadKey();
				Environment.Exit(0);
			}
		}

		private static void Connect()
		{
			try
			{
				dClient.ExecuteAndWait(async () =>
				{
					await dClient.Connect(Settings.Default.Token, TokenType.Bot);
					dClient.SetGame(Data.randomGames[Utils.RNG(0, Data.randomGames.Count())]);

					// Module tasks
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
				});
			}
			catch (Exception ex)
			{
				Logging.CON($"Something bad happened:\n{ex.ToString()}");
				Console.ReadKey();
				Environment.Exit(0);
			}
		}
	}
}