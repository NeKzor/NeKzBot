using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Addons.Preconditions;
using Discord.Commands;
using NeKzBot.Extensions;
using NeKzBot.Services;
using SourceDemoParser.Extensions;

namespace NeKzBot.Modules.Public
{
	public class SourceModule : ModuleBase<SocketCommandContext>
	{
		[Group("cvars"), Alias("cvar")]
		public class CvarDictionary : InteractiveBase<SocketCommandContext>
		{
			public SourceCvarService Service { get; set; }

			[Command("?"), Alias("info", "help")]
			public Task QuestionMark()
			{
				return ReplyAndDeleteAsync
				(
					string.Empty,
					embed: new EmbedBuilder()
					{
						Color = Color.Orange,
						Description =
							"**Cvar Database**\n" +
							"Usage: .cvars.<game> <name>\n" +
							"Available Games: halflife2, portal, portal2\n" +
							"Generated with [gen](https://github.com/NeKzor/NeKzBot/tree/master/src/gen)",
					}
					.Build(),
					timeout: TimeSpan.FromSeconds(60)
				);
			}

			[Command("halflife2"), Alias("hl2")]
			public async Task HalfLife2(string cvar)
			{
				var result = await Service.LookUpCvar(cvar, CvarGameType.HalfLife2);
				if (result != null)
				{
					await ReplyAndDeleteAsync
					(
						string.Empty,
						embed: new EmbedBuilder()
						{
							Color = Color.Orange,
							Description =
								$"**{result.Name.ToRawText()}**" +
								$"\nDefault Value: {result.DefaultValue}" +
								$"\nFlags: " +
								((result.Flags.ToList().Count > 0)
									? string.Join("/",result.Flags)
									: "-") +
								$"\nDescription: " +
								(!string.IsNullOrEmpty(result.HelpText)
									? result.HelpText.Replace('\n', ' ').Replace('\t', ' ').ToRawText()
									: "-"),
						}
						.Build(),
						timeout: TimeSpan.FromSeconds(60)
					);
				}
				else
					await ReplyAndDeleteAsync("Unknown Half-Life 2 cvar.", timeout: TimeSpan.FromSeconds(10));
			}
			[Command("portal"), Alias("p", "p1")]
			public async Task Portal(string cvar)
			{
				var result = await Service.LookUpCvar(cvar, CvarGameType.Portal);
				if (result != null)
				{
					await ReplyAndDeleteAsync
					(
						string.Empty,
						embed: new EmbedBuilder()
						{
							Color = Color.Orange,
							Description =
								$"**{result.Name.ToRawText()}**" +
								$"\nDefault Value: {result.DefaultValue}" +
								$"\nFlags: " +
								((result.Flags.ToList().Count > 0)
									? string.Join("/",result.Flags)
									: "-") +
								$"\nDescription: " +
								(!string.IsNullOrEmpty(result.HelpText)
									? result.HelpText.Replace('\n', ' ').Replace('\t', ' ').ToRawText()
									: "-"),
						}
						.Build(),
						timeout: TimeSpan.FromSeconds(60)
					);
				}
				else
					await ReplyAndDeleteAsync("Unknown Portal cvar.", timeout: TimeSpan.FromSeconds(10));
			}
			[Command("portal2"), Alias("p2")]
			public async Task Portal2(string cvar)
			{
				var result = await Service.LookUpCvar(cvar, CvarGameType.Portal2);
				if (result != null)
				{
					await ReplyAndDeleteAsync
					(
						string.Empty,
						embed: new EmbedBuilder()
						{
							Color = Color.Orange,
							Description =
								$"**{result.Name.ToRawText()}**" +
								$"\nDefault Value: {result.DefaultValue}" +
								$"\nFlags: " +
								((result.Flags.ToList().Count > 0)
									? string.Join("/",result.Flags)
									: "-") +
								$"\nDescription: " +
								(!string.IsNullOrEmpty(result.HelpText)
									? result.HelpText.Replace('\n', ' ').Replace('\t', ' ').ToRawText()
									: "-"),
						}
						.Build(),
						timeout: TimeSpan.FromSeconds(60)
					);
				}
				else
					await ReplyAndDeleteAsync("Unknown Portal 2 cvar.", timeout: TimeSpan.FromSeconds(10));
			}
		}

		[Group("demo"), Alias("dem")]
		public class DemoInfo : InteractiveBase<SocketCommandContext>
		{
			public SourceDemoService Service { get; set; }

