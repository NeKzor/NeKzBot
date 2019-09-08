using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
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
            AppContext.SetSwitch("System.Net.Http.UseSocketsHttpHandler", false);

            _config = BuildConfig();

            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                MessageCacheSize = 100,
#if DEBUG
                LogLevel = LogSeverity.Debug
#endif
            });

            var services = ConfigureServices();

            var log = services.GetRequiredService<LogService>();
            var chs = services.GetRequiredService<CommandHandlingService>();
            var srs = services.GetRequiredService<SpeedrunNotificationService>();
            var scs = services.GetRequiredService<SourceCvarService>();
            var ims = services.GetRequiredService<ImageService>();
            var pcs = services.GetRequiredService<Portal2CampaignService>();
            var aus = services.GetRequiredService<AuditNotificationService>();

            await log.Initialize();
            await chs.Initialize();
            await srs.Initialize();
            await scs.Initialize();
            await ims.Initialize();
            await pcs.Initialize();
            await aus.Initialize();

#if DB_CLEANUP
			await srs.CleanupAsync();
#else
            await _client.LoginAsync(TokenType.Bot, _config["discord_token"]);
            await _client.StartAsync();

            await Task.WhenAll
            (
                //srs.StartAsync(),
                aus.StartAsync()
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
                .AddSingleton(new InteractiveService(_client, new InteractiveServiceConfig() { DefaultTimeout = TimeSpan.FromSeconds(5 * 60) }))
                // Others
                .AddSingleton<SpeedrunNotificationService>()
                .AddSingleton<SourceCvarService>()
                .AddSingleton<ImageService>()
                .AddSingleton<Portal2CampaignService>()
                .AddSingleton<AuditNotificationService>()
                .BuildServiceProvider();
        }
    }
}
