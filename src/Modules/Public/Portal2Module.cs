using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Addons.Preconditions;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using NeKzBot.Extensions;
using NeKzBot.Services;
using Portal2Boards;
using Portal2Boards_LogMessage = Portal2Boards.LogMessage;
using Portal2Boards.Extensions;

namespace NeKzBot.Modules.Public
{
    public class Portal2Module : ModuleBase<SocketCommandContext>
    {
        [Group("portal2"), Alias("p2")]
        public class Portal2Boards : InteractiveBase<SocketCommandContext>
        {
            private readonly Portal2BoardsClient _client;
            private readonly Portal2CampaignService _portal2;

            public Portal2Boards(IConfiguration config, Portal2CampaignService portal2)
            {
                _portal2 = portal2;
                _client = new Portal2BoardsClient(config["user_agent"]);
                _client.Log += LogPortal2Boards;
            }

            private Task LogPortal2Boards(object _, Portal2Boards_LogMessage message)
            {
                System.Console.WriteLine(message.ToString());
                return Task.CompletedTask;
            }

            [Ratelimit(6, 1, Measure.Minutes)]
            [Command("map")]
            public async Task Map([Remainder] string? mapName = null)
            {
                var map = (!string.IsNullOrEmpty(mapName))
                    ? _portal2.GetMap(mapName)
                    : _portal2.GetRandomMap();

                if (map != null)
                {
                    var embed = new EmbedBuilder()
                        .WithColor(Color.Blue)
                        .WithDescription(
                            $"**{map.ChallengeModeName}**\n" +
                            $"File Name: {map.Name}.bsp\n" +
                            $"Abbreviation: {map.ThreeLetterCode}\n" +
                            $"Elevator Timing: *{map.ElevatorTiming}*");

                    var message = await Context.Channel.SendFileAsync
                    (
                        $"public/resources/images/maps/{map.BestTimeId}.jpg",
                        embed: embed.Build()
                    ).ConfigureAwait(false);

                    _ = Task.Delay(TimeSpan.FromSeconds(5 * 60))
                        .ContinueWith(_ => message.DeleteAsync().ConfigureAwait(false))
                        .ConfigureAwait(false);
                }
                else
                    await ReplyAndDeleteAsync("Invalid map name.", timeout: TimeSpan.FromSeconds(10));
            }

            [Ratelimit(6, 1, Measure.Minutes)]
            [Command("discovery"), Alias("exploit", "glitch")]
            public async Task Discovery([Remainder] string? discoveryName = null)
            {
                var discovery = (!string.IsNullOrEmpty(discoveryName))
                    ? _portal2.GetDiscovery(discoveryName)
                    : _portal2.GetRandomDiscovery();

                if (discovery != null)
                {
                    var embed = new EmbedBuilder()
                        .WithColor(Color.Blue)
                        .WithDescription(
                            $"**{discovery.Name}**\n" +
                            $"Type: {discovery.Type}\n" +
                            $"Category: {discovery.Category}\n" +
                            $"Status: {discovery.Status}\n" +
                            $"[Showcase]({discovery.Showcase})");

                    await ReplyAndDeleteAsync(string.Empty, embed: embed.Build());
                }
                else
                    await ReplyAndDeleteAsync("Unknown name.", timeout: TimeSpan.FromSeconds(10));
            }

            [Command("?"), Alias("info", "help")]
            public Task QuestionMark()
            {
                var embed = new EmbedBuilder()
                    .WithColor(Color.Blue)
                    .WithDescription("[Powered by Portal2Boards.Net (v2.2)](https://nekz.me/Portal2Boards.Net)");

                return ReplyAndDeleteAsync(string.Empty, embed: embed.Build());
            }

