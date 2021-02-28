using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using NeKzBot.Extensions;
using NeKzBot.Services;

namespace NeKzBot.Modules.Private
{
    [RequireContext(ContextType.Guild)]
    public class AdminModule : InteractiveBase<SocketCommandContext>
    {
        private PinBoardService _pinBoard;

        public AdminModule(PinBoardService pinBoard)
        {
            _pinBoard = pinBoard;
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireBotPermission(GuildPermission.ManageGuild)]
        [Command("invites")]
        public async Task Invites()
        {
            var invites = await Context.Guild.GetInvitesAsync();

            var page = string.Empty;
            var pages = new List<string>();
            var count = 0;

            foreach (var invite in invites.OrderBy(x => x.CreatedAt))
            {
                if ((count % 5 == 0) && (count != 0))
                {
                    pages.Add(page);
                    page = string.Empty;
                }

                page += $"\n{invite.Id}" +
                    $" created by {invite.Inviter.Username}#{invite.Inviter.Discriminator}" +
                    $" at {(invite.CreatedAt.HasValue ? invite.CreatedAt.Value.ToString("yyyy-MM-dd") : "Unknown")}" +
                    $" ({invite.Uses}/{(((invite.MaxUses ?? 0) == 0) ? "∞" : $"{invite.MaxUses}")})";

                count++;
            }
            pages.Add(page);

            await PagedReplyAsync
            (
                new PaginatedMessage()
                {
                    Color = await Context.User.GetRoleColor(Context.Guild),
                    Pages = pages,
                    Title = "Server Invites",
                    Options = new PaginatedAppearanceOptions
                    {
                        DisplayInformationIcon = false,
                        Timeout = TimeSpan.FromSeconds(5 * 60)
                    }
                },
                false
            );
        }

        [RequireUserPermission(GuildPermission.ViewAuditLog)]
        [RequireBotPermission(GuildPermission.ViewAuditLog)]
        [Command("audits")]
        public async Task Audits(int auditCount = 10)
        {
            var audits = await (Context.Guild as IGuild).GetAuditLogsAsync(Math.Max(5, Math.Min(50, auditCount)));

            var page = string.Empty;
            var pages = new List<string>();
            var count = 0;

            foreach (var audit in audits)
            {
                if ((count % 5 == 0) && (count != 0))
                {
                    pages.Add(page);
                    page = string.Empty;
                }

                page += $"{audit.CreatedAt.ToString("yyyy-MM-dd")} {audit.Action}" +
                    $" by {audit.User.Username}#{audit.User.Discriminator}" +
                    (string.IsNullOrEmpty(audit.Reason) ? string.Empty : $" ({audit.Reason})") +
                    "\n";

                count++;
            }
            pages.Add(page);

            await PagedReplyAsync
            (
                new PaginatedMessage()
                {
                    Color = await Context.User.GetRoleColor(Context.Guild),
                    Pages = pages,
                    Title = "Server Audits",
                    Options = new PaginatedAppearanceOptions
                    {
                        DisplayInformationIcon = false,
                        Timeout = TimeSpan.FromSeconds(5 * 60)
                    }
                },
                true
            );
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireBotPermission(GuildPermission.ManageWebhooks)]
        [Command("pin", RunMode = RunMode.Async)]
        public async Task<RuntimeResult> Pin(ulong messageId)
        {
            var message = await Context.Channel.GetMessageAsync(messageId);
            if (message is null)
            {
                await ReplyAsync("Unable to find message.");
                return Ok();
            }

            var board = _pinBoard.Get(Context.Guild.Id);
            if (board is null)
            {
                var reply = await ReplyAsync(
                    "A pin board for this server does not exist yet. Do you want me to create one?");

                var response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(20));
                if (response is {})
                {
                    switch (response.Content.Trim().ToLower())
                    {
                        case "y":
                        case "ya":
                        case "ye":
                        case "yes":
                        case "yea":
                        case "yeah":
                        case "yep":
                            var channel = Context.Channel as ITextChannel;
                            if (channel is {})
                            {
                                var hook = await channel.CreateWebhookAsync("NeKzBot_PinBoardHook");
                                board = _pinBoard.Create(hook);
                                goto pin;
                            }
                            break;
                        default:
                            await reply.ModifyAsync(r => r.Content = "Interpreted answer as **NO**");
                            break;
                    }
                }

                return Ok();
            }

        pin:
            await _pinBoard.Send(message, board);

            return Ok();
        }

        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireBotPermission(GuildPermission.ManageWebhooks)]
        [Command("pin.set", RunMode = RunMode.Async)]
        public async Task<RuntimeResult> PinSet()
        {
            var guild = (Context.Guild as IGuild);
            if (guild is null) return Ok();

            var board = _pinBoard.Get(Context.Guild.Id);
            if (board is null)
            {
                var createReply = await ReplyAsync(
                    "A pin board for this server does not exist yet. Do you want me to create one?");

                var createResponse = await NextMessageAsync(timeout: TimeSpan.FromSeconds(20));
                if (createResponse is not null)
                {
                    switch (createResponse.Content.Trim().ToLower())
                    {
                        case "y":
                        case "ya":
                        case "ye":
                        case "yes":
                        case "yea":
                        case "yeah":
                        case "yep":
                            var channel = Context.Channel as ITextChannel;
                            if (channel is not null)
                            {
                                var hook = await channel.CreateWebhookAsync("NeKzBot_PinBoardHook");
                                board = _pinBoard.Create(hook);
                                if (board is not null)
                                    goto setting;
                            }
                            break;
                        default:
                            await createReply.ModifyAsync(r => r.Content = "Interpreted answer as **NO**");
                            break;
                    }
                }
                return Ok();
            }

        setting:
            var reply = await ReplyAsync("How many pins should be required?");
            var response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(20));

            if (response is not null
                && uint.TryParse(response.Content.Trim(), out var minimumReactions)
                && minimumReactions != 0)
            {
                reply = await ReplyAsync("Which emoji should be used for pinning messages?");
                response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(20));

                var emoji = response?.Content?.Trim();
                var (emojiName, isStandardEmoji) = ParseEmojiName(emoji);
                if (emojiName is not null)
                {
                    if (!isStandardEmoji)
                    {
                        var emotes = await guild.GetEmotesAsync();
                        var emote = emotes.FirstOrDefault(emote => emote.Name == emojiName);
                        if (emote is null)
                        {
                            await reply.ModifyAsync(r => r.Content = "Could not find this server emoji :(");
                            return Ok();
                        }

                        board.PinEmoji = emote.Name;
                    }
                    else
                    {
                        board.PinEmoji = emojiName;
                    }

                    board.MinimumReactions = minimumReactions;
                    _pinBoard.Update(board);

                    await ReplyAsync("Messages will now be pinned if they reach"
                        + $" {board.MinimumReactions} reaction{(board.MinimumReactions == 1 ? string.Empty : "s")}"
                        + $" with the {emoji} emoji.");

                    return Ok();
                }
            }

            await reply.ModifyAsync(r => r.Content = "Failed to interpret answer.");
            return Ok();
        }

        private (string?, bool) ParseEmojiName(string? emoji)
        {
            if (string.IsNullOrEmpty(emoji)) return (null, false);

            var isStandardEmoji = (new System.Globalization.StringInfo(emoji)).LengthInTextElements == 1;
            if (isStandardEmoji) return (emoji, true);

            var segments = emoji.Split(":");
            if (segments.Length != 3) return (null, false);

            return (segments[1], false);
        }
    }
}
