using System;
using System.Linq;
using System.Threading.Tasks;
using NeKzBot.Server;
using NeKzBot.Resources;

namespace NeKzBot.Modules
{
	public class Info : Commands
	{
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Bot Info Commands", LogColor.Init);
			await InfoCommands(Configuration.Default.BotCmd);
		}

		private static Task InfoCommands(string name)
		{
			CService.CreateGroup(name, GBuilder =>
			{
				GBuilder.CreateCommand("uptime")
						.Description($"• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} uptime` shows you how long the bot is running for.")
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage($"Bot is running for **{(await Utils.GetUptime()).ToString(@"hh\:mm\:ss")}**");
						});

				GBuilder.CreateCommand("location")
						.Description($"• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} location` gives you information about the server where the bot is located.")
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage("**Bot Location • Graz, Austria :flag_at:");
						});

				GBuilder.CreateCommand("info")
						.Alias("status")
						.Description($"• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} info` shows some information about the bot.")
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage($"**AppName - ** {Bot.Client.Config.AppName}"
													  + $"\n**AppVersion - **{Bot.Client.Config.AppVersion}"
													  + $"\n**AppUrl - **<{Bot.Client.Config.AppUrl}>"
													  + $"\n**Log Level -** {Bot.Client.Config.LogLevel}"
													  + $"\n**Connection Timeout -** {Bot.Client.Config.ConnectionTimeout}"
													  + $"\n**Reconnect Delay-** {Bot.Client.Config.ReconnectDelay}"
													  + $"\n**Failed Reconnect Delay -** {Bot.Client.Config.FailedReconnectDelay}"
													  + $"\n**Total Shards -** {Bot.Client.Config.TotalShards}"
													  + $"\n**Cache Dir -** {Bot.Client.Config.CacheDir}"
													  + $"\n**User Agent -** {Bot.Client.Config.UserAgent}"
													  + $"\n**User Discriminator -** {Bot.Client.CurrentUser.Discriminator}"
													  + $"\n**User Id -** {Bot.Client.CurrentUser.Id}"
													  + $"\n**User Is Verfied -** {Bot.Client.CurrentUser.IsVerified}"
													  + $"\n**User Status -** {Bot.Client.CurrentUser.Status}"
													  + $"\n**GatewaySocket Hosts -** {Bot.Client.GatewaySocket.Host.Count()}"
													  + $"\n**GatewaySocket State -** {Bot.Client.GatewaySocket.State}"
													  + $"\n**Current Game -** {Bot.Client.CurrentGame.Name}"
													  + $"\n**Client State -** {Bot.Client.State}"
													  + $"\n**Client Status -** {Bot.Client.Status.Value}"
													  + $"\n**Regions -** {Bot.Client.Regions.Count()}"
													  + $"\n**Servers -** {Bot.Client.Servers.Count()}"
													  + $"\n**Commands -** {CService.AllCommands.Count()}"
													  + $"\n**Services -** {Bot.Client.Services.Count()}"
													  + $"\n**Session Id -** {Bot.Client.SessionId}"
													  + $"\n**Error Count -** {Logger.ErrorCount}"
													  + $"\n**Heap Size -** {Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2)} MB"
													  + "\n**Bot Location -** Graz, Austria"
													  + $"\n**Application Uptime -** {(await Utils.GetUptime()).ToString(@"hh\:mm\:ss")}"
							);
						});

				GBuilder.CreateCommand("help")
						.Alias("ayy")
						.Description($"• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} help` doesn't help you at all.")
						.Hide()
						.Do(async e =>
						{
							// Don't ask me why because I ask that to myself too whenever I see this...
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage("AYY, WADDUP!");
						});

				GBuilder.CreateCommand("settings")
						.Alias("debug")
						.Description($"• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} settings` gives you some debug information about the application.")
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage(Data.settingsMsg);
						});

				GBuilder.CreateCommand("version")
						.Description($"• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} version` returns the current version of the bot.")
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage($"{Configuration.Default.AppName} v{Configuration.Default.AppVersion}\n<{Configuration.Default.AppUrl}>");
						});

				GBuilder.CreateCommand("changelog")
						.Description($"• `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} version` returns the current version of the bot.")
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage("**Version 1.4**"
													  + "\n• Added documentation"
													  + "\n• Massive code cleanup"
													  + "\n• Automatic Twitch channel detection with role assignment"
													  + "\n• Added internal watches to make parallel tasks more accurate"
													  + "\n• Fixed giveaway cache"
													  + "\n• Send world record comment as tweet reply"
													  + "\n• Improved server logger and separated events"
													  + "\n• Added class management for data manager"
													  + "\n• Added automatic role assignment when somebody has a world record"
													  + "\n• Twitch stream preview is now a static attachement"
													  + "\n• Added version, changelog join, invite and staticinvite commands"
													  + "\n• Improved idinfo command"
													  + "\n• Send remaining or missed world records and notifications"
													  + "\n• Steam workshop item links are correctly parsed now"
													  + "\n• Added additional symbols to RIS algorithm"
													  + "\n• Fixed wrong encoding in fetching system"
													  + "\n• Added application exit handler"
													  + "\n• Update Twitter location after a leaderboard update"
													  + "\n• Update Twitter description when going online or offline"
													  + "\n• Disabled automatic link embedding"
													  + "\n• Send updates to every connected server"
													  + "\n• Changed every method to task"
													  + "\n• Improved data deletion code"
													  + "\n• Added tickrate and startdemos converter"
													  + "\n• Fixed and improved many other things"
													  + $"\n<{Configuration.Default.AppUrl}/blob/master/NeKzBot/Docs/Changelog.md>");
						});
			});
			return Task.FromResult(0);
		}
	}
}