			[Command("?"), Alias("info", "help")]
			public Task QuestionMark()
			{
				return ReplyAndDeleteAsync
				(
					string.Empty,
					embed: new EmbedBuilder()
					{
						Color = Color.Green,
						Description =
							"**Source Engine Demo Parser**\n" +
							"Attach the file and use **.demo.parse**\n" +
							"[Powered by SourceDemoParser.Net (v1.0-alpha)](https://github.com/NeKzor/SourceDemoParser.Net)",
					}
					.Build(),
					timeout: TimeSpan.FromSeconds(60)
				);
			}
			// Downloading + parsing + adjusting might take a while
			// Run this async or the gateway gets blocked
			[Command("parse", RunMode = RunMode.Async)]
			public async Task Parse()
			{
				var demo = Context.Message.Attachments
					.Where(a => a.Filename.EndsWith(".dem"))
					.FirstOrDefault() as IAttachment;
				
				if (demo == null)
				{
					await ReplyAndDeleteAsync
					(
						"You didn't attach a demo file!\n" +
						"Should I search for your last uploaded demo here?",
						timeout: TimeSpan.FromSeconds(20)
					);

					// Waiting for message here
					// RunMode is async anyway
					var response = await NextMessageAsync(timeout: TimeSpan.FromSeconds(20));

					if (response != null)
					{
						switch (response.Content.Trim().ToLower())
						{
							case "y":
							case "ya":
							case "ye":
							case "yes":
							case "yea":
							case "yeah":
							case "yep":
								demo = (await Context.Channel
									.GetMessagesAsync()
									.Flatten())
									.Where(m => m.Author.Id == Context.User.Id)
									.Where(m => m.Attachments.Count == 1)
									.Where(m => m.Attachments.First().Filename.EndsWith(".dem"))
									.Select(m => m.Attachments.First())
									.FirstOrDefault();
								break;
						}
					}
				}

				if (demo != null)
				{
					if (demo.Size <= 5 * 1000 * 1000)
					{
						if (await Service.DownloadNewDemoAsync(Context.Message.Author.Id, demo.Url))
							await Get();
						else
							await ReplyAndDeleteAsync("Download or parsing failed!", timeout: TimeSpan.FromSeconds(10));
					}
					else
						await ReplyAndDeleteAsync("File is too big! Max size should be less than 5Mb.", timeout: TimeSpan.FromSeconds(10));
				}
				else
					await ReplyAndDeleteAsync("Could not find a demo.", timeout: TimeSpan.FromSeconds(10));
			}
			[Command("get")]
			public async Task Get()
			{
				var demo = await Service.GetDemo(Context.User.Id);
				if (demo != null)
				{
					await demo.AdjustExact();
					await ReplyAndDeleteAsync
					(
						string.Empty,
						embed: new EmbedBuilder()
						{
							Color = Color.Green,
							Description =
								$"**Player** {demo.ClientName.ToRawText()}\n" +
								$"**Map** {demo.MapName.ToRawText()}\n" +
								$"**Ticks** {demo.PlaybackTicks}\n" +
								$"**Seconds** {demo.PlaybackTime.ToString("n3")}\n" +
								$"**Tickrate** {demo.GetTickrate()}"
						}
						.Build(),
						timeout: TimeSpan.FromSeconds(60)
					);
				}
				else
					await ReplyAndDeleteAsync("Demo not found!", timeout: TimeSpan.FromSeconds(10));
			}
			[Command("filestamp"), Alias("magic")]
			public async Task FileStamp()
			{
				var demo = await Service.GetDemo(Context.User.Id);
				if (demo != null)
					await ReplyAndDeleteAsync(demo.FileStamp, timeout: TimeSpan.FromSeconds(60));
				else
					await ReplyAndDeleteAsync("You didn't upload a demo.", timeout: TimeSpan.FromSeconds(10));
			}
			[Command("protocol"), Alias("protoc")]
			public async Task Protocol()
			{
				var demo = await Service.GetDemo(Context.User.Id);
				if (demo != null)
					await ReplyAndDeleteAsync($"{demo.Protocol}", timeout: TimeSpan.FromSeconds(60));
				else
					await ReplyAndDeleteAsync("You didn't upload a demo.", timeout: TimeSpan.FromSeconds(10));
			}
			[Command("servername"), Alias("server")]
			public async Task ServerName()
			{
				var demo = await Service.GetDemo(Context.User.Id);
				if (demo != null)
					await ReplyAndDeleteAsync(demo.ServerName, timeout: TimeSpan.FromSeconds(60));
				else
					await ReplyAndDeleteAsync("You didn't upload a demo.", timeout: TimeSpan.FromSeconds(10));
			}
			[Command("clientname"), Alias("client")]
			public async Task ClientName()
			{
				var demo = await Service.GetDemo(Context.User.Id);
				if (demo != null)
					await ReplyAndDeleteAsync(demo.ClientName, timeout: TimeSpan.FromSeconds(60));
				else
					await ReplyAndDeleteAsync("You didn't upload a demo.", timeout: TimeSpan.FromSeconds(10));
			}
			[Command("mapname"), Alias("map")]
			public async Task MapName()
			{
				var demo = await Service.GetDemo(Context.User.Id);
				if (demo != null)
					await ReplyAndDeleteAsync(demo.MapName, timeout: TimeSpan.FromSeconds(60));
				else
					await ReplyAndDeleteAsync("You didn't upload a demo.", timeout: TimeSpan.FromSeconds(10));
			}
			[Command("gamedirectory"), Alias("dir")]
			public async Task GameDirectory()
			{
				var demo = await Service.GetDemo(Context.User.Id);
				if (demo != null)
					await ReplyAndDeleteAsync(demo.GameDirectory, timeout: TimeSpan.FromSeconds(60));
				else
					await ReplyAndDeleteAsync("You didn't upload a demo.", timeout: TimeSpan.FromSeconds(10));
			}
			[Command("playbacktime"), Alias("time")]
			public async Task PlaybackTime()
			{
				var demo = await Service.GetDemo(Context.User.Id);
				if (demo != null)
					await ReplyAndDeleteAsync($"{demo.PlaybackTime}", timeout: TimeSpan.FromSeconds(60));
				else
					await ReplyAndDeleteAsync("You didn't upload a demo.", timeout: TimeSpan.FromSeconds(10));
			}
			[Command("playbackticks"), Alias("ticks")]
			public async Task PlaybackTicks()
			{
				var demo = await Service.GetDemo(Context.User.Id);
				if (demo != null)
					await ReplyAndDeleteAsync($"{demo.PlaybackTicks}", timeout: TimeSpan.FromSeconds(60));
				else
					await ReplyAndDeleteAsync("You didn't upload a demo.", timeout: TimeSpan.FromSeconds(10));
			}
			[Command("playbackframes"), Alias("frames")]
			public async Task PlaybackFrames()
			{
				var demo = await Service.GetDemo(Context.User.Id);
				if (demo != null)
					await ReplyAndDeleteAsync($"{demo.PlaybackFrames}", timeout: TimeSpan.FromSeconds(60));
				else
					await ReplyAndDeleteAsync("You didn't upload a demo.", timeout: TimeSpan.FromSeconds(10));
			}
			[Command("signonlength"), Alias("signon")]
			public async Task SignOnLength()
			{
				var demo = await Service.GetDemo(Context.User.Id);
				if (demo != null)
					await ReplyAndDeleteAsync($"{demo.SignOnLength}", timeout: TimeSpan.FromSeconds(60));
				else
					await ReplyAndDeleteAsync("You didn't upload a demo.", timeout: TimeSpan.FromSeconds(10));
			}
			[Ratelimit(6, 1, Measure.Minutes)]
			[Command("messages"), Alias("msg")]
			public async Task Messages()
			{
				var demo = await Service.GetDemo(Context.User.Id);
				if (demo == null)
					await ReplyAndDeleteAsync("You didn't upload a demo.", timeout: TimeSpan.FromSeconds(10));
				else if (demo.Messages.Count == 0)
					await ReplyAndDeleteAsync("Demo parser didn't parse any messages.", timeout: TimeSpan.FromSeconds(10));
				else
				{
					var pages = new List<string>();
					for (int i = 0; i < demo.Messages.Count - 1; i += 5)
					{
						var line = string.Empty;
						for (int j = 0; j < 5; j++)
						{
							if ((i + j) >= demo.Messages.Count)
								goto end;
							line += $"[{i + j}] {demo.Messages[i + j].Type} at {demo.Messages[i + j].CurrentTick} -> {demo.Messages[i + j].Frame?.ToString() ?? "NULL"}\n";
						}
						pages.Add(line);
					}
end:
					await PagedReplyAsync(new PaginatedMessage
					{
						Color = Discord.Color.Green,
						Pages = pages
					});
				}
			}
			[Ratelimit(6, 1, Measure.Minutes)]
			[Command("messages"), Alias("msg")]
			public async Task Messages(int index)
			{
				var demo = await Service.GetDemo(Context.User.Id);
				if (demo == null)
					await ReplyAndDeleteAsync("You didn't upload a demo.", timeout: TimeSpan.FromSeconds(10));
				else if (demo.Messages.Count == 0)
					await ReplyAndDeleteAsync("Demo parser didn't parse any messages.", timeout: TimeSpan.FromSeconds(10));
				else if ((index < 0) || (index >= demo.Messages.Count))
					await ReplyAndDeleteAsync($"Invalid index. Take a number between 0 and {demo.Messages.Count - 1}.", timeout: TimeSpan.FromSeconds(10));
				else
				{
					var result = demo.Messages[index];
					await ReplyAndDeleteAsync
					(
						$"Type: {result.Type}\n" +
						$"Tick: {result.CurrentTick}\n" +
						$"Frame: {result.Frame?.ToString() ?? "NULL"}",
						timeout: TimeSpan.FromSeconds(60)
					);
				}
			}
			[Command("gettickrate"), Alias("tickrate")]
			public async Task GetTickrate()
			{
				var demo = await Service.GetDemo(Context.User.Id);
				if (demo != null)
					await ReplyAndDeleteAsync($"{demo.GetTickrate()}", timeout: TimeSpan.FromSeconds(60));
				else
					await ReplyAndDeleteAsync("You didn't upload a demo.", timeout: TimeSpan.FromSeconds(10));
			}
			[Command("gettickspersecond"), Alias("tickspersecond", "tps", "intervalpertick", "ipt")]
			public async Task GetTicksPerSecond()
			{
				var demo = await Service.GetDemo(Context.User.Id);
				if (demo != null)
					await ReplyAndDeleteAsync($"{demo.GetTicksPerSecond()}", timeout: TimeSpan.FromSeconds(60));
				else
					await ReplyAndDeleteAsync("You didn't upload a demo.", timeout: TimeSpan.FromSeconds(10));
			}
			[Ratelimit(3, 1, Measure.Minutes)]
			[Command("adjustexact"), Alias("adj")]
			public async Task AdjustExact()
			{
				var demo = await Service.GetDemo(Context.User.Id);

				var before = demo.PlaybackTicks;
				await demo.AdjustExact();
				var after = demo.PlaybackTicks;
				
				if (await Service.SaveDemo(Context.User.Id, demo))
					await ReplyAndDeleteAsync($"Adjusted demo by {after - before} ticks.", timeout: TimeSpan.FromSeconds(60));
				else
					await ReplyAndDeleteAsync("Failed to adjust demo.", timeout: TimeSpan.FromSeconds(10));
			}
			[Ratelimit(3, 1, Measure.Minutes)]
			[Command("adjustflag"), Alias("adjf")]
			public async Task AdjustFlag()
			{
				var demo = await Service.GetDemo(Context.User.Id);
				
				var before = demo.PlaybackTicks;
				await demo.AdjustFlagAsync();
				var after = demo.PlaybackTicks;

				if (await Service.SaveDemo(Context.User.Id, demo))
					await ReplyAndDeleteAsync($"Adjusted demo by {after - before} ticks.", timeout: TimeSpan.FromSeconds(60));
				else
					await ReplyAndDeleteAsync("Failed to adjust demo.", timeout: TimeSpan.FromSeconds(10));
			}
			// TODO: Check if this needs RunMode.Async
			[Ratelimit(3, 1, Measure.Minutes)]
			[Command("adjust"), Alias("adj2")]
			public async Task Adjust()
			{
				var demo = await Service.GetDemo(Context.User.Id);

				var before = demo.PlaybackTicks;
				await demo.AdjustAsync();
				var after = demo.PlaybackTicks;

				if (await Service.SaveDemo(Context.User.Id, demo))
					await ReplyAndDeleteAsync($"Adjusted demo by {after - before} ticks.", timeout: TimeSpan.FromSeconds(60));
				else
					await ReplyAndDeleteAsync("Failed to adjust demo.", timeout: TimeSpan.FromSeconds(10));
			}
		}
	}
}