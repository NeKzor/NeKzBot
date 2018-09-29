﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Addons.Preconditions;
using Discord.Commands;
using NeKzBot.Data;
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
                var embed = new EmbedBuilder()
                    .WithColor(Color.Orange)
                    .WithDescription("**Cvar Database**\n" +
                        "Usage: .cvars.<game> <name>\n" +
                        "Available Games: halflife2, portal, portal2, thebeginnersguide, thestanleyparable\n" +
                        "Made with [SourceAutoRecord](https://nekzor.github.io/SourceAutoRecord)");

                return ReplyAndDeleteAsync(string.Empty, embed: embed.Build());
            }

            [Command("halflife2"), Alias("hl2")]
            public async Task HalfLife2(string cvar)
            {
                var result = await Service.LookUpCvar(cvar, CvarGameType.HalfLife2);
                if (result != null)
                    await PrintResult(result);
                else
                    await ReplyAndDeleteAsync("Unknown Half-Life 2 cvar.", timeout: TimeSpan.FromSeconds(10));
            }
            [Command("portal"), Alias("p", "p1")]
            public async Task Portal(string cvar)
            {
                var result = await Service.LookUpCvar(cvar, CvarGameType.Portal);
                if (result != null)
                    await PrintResult(result);
                else
                    await ReplyAndDeleteAsync("Unknown Portal cvar.", timeout: TimeSpan.FromSeconds(10));
            }
            [Command("portal2"), Alias("p2")]
            public async Task Portal2(string cvar)
            {
                var result = await Service.LookUpCvar(cvar, CvarGameType.Portal2);
                if (result != null)
                    await PrintResult(result);
                else
                    await ReplyAndDeleteAsync("Unknown Portal 2 cvar.", timeout: TimeSpan.FromSeconds(10));
            }
            [Command("thebeginnersguide"), Alias("beginnersguide", "tbg")]
            public async Task TheBeginnersGuide(string cvar)
            {
                var result = await Service.LookUpCvar(cvar, CvarGameType.TheBeginnersGuide);
                if (result != null)
                    await PrintResult(result);
                else
                    await ReplyAndDeleteAsync("Unknown The Beginners Guide cvar.", timeout: TimeSpan.FromSeconds(10));
            }
            [Command("thestanleyparable"), Alias("stanley", "tsp")]
            public async Task TheStanleyParable(string cvar)
            {
                var result = await Service.LookUpCvar(cvar, CvarGameType.TheStanleyParable);
                if (result != null)
                    await PrintResult(result);
                else
                    await ReplyAndDeleteAsync("Unknown The Stanley Parable cvar.", timeout: TimeSpan.FromSeconds(10));
            }

            private async Task PrintResult(SourceCvarData result)
            {
                var flags = (result.Flags.Any())
                    ? string.Join("/", result.Flags).ToLower()
                    : "-";
                var description = (!string.IsNullOrEmpty(result.HelpText))
                    ? result.HelpText.Replace('\n', ' ').Replace('\t', ' ').ToRawText()
                    : "-";
                var os = (result.Os != OperatingSystem.Both)
                    ? result.Os.ToString()
                    : "Windows/Linux";

                var embed = new EmbedBuilder()
                    .WithColor(Color.Orange)
                    .WithDescription($"**{result.Name.ToRawText()}**" +
                        $"\nDefault Value: {result.DefaultValue}" +
                        $"\nFlags: {flags}" +
                        $"\nOS: {os}" +
                        $"\nDescription: {description}");

                await ReplyAndDeleteAsync(string.Empty, embed: embed.Build());
            }
        }

        [Group("demo"), Alias("dem")]
        public class DemoInfo : InteractiveBase<SocketCommandContext>
        {
            public SourceDemoService Service { get; set; }

            [Command("?"), Alias("info", "help")]
            public Task QuestionMark()
            {
                var embed = new EmbedBuilder()
                    .WithColor(Color.Green)
                    .WithDescription("**Source Engine Demo Parser**\n" +
                        "Attach the file and use **.demo.parse**\n" +
                        "[Powered by SourceDemoParser.Net (v1.0-alpha)](https://nekzor.github.io/SourceDemoParser.Net)");

                return ReplyAndDeleteAsync(string.Empty, embed: embed.Build());
            }
            [Ratelimit(3, 1, Measure.Minutes)]
            [Command("parse", RunMode = RunMode.Async)]
            public async Task<RuntimeResult> Parse()
            {
                var file = Context.Message.Attachments
                    .Where(a => a.Filename.EndsWith(".dem"))
                    .FirstOrDefault() as IAttachment;

                if (file == null)
                {
                    var reply = await ReplyAndDeleteAsync
                    (
                        "You didn't attach a demo file!\n" +
                        "Do you want me to look for your last uploaded demo here?",
                        timeout: TimeSpan.FromSeconds(30)
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
                                var messages = await Context.Channel
                                    .GetMessagesAsync()
                                    .FlattenAsync();

                                file = messages
                                    .Where(m => m.Author.Id == Context.User.Id)
                                    .Where(m => m.Attachments.Count == 1)
                                    .Where(m => m.Attachments.First().Filename.EndsWith(".dem"))
                                    .Select(m => m.Attachments.First())
                                    .FirstOrDefault();
                                break;
                            default:
                                await reply.ModifyAsync(r => r.Content = "Interpreted answer as **NO**");
                                return Ok();
                        }
                    }
                }

                if (file != null)
                {
                    if (file.Size <= 5 * 1000 * 1000)
                    {
                        // Downloading the same file is unnecessary
                        // Check if we haven't already downloaded this
                        var data = await Service.Get(Context.User.Id);

                        if (file.Url == data.DownloadUrl)
                            await Get();
                        else if (await Service.DownloadDemoAsync(Context.Message.Author.Id, file.Url))
                            await Get();
                        else
                            await ReplyAndDeleteAsync("Download or parsing failed!", timeout: TimeSpan.FromSeconds(10));
                    }
                    else
                        await ReplyAndDeleteAsync("File is too big! Max size should be less than 5Mb.", timeout: TimeSpan.FromSeconds(10));
                }
                else
                    await ReplyAndDeleteAsync("Could not find a demo.", timeout: TimeSpan.FromSeconds(10));

                return Ok();
            }
            [Ratelimit(3, 1, Measure.Minutes)]
            [Command("get")]
            public async Task Get()
            {
                var demo = await Service.GetDemo(Context.User.Id);
                if (demo != null)
                {
                    // Automatically adjust but don't
                    // save it in the database
                    await demo.AdjustExact();

                    var embed = new EmbedBuilder()
                        .WithColor(Color.Green)
                        .WithDescription($"**Player** {demo.ClientName.ToRawText()}\n" +
                            $"**Map** {demo.MapName.ToRawText()}\n" +
                            $"**Ticks** {demo.PlaybackTicks}\n" +
                            $"**Seconds** {demo.PlaybackTime.ToString("n3")}\n" +
                            $"**Tickrate** {demo.GetTickrate()}");

                    await ReplyAndDeleteAsync(string.Empty, embed: embed.Build());
                }
                else
                    await ReplyAndDeleteAsync("Demo not found!\nTry **.demo.parse**", timeout: TimeSpan.FromSeconds(10));
            }
            [Command("filestamp"), Alias("magic")]
            public async Task FileStamp()
            {
                var demo = await Service.GetDemo(Context.User.Id);
                if (demo != null)
                    await ReplyAndDeleteAsync(demo.FileStamp);
                else
                    await ReplyAndDeleteAsync("You didn't upload a demo.", timeout: TimeSpan.FromSeconds(10));
            }
            [Command("protocol"), Alias("protoc")]
            public async Task Protocol()
            {
                var demo = await Service.GetDemo(Context.User.Id);
                if (demo != null)
                    await ReplyAndDeleteAsync($"{demo.Protocol}");
                else
                    await ReplyAndDeleteAsync("You didn't upload a demo.", timeout: TimeSpan.FromSeconds(10));
            }
            [Command("servername"), Alias("server")]
            public async Task ServerName()
            {
                var demo = await Service.GetDemo(Context.User.Id);
                if (demo != null)
                    await ReplyAndDeleteAsync(demo.ServerName);
                else
                    await ReplyAndDeleteAsync("You didn't upload a demo.", timeout: TimeSpan.FromSeconds(10));
            }
            [Command("clientname"), Alias("client")]
            public async Task ClientName()
            {
                var demo = await Service.GetDemo(Context.User.Id);
                if (demo != null)
                    await ReplyAndDeleteAsync(demo.ClientName);
                else
                    await ReplyAndDeleteAsync("You didn't upload a demo.", timeout: TimeSpan.FromSeconds(10));
            }
            [Command("mapname"), Alias("map")]
            public async Task MapName()
            {
                var demo = await Service.GetDemo(Context.User.Id);
                if (demo != null)
                    await ReplyAndDeleteAsync(demo.MapName);
                else
                    await ReplyAndDeleteAsync("You didn't upload a demo.", timeout: TimeSpan.FromSeconds(10));
            }
            [Command("gamedirectory"), Alias("dir")]
            public async Task GameDirectory()
            {
                var demo = await Service.GetDemo(Context.User.Id);
                if (demo != null)
                    await ReplyAndDeleteAsync(demo.GameDirectory);
                else
                    await ReplyAndDeleteAsync("You didn't upload a demo.", timeout: TimeSpan.FromSeconds(10));
            }
            [Command("playbacktime"), Alias("time")]
            public async Task PlaybackTime()
            {
                var demo = await Service.GetDemo(Context.User.Id);
                if (demo != null)
                    await ReplyAndDeleteAsync($"{demo.PlaybackTime}");
                else
                    await ReplyAndDeleteAsync("You didn't upload a demo.", timeout: TimeSpan.FromSeconds(10));
            }
            [Command("playbackticks"), Alias("ticks")]
            public async Task PlaybackTicks()
            {
                var demo = await Service.GetDemo(Context.User.Id);
                if (demo != null)
                    await ReplyAndDeleteAsync($"{demo.PlaybackTicks}");
                else
                    await ReplyAndDeleteAsync("You didn't upload a demo.", timeout: TimeSpan.FromSeconds(10));
            }
            [Command("playbackframes"), Alias("frames")]
            public async Task PlaybackFrames()
            {
                var demo = await Service.GetDemo(Context.User.Id);
                if (demo != null)
                    await ReplyAndDeleteAsync($"{demo.PlaybackFrames}");
                else
                    await ReplyAndDeleteAsync("You didn't upload a demo.", timeout: TimeSpan.FromSeconds(10));
            }
            [Command("signonlength"), Alias("signon")]
            public async Task SignOnLength()
            {
                var demo = await Service.GetDemo(Context.User.Id);
                if (demo != null)
                    await ReplyAndDeleteAsync($"{demo.SignOnLength}");
                else
                    await ReplyAndDeleteAsync("You didn't upload a demo.", timeout: TimeSpan.FromSeconds(10));
            }
            [Ratelimit(1, 1, Measure.Minutes)]
            [Command("messages", RunMode = RunMode.Async), Alias("msg")]
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

                            line += $"[{i + j}] {demo.Messages[i + j].Type} " +
                                $"at {demo.Messages[i + j].CurrentTick} " +
                                $"-> {demo.Messages[i + j].Frame?.ToString() ?? "NULL"}\n";
                        }
                        pages.Add(line);
                    }
                end:
                    await PagedReplyAsync
                    (
                        new PaginatedMessage
                        {
                            Color = Discord.Color.Green,
                            Pages = pages
                        },
                        false // Allow other users to control the pages too
                    );
                }
            }
            [Ratelimit(6, 1, Measure.Minutes)]
            [Command("messages", RunMode = RunMode.Async), Alias("msg")]
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
                        $"Frame: {result.Frame?.ToString() ?? "NULL"}"
                    );
                }
            }
            [Command("gettickrate"), Alias("tickrate")]
            public async Task GetTickrate()
            {
                var demo = await Service.GetDemo(Context.User.Id);
                if (demo != null)
                    await ReplyAndDeleteAsync($"{demo.GetTickrate()}");
                else
                    await ReplyAndDeleteAsync("You didn't upload a demo.", timeout: TimeSpan.FromSeconds(10));
            }
            [Command("gettickspersecond")]
            [Alias("tickspersecond", "tps", "intervalpertick", "ipt")]
            public async Task GetTicksPerSecond()
            {
                var demo = await Service.GetDemo(Context.User.Id);
                if (demo != null)
                    await ReplyAndDeleteAsync($"{demo.GetTicksPerSecond()}");
                else
                    await ReplyAndDeleteAsync("You didn't upload a demo.", timeout: TimeSpan.FromSeconds(10));
            }
            [Ratelimit(1, 1, Measure.Minutes)]
            [Command("adjustexact", RunMode = RunMode.Async), Alias("adj")]
            public async Task AdjustExact(bool overwrite = false)
            {
                var demo = await Service.GetDemo(Context.User.Id);

                var before = demo.PlaybackTicks;
                await demo.AdjustExact();
                var after = demo.PlaybackTicks;

                await ReplyAndDeleteAsync($"Adjusted demo by {after - before} ticks.");

                if ((overwrite) && !(await Service.SaveDemoAsync(Context.User.Id, demo)))
                    await ReplyAndDeleteAsync("Failed to overwrite demo.", timeout: TimeSpan.FromSeconds(10));
            }
            [Ratelimit(1, 1, Measure.Minutes)]
            [Command("adjustflag", RunMode = RunMode.Async), Alias("adjf")]
            public async Task AdjustFlag(bool overwrite = false)
            {
                var demo = await Service.GetDemo(Context.User.Id);

                var before = demo.PlaybackTicks;
                await demo.AdjustFlagAsync();
                var after = demo.PlaybackTicks;

                await ReplyAndDeleteAsync($"Adjusted demo by {after - before} ticks.");

                if ((overwrite) && !(await Service.SaveDemoAsync(Context.User.Id, demo)))
                    await ReplyAndDeleteAsync("Failed to overwrite demo.", timeout: TimeSpan.FromSeconds(10));
            }
            [Ratelimit(1, 1, Measure.Minutes)]
            [Command("adjust", RunMode = RunMode.Async), Alias("adj2")]
            public async Task Adjust(bool overwrite = false)
            {
                var demo = await Service.GetDemo(Context.User.Id);

                var before = demo.PlaybackTicks;
                await demo.AdjustAsync();
                var after = demo.PlaybackTicks;

                await ReplyAndDeleteAsync($"Adjusted demo by {after - before} ticks.");

                if ((overwrite) && !(await Service.SaveDemoAsync(Context.User.Id, demo)))
                    await ReplyAndDeleteAsync("Failed to overwrite demo.", timeout: TimeSpan.FromSeconds(10));
            }
        }
    }
}