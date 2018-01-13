using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

			[Command("halflife2"), Alias("hl2")]
			public async Task HalfLife2(string cvar)
			{
				var result = await Service.LookUpCvar(cvar, CvarGameType.HalfLife2);
				await ReplyAndDeleteAsync((result != null)
					? $"**{result.Name.ToRawText()}**\n" +
					  $"Default Value: {result.DefaultValue}\n" +
					  $"Flags: {string.Join("/",result.Flags)}\n" +
					  $"Help Text: {result.HelpText.Replace('\n', ' ').Replace('\t', ' ').ToRawText()}"
					: "Unknown Half-Life 2 cvar.",
				timeout: TimeSpan.FromSeconds(60));
			}
			[Command("portal"), Alias("p")]
			public async Task Portal(string cvar)
			{
				var result = await Service.LookUpCvar(cvar, CvarGameType.Portal);
				await ReplyAndDeleteAsync((result != null)
					? $"**{result.Name.ToRawText()}**\n" +
					  $"Default Value: {result.DefaultValue}\n" +
					  $"Flags: {string.Join("/",result.Flags)}\n" +
					  $"Help Text: {result.HelpText.Replace('\n', ' ').Replace('\t', ' ').ToRawText()}"
					: "Unknown Portal cvar.",
				timeout: TimeSpan.FromSeconds(60));
			}
			[Command("portal2"), Alias("p2")]
			public async Task Portal2(string cvar)
			{
				var result = await Service.LookUpCvar(cvar, CvarGameType.Portal2);
				await ReplyAndDeleteAsync((result != null)
					? $"**{result.Name.ToRawText()}**\n" +
					  $"Default Value: {result.DefaultValue}\n" +
					  $"Flags: {string.Join("/",result.Flags)}\n" +
					  $"Help Text: {result.HelpText.Replace('\n', ' ').Replace('\t', ' ').ToRawText()}"
					: "Unknown Portal 2 cvar.",
				timeout: TimeSpan.FromSeconds(60));
			}
		}

		[Group("demo"), Alias("dem")]
		public class DemoInfo : InteractiveBase<SocketCommandContext>
		{
			public SourceDemoService Service { get; set; }

			[Command("parser"), Alias("info")]
			public Task ParserAsync()
			{
				return ReplyAndDeleteAsync("Powered by SourceDemoParser.Net (v1.0-alpha)", timeout: TimeSpan.FromSeconds(60));
			}
			[Command("?"), Alias("o", "help")]
			public async Task QuestionMark()
			{
				var data = await Service.GetDemoData(Context.User.Id);
				if (data == null)
					await ReplyAndDeleteAsync("Try to attach a Source Engine demo to a message, without the bot prefix.", timeout: TimeSpan.FromSeconds(60));
				else
				{
					var index = data.DownloadUrl.LastIndexOf('/') + 1;
					var demoname = data.DownloadUrl.Substring(index, data.DownloadUrl.Length - index);
					await ReplyAndDeleteAsync(
						$"Demo file *{demoname}* was uploaded by {data.Id}\n" +
						$"Link: {data.DownloadUrl}", timeout: TimeSpan.FromSeconds(60));
				}
			}

			// SourceDemo
			[Command("filestamp"), Alias("magic")]
			public async Task FileStamp()
			{
				var demo = await Service.GetDemo(Context.User.Id);
				await ReplyAndDeleteAsync((demo != null) ? demo.FileStamp : "You didn't upload a demo.", timeout: TimeSpan.FromSeconds(60));
			}
			[Command("protocol"), Alias("protoc")]
			public async Task Protocol()
			{
				var demo = await Service.GetDemo(Context.User.Id);
				await ReplyAndDeleteAsync((demo != null) ? $"{demo.Protocol}" : "You didn't upload a demo.", timeout: TimeSpan.FromSeconds(60));
			}
			[Command("servername"), Alias("server")]
			public async Task ServerName()
			{
				var demo = await Service.GetDemo(Context.User.Id);
				await ReplyAndDeleteAsync((demo != null) ? demo.ServerName : "You didn't upload a demo.", timeout: TimeSpan.FromSeconds(60));
			}
			[Command("clientname"), Alias("client")]
			public async Task ClientName()
			{
				var demo = await Service.GetDemo(Context.User.Id);
				await ReplyAndDeleteAsync((demo != null) ? demo.FileStamp : "You didn't upload a demo.", timeout: TimeSpan.FromSeconds(60));
			}
			[Command("mapname"), Alias("map")]
			public async Task MapName()
			{
				var demo = await Service.GetDemo(Context.User.Id);
				await ReplyAndDeleteAsync((demo != null) ? demo.MapName : "You didn't upload a demo.", timeout: TimeSpan.FromSeconds(60));
			}
			[Command("gamedirectory"), Alias("dir")]
			public async Task GameDirectory()
			{
				var demo = await Service.GetDemo(Context.User.Id);
				await ReplyAndDeleteAsync((demo != null) ? demo.GameDirectory : "You didn't upload a demo.", timeout: TimeSpan.FromSeconds(60));
			}
			[Command("playbacktime"), Alias("time")]
			public async Task PlaybackTime()
			{
				var demo = await Service.GetDemo(Context.User.Id);
				await ReplyAndDeleteAsync((demo != null) ? $"{demo.PlaybackTime}" : "You didn't upload a demo.", timeout: TimeSpan.FromSeconds(60));
			}
			[Command("playbackticks"), Alias("ticks")]
			public async Task PlaybackTicks()
			{
				var demo = await Service.GetDemo(Context.User.Id);
				await ReplyAndDeleteAsync((demo != null) ? $"{demo.PlaybackTicks}" : "You didn't upload a demo.", timeout: TimeSpan.FromSeconds(60));
			}
			[Command("playbackframes"), Alias("frames")]
			public async Task PlaybackFrames()
			{
				var demo = await Service.GetDemo(Context.User.Id);
				await ReplyAndDeleteAsync((demo != null) ? $"{demo.PlaybackFrames}" : "You didn't upload a demo.", timeout: TimeSpan.FromSeconds(60));
			}
			[Command("signonlength"), Alias("signon")]
			public async Task SignOnLength()
			{
				var demo = await Service.GetDemo(Context.User.Id);
				await ReplyAndDeleteAsync((demo != null) ? $"{demo.SignOnLength}" : "You didn't upload a demo.", timeout: TimeSpan.FromSeconds(60));
			}
			[Ratelimit(6, 1, Measure.Minutes)]
			[Command("messages"), Alias("msg")]
			public async Task Messages()
			{
				var demo = await Service.GetDemo(Context.User.Id);
				if (demo == null)
					await ReplyAndDeleteAsync("You didn't upload a demo.", timeout: TimeSpan.FromSeconds(60));
				else if (demo.Messages.Count == 0)
					await ReplyAndDeleteAsync("Demo parser didn't parse any messages.", timeout: TimeSpan.FromSeconds(60));
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
						Color = Discord.Color.Blue,
						Pages = pages
					});
				}
			}
			[Ratelimit(6, 1, Measure.Minutes)]
			[Command("Messages"), Alias("msg")]
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
					var message = $"Type: {result.Type}\n" +
						$"Tick: {result.CurrentTick}\n" +
						$"Frame: {result.Frame?.ToString() ?? "NULL"}";
					await ReplyAndDeleteAsync(message, timeout: TimeSpan.FromSeconds(60));
				}
			}
			[Command("gettickrate"), Alias("tickrate")]
			public async Task GetTickrate()
			{
				var demo = await Service.GetDemo(Context.User.Id);
				await ReplyAndDeleteAsync((demo != null) ? $"{demo.GetTickrate()}" : "You didn't upload a demo.", timeout: TimeSpan.FromSeconds(60));
			}
			[Command("gettickspersecond"), Alias("tps")]
			public async Task GetTicksPerSecond()
			{
				var demo = await Service.GetDemo(Context.User.Id);
				await ReplyAndDeleteAsync((demo != null) ? $"{demo.GetTicksPerSecond()}" : "You didn't upload a demo.", timeout: TimeSpan.FromSeconds(60));
			}
			[Ratelimit(3, 1, Measure.Minutes)]
			[Command("adjustexact"), Alias("adj")]
			public async Task AdjustExact()
			{
				var demo = await Service.GetDemo(Context.User.Id);
				var before = demo.PlaybackTicks;
				await demo.AdjustExact();
				var after = demo.PlaybackTicks;
				await Service.SaveDemo(Context.User.Id, demo);
				await ReplyAndDeleteAsync($"Adjusted demo by {after - before} ticks.", timeout: TimeSpan.FromSeconds(60));
			}
			[Ratelimit(3, 1, Measure.Minutes)]
			[Command("adjustflag"), Alias("adjf")]
			public async Task AdjustFlag()
			{
				var demo = await Service.GetDemo(Context.User.Id);
				var before = demo.PlaybackTicks;
				await demo.AdjustFlagAsync();
				var after = demo.PlaybackTicks;
				await Service.SaveDemo(Context.User.Id, demo);
				await ReplyAndDeleteAsync($"Adjusted demo by {after - before} ticks.", timeout: TimeSpan.FromSeconds(60));
			}
			[Ratelimit(3, 1, Measure.Minutes)]
			[Command("adjust"), Alias("adj2")]
			public async Task Adjust()
			{
				var demo = await Service.GetDemo(Context.User.Id);
				var before = demo.PlaybackTicks;
				await demo.AdjustAsync();
				var after = demo.PlaybackTicks;
				await Service.SaveDemo(Context.User.Id, demo);
				await ReplyAndDeleteAsync($"Adjusted demo by {after - before} ticks.", timeout: TimeSpan.FromSeconds(60));
			}
		}
	}
}