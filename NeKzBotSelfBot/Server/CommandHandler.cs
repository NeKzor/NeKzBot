using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using NeKzBot.TypeReaders;

namespace NeKzBot.Server
{
	public class CommandHandler
	{
		public CommandService Service { get; set; }
		public DiscordSocketClient Client { get; set; }
		public IDependencyMap Map { get; set; }

		public async Task InstallAsync(IDependencyMap map)
		{
			Client = map.Get<DiscordSocketClient>();
			Service = new CommandService();
			map.Add(Service);
			Map = map;
			// Add custom type reader
			Service.AddTypeReader<Uri>(new UriTypeReader());
			// Load modules
			await Service.AddModulesAsync(Assembly.GetEntryAssembly());
			// Command event
			Client.MessageReceived += HandleCommandAsync;
		}

		public async Task HandleCommandAsync(SocketMessage arg)
		{
			var msg = arg as SocketUserMessage;
			if ((msg == null)
			|| (msg?.Author.Id != Bot.Client.CurrentUser.Id))
				return;

			// Find the prefix command
			var pos = -1;
			if (!(msg.HasStringPrefix(Configuration.Default.PrefixCmd, ref pos)))
				return;

			// Execute command
			var result = default(IResult);
			if ((result = await Service.ExecuteAsync(new CommandContext(Client, msg), pos, Map)).IsSuccess)
				return;

			// Error handler
			switch (result.Error)
			{
				case CommandError.UnknownCommand:
					await Logger.SendAsync("Unknown command!", LogColor.Error);
					break;
				case CommandError.ParseFailed:
					await Logger.SendAsync("Failed to parse value!", LogColor.Error);
					break;
				case CommandError.Exception:
					await Logger.SendAsync("Error!", LogColor.Error);
					break;
				case CommandError.BadArgCount:
					var command = Service.Search(new CommandContext(Client, msg), pos).Commands.FirstOrDefault().Command;
					var parameters = string.Empty;
					foreach (var parameter in command.Parameters)
					{
						parameters += (parameter.IsOptional)
												? (parameter.IsRemainder)
															? $" ({parameter.Type.Name}: {parameter.Name} ... )"
															: $" ({parameter.Type.Name}: {parameter.Name})"
												: (parameter.IsRemainder)
															? $" <{parameter.Type.Name}: {parameter.Name} ... >"
															: $" <{parameter.Type.Name}: {parameter.Name}>";
					}
					// Return command info
					await Message.EditAsync(msg, $"`{Configuration.Default.PrefixCmd}{(parameters != string.Empty ? $"{command.Name}{parameters}`" : $"{command.Name}` has no parameters.")}");
					await Logger.SendAsync("Error. Invalid parameter count!", LogColor.Error);
					break;
				default:
					await Logger.SendAsync("Something failed!", LogColor.Error);
					break;
			}
		}
	}
}