            [Ratelimit(3, 1, Measure.Minutes)]
            [Command("leaderboard", RunMode = RunMode.Async), Alias("lb")]
            public async Task Leaderboard([Remainder] string? mapName = null)
            {
                if (!string.IsNullOrEmpty(mapName))
                {
                    var map = Portal2Map.Search(mapName);
                    if (map != null)
                    {
                        if (map.BestTimeId != null)
                        {
                            var board = await _client.GetChamberAsync(map);

                            var page = string.Empty;
                            var pages = new List<string>();
                            var count = 0;

                            foreach (var entry in board.Entries.Take(100))
                            {
                                if ((count % 5 == 0) && (count != 0))
                                {
                                    pages.Add(page);
                                    page = string.Empty;
                                }

                                page += $"\n{entry.ScoreRank.FormatRankToString()} " +
                                    $"{entry.Score.AsTimeToString()} " +
                                    $"by [{entry.Player.Name.ToRawText()}]({(entry.Player as SteamUser)!.Url})";

                                count++;
                            }
                            pages.Add(page);

                            await PagedReplyAsync
                            (
                                new PaginatedMessage()
                                {
                                    Color = Color.Blue,
                                    Pages = pages,
                                    Title = $"Top 100 - {map.Alias}",
                                    Options = new PaginatedAppearanceOptions
                                    {
                                        DisplayInformationIcon = false,
                                        Timeout = TimeSpan.FromSeconds(5 * 60)
                                    }
                                },
                                false
                            );
                        }
                        else
                            await ReplyAndDeleteAsync("This map does not have a leaderboard.", timeout: TimeSpan.FromSeconds(10));
                    }
                    else
                        await ReplyAndDeleteAsync($"Could not find a map named *{mapName.ToRawText()}*.", timeout: TimeSpan.FromSeconds(10));
                }
                else
                    await ReplyAndDeleteAsync("Invalid map name.", timeout: TimeSpan.FromSeconds(10));
            }

            [Ratelimit(3, 1, Measure.Minutes)]
            [Command("changelog", RunMode = RunMode.Async), Alias("cl", "clog")]
            public async Task Changelog([Remainder] string mapName)
            {
                var map = Portal2Map.Search(mapName);
                if (map is null)
                {
                    await ReplyAndDeleteAsync("Invalid map name.", timeout: TimeSpan.FromSeconds(10));
                    return;
                }
                if (map.BestTimeId is null)
                {
                    await ReplyAndDeleteAsync("This map does not have a leaderboard.", timeout: TimeSpan.FromSeconds(10));
                    return;
                }

                var changelog = await _client.GetChangelogAsync($"?chamber={map.BestTimeId}");

                var page = string.Empty;
                var pages = new List<string>();
                var count = 0;

                foreach (var entry in changelog.Entries.Take(20))
                {
                    if ((count % 5 == 0) && (count != 0))
                    {
                        pages.Add(page);
                        page = string.Empty;
                    }

                    page += $"\n{entry.Rank.Current.FormatRankToString("WR")}" +
                        $" {entry.Score.Current.AsTimeToString()}" +
                        ((entry.Score.Improvement.HasValue)
                            ? $" (-{((uint?)entry.Score.Improvement).AsTimeToString()})"
                            : string.Empty) +
                        $" by [{entry.Player.Name.ToRawText()}]({(entry.Player as SteamUser)!.Url})";

                    count++;
                }
                pages.Add(page);

                await PagedReplyAsync
                (
                    new PaginatedMessage()
                    {
                        Color = Color.Blue,
                        Pages = pages,
                        Title = $"Latest 20 - {map.Alias}",
                        Options = new PaginatedAppearanceOptions
                        {
                            DisplayInformationIcon = false,
                            Timeout = TimeSpan.FromSeconds(5 * 60)
                        }
                    },
                    false
                );
            }

