using System;
using System.Threading.Tasks;
using Discord;
using NeKzBot.Classes;
using NeKzBot.Extensions;
using NeKzBot.Internals.Entities;
using NeKzBot.Modules.Private.Members;
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
using NeKzBot.Utilities;
using NeKzBot.Webhooks;

namespace NeKzBot
{
	public class Bot : IDisposable
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
			=> Task.WaitAll(Logger.SendAsync("Bot Shutdown..."),
							Twitter.UpdateDescriptionAsync(Portal2Board.AutoUpdater.LeaderboardTwitterAccount,
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
			await Logger.SendAsync("Creating Client");
			Client = new DiscordClient(c =>
			{
#if DEBUG
				c.LogLevel = LogSeverity.Verbose;
#else
				c.LogLevel = LogSeverity.Error;
#endif
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
				await CommandModule.InitAsync();
				await Data.InitMangerAsync();
				await Caching.InitAsync();
				await WebhookService.InitAsync();
				// ^Important stuff first
				await Steam.InitAsync();
				await TwitchTv.InitAsync();
				await DropboxCom.InitAsync();
				await Portal2Board.AutoUpdater.InitAsync();
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
				await Logger.SendAsync("Loading Modules");
				// Private
				await Admin.LoadAsync();
				await Cloud.LoadAsync();
				await Contest.LoadAsync();
				await DataBase.LoadAsync();
				await Debugging.LoadAsync();
				await Special.LoadAsync();
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
				await Service.LoadAsync();
				// ^Load these before generating module lists
				await Data.GenerateModuleListsAsync();
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
					await Logger.SendAsync("Connecting");
					await Client.Connect(Credentials.Default.DiscordBotToken, TokenType.Bot);
					await Logger.SendAsync("Connected");
					Client.SetGame(await Utils.RngAsync((await Data.Get<Simple>("games")).Value));
#if RELEASE
					await LoadTasksAndWaitAsync();
#endif
				});
			}
			catch (Exception e)
			{
				await Logger.SendAsync("Something Failed", e);
			}
		}

		private static async Task LoadTasksAndWaitAsync()
		{
			await Logger.SendAsync("Loading Tasks");
			Task.WaitAll(
				Portal2Board.AutoUpdater.StartAsync(),
				TwitchTv.StartAsync(),
				SpeedrunCom.AutoNotification.StartAsync()
			);
			await Task.Delay(-1);
		}

		public void Dispose()
		{
			Client = default(DiscordClient);
			GC.Collect();
		}
	}
}