using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Addons.Preconditions;
using Discord.Commands;
using Discord.WebSocket;
using NeKzBot.Data;
using NeKzBot.Extensions;
using NeKzBot.Services;

namespace NeKzBot.Modules.Public
{
    [RequireContext(ContextType.Guild)]
    public class StatsModule : InteractiveBase<SocketCommandContext>
    {
        private PinBoardService _pinBoard;

        public StatsModule(PinBoardService pinBoard)
        {
            _pinBoard = pinBoard;
        }

        [Ratelimit(6, 1, Measure.Minutes, RatelimitFlags.NoLimitForAdmins | RatelimitFlags.ApplyPerGuild)]
        [Command("guild"), Alias("server")]
        public async Task Guild()
        {
            var owner = (Context.Guild.Owner != null)
                ? $"{Context.Guild.Owner.Username}#{Context.Guild.Owner.Discriminator}\n{Context.Guild.OwnerId}"
                : $"{Context.Guild.OwnerId}";

            var members = Context.Guild.MemberCount;
            var users = Context.Guild.Users.Count;
            var bots = Context.Guild.Users.Count(u => u.IsBot);

            var features = string.Empty;
            foreach (var feature in Context.Guild.Features)
                features += $"\n`{feature}`";

            var splash = (!string.IsNullOrEmpty(Context.Guild.SplashUrl))
                ? $"\n[Link]({Context.Guild.SplashUrl})"
                : string.Empty;

            var embed = new EmbedBuilder()
                .WithColor(await Context.User.GetRoleColor(Context.Guild))
                .AddField("Owner", owner, true)
                .AddField("Created At", Context.Guild.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"), true)
                .AddField("ID", $"{Context.Guild.Id}", true)
                .AddField("Members", (users != members)
                    ? $"Users • {users - bots}\n" +
                        $"Bots • {bots}\n" +
                        $"Online • {users}\n" +
                        $"Offline • {members - users}\n" +
                        $"Total • {members}"
                    : $"Users • {users - bots}\n" +
                        $"Bots • {bots}\n" +
                        $"Total • {members}",
                      true)
                .AddField("Channels",
                    $"Text • {Context.Guild.VoiceChannels.Count}\n" +
                    $"Voice • {Context.Guild.TextChannels.Count}\n" +
                    $"Total • {Context.Guild.Channels.Count}",
                    true)
                .AddField("Features", (features != string.Empty)
                    ? $"{Context.Guild.Features.Count}{features}"
                    : "None",
                    true)
                .AddField("Default Channel", Context.Guild.DefaultChannel.Name, true)
                .AddField("Verification Level", $"{Context.Guild.VerificationLevel}", true)
                .AddField("Links", $"[Icon]({Context.Guild.IconUrl})" +
                    splash +
                    $"\n[Banner](https://discordapp.com/api/guilds/{Context.Guild.Id}/embed.png?style=banner1)",
                    true);

            await ReplyAndDeleteAsync(string.Empty, embed: embed.Build());
        }

        [Ratelimit(6, 1, Measure.Minutes, RatelimitFlags.NoLimitForAdmins | RatelimitFlags.ApplyPerGuild)]
        [Command("hierarchy")]
        public async Task Hierarchy()
        {
            var members = Context.Guild.Roles
                .OrderByDescending(r => r.Position);

            var result = string.Empty;
            var position = -1;

            foreach (var member in members)
            {
                var temp = string.Empty;
                if (position != member.Position)
                    temp = $"\n{members.FirstOrDefault().Position - member.Position + 1}. • `{member.Name}`";
                else
                    temp = $", `{member.Name}`";

                if (result.Length + temp.Length > 2048)
                    break;

                result += temp;
                position = member.Position;
            }

            var embed = new EmbedBuilder()
                .WithTitle("Guild Hierarchy")
                .WithColor(await Context.User.GetRoleColor(Context.Guild))
                .WithDescription(result);

            await ReplyAndDeleteAsync(string.Empty, embed: embed.Build());
        }

        [Ratelimit(6, 1, Measure.Minutes, RatelimitFlags.NoLimitForAdmins | RatelimitFlags.ApplyPerGuild)]
        [Command("channel")]
        public async Task Channel()
        {
            var channel = Context.Channel as SocketGuildChannel;
            if (channel is null)
            {
                await ReplyAndDeleteAsync("This is not a socket guild channel.", timeout: TimeSpan.FromSeconds(10));
                return;
            }

            var users = channel.Users;
            var bots = users.Count(m => m.IsBot);

            var embed = new EmbedBuilder()
                .WithColor(await Context.User.GetRoleColor(Context.Guild))
                .AddField("Name", Context.Channel.Name, true)
                .AddField("Created At", Context.Channel.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"), true)
                .AddField("ID", $"{Context.Channel.Id}", true)
                .AddField("Members",
                    $"Users • {users.Count - bots}\n" +
                    $"Bots • {bots}\n" +
                    $"Total • {users.Count}",
                    true)
                .AddField("Position", $"{channel.Position}", true)
                .AddField("Permissions", $"{channel.PermissionOverwrites.Count}", true);

            await ReplyAndDeleteAsync(string.Empty, embed: embed.Build());
        }

        [Ratelimit(6, 1, Measure.Minutes, RatelimitFlags.NoLimitForAdmins | RatelimitFlags.ApplyPerGuild)]
        [Command("id")]
        public async Task Id(bool ascending = true)
        {
            var order = string.Empty;
            var users = default(IEnumerable<SocketGuildUser>);

            if (ascending)
            {
                order = "(asc.)";
                users = Context.Guild.Users
                    .OrderBy(u => u.Id);
            }
            else
            {
                order = "(desc.)";
                users = Context.Guild.Users
                    .OrderByDescending(u => u.Id);
            }

            var page = string.Empty;
            var pages = new List<string>();
            var count = 0;

            foreach (var user in users)
            {
                if ((count % 5 == 0) && (count != 0))
                {
                    pages.Add(page);
                    page = string.Empty;
                }

                page += $"\n{user.Id} = {user.Username}";

                count++;
            }
            pages.Add(page);

            await PagedReplyAsync
            (
                new PaginatedMessage()
                {
                    Color = await Context.User.GetRoleColor(Context.Guild),
                    Pages = pages,
                    Title = $"Top User IDs {order}",
                    Options = new PaginatedAppearanceOptions
                    {
                        DisplayInformationIcon = false,
                        Timeout = TimeSpan.FromSeconds(5 * 60)
                    }
                },
                false
            );
        }

        [Ratelimit(6, 1, Measure.Minutes, RatelimitFlags.NoLimitForAdmins | RatelimitFlags.ApplyPerGuild)]
        [Command("disc"), Alias("discriminator")]
        public async Task Disc(bool ascending = true)
        {
            var order = string.Empty;
            var users = default(IEnumerable<SocketGuildUser>);

            if (ascending)
            {
                order = "(asc.)";
                users = Context.Guild.Users
                    .OrderBy(u => u.DiscriminatorValue);
            }
            else
            {
                order = "(desc.)";
                users = Context.Guild.Users
                    .OrderByDescending(u => u.DiscriminatorValue);
            }

            var page = string.Empty;
            var pages = new List<string>();
            var count = 0;

            foreach (var user in users)
            {
                if ((count % 5 == 0) && (count != 0))
                {
                    pages.Add(page);
                    page = string.Empty;
                }

                page += $"\n#{user.Discriminator} = {user.Username}";

                count++;
            }
            pages.Add(page);

            await PagedReplyAsync
            (
                new PaginatedMessage()
                {
                    Color = await Context.User.GetRoleColor(Context.Guild),
                    Pages = pages,
                    Title = $"Top User Discriminators {order}",
                    Options = new PaginatedAppearanceOptions
                    {
                        DisplayInformationIcon = false,
                        Timeout = TimeSpan.FromSeconds(5 * 60)
                    }
                },
                false
            );
        }

        [Ratelimit(6, 1, Measure.Minutes, RatelimitFlags.NoLimitForAdmins | RatelimitFlags.ApplyPerGuild)]
        [Command("joined")]
        public async Task Joined(bool ascending = true)
        {
            var order = string.Empty;
            var users = default(IEnumerable<SocketGuildUser>);

            if (ascending)
            {
                order = "(asc.)";
                users = Context.Guild.Users
                    .OrderBy(u => u.JoinedAt);
            }
            else
            {
                order = "(desc.)";
                users = Context.Guild.Users
                    .OrderByDescending(u => u.JoinedAt);
            }

            var page = string.Empty;
            var pages = new List<string>();
            var count = 0;

            foreach (var user in users)
            {
                if ((count % 5 == 0) && (count != 0))
                {
                    pages.Add(page);
                    page = string.Empty;
                }

                page += $"\n{user.JoinedAt?.ToString(@"yyyy\-MM\-dd hh\:mm\:ss")} = {user.Username}";

                count++;
            }
            pages.Add(page);

            await PagedReplyAsync
            (
                new PaginatedMessage()
                {
                    Color = await Context.User.GetRoleColor(Context.Guild),
                    Pages = pages,
                    Title = $"Top User Joined Dates {order}",
                    Options = new PaginatedAppearanceOptions
                    {
                        DisplayInformationIcon = false,
                        Timeout = TimeSpan.FromSeconds(5 * 60)
                    }
                },
                false
            );
        }

        [Ratelimit(6, 1, Measure.Minutes, RatelimitFlags.NoLimitForAdmins | RatelimitFlags.ApplyPerGuild)]
        [Command("created")]
        public async Task Created(bool ascending = true)
        {
            var order = string.Empty;
            var users = default(IEnumerable<SocketGuildUser>);

            if (ascending)
            {
                order = "(asc.)";
                users = Context.Guild.Users
                    .OrderBy(u => u.CreatedAt);
            }
            else
            {
                order = "(desc.)";
                users = Context.Guild.Users
                    .OrderByDescending(u => u.CreatedAt);
            }

            var page = string.Empty;
            var pages = new List<string>();
            var count = 0;

            foreach (var user in users)
            {
                if ((count % 5 == 0) && (count != 0))
                {
                    pages.Add(page);
                    page = string.Empty;
                }

                page += $"\n{user.CreatedAt.ToString(@"yyyy\-MM\-dd hh\:mm\:ss")} = {user.Username}";

                count++;
            }
            pages.Add(page);

            await PagedReplyAsync
            (
                new PaginatedMessage()
                {
                    Color = await Context.User.GetRoleColor(Context.Guild),
                    Pages = pages,
                    Title = $"Top User Created Dates {order}",
                    Options = new PaginatedAppearanceOptions
                    {
                        DisplayInformationIcon = false,
                        Timeout = TimeSpan.FromSeconds(5 * 60)
                    }
                },
                false
            );
        }

        [Ratelimit(3, 1, Measure.Minutes, RatelimitFlags.NoLimitForAdmins | RatelimitFlags.ApplyPerGuild)]
        [Command("pin.champions")]
        public async Task PinChampions(bool descending = true)
        {
            var guild = (Context.Guild as IGuild);
            if (guild is null) return;

            var pinData = _pinBoard.Get(Context.Guild.Id);
            if (pinData is null || pinData.PinEmoji is null) return;

            var champions = _pinBoard
                .GetMessages(Context.Guild.Id)
                .Where(p => p.Champion is not null)
                .GroupBy(p => p.Champion!.UserId)
                .Select(g => new { Champion = g.First().Champion!, Count = g.Count() });

            var order = string.Empty;

            if (descending)
            {
                order = "(desc.)";
                champions = champions.OrderByDescending(p => p.Count);
            }
            else
            {
                order = "(asc.)";
                champions = champions.OrderBy(p => p.Count);
            }

            var page = string.Empty;
            var pages = new List<string>();
            var count = 0;

            foreach (var champion in champions)
            {
                if ((count % 5 == 0) && (count != 0))
                {
                    pages.Add(page);
                    page = string.Empty;
                }

                var championName = champion.Champion.Nickname
                    ?? champion.Champion.Name
                    ?? champion.Champion.UserId.ToString();

                page += $"\n{championName} | {champion.Count}";

                count++;
            }
            pages.Add(page);

            await PagedReplyAsync
            (
                new PaginatedMessage()
                {
                    Color = await Context.User.GetRoleColor(Context.Guild),
                    Pages = pages,
                    Title = $"Top Pin Champions {order}",
                    Options = new PaginatedAppearanceOptions
                    {
                        DisplayInformationIcon = false,
                        Timeout = TimeSpan.FromSeconds(5 * 60)
                    }
                },
                false
            );
        }

        [Ratelimit(3, 1, Measure.Minutes, RatelimitFlags.NoLimitForAdmins | RatelimitFlags.ApplyPerGuild)]
        [Command("pin.pinners")]
        public async Task PinPinners(bool descending = true)
        {
            var guild = (Context.Guild as IGuild);
            if (guild is null) return;

            var pinData = _pinBoard.Get(Context.Guild.Id);
            if (pinData is null || pinData.PinEmoji is null) return;

            var pinners = _pinBoard
                .GetMessages(Context.Guild.Id)
                .Where(p => p.Pinners is not null)
                .SelectMany(p => p.Pinners)
                .GroupBy(p => p.UserId)
                .Select(g => new { Pinner = g.First(), Count = g.Count() });

            var order = string.Empty;

            if (descending)
            {
                order = "(desc.)";
                pinners = pinners.OrderByDescending(p => p.Count);
            }
            else
            {
                order = "(asc.)";
                pinners = pinners.OrderBy(p => p.Count);
            }

            var page = string.Empty;
            var pages = new List<string>();
            var count = 0;

            foreach (var pinner in pinners)
            {
                if ((count % 5 == 0) && (count != 0))
                {
                    pages.Add(page);
                    page = string.Empty;
                }

                var pinnerName = pinner.Pinner.Nickname ?? pinner.Pinner.Name ?? pinner.Pinner.UserId.ToString();

                page += $"\n{pinnerName} | {pinner.Count}";

                count++;
            }
            pages.Add(page);

            await PagedReplyAsync
            (
                new PaginatedMessage()
                {
                    Color = await Context.User.GetRoleColor(Context.Guild),
                    Pages = pages,
                    Title = $"Top Pin Pinners {order}",
                    Options = new PaginatedAppearanceOptions
                    {
                        DisplayInformationIcon = false,
                        Timeout = TimeSpan.FromSeconds(5 * 60)
                    }
                },
                false
            );
        }
    }
}
