using System;
using System.Threading.Tasks;
using Discord;
using NeKzBot.Classes;
using NeKzBot.Extensions;
using NeKzBot.Modules.Private.MainServer;
using NeKzBot.Modules.Private.Owner;
using NeKzBot.Modules.Public;
using NeKzBot.Modules.Public.Others;
using NeKzBot.Modules.Public.Vip;
using NeKzBot.Resources;
using NeKzBot.Server;
using NeKzBot.Tasks;
using NeKzBot.Tasks.Leaderboard;
using NeKzBot.Tasks.NonModules;
using NeKzBot.Tasks.Speedrun;

namespace NeKzBot
{
	public class Bot
	{
		public static DiscordClient Client { get; private set; }

		public static async Task SendAsync(Request req, CustomMessage msg)
		{
			try
			{
				await Client.ClientAPI.Send(new RequestExtension<string>(req, msg));
			}
			catch (Exception e)
			{
				await Logger.SendAsync("Bot.SendAsync Error", e);
			}
		}

		private static void OnExit(object _, EventArgs __)
			=> Task.WaitAll(Logger.SendAsync("Bot Shutdown...", LogColor.Default),
							Twitter.UpdateDescriptionAsync(Portal2.AutoUpdater.LeaderboardTwitterAccount,
														   $"{Configuration.Default.TwitterDescription} #OFFLINE"));

		public async Task StartAsync()
		{
			// Do things on exit
			if (await Utils.IsLinux())
				AppDomain.CurrentDomain.ProcessExit += OnExit;
			else
				Console.CancelKeyPress += new ConsoleCancelEventHandler(OnExit);

			// Start
			Console.Title = $"{Configuration.Default.AppName} v{Configuration.Default.AppVersion} - Discord Server Bot";
			await CreateAsync();
			await InitAsync();
			await LoadAsync();
			await ConnectAsync();
		}

		private static async Task CreateAsync()
		{
			await Logger.SendAsync("Creating Client", LogColor.Default);
			Client = new DiscordClient(c =>
			{
				c.LogLevel = LogSeverity.Error;
				c.LogHandler = Logger.CON;
				c.AppName = Configuration.Default.AppName;
				c.AppVersion = Configuration.Default.AppVersion;
				c.AppUrl = Configuration.Default.AppUrl;
			});
		}

		private static async Task InitAsync()
		{
			try
			{
				await Logger.SendAsync("Initialization", LogColor.Init);
				await Audio.InitAsync();
				await Logger.InitAsync();
				await Commands.InitAsync();
				await Data.InitAsync();
				await Data.InitMangerAsync();
				await Caching.InitAsync();
				// ^Important stuff first
				await Portal2.Cache.InitAsync();
				await Portal2.AutoUpdater.InitAsync();
				await Steam.InitAsync();
				await Twitch.InitAsync();
				await Giveaway.InitAsync();
				await DropboxCom.InitAsync();
				await SpeedrunCom.InitAsync();
				await SpeedrunCom.AutoNotification.InitAsync();
			}
			catch (Exception e)
			{
				await Logger.SendAsync("Initialization Failed", e);
			}
		}

		private static async Task LoadAsync()
		{
			try
			{
				await Logger.SendAsync("Loading Modules", LogColor.Default);
				// Private
				await Admin.LoadAsync();
				await DataBase.LoadAsync();
				await Debugging.LoadAsync();
				await Giveaway.LoadAsync();
				// Public
				await Help.LoadAsync();
				await Info.LoadAsync();
				await Leaderboard.LoadAsync();
				await Sound.LoadAsync();
				await Speedrun.LoadAsync();
				await Builder.LoadAsync();
				await Fun.LoadAsync();
				await RaspberryPi.LoadAsync();
				await Resource.LoadAsync();
				await Rest.LoadAsync();
				await Subscription.LoadAsync();
			}
			catch (Exception e)
			{
				await Logger.SendAsync("Module Error", e);
			}
		}

		private static async Task ConnectAsync()
		{
			try
			{
				Client.ExecuteAndWait(async () =>
				{
					await Logger.SendAsync("Connecting", LogColor.Default);
					await Client.Connect(Credentials.Default.DiscordBotToken, TokenType.Bot);
					await Logger.SendAsync("Connected", LogColor.Default);
					Client.SetGame(await Utils.RngAsync(Data.RandomGames) as string);
					await LoadTasksAndWaitAsync();
				});
			}
			catch (Exception e)
			{
				await Logger.SendAsync("Something Failed", e);
			}
		}

		private static async Task LoadTasksAndWaitAsync()
		{
			await Logger.SendAsync("Loading Tasks", LogColor.Default);
			Task.WaitAll(Giveaway.ResetAsync(),
						 Twitch.StartAsync(),
						 Portal2.Cache.ResetAsync(),
						 Portal2.AutoUpdater.StartAsync(),
						 SpeedrunCom.AutoNotification.StartAsync());
			await Task.Delay(-1);
		}
	}
}