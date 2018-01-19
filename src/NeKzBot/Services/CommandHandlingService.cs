using System;
#if DOCS
using System.Collections.Generic;
#endif
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
#if DOCS
			GenerateDocs()
				.GetAwaiter()
				.GetResult();
#endif
			return Task.CompletedTask;
		}

		private async Task MessageReceived(SocketMessage rawMessage)
		{
			if (!(rawMessage is SocketUserMessage message)) return;
			if (message.Source != MessageSource.User) return;

			int argPos = 0;
			if (!message.HasMentionPrefix(_client.CurrentUser, ref argPos))
				if (!message.HasStringPrefix(".", ref argPos)) return;
			
			var context = new SocketCommandContext(_client, message);
			var result = await _commands.ExecuteAsync(context, argPos, _provider);

			if ((result.Error.HasValue) && (result.Error.Value != CommandError.UnknownCommand))
				await _interactiveService.ReplyAndDeleteAsync(context, $"{result}", timeout: TimeSpan.FromSeconds(10));
		}
#if DOCS
		private Task GenerateDocs()
		{
			System.IO.File.WriteAllText("Modules.md", String.Empty);
			using (var fs = System.IO.File.OpenWrite("Modules.md"))
			using (var sw = new System.IO.StreamWriter(fs))
			{
				sw.WriteLine("| Command | Alias | Module |");
				sw.WriteLine("| --- | --- | --- |");

				string GetParameters(CommandInfo command)
				{
					var result = command.Name;
					foreach (var parameter in command.Parameters)
					{
						var before = " ";
						var after = string.Empty;

						var type = parameter.Type.Name;
						
						if (parameter.IsOptional)
						{
							before += "(";
							
							if (type != "String")
								after += $"={parameter.DefaultValue}";
							if (parameter.IsRemainder)
								after += "...)";
							else
								after += ")";
						}
						else
						{
							before += "<";
							if (parameter.IsRemainder)
								after += "...";
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

				string GetAliases(string name, IEnumerable<string> allAliases)
				{
					var aliases = new List<string>();
					foreach (var alias in allAliases)
					{
						if (alias.Contains('.'))
							aliases.Add(alias.Split('.').Last());
						else
							aliases.Add(alias);
					}
					
					aliases = aliases.Distinct().ToList();
					aliases.Remove(name);
					
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
						sw.WriteLine($"| `.{GetParameters(cmd)}` | {GetAliases(cmd.Name, cmd.Aliases)} | {module.Name} |");
					}
					foreach (var submodule in module.Submodules)
					{
						sw.WriteLine($"| `.{submodule.Name}.` | {GetAliases(submodule.Name, submodule.Aliases)} | {module.Name} |");
						foreach (var cmd in submodule.Commands)
						{
							sw.WriteLine($"| `.{submodule.Name}.{GetParameters(cmd)}` | {GetAliases(cmd.Name, cmd.Aliases)} | {module.Name} |");
						}
						foreach (var subsubmodule in submodule.Submodules)
						{
							sw.WriteLine($"| `.{submodule.Name}.{subsubmodule.Name}.` | {GetAliases(subsubmodule.Name, subsubmodule.Aliases)} | {module.Name} |");
							foreach (var cmd in subsubmodule.Commands)
							{
								sw.WriteLine($"| `.{submodule.Name}.{subsubmodule.Name}.{GetParameters(cmd)}` | {GetAliases(cmd.Name, cmd.Aliases)} | {module.Name} |");
							}
						}
					}
				}
			}
			return Task.CompletedTask;
		}
#endif
	}
}