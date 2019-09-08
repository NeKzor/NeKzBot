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
    public class AdminModule : InteractiveBase<SocketCommandContext>
    {
        [Ratelimit(3, 1, Measure.Minutes)]
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
                    $" at {invite.CreatedAt.Value.ToString("yyyy-MM-dd")} ({invite.Uses}/{(((invite.MaxUses ?? 0) == 0) ? "∞" : $"{invite.MaxUses}")})";

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
        [Ratelimit(3, 1, Measure.Minutes)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireBotPermission(GuildPermission.ManageGuild)]
        [Command("audits")]
        public async Task Audits(int auditCount = 10)
        {
            var audits = await (Context.Guild as IGuild).GetAuditLogsAsync(auditCount);

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
                false
            );
        }
    }
}
