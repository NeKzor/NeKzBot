using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Addons.Preconditions;
using Discord.Commands;
using NeKzBot.Services;
using SourceDemoParser.Extensions;

namespace NeKzBot.Modules.Public
{
	public class SourceModule : ModuleBase<SocketCommandContext>
	{
		[Group("cvars"), Alias("cvar")]
		public class CvarDictionary : InteractiveBase<SocketCommandContext>
		{
			public SourceCvarService CvarService { get; set; }

			[Command("HalfLife2"), Alias("hl2")]
			public async Task HalfLife2(string cvar)
			{
				var result = await CvarService.LookUpCvar(cvar, CvarGameType.HalfLife2);
				await ReplyAndDeleteAsync((result != null) ? $"**{result.Cvar}**\n{result.Description}" : "Unknown Half-Life 2 cvar.", timeout: TimeSpan.FromSeconds(60));
			}
			[Command("Portal"), Alias("p")]
			public async Task Portal(string cvar)
			{
				var result = await CvarService.LookUpCvar(cvar, CvarGameType.Portal);
				await ReplyAndDeleteAsync((result != null) ? $"**{result.Cvar}**\n{result.Description}" : "Unknown Portal cvar.", timeout: TimeSpan.FromSeconds(60));
			}
			[Command("Portal2"), Alias("p2")]
			public async Task Portal2(string cvar)
			{
				var result = await CvarService.LookUpCvar(cvar, CvarGameType.Portal2);
				await ReplyAndDeleteAsync((result != null) ? $"**{result.Cvar}**\n{result.Description}" : "Unknown Portal 2 cvar.", timeout: TimeSpan.FromSeconds(60));
			}
		}

		[Group("demo"), Alias("dem")]
		public class DemoInfo : InteractiveBase<SocketCommandContext>
		{
			public SourceDemoService DemoService { get; set; }

			// SourceDemo
			[Command("FileStamp"), Alias("magic")]
			public async Task FileStamp()
			{
				var demo = await DemoService.GetDemo(Context.User.Id);
				await ReplyAndDeleteAsync((demo != null) ? demo.FileStamp : "You didn't upload a demo.", timeout: TimeSpan.FromSeconds(60));
			}
			[Command("Protocol"), Alias("pro")]
			public async Task Protocol()
			{
				var demo = await DemoService.GetDemo(Context.User.Id);
				await ReplyAndDeleteAsync((demo != null) ? $"{demo.Protocol}" : "You didn't upload a demo.", timeout: TimeSpan.FromSeconds(60));
			}
			[Command("ServerName"), Alias("server")]
			public async Task ServerName()
			{
				var demo = await DemoService.GetDemo(Context.User.Id);
				await ReplyAndDeleteAsync((demo != null) ? $"{demo.ServerName}" : "You didn't upload a demo.", timeout: TimeSpan.FromSeconds(60));
			}
			[Command("ClientName"), Alias("client")]
			public async Task ClientName()
			{
				var demo = await DemoService.GetDemo(Context.User.Id);
				await ReplyAndDeleteAsync((demo != null) ? $"{demo.FileStamp}" : "You didn't upload a demo.", timeout: TimeSpan.FromSeconds(60));
			}
			[Command("MapName"), Alias("map")]
			public async Task MapName()
			{
				var demo = await DemoService.GetDemo(Context.User.Id);
				await ReplyAndDeleteAsync((demo != null) ? $"{demo.MapName}" : "You didn't upload a demo.", timeout: TimeSpan.FromSeconds(60));
			}
			[Command("GameDirectory"), Alias("dir")]
			public async Task GameDirectory()
			{
				var demo = await DemoService.GetDemo(Context.User.Id);
				await ReplyAndDeleteAsync((demo != null) ? $"{demo.GameDirectory}" : "You didn't upload a demo.", timeout: TimeSpan.FromSeconds(60));
			}
			[Command("PlaybackTime"), Alias("time")]
			public async Task PlaybackTime()
			{
				var demo = await DemoService.GetDemo(Context.User.Id);
				await ReplyAndDeleteAsync((demo != null) ? $"{demo.PlaybackTime}" : "You didn't upload a demo.", timeout: TimeSpan.FromSeconds(60));
			}
			[Command("PlaybackTicks"), Alias("ticks")]
			public async Task PlaybackTicks()
			{
				var demo = await DemoService.GetDemo(Context.User.Id);
				await ReplyAndDeleteAsync((demo != null) ? $"{demo.PlaybackTicks}" : "You didn't upload a demo.", timeout: TimeSpan.FromSeconds(60));
			}
			[Command("PlaybackFrames"), Alias("frames")]
			public async Task PlaybackFrames()
			{
				var demo = await DemoService.GetDemo(Context.User.Id);
				await ReplyAndDeleteAsync((demo != null) ? $"{demo.PlaybackFrames}" : "You didn't upload a demo.", timeout: TimeSpan.FromSeconds(60));
			}
			[Command("SignOnLength"), Alias("signon")]
			public async Task SignOnLength()
			{
				var demo = await DemoService.GetDemo(Context.User.Id);
				await ReplyAndDeleteAsync((demo != null) ? $"{demo.SignOnLength}" : "You didn't upload a demo.", timeout: TimeSpan.FromSeconds(60));
			}
			[Ratelimit(6, 1, Measure.Minutes)]
			[Command("Messages"), Alias("msg")]
			public async Task Messages()
			{
				var demo = await DemoService.GetDemo(Context.User.Id);
				if (demo != null)
					await ReplyAndDeleteAsync("You didn't upload a demo.", timeout: TimeSpan.FromSeconds(60));
				else if (demo.Messages.Count == 0)
					await ReplyAndDeleteAsync("Demo parser didn't parse any messages.", timeout: TimeSpan.FromSeconds(60));
				else
				{
					var pages = new List<string>();
					for (int i = 0; i < demo.Messages.Count - 1; i += 3)
					{
						var line = string.Empty;
						for (int j = 0; j < 3; j++)
						{
							if ((i + j) >= demo.Messages.Count)
								goto end;
							line += $"[{demo.Messages[i + j].Type}] at {demo.Messages[i + j].CurrentTick}\n-> {demo.Messages[i + j].Frame}";
						}
						pages.Add(line);
					}
end:
					await PagedReplyAsync(pages);
				}
			}
			[Ratelimit(6, 1, Measure.Minutes)]
			[Command("Messages"), Alias("msg")]
			public async Task Messages(int index)
			{
				var demo = await DemoService.GetDemo(Context.User.Id);
				if (demo != null)
					await ReplyAndDeleteAsync("You didn't upload a demo.", timeout: TimeSpan.FromSeconds(60));
				else if (demo.Messages.Count == 0)
					await ReplyAndDeleteAsync("Demo parser didn't parse any messages.", timeout: TimeSpan.FromSeconds(60));
				else if ((index < 0) || (index >= demo.Messages.Count))
					await ReplyAndDeleteAsync($"Invalid index. Take a number between 0 and {demo.Messages.Count - 1}.", timeout: TimeSpan.FromSeconds(60));
				else
				{
					var result = demo.Messages[index];
					var message = $"Type: {result.Type}\nTick: {result.CurrentTick}\nFrame: {result.Frame}";
					await ReplyAndDeleteAsync(message, timeout: TimeSpan.FromSeconds(60));
				}
			}

