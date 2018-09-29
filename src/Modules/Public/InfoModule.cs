using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Addons.Preconditions;
using Discord.Commands;
using Discord.WebSocket;

namespace NeKzBot.Modules.Public
{
    public class InfoModule : InteractiveBase<SocketCommandContext>
    {
        public CommandService Commands { get; set; }

        [Ratelimit(3, 1, Measure.Minutes)]
        [Command("info"), Alias("?")]
        public async Task Info()
        {
            var embed = new EmbedBuilder()
                .WithColor(await Context.User.GetRoleColor(Context.Guild))
                .WithTitle("NeKzBot Info")
                .WithUrl("https://nekzor.github.io/NeKzBot")
                // Fields
                .AddField("Latency", $"{Context.Client.Latency} ms", true)
                .AddField("Heap Size", $"{Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2)} MB", true)
                .AddField("Threads", $"{Process.GetCurrentProcess().Threads.Count}", true)
                .AddField("Uptime", (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"hh\:mm\:ss"), true)
                .AddField("Local Time (UTC)", DateTime.UtcNow.ToString("HH:mm:ss"), true)
                .AddField("Location", "Graz, Austria", true)
                .AddField("Library", $"Discord.Net {DiscordConfig.Version}", true)
                .AddField("Runtime", RuntimeInformation.FrameworkDescription, false)
                .AddField("Operating System", RuntimeInformation.OSDescription, true);

            await ReplyAndDeleteAsync(string.Empty, embed: embed.Build());
        }
        [Ratelimit(3, 1, Measure.Minutes)]
        [Command("stats")]
        public async Task Stats()
        {
            var watching = Context.Client.Guilds.Count;
            var hosting = Context.Client.Guilds
                .Count(g => g.OwnerId == Context.User.Id);

            var channels = Context.Client.Guilds
                .Sum(g => g.Channels.Count);
            var text = Context.Client.Guilds
                .Sum(g => g.TextChannels.Count);
            var voice = Context.Client.Guilds
                .Sum(g => g.VoiceChannels.Count);

            var users = Context.Client.Guilds
                .Sum(g => g.Users.Count);
            var bots = Context.Client.Guilds
                .SelectMany(g => g.Users)
                .Count(u => u.IsBot);

            var embed = new EmbedBuilder()
                .WithColor(await Context.User.GetRoleColor(Context.Guild))
                .WithTitle("NeKzBot Stats")
                .WithUrl("https://nekzor.github.io/NeKzBot")
                // Fields
                .AddField("Guilds",
                    $"Watching • {watching - hosting}\n" +
                    $"Hosting • {hosting}\n" +
                    $"Total • {watching}",
                    true)
                .AddField("Channels",
                    $"Text • {text}\n" +
                    $"Voice • {voice}\n" +
                    $"Total • {channels}",
                    true)
                .AddField("Users",
                    $"People • {(users - bots).ToString("#,###,###.##")}\n" +
                    $"Bots • {bots.ToString("#,###,###.##")}\n" +
                    $"Total • {users.ToString("#,###,###.##")}",
                    true);

            await ReplyAndDeleteAsync(string.Empty, embed: embed.Build());
        }
        [Command("invite")]
        public async Task Invite()
        {
            var invite = "https://discordapp.com/oauth2/authorize?scope=bot" +
                $"&client_id={Context.Client.CurrentUser.Id}" +
                "&permissions=536873984";

            var embed = new EmbedBuilder()
                .WithColor(await Context.User.GetRoleColor(Context.Guild))
                .WithDescription($"[Click here to add NeKzBot to your server!]({invite})");

            await ReplyAndDeleteAsync(string.Empty, embed: embed.Build());
        }
        [Command("modules"), Alias("help")]
        public async Task Modules()
        {
            var modules = Commands.Modules
                .Where(m => !m.IsSubmodule)
                .OrderBy(m => m.Name);

            var list = string.Empty;
            foreach (var module in modules)
            {
                var commands = module.Commands.Count
                    + module.Submodules
                        .Sum(m => m.Commands.Count + m.Submodules
                        .Sum(mm => mm.Commands.Count));
                list +=
                    $"\n**{module.Name}**" +
                    $" with {commands}" +
                    $" command{((commands == 1) ? string.Empty : "s")}";
            }

            var embed = new EmbedBuilder()
                .WithColor(await Context.User.GetRoleColor(Context.Guild))
                .WithTitle("NeKzBot Modules")
                .WithUrl("https://nekzor.github.io/NeKzBot#modules")
                .WithDescription((list != string.Empty) ? list : "Modules are not loaded.");

            await ReplyAndDeleteAsync(string.Empty, embed: embed.Build());
        }
    }
}
