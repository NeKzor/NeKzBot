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
				return ReplyAndDeleteAsync
				(
					"**Webhook Services**\n" +
					"Creation: .services.<subscription>.subscribe\n" +
					"Deletion: .services.<subscription>.unsubscribe\n" +
					"Available Subscriptions: portal2boards, speedruncom",
					timeout: TimeSpan.FromSeconds(60)
				);
			}

			[RequireContext(ContextType.Guild)]
			[RequireUserPermission(GuildPermission.ManageWebhooks)]
			[RequireBotPermission(GuildPermission.ManageWebhooks)]
			[Group("portal2boards"), Alias("portal2")]
			public class Portal2Boards : InteractiveBase<SocketCommandContext>
			{
				public Portal2NotificationService Service { get; set; }

				[Command("subscribe"), Alias("sub", "create", "hook")]
				public async Task Subscribe()
				{
					var subscription = await Service.FindSubscription(Context.Channel.Id);
					if (subscription == null)
					{
						var webhook = await (Context.Channel as ITextChannel)?.CreateWebhookAsync(Service._userName);
						if (webhook != null)
						{
							if (!await Service.SubscribeAsync(webhook, true, "Portal2Boards Webhook Test!"))
								await ReplyAndDeleteAsync("Failed to subscribe.", timeout: TimeSpan.FromSeconds(10));
						}
						else
							await ReplyAndDeleteAsync("Failed to create a webhook.", timeout: TimeSpan.FromSeconds(10));
					}
					else
						await ReplyAndDeleteAsync("Channel already subscribed to this service.", timeout: TimeSpan.FromSeconds(10));
				}
				[Command("unsubscribe"), Alias("unsub", "delete", "unhook")]
				public async Task Unsubscribe()
				{
					var subscription = await Service.FindSubscription(Context.Channel.Id);
					if (subscription != null)
					{
						var webhook = await (Context.Channel as ITextChannel)?.GetWebhookAsync(subscription.Webhook.Id);
						if (webhook != null)
							await webhook.DeleteAsync();

						if (await Service.UnsubscribeAsync(subscription))
							await ReplyAndDeleteAsync("Unsubscribed from Portal2Boards service.", timeout: TimeSpan.FromSeconds(60));
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
			[Group("speedruncom"), Alias("srcom")]
			public class SpeedrunCom : InteractiveBase<SocketCommandContext>
			{
				public SpeedrunNotificationService Service { get; set; }

				[Command("subscribe"), Alias("sub", "create", "hook")]
				public async Task Subscribe()
				{
					var subscription = await Service.FindSubscription(Context.Channel.Id);
					if (subscription == null)
					{
						var webhook = await (Context.Channel as ITextChannel)?.CreateWebhookAsync(Service._userName);
						if (webhook != null)
						{
							if (!await Service.SubscribeAsync(webhook, true, "SpeedrunCom Webhook Test!"))
								await ReplyAndDeleteAsync("Failed to subscribe.", timeout: TimeSpan.FromSeconds(10));
						}
						else
							await ReplyAndDeleteAsync("Failed to create a webhook.", timeout: TimeSpan.FromSeconds(10));
					}
					else
						await ReplyAndDeleteAsync("Channel already subscribed to this service.", timeout: TimeSpan.FromSeconds(10));
				}
				[Command("unsubscribe"), Alias("unsub", "delete", "unhook")]
				public async Task Unsubscribe()
				{
					var subscription = await Service.FindSubscription(Context.Channel.Id);
					if (subscription != null)
					{
						var webhook = await (Context.Channel as ITextChannel)?.GetWebhookAsync(subscription.Webhook.Id);
						if (webhook != null)
							await webhook.DeleteAsync();

						if (await Service.UnsubscribeAsync(subscription))
							await ReplyAndDeleteAsync("Unsubscribed from SpeedrunCom service.", timeout: TimeSpan.FromSeconds(60));
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