            [Ratelimit(3, 1, Measure.Minutes)]
            [Command("profile", RunMode = RunMode.Async), Alias("pro", "user")]
            public async Task Profile([Remainder] string? userNameOrSteamId64 = null)
            {
                var user = await GetProfileAsync(userNameOrSteamId64) as Profile;
                if (user is null)
                {
                    await ReplyAndDeleteAsync("Invalid user name or id.", timeout: TimeSpan.FromSeconds(10));
                    return;
                }

                // Local function
                string GetMap(IDataScore score)
                {
                    var map = Portal2Map.Search((score as DataScore)!.Id);
                    return (map != null) ? map.Alias : "several chambers";
                }

                var pages = new List<string>
                {
                    $"\nSP WRs | {user!.Times.SinglePlayerChapters.WorldRecords}" +
                        $"\nMP WRs | {user.Times.CooperativeChapters.WorldRecords}" +
                        $"\nTotal | {user.WorldRecords}",
                    $"\nSP Points | {user.Points.SinglePlayer.Score.FormatPointsToString()}" +
                        $"\nMP Points | {user.Points.Cooperative.Score.FormatPointsToString()}" +
                        $"\nTotal | {user.Points.Global.Score.FormatPointsToString()}",
                    $"\nSP Rank | {user.Points.SinglePlayer.PlayerRank.FormatRankToString()}" +
                        $"\nMP Rank | {user.Points.Cooperative.PlayerRank.FormatRankToString()}" +
                        $"\nOverall | {user.Points.Global.PlayerRank.FormatRankToString()}",
                    $"\nSP Avg Rank | {user.Times.SinglePlayerChapters.AveragePlace.FormatAveragePlaceToString()}" +
                        $"\nMP Avg Rank | {user.Times.CooperativeChapters.AveragePlace.FormatAveragePlaceToString()}" +
                        $"\nOverall | {user.GlobalAveragePlace.FormatAveragePlaceToString()}",
                    $"\nSP Best Rank | {user.Times.SinglePlayerChapters.BestScore.PlayerRank.FormatRankToString()} on {GetMap(user.Times.SinglePlayerChapters.BestScore)}" +
                        $"\nMP Best Rank | {user.Times.CooperativeChapters.BestScore.PlayerRank.FormatRankToString()} on {GetMap(user.Times.CooperativeChapters.BestScore)}",
                    $"\nSP Oldest Score | {user.Times.SinglePlayerChapters.OldestScore.Score.AsTimeToString()} on {GetMap(user.Times.SinglePlayerChapters.OldestScore)}" +
                        $"\nMP Oldest Score | {user.Times.CooperativeChapters.OldestScore.Score.AsTimeToString()} on {GetMap(user.Times.CooperativeChapters.OldestScore)}",
                    $"\nSP Newest Score | {user.Times.SinglePlayerChapters.NewestScore.Score.AsTimeToString()} on {GetMap(user.Times.SinglePlayerChapters.NewestScore)}" +
                        $"\nMP Newest Score | {user.Times.CooperativeChapters.NewestScore.Score.AsTimeToString()} on {GetMap(user.Times.CooperativeChapters.NewestScore)}"
                };

                var page = string.Empty;
                var count = 0;

                foreach (var map in Portal2.CampaignMaps)
                {
                    var chamber = (user.Times as DataTimes)!.GetMapData(map);
                    if (chamber is null) continue;

                    if ((count % 5 == 0) && (count != 0))
                    {
                        pages.Add(page);
                        page = string.Empty;
                    }

                    page += $"\n[{map.Alias}]({map.Url}) | " +
                        $"{chamber.PlayerRank.FormatRankToString("WR")} | " +
                        $"{chamber.Score.AsTimeToString()}";

                    count++;
                }
                pages.Add(page);

                var lastpage = $"{user.Title}\n[Steam]({user.SteamUrl})";
                if (!string.IsNullOrEmpty(user.YouTubeUrl))
                    lastpage += $"\n[YouTube](https://youtube.com{user.YouTubeUrl})";
                if (!string.IsNullOrEmpty(user.TwitchName))
                    lastpage += $"\n[Twitch](https://twitch.tv/{user.TwitchName})";
                pages.Add(lastpage);

                await PagedReplyAsync
                (
                    new PaginatedMessage()
                    {
                        Color = Color.Blue,
                        Pages = pages,
                        Author = new EmbedAuthorBuilder()
                        {
                            Name = user.DisplayName,
                            IconUrl = user.SteamAvatarUrl,
                            Url = user.Url
                        },
                        Options = new PaginatedAppearanceOptions()
                        {
                            DisplayInformationIcon = false,
                            Timeout = TimeSpan.FromSeconds(5 * 60)
                        }
                    },
                    false
                );
            }

