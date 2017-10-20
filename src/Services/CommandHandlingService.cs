using System;
using System.Linq;
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
		private readonly SourceDemoService _demoService;

		public CommandHandlingService(
			IServiceProvider provider,
			DiscordSocketClient client,
			CommandService commands,
			IConfiguration config,
			SourceDemoService demoService)
		{
			_provider = provider;
			_client = client;
			_commands = commands;
			_config = config;
			_demoService = demoService;

			_client.MessageReceived += MessageReceived;
		}

		public Task Initialize(IServiceProvider provider)
		{
			_provider = provider;
			return _commands.AddModulesAsync(Assembly.GetEntryAssembly());
		}

		private async Task MessageReceived(SocketMessage rawMessage)
		{
			if (!(rawMessage is SocketUserMessage message)) return;
			if (message.Source != MessageSource.User) return;

			int argPos = 0;
			if ((!message.HasMentionPrefix(_client.CurrentUser, ref argPos))
				&& (!message.HasStringPrefix(_config["global_prefix"], ref argPos)))
			{
				if (message.Attachments.Count == 1)
				{
					var attachement = message.Attachments.ToList().First();
					if ((attachement.Filename.EndsWith(".dem")) && (attachement.Size <= 5 * 1000 * 1000))
						await _demoService.GetNewDemoAsync(message.Author.Id, attachement.Url);
				}
				return;
			}

			var context = new SocketCommandContext(_client, message);
			var result = await _commands.ExecuteAsync(context, argPos, _provider);

#if DEBUG
			if ((result.Error.HasValue) && (result.Error.Value != CommandError.UnknownCommand))
				await context.Channel.SendMessageAsync(result.ToString());
#endif
		}
	}
}