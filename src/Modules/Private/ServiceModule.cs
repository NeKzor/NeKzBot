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
				return ReplyAndDeleteAsync("**Webhook Services**\nCreation: .services.<subscription>.Subscribe\nDeletion: .services.<subscription>.Unsubscribe\nAvailable Subscriptions: Portal2Boards, SpeedrunCom", timeout: TimeSpan.FromSeconds(60));
			}

			[RequireUserPermission(GuildPermission.ManageWebhooks)]
			[RequireBotPermission(GuildPermission.ManageWebhooks)]
			[Group("Portal2Records"), Alias("portal2", "portal2boards")]
			public class Portal2Records : InteractiveBase<SocketCommandContext>
			{
				public Portal2NotificationService Portal2BoardsService { get; set; }

				[Command("Subscribe"), Alias("create", "hook")]
				public async Task Subscribe()
				{
					// TODO: Create webhook + creation logic
					var result = await Portal2BoardsService.SubscribeAsync(0ul, "", true);
					await ReplyAndDeleteAsync((result) ? "Webhook test has been sent." : "Could not subscribe.", timeout: TimeSpan.FromSeconds(60));
				}

				[Command("Unsubscribe"), Alias("unsub", "delete", "unhook")]
				public async Task Unsubscribe()
				{
					// TODO: Delete webhook + deletion logic
					var result = await Portal2BoardsService.UnsubscribeAsync(0ul, true);
					await ReplyAndDeleteAsync((result) ? "Unsubscribed from SpeedrunCom service." : "Failed to unsubscribe.", timeout: TimeSpan.FromSeconds(60));
				}
			}

			[RequireUserPermission(GuildPermission.ManageWebhooks)]
			[RequireBotPermission(GuildPermission.ManageWebhooks)]
			[Group("SpeedrunCom"), Alias("srcom")]
			public class SpeedrunCom : InteractiveBase<SocketCommandContext>
			{
				public SpeedrunNotificationService SpeedrunComService { get; set; }

				[Command("Subscribe"), Alias("sub", "create", "hook")]
				public async Task Subscribe()
				{
					// TODO: Create webhook + creation logic
					var result = await SpeedrunComService.SubscribeAsync(0ul, "", true);
					await ReplyAndDeleteAsync((result) ? "Webhook test has been sent." : "Could not subscribe.", timeout: TimeSpan.FromSeconds(60));
				}

				[Command("Unsubscribe"), Alias("unsub", "delete", "unhook")]
				public async Task Unsubscribe()
				{
					// TODO: Delete webhook + deletion logic
					var result = await SpeedrunComService.UnsubscribeAsync(0ul, true);
					await ReplyAndDeleteAsync((result) ? "Unsubscribed from SpeedrunCom service." : "Failed to unsubscribe.", timeout: TimeSpan.FromSeconds(60));
				}
			}
		}
	}
}