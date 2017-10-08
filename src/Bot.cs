#define WIN7
using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using LiteDB;
using NeKzBot.Services;
using NeKzBot.Services.Notifciations;
#if WIN7
using Discord.Net.Providers.WS4Net;
#endif

namespace NeKzBot
{
	internal class Bot
	{
		private DiscordSocketClient _client;
		private IConfiguration _config;

		public async Task RunAsync()
		{
			_client = new DiscordSocketClient(new DiscordSocketConfig
			{
#if WIN7
				WebSocketProvider = WS4NetProvider.Instance,
#endif
				MessageCacheSize = 100,
#if DEBUG
				LogLevel = LogSeverity.Debug
#else
				LogLevel = LogSeverity.Error
#endif
			});

			_config = BuildConfig();

			var services = ConfigureServices();
			services.GetRequiredService<LogService>();

			await services
				.GetRequiredService<CommandHandlingService>()
				.InitializeAsync(services);

			await _client.LoginAsync(TokenType.Bot, _config["discord_token"]);
			await _client.StartAsync();

			// Other services
			var p2s = new Portal2NotificationService();
			var srs = new SpeedrunNotificationService();
			await Task.WhenAll(
				p2s.StartAsync(),
				srs.StartAsync()
			);

			await Task.Delay(-1);
		}

		private IServiceProvider ConfigureServices()
		{
			return new ServiceCollection()
				.AddSingleton(_client)
				.AddSingleton<CommandService>()
				.AddSingleton<CommandHandlingService>()
				.AddLogging()
				.AddSingleton<LogService>()
				.AddSingleton(_config)
				.AddSingleton(new LiteDatabase("nekzbot.db"))
				.BuildServiceProvider();
		}

		private IConfiguration BuildConfig()
		{
			return new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("credentials.json")
				.Build();
		}
	}
}