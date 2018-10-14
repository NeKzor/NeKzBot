using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Addons.Interactive;
using NeKzBot.Services.Notifications;

namespace NeKzBot.Modules.Private
{
    public class ServiceModule : ModuleBase<SocketCommandContext>
    {
        [Group("services"), Alias("service")]
        public class Services : InteractiveBase<SocketCommandContext>
        {
            [Command("?"), Alias("info", "help")]
            public Task QuestionMark()
            {
                var embed = new EmbedBuilder()
                    .WithColor(Color.Purple)
                    .WithDescription("**Webhook Services**\n" +
                        "Creation: .services.<subscription>.subscribe\n" +
                        "Deletion: .services.<subscription>.unsubscribe\n" +
                        "Available subscriptions: " +
                        //"[portal2boards](https://board.iverb.me), " +
                        "[speedruncom](https://www.speedrun.com)");

                return ReplyAndDeleteAsync(string.Empty, embed: embed.Build());
            }

            /* [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.ManageWebhooks)]
            [RequireBotPermission(GuildPermission.ManageWebhooks)]
            [Group("portal2boards"), Alias("p2b")]
            public class Portal2Boards : InteractiveBase<SocketCommandContext>
            {
                public Portal2NotificationService Service { get; set; }

                [Command("subscribe"), Alias("sub", "create", "hook")]
                public async Task Subscribe()
                {
                    var hooks = await Context.Guild.GetWebhooksAsync();
                    var (_, sub) = await Service.FindSubscriptionAsync(hooks);
                    if (sub == null)
                    {
                        var hook = await (Context.Channel as ITextChannel)?.CreateWebhookAsync("Portal2BoardsHook");
                        if (hook != null)
                        {
                            if (!await Service.SubscribeAsync(hook, "Portal2Boards Webhook Test!"))
                                await ReplyAndDeleteAsync("Failed to subscribe.", timeout: TimeSpan.FromSeconds(10));
                        }
                        else
                            await ReplyAndDeleteAsync("Failed to create a webhook.", timeout: TimeSpan.FromSeconds(10));
                    }
                    else
                        await ReplyAndDeleteAsync("Already subscribed to this service.", timeout: TimeSpan.FromSeconds(10));
                }
                [Command("unsubscribe"), Alias("unsub", "delete", "unhook")]
                public async Task Unsubscribe()
                {
                    var hooks = await Context.Guild.GetWebhooksAsync();
                    var (hook, sub) = await Service.FindSubscriptionAsync(hooks);
                    if (sub != null)
                    {
                        await hook.DeleteAsync();
                        if (await Service.UnsubscribeAsync(sub))
                            await ReplyAndDeleteAsync("Unsubscribed from Portal2Boards service.");
                        else
                            await ReplyAndDeleteAsync("Failed to unsubscribe.", timeout: TimeSpan.FromSeconds(10));
                    }
                    else
                        await ReplyAndDeleteAsync("Could not find a subscription in the database.", timeout: TimeSpan.FromSeconds(10));
                }
            } */

            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.ManageWebhooks)]
            [RequireBotPermission(GuildPermission.ManageWebhooks)]
            [Group("speedruncom"), Alias("srcom")]
            public class SpeedrunCom : InteractiveBase<SocketCommandContext>
            {
                public SpeedrunNotificationService Service { get; set; }

                [Command("subscribe"), Alias("sub", "create", "hook")]
                public async Task Subscribe()
                {
                    var hooks = await Context.Guild.GetWebhooksAsync();
                    var (_, sub) = await Service.FindSubscriptionAsync(hooks);
                    if (sub == null)
                    {
                        var webhook = await (Context.Channel as ITextChannel)?.CreateWebhookAsync("SpeedrunComHook");
                        if (webhook != null)
                        {
                            if (!await Service.SubscribeAsync(webhook, "SpeedrunCom Webhook Test!"))
                                await ReplyAndDeleteAsync("Failed to subscribe.", timeout: TimeSpan.FromSeconds(10));
                        }
                        else
                            await ReplyAndDeleteAsync("Failed to create a webhook.", timeout: TimeSpan.FromSeconds(10));
                    }
                    else
                        await ReplyAndDeleteAsync("Already subscribed to this service.", timeout: TimeSpan.FromSeconds(10));
                }
                [Command("unsubscribe"), Alias("unsub", "delete", "unhook")]
                public async Task Unsubscribe()
                {
                    var hooks = await Context.Guild.GetWebhooksAsync();
                    var (hook, sub) = await Service.FindSubscriptionAsync(hooks);
                    if (sub != null)
                    {
                        await hook.DeleteAsync();
                        if (await Service.UnsubscribeAsync(sub))
                            await ReplyAndDeleteAsync("Unsubscribed from SpeedrunCom service.");
                        else
                            await ReplyAndDeleteAsync("Failed to unsubscribe.", timeout: TimeSpan.FromSeconds(10));
                    }
                    else
                        await ReplyAndDeleteAsync("Could not find a subscription in the database.", timeout: TimeSpan.FromSeconds(10));
                }
            }
        }
    }
}
