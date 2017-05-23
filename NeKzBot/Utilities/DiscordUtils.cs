using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using NeKzBot.Resources;
using NeKzBot.Server;

namespace NeKzBot.Utilities
{
	public static partial class Utils
	{
		// Find channel
		public static Task<Channel> FindTextChannel(string name, Discord.Server guild = null)
			=> Task.FromResult(((guild == null)
										? Bot.Client?.Servers?.FirstOrDefault(server => server.Id == Credentials.Default.DiscordMainServerId)
										: Bot.Client?.Servers?.FirstOrDefault(server => server.Id == guild.Id))?.TextChannels?
											.FirstOrDefault(channel => channel.Name == name));

		// Find guild
		public static Task<Discord.Server> FindGuild(ulong id)
			=> Task.FromResult(Bot.Client?.Servers?.FirstOrDefault(guild => guild.Id == id));

		// Find channels of every connected server
		public static Task<List<Channel>> GetChannels(string name)
		{
			var list = new List<Channel>();
			foreach (var server in Bot.Client?.Servers)
			{
				var channel = server?.TextChannels?.FirstOrDefault(cha => cha.Name == name);
				if (channel != null)
					list.Add(channel);
			}
			return Task.FromResult((list.Count > 0) ? list : default(List<Channel>));
		}

		// Find command and return its description
		public static async Task<string> FindDescriptionAsync(string name)
		{
			if (string.IsNullOrEmpty(name))
				return Data.ListModules + Data.MoreInformation;
			foreach (var command in CommandModule.CService.AllCommands)
			{
				if ((name == command.Text)
				|| (command.Aliases.Contains(name)))
					return await GetDescription(command);
			}
			return "This command does not exist.";
		}

		public static Task<string> GetDescription(Command cmd)
		{
			var output = $"`{Configuration.Default.PrefixCmd}{cmd.Text}";
			if (cmd.Parameters.Any())
			{
				foreach (var parameter in cmd.Parameters)
				{
					if (parameter.Type == ParameterType.Multiple)
						output += $" <{parameter.Name}> <etc.>";
					else if (parameter.Type == ParameterType.Optional)
						output += $" ({parameter.Name})";
					else
						output += $" <{parameter.Name}>";
				}
			}
			output += (string.IsNullOrEmpty(cmd.Description))
							 ? "`\nNo description."
							 : $"`\n{cmd.Description}";

			var aliases = "\n\nKnown aliases:";
			if (cmd.Aliases.Any())
				foreach (var alias in cmd.Aliases)
					aliases += $" `{alias}`, ";
			return CutMessageAsync((aliases != "\n\nKnown aliases:")
									   ? output += aliases.Substring(0, aliases.Length - 2)
									   : output, badchars: false);
		}

		public static Task<Command> FindCommandByName(string name)
		{
			foreach (var command in CommandModule.CService.AllCommands)
			{
				if ((name == command.Text)
				|| (command.Aliases.Contains(name)))
					return Task.FromResult(command);
			}
			return Task.FromResult(default(Command));
		}

		public static Task<User> GetBotUserObject(Channel cha)
			=> Task.FromResult(cha.Users.FirstOrDefault(usr => usr.Id == Bot.Client.CurrentUser.Id));

		public static async Task<bool> CheckRolePermissionsAsync(User usr, byte flag)
		{
			foreach (var role in usr?.Roles)
				if (await FlagIsSet(role.Permissions.RawValue, flag))
					return true;
			return false;
		}

		public static async Task DeleteMessagesAsync(CommandEventArgs e, ulong id, int count)
		{
			// Bulk delete only
			var messages = (await e.Channel.DownloadMessages(count, id, Relative.Before))
				// Am I doing this right?
				.Where(m => (DateTime.UtcNow - m.Timestamp.Date.ToUniversalTime()).TotalMilliseconds < (1000 * 60 * 60 * 24 * 7 * 2))
				.Select(m => m.Id)
				.ToList();
			messages.Add(id);
			await e.Channel.DeleteMessages(messages.Take(101).ToArray());
			await Logger.SendAsync($"{e.User.Name}#{e.User.Discriminator.ToString("D4")} (ID {e.User.Id}) deleted {messages.Count} message{((messages.Count == 1) ? string.Empty : "s")} in channel #{e.Channel.Name} (ID {e.Channel.Id}).");
		}
	}
}