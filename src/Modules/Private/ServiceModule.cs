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
                        "[speedruncom](https://www.speedrun.com), " +
                        "auditor");

                return ReplyAndDeleteAsync(string.Empty, embed: embed.Build());
            }

            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.ManageWebhooks)]
            [RequireBotPermission(GuildPermission.ManageWebhooks)]
            [Group("speedruncom"), Alias("srcom")]
            public class SpeedrunCom : InteractiveBase<SocketCommandContext>
            {
                public SpeedrunNotificationService? Service { get; set; }

                [Command("subscribe"), Alias("sub", "create", "hook")]
                public async Task Subscribe()
                {
                    var hooks = await Context.Guild.GetWebhooksAsync();
                    var (_, sub) = await Service!.FindSubscriptionAsync(hooks);
                    if (sub is null)
                    {
                        var webhook = await (Context.Channel as ITextChannel)!.CreateWebhookAsync("NeKzBot_SpeedrunComHook");
                        if (webhook is {})
                        {
                            if (!await Service.SubscribeAsync(webhook, "Subscribed!"))
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
                    var (hook, sub) = await Service!.FindSubscriptionAsync(hooks);
                    if (hook is {} && sub is {})
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

            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.ManageWebhooks)]
            [RequireBotPermission(GuildPermission.ManageWebhooks)]
            [Group("auditor"), Alias("audits")]
            public class Auditor : InteractiveBase<SocketCommandContext>
            {
                public AuditNotificationService? Service { get; set; }

                [Command("subscribe"), Alias("sub", "create", "hook")]
                public async Task Subscribe()
                {
                    var hooks = await Context.Guild.GetWebhooksAsync();
                    var (_, sub) = await Service!.FindSubscriptionAsync(hooks);
                    if (sub is null)
                    {
                        var webhook = await (Context.Channel as ITextChannel)!.CreateWebhookAsync("NeKzBot_AuditorHook");
                        if (webhook is {})
                        {
                            if (!await Service.SubscribeAsync(webhook, "Subscribed!"))
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
                    var (hook, sub) = await Service!.FindSubscriptionAsync(hooks);
                    if (hook is {} && sub is {})
                    {
                        await hook.DeleteAsync();
                        if (await Service.UnsubscribeAsync(sub))
                            await ReplyAndDeleteAsync("Unsubscribed from auditor service.");
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
