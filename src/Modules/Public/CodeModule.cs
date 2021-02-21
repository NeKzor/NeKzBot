using System;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Addons.Preconditions;
using Discord.Commands;
using NeKzBot.Services;

namespace NeKzBot.Modules.Public
{
    public class CodeModule : ModuleBase<SocketCommandContext>
    {
        [Group("code"), Alias("piston")]
        public class Piston : InteractiveBase<SocketCommandContext>
        {
            private readonly PistonService _piston;

            public Piston(PistonService piston)
            {
                _piston = piston;
            }

            [Command("?"), Alias("info", "help")]
            public Task QuestionMark()
            {
                var embed = new EmbedBuilder()
                    .WithColor(Color.Orange)
                    .WithDescription("[Powered by Piston](https://github.com/engineer-man/piston)");

                return ReplyAndDeleteAsync(string.Empty, embed: embed.Build());
            }

            [Ratelimit(6, 1, Measure.Minutes)]
            [Command("run"), Alias("evaluate", "eval", "execute", "exec")]
            public async Task run(string language, [Remainder]string code)
            {
                var version = _piston.FindVersion(language);
                if (version is null)
                {
                    await ReplyAndDeleteAsync("Language not supported.", timeout: TimeSpan.FromSeconds(10));
                    return;
                }

                var (source, stdIn, args) = _piston.Parse(code);
                if (source is null)
                {
                    await ReplyAndDeleteAsync("Invalid code input.", timeout: TimeSpan.FromSeconds(10));
                    return;
                }

                var result = await _piston.Run(version, source, stdIn, args);
                if (result is null)
                {
                    await ReplyAndDeleteAsync("Failed to execute code.", timeout: TimeSpan.FromSeconds(10));
                    return;
                }

                var reply = await ReplyAsync(_piston.GetOutput(result));

                await _piston.TrackResult
                (
                    Context.Message.Id,
                    Context.User.Id,
                    Context.Guild.Id,
                    Context.Channel.Id,
                    reply.Id
                );
            }

            [RequireOwner]
            [Command("update")]
            public async Task update()
            {
                await _piston.UpdateVersions();
                await ReplyAndDeleteAsync("Updated list of supported languages.");
            }
        }
    }
}
