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
			_config = BuildConfig();

			_client = new DiscordSocketClient(new DiscordSocketConfig
			{
				MessageCacheSize = 100,
#if WIN7
				WebSocketProvider = WS4NetProvider.Instance,
#endif
#if DEBUG
				LogLevel = LogSeverity.Debug
#endif
			});

			var services = ConfigureServices();
			services.GetRequiredService<LogService>();
			var chs = services.GetRequiredService<CommandHandlingService>();
			var p2s = services.GetRequiredService<Portal2NotificationService>();
			var srs = services.GetRequiredService<SpeedrunNotificationService>();
			var sds = services.GetRequiredService<SourceDemoService>();
			var scs = services.GetRequiredService<SourceCvarService>();

			await chs.Initialize(services);
			await p2s.Initialize();
			await srs.Initialize();
			await sds.Initialize();
			await scs.Initialize();

			await _client.LoginAsync(TokenType.Bot, _config["discord_token"]);
			await _client.StartAsync();

			await Task.WhenAll(
				p2s.StartAsync(),
				srs.StartAsync()
			);

			await Task.Delay(-1);
		}

		private IConfiguration BuildConfig()
		{
			return new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("credentials.json")
				.Build();
		}

		private IServiceProvider ConfigureServices()
		{
			return new ServiceCollection()
				.AddLogging()
				.AddSingleton<LogService>()
				.AddSingleton(_config)
				.AddSingleton(new LiteDatabase(_config["database"]))
				// Discord
				.AddSingleton(_client)
				.AddSingleton(new CommandService(new CommandServiceConfig
				{
					SeparatorChar = _config["global_prefix"][0],
#if DEBUG
					LogLevel = LogSeverity.Debug
#endif
				}))
				.AddSingleton<CommandHandlingService>()
				// Others
				.AddSingleton<Portal2NotificationService>()
				.AddSingleton<SpeedrunNotificationService>()
				.AddSingleton<SourceDemoService>()
				.AddSingleton<SourceCvarService>()
				.BuildServiceProvider();
		}
	}
}