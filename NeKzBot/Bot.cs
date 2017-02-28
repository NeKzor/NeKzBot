﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord;
using NeKzBot.Classes;
using NeKzBot.Modules.Private.MainServer;
using NeKzBot.Modules.Public;
using NeKzBot.Modules.Public.Others;
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

		private static void OnExit(object _, EventArgs __)
			=> Task.WaitAll(Logger.SendAsync("Bot Shutdown...", LogColor.Default),
							Twitter.UpdateDescriptionAsync(Portal2.AutoUpdater.LeaderboardTwitterAccount,
														   $"{Configuration.Default.TwitterDescription} #OFFLINE"));

		public async Task StartAsync()
		{
			// Do things on exit
			if (Debugger.IsAttached)
				Console.CancelKeyPress += new ConsoleCancelEventHandler(OnExit);
			else
				AppDomain.CurrentDomain.ProcessExit += OnExit;

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
				await Exclusive.LoadAsync();
				await Giveaway.LoadAsync();
				await Admin.LoadAsync();
				await DataBase.LoadAsync();
				await Debugging.LoadAsync();
				await Subscription.LoadAsync();
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
					Client.SetGame(await Utils.RNGAsync(Data.RandomGames) as string);
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
			Task.WaitAll(
				// Leaderboard
				Portal2.Cache.ResetCacheAsync(),
				Portal2.AutoUpdater.StartAsync(),
				// Game
				Giveaway.ResetAsync(),
				// SpeedrunCom
				SpeedrunCom.AutoNotification.StartAsync(),
				// TwitchTv
				Twitch.StartAsync()
			);
			await Task.Delay(-1);
		}
	}
}