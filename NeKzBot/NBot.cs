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
			Logging.CON("Creating new client", ConsoleColor.DarkYellow);

			dClient = new DiscordClient(x =>
			{
				x.LogLevel = LogSeverity.Error;
				x.LogHandler = Logging.CON;
				x.AppName = Settings.Default.AppName;
				x.AppVersion = Settings.Default.AppVersion;
				x.AppUrl = Settings.Default.AppUrl;
			});

			try
			{
				Audio.Init();
				Data.Init();
				Logging.Init();
				Commands.Init();
				CmdManager.Init();
				CmdManager.LoadModules();
			}
			catch (Exception ex)
			{
				Logging.CON($"Could not load something:\n{ex.ToString()}");
				Console.ReadKey();
				Environment.Exit(0);
			}

			try
			{
				dClient.ExecuteAndWait(async () =>
				{
					await dClient.Connect(Settings.Default.Token, TokenType.Bot);
					dClient.SetGame(Data.randomGames[Utils.RNG(0, Data.randomGames.Count())]);

					// Module tasks
					await Task.Factory.StartNew(async () =>
					{
						// Leaderboard
						await AutoUpdater.AutoUpdate();
						await Caching.ResetDataCache();
						// Game
						//await GiveawayGame.TimeReset();
					});
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