using System;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using NeKzBot.Services.Notifciations;

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
				return ReplyAndDeleteAsync(
					"**Webhook Services**\n" +
					"Creation: .services.<subscription>.Subscribe\n" +
					"Deletion: .services.<subscription>.Unsubscribe\n" +
					"Available Subscriptions: Portal2Boards, SpeedrunCom", timeout: TimeSpan.FromSeconds(60));
			}

			[RequireContext(ContextType.Guild)]
			[RequireUserPermission(GuildPermission.ManageWebhooks)]
			[RequireBotPermission(GuildPermission.ManageWebhooks)]
			[Group("Portal2Boards"), Alias("portal2")]
			public class Portal2Boards : InteractiveBase<SocketCommandContext>
			{
				public Portal2NotificationService Service { get; set; }

				[Command("Subscribe"), Alias("sub", "create", "hook")]
				public async Task Subscribe()
				{
					var subscription = await Service.FindSubscription(Context.Channel.Id);
					if (subscription == null)
					{
						var webhook = await (Context.Channel as ITextChannel)?.CreateWebhookAsync(Service.UserName);
						if (webhook != null)
						{
							var subscribed = await Service.SubscribeAsync(webhook, true);
							if (!subscribed)
								await ReplyAndDeleteAsync("Failed to subscribe.", timeout: TimeSpan.FromSeconds(10));
						}
						else
							await ReplyAndDeleteAsync("Failed to create a webhook.", timeout: TimeSpan.FromSeconds(10));
					}
					else
						await ReplyAndDeleteAsync("Channel already subscribed to this service.", timeout: TimeSpan.FromSeconds(10));
				}

				[Command("Unsubscribe"), Alias("unsub", "delete", "unhook")]
				public async Task Unsubscribe()
				{
					var subscription = await Service.FindSubscription(Context.Channel.Id);
					if (subscription != null)
					{
						var webhook = await (Context.Channel as ITextChannel)?.GetWebhookAsync(subscription.WebhookId);
						if (webhook != null)
							await webhook.DeleteAsync();

						var result = await Service.UnsubscribeAsync(subscription);
						await ReplyAndDeleteAsync((result) ? "Unsubscribed from Portal2Boards service." : "Failed to unsubscribe.", timeout: TimeSpan.FromSeconds(60));
					}
					else
						await ReplyAndDeleteAsync("Could not find a subscription in the database.", timeout: TimeSpan.FromSeconds(60));
				}
			}

			[RequireContext(ContextType.Guild)]
			[RequireUserPermission(GuildPermission.ManageWebhooks)]
			[RequireBotPermission(GuildPermission.ManageWebhooks)]
			[Group("SpeedrunCom"), Alias("srcom")]
			public class SpeedrunCom : InteractiveBase<SocketCommandContext>
			{
				public SpeedrunNotificationService Service { get; set; }

				[Command("Subscribe"), Alias("sub", "create", "hook")]
				public async Task Subscribe()
				{
					var subscription = await Service.FindSubscription(Context.Channel.Id);
					if (subscription == null)
					{
						var webhook = await (Context.Channel as ITextChannel)?.CreateWebhookAsync(Service.UserName);
						if (webhook != null)
						{
							var subscribed = await Service.SubscribeAsync(webhook, true);
							if (!subscribed)
								await ReplyAndDeleteAsync("Failed to subscribe.", timeout: TimeSpan.FromSeconds(10));
						}
						else
							await ReplyAndDeleteAsync("Failed to create a webhook.", timeout: TimeSpan.FromSeconds(10));
					}
					else
						await ReplyAndDeleteAsync("Channel already subscribed to this service.", timeout: TimeSpan.FromSeconds(10));
				}

				[Command("Unsubscribe"), Alias("unsub", "delete", "unhook")]
				public async Task Unsubscribe()
				{
					var subscription = await Service.FindSubscription(Context.Channel.Id);
					if (subscription != null)
					{
						var webhook = await (Context.Channel as ITextChannel)?.GetWebhookAsync(subscription.WebhookId);
						if (webhook != null)
							await webhook.DeleteAsync();

						var result = await Service.UnsubscribeAsync(subscription);
						await ReplyAndDeleteAsync((result) ? "Unsubscribed from SpeedrunCom service." : "Failed to unsubscribe.", timeout: TimeSpan.FromSeconds(60));
					}
					else
						await ReplyAndDeleteAsync("Could not find a subscription in the database.", timeout: TimeSpan.FromSeconds(60));
				}
			}
		}
	}
}