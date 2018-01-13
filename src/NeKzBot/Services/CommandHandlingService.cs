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
			_commands.AddModulesAsync(Assembly.GetEntryAssembly());

#if MGEN
			using (var fs = System.IO.File.OpenWrite("Modules.md"))
			using (var sw = new System.IO.StreamWriter(fs))
			{
				sw.WriteLine("| Command | Module | Aliases |");
				sw.WriteLine("| --- | --- | --- |");

				string GetParameters(CommandInfo command)
				{
					var result = command.Name;
					foreach (var parameter in command.Parameters)
					{
						var before = " ";
						var after = string.Empty;

						var type = parameter.Type.Name;
						
						if (parameter.IsRemainder)
							after += "...";
						if (parameter.IsOptional)
						{
							before += "(";
							after += ")";
						}
						else
						{
							before += "<";
							after += ">";
						}
						if (parameter.IsMultiple)
						{
							if (parameter.IsOptional)
								after += " (...)";
							else
								after += " <...>";
						}
						result += $"{before}{parameter.Name}:{parameter.Type.Name}{after}";
					}
					return result;
				}

				string GetAliases(CommandInfo command)
				{
					var aliases = new System.Collections.Generic.List<string>();
					foreach (var alias in command.Aliases)
					{
						if (alias.Contains('.'))
							aliases.Add(alias.Split('.').Last());
					}
					
					aliases = aliases.Distinct().ToList();
					aliases.Remove(command.Name);
					
                    if (aliases.Count == 0)
						return "-";

					var result = string.Empty;
					foreach	(var alias in aliases)
						result += $"`{alias}`, ";
					return result.Substring(0, result.Length - 2);
				}

				// Most nested and multiple foreach-loops I've ever used
				foreach	(var module in _commands.Modules.Where(c => !c.IsSubmodule))
				{
					foreach (var cmd in module.Commands)
					{
						sw.WriteLine($"| `.{GetParameters(cmd)}` | {module.Name} | {GetAliases(cmd)} |");
					}
					foreach (var submodule in module.Submodules)
					{
						foreach (var cmd in submodule.Commands)
						{
							sw.WriteLine($"| `.{submodule.Name}.{GetParameters(cmd)}` | {module.Name} | {GetAliases(cmd)} |");
						}
						foreach (var subsubmodule in submodule.Submodules)
						{
							foreach (var cmd in subsubmodule.Commands)
							{
								sw.WriteLine($"| `.{submodule.Name}.{subsubmodule.Name}.{GetParameters(cmd)}` | {module.Name} | {GetAliases(cmd)} |");
							}
						}
					}
				}
			}
#endif
			return Task.CompletedTask;
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