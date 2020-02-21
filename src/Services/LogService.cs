using System;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using NeKzBot.Services.Notifications.Auditor;
using NeKzBot.Services.Notifications.Speedrun;

namespace NeKzBot.Services
{
    public class LogService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly InteractiveService _interactive;
        private readonly SpeedrunNotificationService _speedrun;
        private readonly SourceDemoService _demo;
        private readonly AuditorNotificationService _auditor;
        private readonly ILoggerFactory _loggerFactory;

        private ILogger? _discordLogger;
        private ILogger? _commandsLogger;
        private ILogger? _serviceLogger;

        public LogService(
            DiscordSocketClient client,
            CommandService commands,
            InteractiveService interactive,
            SpeedrunNotificationService speedrun,
            SourceDemoService demo,
            AuditorNotificationService auditor,
            ILoggerFactory loggerFactory)
        {
            _client = client;
            _commands = commands;
            _interactive = interactive;
            _speedrun = speedrun;
            _demo = demo;
            _auditor = auditor;
            _loggerFactory = loggerFactory.AddConsole();
        }

        public Task Initialize()
        {
            _discordLogger = _loggerFactory.CreateLogger("discord");
            _commandsLogger = _loggerFactory.CreateLogger("commands");
            _serviceLogger = _loggerFactory.CreateLogger("services");

            _client.Log += LogDiscord;
            _commands.Log += LogCommand;
            _speedrun.Log += LogInternal;
            _demo.Log += LogInternal;
            _auditor.Log += LogInternal;

            return Task.CompletedTask;
        }

        private Task LogDiscord(LogMessage message)
        {
            _discordLogger!.Log
            (
                LogLevelFromSeverity(message.Severity),
                0,
                message,
                message.Exception,
                (_, __) => $"{message}"
            );
            return Task.CompletedTask;
        }
        private Task LogCommand(LogMessage message)
        {
            if (message.Exception is CommandException command)
            {
                _ = _interactive.ReplyAndDeleteAsync
                (
                    command.Context as SocketCommandContext,
                    $"Error: {command.Message}",
                    timeout: TimeSpan.FromSeconds(10)
                );
            }

            _commandsLogger!.Log
            (
                LogLevelFromSeverity(message.Severity),
                0,
                message,
                message.Exception,
                (_, __) => $"{message}"
            );
            return Task.CompletedTask;
        }
        private Task LogInternal(string message, Exception? ex)
        {
            _serviceLogger!.Log
            (
                // :^)
                (ex is {})
                    ? LogLevel.Error
                    : (message.EndsWith("!"))
                        ? LogLevel.Warning
                        : LogLevel.Information,
                0,
                message,
                ex,
                (_, __) => $"{DateTime.Now.ToString("HH:mm:ss")} {message}"
            );
            return Task.CompletedTask;
        }

        private static LogLevel LogLevelFromSeverity(LogSeverity severity)
            => (LogLevel)(Math.Abs((int)severity - 5));
    }
}
