using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace NeKzBot.Services
{
	public class CommandHandlingService
	{
		private IServiceProvider _provider;
		private readonly DiscordSocketClient _client;
		private readonly CommandService _commands;
		private readonly IConfiguration _config;

		public CommandHandlingService(IServiceProvider provider, DiscordSocketClient client, CommandService commands, IConfiguration config)
		{
			_provider = provider;
			_client = client;
			_commands = commands;
			_config = config;

			_client.MessageReceived += MessageReceived;
		}

		public async Task InitializeAsync(IServiceProvider provider)
		{
			_provider = provider;
			await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
		}

		private async Task MessageReceived(SocketMessage rawMessage)
		{
			if (!(rawMessage is SocketUserMessage message)) return;
			if (message.Source != MessageSource.User) return;

			int argPos = 0;
			if (!message.HasMentionPrefix(_client.CurrentUser, ref argPos)
				&& !message.HasStringPrefix(_config["global_prefix"], ref argPos)) return;

			var context = new SocketCommandContext(_client, message);
			var result = await _commands.ExecuteAsync(context, argPos, _provider);

			//if (result.Error.HasValue && result.Error.Value != CommandError.UnknownCommand)
			//	await context.Channel.SendMessageAsync(result.ToString());
		}
	}
}