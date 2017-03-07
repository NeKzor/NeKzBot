using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NeKzBot.Classes;
using NeKzBot.Resources;
using NeKzBot.Server;
using NeKzBot.Tasks;

namespace NeKzBot
{
	public class Bot
	{
		public static DiscordSocketClient Client { get; set; }
		public static CommandHandler Handler { get; set; }
		public static DependencyMap Map { get; set; }

		public async Task StartAsync()
		{
			await Logger.SendAsync("Creating Client", LogColor.Default);
			Client = new DiscordSocketClient(new DiscordSocketConfig
			{
				WebSocketProvider = Discord.Net.Providers.WS4Net.WS4NetProvider.Instance,
				LogLevel = LogSeverity.Error
			});

			await InitAsync();
			await Logger.SendAsync("Connecting", LogColor.Default);
			await Client.LoginAsync(TokenType.User, Credentials.Default.DiscordUserToken);
			await Client.StartAsync();
			await Logger.SendAsync("Connected", LogColor.Default);

			Map = new DependencyMap();
			Map.Add(Client);

			Handler = new CommandHandler();
			await Handler.InstallAsync(Map);

			await InstallAndWaitAsync();
		}

		private async Task InitAsync()
		{
			try
			{
				await Logger.SendAsync("Initialization", LogColor.Init);
				await Data.InitAsync();
				await Caching.InitAsync();
				await Leaderboard.Cache.InitAsync();
				// ^Init this before other things
				await Steam.InitAsync();
				await DropboxCom.InitAsync();
				await SpeedrunCom.InitAsync();
			}
			catch (Exception e)
			{
				await Logger.SendAsync("Initialization Failed", e);
			}
		}

		private async Task InstallAndWaitAsync()
		{
			Task.WaitAll(Leaderboard.Cache.ResetAsync());
			await Task.Delay(-1);
		}
	}
}