            [Ratelimit(3, 1, Measure.Minutes)]
            [Command("aggregated", RunMode = RunMode.Async), Alias("agg")]
            public async Task Aggregated()
            {
                var agg = await _client.GetAggregatedAsync();

                var page = string.Empty;
                var pages = new List<string>();
                var count = 0;

                foreach (var entry in agg.Points.Take(20))
                {
                    if ((count % 5 == 0) && (count != 0))
                    {
                        pages.Add(page);
                        page = string.Empty;
                    }

                    page += $"\n{entry.Score}" +
                        $"\t[{entry.Player.Name}](https://board.iverb.me/profile/{(entry.Player as SteamUser)!.Id})";

                    count++;
                }
                pages.Add(page);

                await PagedReplyAsync
                (
                    new PaginatedMessage()
                    {
                        Color = Color.Blue,
                        Pages = pages,
                        Options = new PaginatedAppearanceOptions()
                        {
                            DisplayInformationIcon = false,
                            Timeout = TimeSpan.FromSeconds(5 * 60)
                        }
                    },
                    false
                );
            }

            [Ratelimit(3, 1, Measure.Minutes)]
            [Command("compare", RunMode = RunMode.Async), Alias("vs")]
            public async Task Aggregated(string userNameOrSteamId, string? userNameOrSteamId2 = null)
            {
                var profile1 = await GetProfileAsync(userNameOrSteamId, false) as Profile;
                if (profile1 is null)
                {
                    await ReplyAndDeleteAsync("Invalid user name or id for first argument.", timeout: TimeSpan.FromSeconds(10));
                    return;
                }

                var profile2 = await GetProfileAsync(userNameOrSteamId2) as Profile;
                if (profile2 is null)
                {
                    await ReplyAndDeleteAsync("Invalid user name or id for second argument.", timeout: TimeSpan.FromSeconds(10));
                    return;
                }

                // Local function
                string GetMap(IDataScore score)
                {
                    var map = Portal2Map.Search((score as DataScore)!.Id);
                    return (map != null) ? map.Alias : "several chambers";
                }

                var pages = new List<string>
                {
                    $"\nSP WRs | {profile1!.Times.SinglePlayerChapters.WorldRecords} | {profile2!.Times.SinglePlayerChapters.WorldRecords}" +
                        $"\nMP WRs | {profile1.Times.CooperativeChapters.WorldRecords} | {profile2.Times.CooperativeChapters.WorldRecords}" +
                        $"\nTotal | {profile1.WorldRecords} | {profile2.WorldRecords}",
                    $"\nSP Points | {profile1.Points.SinglePlayer.Score.FormatPointsToString()} | {profile2.Points.SinglePlayer.Score.FormatPointsToString()}" +
                        $"\nMP Points | {profile1.Points.Cooperative.Score.FormatPointsToString()} | {profile2.Points.Cooperative.Score.FormatPointsToString()}" +
                        $"\nTotal | {profile1.Points.Global.Score.FormatPointsToString()} | {profile2.Points.Global.Score.FormatPointsToString()}",
                    $"\nSP Rank | {profile1.Points.SinglePlayer.PlayerRank.FormatRankToString()} | {profile2.Points.SinglePlayer.PlayerRank.FormatRankToString()}" +
                        $"\nMP Rank | {profile1.Points.Cooperative.PlayerRank.FormatRankToString()} | {profile2.Points.Cooperative.PlayerRank.FormatRankToString()}" +
                        $"\nOverall | {profile1.Points.Global.PlayerRank.FormatRankToString()} | {profile2.Points.Global.PlayerRank.FormatRankToString()}",
                    $"\nSP Avg Rank | {profile1.Times.SinglePlayerChapters.AveragePlace.FormatAveragePlaceToString()} | {profile2.Times.SinglePlayerChapters.AveragePlace.FormatAveragePlaceToString()}" +
                        $"\nMP Avg Rank | {profile1.Times.CooperativeChapters.AveragePlace.FormatAveragePlaceToString()} | {profile2.Times.CooperativeChapters.AveragePlace.FormatAveragePlaceToString()}" +
                        $"\nOverall | {profile1.GlobalAveragePlace.FormatAveragePlaceToString()} | {profile2.GlobalAveragePlace.FormatAveragePlaceToString()}",
                    $"\nSP Best Rank | {profile1.Times.SinglePlayerChapters.BestScore.PlayerRank.FormatRankToString()} on {GetMap(profile1.Times.SinglePlayerChapters.BestScore)} | {profile2.Times.SinglePlayerChapters.BestScore.PlayerRank.FormatRankToString()} on {GetMap(profile2.Times.SinglePlayerChapters.BestScore)}" +
                        $"\nMP Best Rank | {profile1.Times.CooperativeChapters.BestScore.PlayerRank.FormatRankToString()} on {GetMap(profile1.Times.CooperativeChapters.BestScore)} | {profile2.Times.CooperativeChapters.BestScore.PlayerRank.FormatRankToString()} on {GetMap(profile2.Times.CooperativeChapters.BestScore)}",
                    $"\nSP Oldest Score | {profile1.Times.SinglePlayerChapters.OldestScore.Score.AsTimeToString()} on {GetMap(profile1.Times.SinglePlayerChapters.OldestScore)} | {profile2.Times.SinglePlayerChapters.OldestScore.Score.AsTimeToString()} on {GetMap(profile2.Times.SinglePlayerChapters.OldestScore)}" +
                        $"\nMP Oldest Score | {profile1.Times.CooperativeChapters.OldestScore.Score.AsTimeToString()} on {GetMap(profile1.Times.CooperativeChapters.OldestScore)} | {profile2.Times.CooperativeChapters.OldestScore.Score.AsTimeToString()} on {GetMap(profile2.Times.CooperativeChapters.OldestScore)}",
                    $"\nSP Newest Score | {profile1.Times.SinglePlayerChapters.NewestScore.Score.AsTimeToString()} on {GetMap(profile1.Times.SinglePlayerChapters.NewestScore)} | {profile2.Times.SinglePlayerChapters.NewestScore.Score.AsTimeToString()} on {GetMap(profile2.Times.SinglePlayerChapters.NewestScore)}" +
                        $"\nMP Newest Score | {profile1.Times.CooperativeChapters.NewestScore.Score.AsTimeToString()} on {GetMap(profile1.Times.CooperativeChapters.NewestScore)} | {profile2.Times.CooperativeChapters.NewestScore.Score.AsTimeToString()} on {GetMap(profile2.Times.CooperativeChapters.NewestScore)}"
                };

                var page = string.Empty;
                var count = 0;

                foreach (var map in Portal2.CampaignMaps)
                {
                    var chamber1 = (profile1.Times as DataTimes)!.GetMapData(map);
                    if (chamber1 is null) continue;

                    var chamber2 = (profile2.Times as DataTimes)!.GetMapData(map);
                    if (chamber2 is null) continue;

                    if ((count % 5 == 0) && (count != 0))
                    {
                        pages.Add(page);
                        page = string.Empty;
                    }

                    page += $"\n[{map.Alias}]({map.Url}) | " +
                        $"{chamber1.PlayerRank.FormatRankToString("WR")} " +
                        $"{chamber1.Score.AsTimeToString()} | " +
                        $"{chamber2.PlayerRank.FormatRankToString("WR")} " +
                        $"{chamber2.Score.AsTimeToString()}";

                    count++;
                }
                pages.Add(page);

                var author = default(EmbedAuthorBuilder);
                if (string.IsNullOrEmpty(userNameOrSteamId2))
                {
                    author = new EmbedAuthorBuilder()
                    {
                        Name = profile2.DisplayName,
                        IconUrl = profile2.SteamAvatarUrl,
                        Url = profile2.Url
                    };
                }

                await PagedReplyAsync
                (
                    new PaginatedMessage()
                    {
                        Title = $"{profile1.DisplayName ?? profile1.SteamName} vs {profile2.DisplayName ?? profile2.SteamName}",
                        Color = Color.Blue,
                        Pages = pages,
                        Author = author,
                        Options = new PaginatedAppearanceOptions()
                        {
                            DisplayInformationIcon = false,
                            Timeout = TimeSpan.FromSeconds(5 * 60)
                        }
                    },
                    false
                );
            }

            private async Task<IProfile?> GetProfileAsync(string? userNameOrSteamId64, bool checkSelf = true)
            {
                if (string.IsNullOrEmpty(userNameOrSteamId64) && checkSelf)
                {
                    // Get user's linked SteamId64 when the DAPI supports this
                    // This will never happen right :(?
                    var nick = (Context.User as SocketGuildUser)?.Nickname;
                    if (nick != null)
                    {
                        var profile = await _client.GetProfileAsync(nick);
                        if (profile is {})
                            return profile;
                    }

                    return await _client.GetProfileAsync(Context.User.Username);
                }
                else
                {
                    return await _client.GetProfileAsync(userNameOrSteamId64);
                }
            }
        }
    }
}
