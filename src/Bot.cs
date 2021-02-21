using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using LiteDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NeKzBot.Services;
using NeKzBot.Services.Notifications.Auditor;
using NeKzBot.Services.Notifications.Speedrun;

namespace NeKzBot
{
    internal class Bot
    {
        private DiscordSocketClient? _client;
        private IConfiguration? _config;

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
            var sds = services.GetRequiredService<SourceDemoService>();
            var ims = services.GetRequiredService<ImageService>();
            var pcs = services.GetRequiredService<Portal2CampaignService>();
            var aus = services.GetRequiredService<AuditorNotificationService>();
            var pnb = services.GetRequiredService<PinBoardService>();
            var pst = services.GetRequiredService<PistonService>();

            await log.Initialize();
            await chs.Initialize();
            await srs.Initialize();
            await scs.Initialize();
            await sds.Initialize();
            await ims.Initialize();
            await pcs.Initialize();
            await aus.Initialize();
            await pnb.Initialize();
            await pst.Initialize();

#if DB_CLEANUP
            await srs.CleanupAsync();
#else
            await _client.LoginAsync(TokenType.Bot, _config["discord_token"]);
            await _client.StartAsync();

            _ = Task.WhenAll
            (
                srs.StartAsync(),
                aus.StartAsync()
            );

            await Task.Delay(Timeout.Infinite);
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
                .AddSingleton(new LiteDatabase("private/nekzbot.db"))
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
                .AddSingleton<SourceDemoService>()
                .AddSingleton<ImageService>()
                .AddSingleton<Portal2CampaignService>()
                .AddSingleton<AuditorNotificationService>()
                .AddSingleton<PinBoardService>()
                .AddSingleton<PistonService>()
                .BuildServiceProvider();
        }
    }

    internal class App
    {
        private static async Task Main()
            => await new Bot().RunAsync();
    }
}
