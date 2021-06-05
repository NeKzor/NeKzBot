using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Addons.Interactive;
using NeKzBot.Services.Notifications.Auditor;
using NeKzBot.Services.Notifications.Speedrun;

namespace NeKzBot.Modules.Private
{
    [RequireContext(ContextType.Guild)]
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

            [RequireUserPermission(GuildPermission.ManageWebhooks)]
            [RequireBotPermission(GuildPermission.ManageWebhooks)]
            [Group("speedruncom"), Alias("srcom")]
            public class SpeedrunCom : InteractiveBase<SocketCommandContext>
            {
                private readonly SpeedrunNotificationService _service;

                public SpeedrunCom(SpeedrunNotificationService service)
                {
                    _service = service;
                }

                [Command("subscribe"), Alias("sub", "create", "hook")]
                public async Task Subscribe()
                {
                    var channel = Context.Channel as ITextChannel;
                    if (channel is null)
                    {
                        await ReplyAndDeleteAsync("This is not a text channel.", timeout: TimeSpan.FromSeconds(10));
                        return;
                    }

                    var hooks = await Context.Guild.GetWebhooksAsync();
                    var (_, sub) = await _service.FindSubscriptionAsync(hooks);
                    if (sub is null)
                    {
                        var webhook = await channel.CreateWebhookAsync("NeKzBot_SpeedrunComHook");
                        if (webhook is not null)
                        {
                            if (!await _service.SubscribeAsync(webhook, "Subscribed!"))
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
                    var (hook, sub) = await _service.FindSubscriptionAsync(hooks);
                    if (hook is not null && sub is not null)
                    {
                        await hook.DeleteAsync();
                        if (await _service.UnsubscribeAsync(sub))
                            await ReplyAndDeleteAsync("Unsubscribed from SpeedrunCom service.");
                        else
                            await ReplyAndDeleteAsync("Failed to unsubscribe.", timeout: TimeSpan.FromSeconds(10));
                    }
                    else
                        await ReplyAndDeleteAsync("Could not find a subscription in the database.", timeout: TimeSpan.FromSeconds(10));
                }
            }

            [RequireUserPermission(GuildPermission.ManageWebhooks)]
            [RequireBotPermission(GuildPermission.ManageWebhooks)]
            [Group("auditor"), Alias("audits")]
            public class Auditor : InteractiveBase<SocketCommandContext>
            {
                private readonly AuditorNotificationService _service;

                public Auditor(AuditorNotificationService service)
                {
                    _service = service;
                }

                [Command("subscribe"), Alias("sub", "create", "hook")]
                public async Task Subscribe()
                {
                    var channel = Context.Channel as ITextChannel;
                    if (channel is null)
                    {
                        await ReplyAndDeleteAsync("This is not a text channel.", timeout: TimeSpan.FromSeconds(10));
                        return;
                    }

                    var hooks = await Context.Guild.GetWebhooksAsync();
                    var (_, sub) = await _service.FindSubscriptionAsync(hooks);
                    if (sub is null)
                    {
                        var webhook = await channel.CreateWebhookAsync("NeKzBot_AuditorHook");
                        if (webhook is not null)
                        {
                            if (!await _service.SubscribeAsync(webhook, "Subscribed!"))
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
                    var (hook, sub) = await _service.FindSubscriptionAsync(hooks);
                    if (hook is not null && sub is not null)
                    {
                        await hook.DeleteAsync();
                        if (await _service.UnsubscribeAsync(sub))
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