			// Extensions
			[Command("GetTickrate()"), Alias("tickrate")]
			public async Task GetTickrate()
			{
				var demo = await DemoService.GetDemo(Context.User.Id);
				await ReplyAndDeleteAsync((demo != null) ? $"{demo.GetTickrate()}" : "You didn't upload a demo.", timeout: TimeSpan.FromSeconds(60));
			}
			[Command("GetTicksPerSecond()"), Alias("tps")]
			public async Task GetTicksPerSecond()
			{
				var demo = await DemoService.GetDemo(Context.User.Id);
				await ReplyAndDeleteAsync((demo != null) ? $"{demo.GetTicksPerSecond()}" : "You didn't upload a demo.", timeout: TimeSpan.FromSeconds(60));
			}
			[Ratelimit(3, 1, Measure.Minutes)]
			[Command("AdjustExact()"), Alias("adj")]
			public async Task AdjustExact()
			{
				var demo = await DemoService.GetDemo(Context.User.Id);
				try
				{
					var before = demo.PlaybackTicks;
					await demo.AdjustExact();
					var after = demo.PlaybackTicks;
					await DemoService.SaveDemo(Context.User.Id, demo);
					await ReplyAndDeleteAsync($"Adjusted demo by {after - before} ticks.", timeout: TimeSpan.FromSeconds(60));
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
				}
			}
			[Ratelimit(3, 1, Measure.Minutes)]
			[Command("AdjustFlag()"), Alias("adjf")]
			public async Task AdjustFlag()
			{
				var demo = await DemoService.GetDemo(Context.User.Id);
				try
				{
					var before = demo.PlaybackTicks;
					await demo.AdjustFlagAsync();
					var after = demo.PlaybackTicks;
					await DemoService.SaveDemo(Context.User.Id, demo);
					await ReplyAndDeleteAsync($"Adjusted demo by {after - before} ticks.", timeout: TimeSpan.FromSeconds(60));
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
				}
			}
			[Ratelimit(3, 1, Measure.Minutes)]
			[Command("Adjust()"), Alias("adj2")]
			public async Task Adjust()
			{
				var demo = await DemoService.GetDemo(Context.User.Id);
				try
				{
					var before = demo.PlaybackTicks;
					await demo.AdjustAsync();
					var after = demo.PlaybackTicks;
					await DemoService.SaveDemo(Context.User.Id, demo);
					await ReplyAndDeleteAsync($"Adjusted demo by {after - before} ticks.", timeout: TimeSpan.FromSeconds(60));
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.ToString());
				}
			}

			// Others
			[Command("Parser"), Alias("info")]
			public Task ParserAsync()
			{
				return ReplyAndDeleteAsync("Powered by SourceDemoParser.Net (v1.0-alpha)", timeout: TimeSpan.FromSeconds(60));
			}
			[Command("?"), Alias("o", "help")]
			public async Task QuestionMark()
			{
				var data = await DemoService.GetDemoData(Context.User.Id);
				if (data == null)
					await ReplyAndDeleteAsync("Try to attach a Source Engine demo to a message, without invoking a bot command.", timeout: TimeSpan.FromSeconds(60));
				else
				{
					var index = data.DownloadUrl.LastIndexOf('\\');
					var demoname = data.DownloadUrl.Substring(index, data.DownloadUrl.Length - index - 1);
					await ReplyAndDeleteAsync($"Demo file *{demoname}* was uploaded by {data.Id}\nLink: {data.DownloadUrl}", timeout: TimeSpan.FromSeconds(60));
				}
			}
		}
	}
}