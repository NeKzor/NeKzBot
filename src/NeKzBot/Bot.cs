using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
#if WIN7
using Discord.Net.Providers.WS4Net;
#endif
using Discord.WebSocket;
using LiteDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NeKzBot.Services;
using NeKzBot.Services.Notifications;

namespace NeKzBot
{
	internal class Bot
	{
		private DiscordSocketClient _client;
		private IConfiguration _config;

		public async Task RunAsync()
		{
			_config = BuildConfig();

			_client = new DiscordSocketClient(new DiscordSocketConfig()
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

			var log = services.GetRequiredService<LogService>();
			var chs = services.GetRequiredService<CommandHandlingService>();
			var p2s = services.GetRequiredService<Portal2NotificationService>();
			var srs = services.GetRequiredService<SpeedrunNotificationService>();
			var sds = services.GetRequiredService<SourceDemoService>();
			var scs = services.GetRequiredService<SourceCvarService>();

			await log.Initialize();
			await chs.Initialize();
			await p2s.Initialize();
			await srs.Initialize();
			await sds.Initialize();
			await scs.Initialize();

#if DB_CLEANUP
			await p2s.CleanupAsync();
			await srs.CleanupAsync();
			await sds.DeleteExpiredDemos();
#else
			await _client.LoginAsync(TokenType.Bot, _config["discord_token"]);
			await _client.StartAsync();

			await Task.WhenAll
			(
				p2s.StartAsync(),
				srs.StartAsync()
			);

			await Task.Delay(-1);
#endif
		}

		private IConfiguration BuildConfig()
		{
			return new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("private/credentials.json")
				.Build();
		}
		private IServiceProvider ConfigureServices()
		{
			return new ServiceCollection()
				.AddLogging()
				.AddSingleton<LogService>()
				.AddSingleton(_config)
				.AddSingleton(new LiteDatabase(_config["database_path"]))
				// Discord
				.AddSingleton(_client)
				.AddSingleton(new CommandService(new CommandServiceConfig
				{
					SeparatorChar = '.',
#if DEBUG
					LogLevel = LogSeverity.Debug
#endif
				}))
				.AddSingleton<CommandHandlingService>()
				.AddSingleton(new InteractiveService(_client, TimeSpan.FromSeconds(5 * 60)))
				// Others
				.AddSingleton<Portal2NotificationService>()
				.AddSingleton<SpeedrunNotificationService>()
				.AddSingleton<SourceDemoService>()
				.AddSingleton<SourceCvarService>()
				.BuildServiceProvider();
		}
	}
}