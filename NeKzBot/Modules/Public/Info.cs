using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using NeKzBot.Resources;
using NeKzBot.Server;
using NeKzBot.Utilities;

namespace NeKzBot.Modules.Public
{
	public class Info : CommandModule
	{
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Info Module", LogColor.Init);
			await InfoCommands(Configuration.Default.BotCmd);
			await OtherCommands();
		}

		private static Task InfoCommands(string c)
		{
			CService.CreateGroup(c, GBuilder =>
			{
				GBuilder.CreateCommand("uptime")
						.Description("Shows you how long the application is running for.")
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage($"Uptime • {await Utils.GetDurationFromTimeSpan(await Utils.GetUptime(), false)}");
						});

				GBuilder.CreateCommand("location")
						.Description("Returns the current location of the server bot.")
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage("Bot Location • Graz, Austria :flag_at:\n" +
														$"Local Time • {await Utils.GetLocalTime(true)}");
						});

				GBuilder.CreateCommand("info")
						.Alias("status")
						.Description("Shows some information about the application.")
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							var workers = 0;
							var ports = 0;
							await Task.Run(() => ThreadPool.GetMaxThreads(out workers, out ports));
							await e.Channel.SendMessage($"\n**Log Level** {Bot.Client.Config.LogLevel}" +
														$"\n**Total Shards** {Bot.Client.Config.TotalShards}" +
														$"\n**Cache Dir** {Bot.Client.Config.CacheDir}" +
														$"\n**User Discriminator** {Bot.Client.CurrentUser.Discriminator.ToString("D4")}" +
														$"\n**User Id** {Bot.Client.CurrentUser.Id}" +
														$"\n**GatewaySocket Hosts** {Bot.Client.GatewaySocket.Host.Count()}" +
														$"\n**Current Game** {await Utils.AsRawText(Bot.Client.CurrentGame.Name)}" +
														$"\n**Regions** {Bot.Client.Regions.Count()}" +
														$"\n**Servers** {Bot.Client.Servers.Count()}" +
														$"\n**Commands** {CService.AllCommands.Count()}" +
														$"\n**Services** {Bot.Client.Services.Count()}" +
														$"\n**Error Count** {Logger.Errors.Count}" +
														$"\n**Heap Size** {Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2)} MB" +
														$"\n**Thread Count** {Process.GetCurrentProcess().Threads.Count}/{workers}/{ports}" +
														$"\n**Application Uptime** {await Utils.GetDurationFromTimeSpan(await Utils.GetUptime(), false)}" +
														"\n**Bot Location** Graz, Austria" +
														$"\n**Local Time** {await Utils.GetLocalTime()}" +
														$"\n**Actual Local Time** {await Utils.GetLocalTime(true)}");
						});

				GBuilder.CreateCommand("version")
						.Description("Returns the current version of the application.")
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage($"{Configuration.Default.AppName} v{Configuration.Default.AppVersion}\n<{Configuration.Default.AppUrl}>");
						});

				GBuilder.CreateCommand("changelog")
						.Description("Returns the latest changelog of this project.")
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage($"**Version {Configuration.Default.AppVersion}**" +
														Data.LatestChangelog +
														$"\nRead more here: <{Configuration.Default.AppUrl}/blob/master/NeKzBot/Docs/Changelog.md#version-{Configuration.Default.AppVersion.Replace(".", string.Empty)}>");
						});

				GBuilder.CreateCommand("guilds")
						.Description("Lists all guilds where the bot is connected to.")
						.Do(async e =>
						{
							var output = string.Empty;
							var guilds = Bot.Client.Servers.OrderBy(server => server.Name).ToList();
							foreach (var guild in guilds)
							{
								output += $"\n• {await Utils.AsRawText(guild.Name)} (ID {guild.Id})" +
										  $"\n\t• Owner {await Utils.AsRawText(guild.Owner?.Name) ?? "_Unknown._"} (ID {guild.Owner.Id})" +
										  $"\n\t• Users {guild.Users.Count(user => !(user.IsBot))}" +
										  $"\n\t• Bots {guild.Users.Count(user => user.IsBot)}";
							}
							var msg = await Utils.CutMessageAsync($"**Guild Count: {Bot.Client.Servers.Count()}**{output}", badchars: false);
							if (guilds.Count > 3)
								await (await e.User.CreatePMChannel())?.SendMessage(msg);
							else
							{
								await e.Channel.SendIsTyping();
								await e.Channel.SendMessage(msg);
							}
						});
			});
			return Task.FromResult(0);
		}

		private static Task OtherCommands()
		{
			// Small user information
			CService.CreateCommand("when")
					.Description("Returns your joined server date.")
					.Do(async e =>
					{
						await e.Channel.SendIsTyping();
						await e.Channel.SendMessage($"{await Utils.AsRawText(e.User.Nickname ?? e.User.Name)} joined this server on **{e.User.JoinedAt}**.");
					});

			CService.CreateCommand("idinfo")
					.Description("Returns some user id stats")
					.Do(async e =>
					{
						var users = e.Server.Users.ToArray();
						var lowestid = users[0];
						var highestid = users[0];
						var lowestids = new List<User>();
						var highestids = new List<User>();
						var sumids = 0;
						foreach (var user in users)
						{
							if (user.Id == 0)
								continue;

							// Search for lowest id
							if (lowestid.Id > user.Id)
							{
								lowestid = user;
								lowestids = new List<User> { user };
							}
							else if (lowestid.Id == user.Id)
								lowestids.Add(user);

							// Search for highest id
							if (highestid.Id < user.Id)
							{
								highestid = user;
								highestids = new List<User> { user };
							}
							else if (highestid.Id == user.Id)
								highestids.Add(user);

							sumids += user.Discriminator;
						}

						var lowest = string.Empty;
						foreach (var item in lowestids)
							lowest += $"• {await Utils.AsRawText(item.Name)}#{item.Discriminator.ToString("D4")}\n";

						var highest = string.Empty;
						foreach (var item in highestids)
							highest += $"• {await Utils.AsRawText(item.Name)}#{item.Discriminator.ToString("D4")}\n";

						await e.Channel.SendMessage($"{(lowestids.Count > 1 ? $"Lowest Discriminators\n{lowest}" : $"Lowest Discriminator • {await Utils.AsRawText(lowestid.Name)}#{lowestid.Discriminator.ToString("D4")}\n")}" +
													$"{(highestids.Count > 1 ? $"Highest Discriminators\n{highest}" : $"Highest Discriminator • {await Utils.AsRawText(highestid.Name)}#{highestid.Discriminator.ToString("D4")}\n")}" +
													$"Average Discriminator • #{((ulong)Math.Round((decimal)sumids / users.Length, 0)).ToString("D4")}");
					});
			return Task.FromResult(0);
		}
	}
}