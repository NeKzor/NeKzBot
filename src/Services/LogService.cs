﻿using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace NeKzBot.Services
{
	public class LogService
	{
		private readonly DiscordSocketClient _client;
		private readonly CommandService _commands;
		private readonly ILoggerFactory _loggerFactory;
		private readonly ILogger _discordLogger;
		private readonly ILogger _commandsLogger;

		public LogService(DiscordSocketClient client, CommandService commands, ILoggerFactory loggerFactory)
		{
			_client = client;
			_commands = commands;

			_loggerFactory = ConfigureLogging(loggerFactory);
			_discordLogger = _loggerFactory.CreateLogger("discord");
			_commandsLogger = _loggerFactory.CreateLogger("commands");

			_client.Log += LogDiscord;
			_commands.Log += LogCommand;
		}

		private ILoggerFactory ConfigureLogging(ILoggerFactory factory)
		{
			factory.AddConsole();
			return factory;
		}

		private Task LogDiscord(LogMessage message)
		{
			_discordLogger.Log(
				LogLevelFromSeverity(message.Severity),
				0,
				message,
				message.Exception,
				(_1, _2) => message.ToString(prependTimestamp: false));
			return Task.CompletedTask;
		}

		private Task LogCommand(LogMessage message)
		{
			// Return an error message for async commands
			if (message.Exception is CommandException command)
				_ = command.Context.Channel.SendMessageAsync($"Error: {command.Message}");

			_commandsLogger.Log(LogLevelFromSeverity(message.Severity), 0, message, message.Exception, (_1, _2) => message.ToString(prependTimestamp: false));
			return Task.CompletedTask;
		}

		private static LogLevel LogLevelFromSeverity(LogSeverity severity)
			=> (LogLevel)(Math.Abs((int)severity - 5));
	}
}