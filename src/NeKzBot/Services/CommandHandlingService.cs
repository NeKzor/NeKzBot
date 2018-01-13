using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace NeKzBot.Services
{
	public class CommandHandlingService
	{
		private readonly DiscordSocketClient _client;
		private readonly CommandService _commands;
		private readonly IConfiguration _config;
		private readonly SourceDemoService _demoService;
		private readonly InteractiveService _interactiveService;
		private readonly IServiceProvider _provider;

		public CommandHandlingService(
			DiscordSocketClient client,
			CommandService commands,
			InteractiveService interactiveService,
			SourceDemoService demoService,
			IConfiguration config,
			IServiceProvider provider)
		{
			_client = client;
			_commands = commands;
			_interactiveService = interactiveService;
			_demoService = demoService;
			_config = config;
			_provider = provider;
		}

		public Task Initialize()
		{
			_client.MessageReceived += MessageReceived;
			return _commands.AddModulesAsync(Assembly.GetEntryAssembly());
		}

		private async Task MessageReceived(SocketMessage rawMessage)
		{
			if (!(rawMessage is SocketUserMessage message)) return;
			if (message.Source != MessageSource.User) return;

			int argPos = 0;
			if ((!message.HasMentionPrefix(_client.CurrentUser, ref argPos))
				&& (!message.HasStringPrefix(".", ref argPos)))
			{
				if (message.Attachments.Count == 1)
				{
					var attachment = message.Attachments.ToList().First();
					if ((attachment.Filename.EndsWith(".dem")) && (attachment.Size <= 5 * 1000 * 1000))
						await _demoService.DownloadNewDemoAsync(message.Author.Id, attachment.Url);
				}
				return;
			}

			var context = new SocketCommandContext(_client, message);
			var result = await _commands.ExecuteAsync(context, argPos, _provider);

			if ((result.Error.HasValue) && (result.Error.Value != CommandError.UnknownCommand))
				await _interactiveService.ReplyAndDeleteAsync(context, result.ToString(), timeout: TimeSpan.FromSeconds(10));
		}
	}
}