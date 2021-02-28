using System;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Addons.Preconditions;
using Discord.Commands;
using NeKzBot.Data;
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

                var (output, build, attempt) = _piston.GetOutput(result);
                var reply = await ReplyAsync(output);

                await _piston.TrackResult(new CodeTrackerData()
                {
                    UserId = Context.User.Id,
                    MessageId = Context.Message.Id,
                    ReplyId = reply.Id,
                    GuildId = Context.Guild?.Id,
                    ChannelId = Context.Channel.Id,
                    Attempt = build,
                    Build = attempt,
                });
            }
        }
    }
}
