using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;

namespace NeKzBot.Modules.Public
{
	public class InfoModule : InteractiveBase<SocketCommandContext>
	{
		public CommandService Commands { get; set; }

		[Command("info"), Alias("?")]
		public async Task Info()
		{
			await ReplyAndDeleteAsync("", embed: new EmbedBuilder
			{
				Color = await Context.User.GetRoleColor(Context.Guild),
				Title = "NeKzBot 2.0",
				Url = "https://github.com/NeKzor/NeKzBot"
			}
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Guilds";
				var guilds = Context.Client.Guilds.Count;
				var ownguilds = Context.Client.Guilds.Count(guild => guild.OwnerId == Context.User.Id);
				field.Value =
					$"Watching • {guilds - ownguilds}\n" +
					$"Hosting • {ownguilds}\n" +
					$"Total • {guilds}";
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Channels";
				var channels = Context.Client.Guilds.Sum(guild => guild.Channels.Count);
				var textchannels = Context.Client.Guilds.Sum(guild => guild.TextChannels.Count);
				var voicechannels = Context.Client.Guilds.Sum(guild => guild.VoiceChannels.Count);
				field.Value =
					$"Text • {textchannels}\n" +
					$"Voice • {voicechannels}\n" +
					$"Total • {channels}";
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Users";
				var users = Context.Client.Guilds.Sum(guild => guild.Users.Count);
				var bots = Context.Client.Guilds.SelectMany(guild => guild.Users).Count(user => user.IsBot);
				field.Value =
					$"People • {(users - bots).ToString("#,###,###.##")}\n" +
					$"Bots • {bots.ToString("#,###,###.##")}\n" +
					$"Total • {users.ToString("#,###,###.##")}";
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
				var modules = string.Empty;
				foreach (var module in Commands.Modules.Where(m => !m.IsSubmodule).OrderBy(m => m.Name))
				{
					modules +=
						$"\n**{module.Name}**" +
						$" with {module.Commands.Count + module.Submodules.Sum(sm => sm.Commands.Count)}" +
						$" command{((module.Commands.Count == 1) ? string.Empty : "s")}";
				}
				field.Value = (modules != string.Empty) ? modules : "Modules are not loaded.";
			})
			.Build(),
			timeout: TimeSpan.FromSeconds(60));
		}
	}
}