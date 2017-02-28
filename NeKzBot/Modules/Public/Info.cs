using System;
using System.Linq;
using System.Threading.Tasks;
using NeKzBot.Resources;
using NeKzBot.Server;

namespace NeKzBot.Modules.Public
{
	public class Info : Commands
	{
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Info Module", LogColor.Init);
			await InfoCommands(Configuration.Default.BotCmd);
		}

		private static Task InfoCommands(string name)
		{
			CService.CreateGroup(name, GBuilder =>
			{
				GBuilder.CreateCommand("uptime")
						.Description("Shows you how long the application is running for.")
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage($"Uptime • **{(await Utils.GetUptime()).ToString(@"hh\:mm\:ss")}**");
						});

				GBuilder.CreateCommand("location")
						.Description("Gives you information about the server where the bot is located.")
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage("Bot Location • Graz, Austria :flag_at:");
						});

				GBuilder.CreateCommand("info")
						.Alias("status")
						.Description("Shows some information about the bot.")
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage($"\n**Log Level** {Bot.Client.Config.LogLevel}"
													  + $"\n**Total Shards** {Bot.Client.Config.TotalShards}"
													  + $"\n**Cache Dir** {Bot.Client.Config.CacheDir}"
													  + $"\n**User Discriminator** {Bot.Client.CurrentUser.Discriminator.ToString("D4")}"
													  + $"\n**User Id** {Bot.Client.CurrentUser.Id}"
													  + $"\n**GatewaySocket Hosts** {Bot.Client.GatewaySocket.Host.Count()}"
													  + $"\n**Current Game** {Bot.Client.CurrentGame.Name}"
													  + $"\n**Regions** {Bot.Client.Regions.Count()}"
													  + $"\n**Servers** {Bot.Client.Servers.Count()}"
													  + $"\n**Commands** {CService.AllCommands.Count()}"
													  + $"\n**Services** {Bot.Client.Services.Count()}"
													  + $"\n**Error Count** {Logger.ErrorCount}"
													  + $"\n**Heap Size** {Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2)} MB"
													  + $"\n**Application Uptime** {(await Utils.GetUptime()).ToString(@"hh\:mm\:ss")}"
							);
						});

				GBuilder.CreateCommand("version")
						.Description("Returns the current version of the bot.")
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage($"{Configuration.Default.AppName} v{Configuration.Default.AppVersion}\n<{Configuration.Default.AppUrl}>");
						});

				GBuilder.CreateCommand("changelog")
						.Description("Returns the current version of the bot.")
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage($"**Version {Configuration.Default.AppVersion}**"
													  + Data.LatestChangelog
													  + $"\n<{Configuration.Default.AppUrl}/blob/master/NeKzBot/Docs/Changelog.md#version-{Configuration.Default.AppVersion.Replace(".", string.Empty)}>");
						});
			});
			return Task.FromResult(0);
		}
	}
}