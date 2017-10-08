using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace NeKzBot.Modules.Public
{
	public class InfoModule : ModuleBase<SocketCommandContext>
	{
		public CommandService Commands { get; set; }

		[Command("info")]
		public async Task Info()
		{
			await ReplyAsync("", embed: new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					Name = Context.User.Username,
					IconUrl = Context.User.GetAvatarUrl(),
					Url = "https://github.com/NeKzor"
				},
				Color = await GetColor(Context.User, Context.Guild),
				Title = "NeKzBot",
				Description = "Version 2.0",
				Url = "https://github.com/NeKzor/NeKzBot"
			}
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Guilds";
				var guilds = Context.Client.Guilds.Count;
				var ownguilds = Context.Client.Guilds.Count(guild => guild.OwnerId == Context.User.Id);
				field.Value = $"Watching • {guilds - ownguilds}\nHosting • {ownguilds}\nTotal • {guilds}";
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Channels";
				var channels = Context.Client.Guilds.Sum(guild => guild.Channels.Count);
				var textchannels = Context.Client.Guilds.Sum(guild => guild.TextChannels.Count);
				var voicechannels = Context.Client.Guilds.Sum(guild => guild.VoiceChannels.Count);
				field.Value = $"Text • {textchannels}\nVoice • {voicechannels}\nTotal • {channels}";
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Users";
				var users = Context.Client.Guilds.Sum(guild => guild.Users.Count);
				var bots = Context.Client.Guilds.SelectMany(guild => guild.Users).Count(user => user.IsBot);
				field.Value = $"People • {(users - bots).ToString("#,###,###.##")}\nBots • {bots.ToString("#,###,###.##")}\nTotal • {users.ToString("#,###,###.##")}";
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Latency";
				field.Value = $"{Context.Client.Latency} ms";
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Heap Size";
				field.Value = $"{Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2)} MB";
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Threads";
				field.Value = Process.GetCurrentProcess().Threads.Count.ToString();
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Uptime";
				field.Value = (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"hh\:mm\:ss");
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Local Time";
				field.Value = DateTime.Now.ToUniversalTime().ToString("HH:mm:ss");
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Location";
				field.Value = "Graz, Austria";
			})
			.AddField(field =>
			{
				field.Name = "Library";
				field.Value = $"Discord.Net {DiscordConfig.Version}";
			})
			.AddField(field =>
			{
				field.Name = "Runtime";
				field.Value = RuntimeInformation.FrameworkDescription;
			})
			.AddField(field =>
			{
				field.Name = "Operating System";
				field.Value = RuntimeInformation.OSDescription;
			})
			.AddField(field =>
			{
				field.Name = "Modules";
				var output = string.Empty;
				foreach (var module in Commands.Modules.OrderBy(module => module.Name))
					output += $"\n**{module.Name}** with {module.Commands.Count} command{((module.Commands.Count == 1) ? string.Empty : "s")}";
				field.Value = (output != string.Empty) ? output : "None modules are loaded.";
			})
			.Build());
		}

		private Task<Color> GetColor(IUser user, IGuild guild)
		{
			if ((user != null) && (guild != null))
				foreach (var role in guild.Roles.Skip(1).OrderByDescending(r => r.Position))
					if ((user as SocketGuildUser)?.Roles.Contains(role) == true)
						return Task.FromResult(role.Color);
			return Task.FromResult(new Color(14, 186, 83));
		}
	}
}