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
    // Really old code here...
    public class StatsModule : InteractiveBase<SocketCommandContext>
    {
        private PinBoardService _pinBoard;

        public StatsModule(PinBoardService pinBoard)
        {
            _pinBoard = pinBoard;
        }

        [Ratelimit(1, 1, Measure.Minutes)]
        [RequireContext(ContextType.Guild)]
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
                // Fields
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
        [Ratelimit(1, 1, Measure.Minutes)]
        [RequireContext(ContextType.Guild)]
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
        [Ratelimit(1, 1, Measure.Minutes)]
        [RequireContext(ContextType.Guild)]
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
                // Fields
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
        [Ratelimit(1, 1, Measure.Minutes)]
        [RequireContext(ContextType.Guild)]
        [Command("id")]
        public async Task Id(bool ascending = true)
        {
            var order = string.Empty;
            var users = default(IEnumerable<SocketGuildUser>);

            if (ascending)
            {
                order = "(asc.)";
                users = Context.Guild.Users
                    .OrderBy(u => u.Id)
                    ;//.Take(10);
            }
            else
            {
                order = "(desc.)";
                users = Context.Guild.Users
                    .OrderByDescending(u => u.Id)
                    ;//.Take(10);
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
        [Ratelimit(1, 1, Measure.Minutes)]
        [RequireContext(ContextType.Guild)]
        [Command("disc"), Alias("discriminator")]
        public async Task Disc(bool ascending = true)
        {
            var order = string.Empty;
            var users = default(IEnumerable<SocketGuildUser>);

            if (ascending)
            {
                order = "(asc.)";
                users = Context.Guild.Users
                    .OrderBy(u => u.DiscriminatorValue)
                    ;//.Take(10);
            }
            else
            {
                order = "(desc.)";
                users = Context.Guild.Users
                    .OrderByDescending(u => u.DiscriminatorValue)
                    ;//.Take(10);
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
        [RequireContext(ContextType.Guild)]
        [Command("joined")]
        public async Task Joined(bool ascending = true)
        {
            var order = string.Empty;
            var users = default(IEnumerable<SocketGuildUser>);

            if (ascending)
            {
                order = "(asc.)";
                users = Context.Guild.Users
                    .OrderBy(u => u.JoinedAt)
                    ;//.Take(10);
            }
            else
            {
                order = "(desc.)";
                users = Context.Guild.Users
                    .OrderByDescending(u => u.JoinedAt)
                    ;//.Take(10);
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
        [RequireContext(ContextType.Guild)]
        [Command("created")]
        public async Task Created(bool ascending = true)
        {
            var order = string.Empty;
            var users = default(IEnumerable<SocketGuildUser>);

            if (ascending)
            {
                order = "(asc.)";
                users = Context.Guild.Users
                    .OrderBy(u => u.CreatedAt)
                    ;//.Take(10);
            }
            else
            {
                order = "(desc.)";
                users = Context.Guild.Users
                    .OrderByDescending(u => u.CreatedAt)
                    ;//.Take(10);
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
        // Not sure if this algorithm is fair:
        // Appending user discriminator with user id
        [RequireContext(ContextType.Guild)]
        [Command("score")]
        public async Task Score(bool ascending = true)
        {
            var order = string.Empty;
            var users = default(IEnumerable<SocketGuildUser>);

            if (ascending)
            {
                order = "(asc.)";
                users = Context.Guild.Users
                    .OrderBy(u => double.Parse($"{u.DiscriminatorValue}{u.Id}"))
                    ;//.Take(10);
            }
            else
            {
                order = "(desc.)";
                users = Context.Guild.Users
                    .OrderByDescending(u => double.Parse($"{u.DiscriminatorValue}{u.Id}"))
                    ;//.Take(10);
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

                var score = Math.Round(Math.Log(double.Parse($"{user.DiscriminatorValue}{user.Id}")), 3);
                page += $"\n{score.ToString("N3")} = {user.Username}";

                count++;
            }
            pages.Add(page);

            await PagedReplyAsync
            (
                new PaginatedMessage()
                {
                    Color = await Context.User.GetRoleColor(Context.Guild),
                    Pages = pages,
                    Title = $"Top User Scores {order}",
                    Options = new PaginatedAppearanceOptions
                    {
                        DisplayInformationIcon = false,
                        Timeout = TimeSpan.FromSeconds(5 * 60)
                    }
                },
                false
            );
        }
        [RequireContext(ContextType.Guild)]
        [Ratelimit(1, 1, Measure.Minutes, RatelimitFlags.NoLimitForAdmins | RatelimitFlags.ApplyPerGuild)]
        [Command("pins")]
        public async Task Pins(bool descending = true)
        {
            var guild = (Context.Guild as IGuild);
            if (guild is null) return;

            var pinData = _pinBoard.Get(Context.Guild.Id);
            if (pinData is null || pinData.PinEmoji is null) return;

            var pins = _pinBoard
                .GetMessages(Context.Guild.Id)
                .OrderByDescending(pin => pin.MessageId)
                .Take(5);

            if (pins.Count() == 0) return;

            var messages = await BuildPinCache(pinData, guild, pins);

            if (descending)
            {
                messages = messages.OrderByDescending(message => message.Reactions).ToList();
            }
            else
            {
                messages = messages.OrderBy(message => message.Reactions).ToList();
            }

            var reaction = pinData.PinEmoji.Length == 1 ? pinData.PinEmoji : $":{pinData.PinEmoji}:";

            var emotes = await guild.GetEmotesAsync();
            var emote = emotes.FirstOrDefault(emote => emote.Name == pinData.PinEmoji);
            if (emote is not null)
            {
                reaction = $"<:{emote.Name}:{emote.Id}>";
            }

            var page = string.Empty;
            var pages = new List<string>();
            var count = 0;

            foreach (var (message, reactions) in messages)
            {
                if ((count % 5 == 0) && (count != 0))
                {
                    pages.Add(page);
                    page = string.Empty;
                }

                var contentLength = message.Content.Length >= 10 ? 10 : message.Content.Length;
                var shortened = message.Content.Length > 10;
                var content = message.Content.Substring(0, contentLength) + (shortened ? "..." : string.Empty);
                content = message.Content.Length == 0 ? "*jump*" : content;

                page += $"\n{reaction} **x{reactions}** - [{content}]({message.GetJumpUrl()}) "
                    + $"{message.Author.Mention}";

                ++count;
            }
            pages.Add(page);

            var embed = new EmbedBuilder()
                .WithColor(await Context.User.GetRoleColor(Context.Guild))
                .WithTitle("Top 5 Latest Pins")
                .WithDescription(string.Join("\n", pages));

            await ReplyAndDeleteAsync(string.Empty, embed: embed.Build());
        }

        private static Dictionary<ulong, (DateTime, List<(IMessage Data, int Reactions)>)> _pinCache = new();

        private async Task<IEnumerable<(IMessage Data, int Reactions)>> BuildPinCache(
            PinBoardData pinData,
            IGuild guild,
            IEnumerable<PinMessageData> pins)
        {
            var cache = default((DateTime, List<(IMessage Data, int Reactions)>));

            if (!_pinCache.TryGetValue(guild.Id, out cache)) {
                cache = (DateTime.Now.AddMinutes(-6), new());
            }

            var (lastCheckedTime, cacheData) = cache;

            if ((DateTime.Now - lastCheckedTime).TotalMinutes < 5) {
                return cacheData;
            }

            cacheData.Clear();
            lastCheckedTime = DateTime.Now;

            var channels = await guild.GetChannelsAsync();

            foreach (var pin in pins)
            {
                var channel = channels.FirstOrDefault(channel => channel.Id == pin.ChannelId);
                if (channel is null) continue;

                var messageChannel = channel as ISocketMessageChannel;
                if (messageChannel is null) continue;

                var message = default(IMessage);
                try
                {
                    message = await messageChannel.GetMessageAsync(pin.MessageId);
                    if (message is null) continue;
                }
                catch
                {
                    continue;
                }

                var reactions = message.Reactions.FirstOrDefault(reaction => reaction.Key.Name == pinData.PinEmoji);
                if (reactions.Equals(default)) continue;

                cacheData.Add((Data: message, Reactions: reactions.Value.ReactionCount));
            }

            _pinCache[guild.Id] = (lastCheckedTime, cacheData);

            return cacheData;
        }
    }
}
