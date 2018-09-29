using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Addons.Preconditions;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using NeKzBot.Extensions;
using Portal2Boards;
using Portal2Boards.Extensions;

namespace NeKzBot.Modules.Public
{
    public class Portal2Module : ModuleBase<SocketCommandContext>
    {
        [Group("portal2boards"), Alias("p2b", "p2")]
        public class Portal2Boards : InteractiveBase<SocketCommandContext>
        {
            private readonly IConfiguration _config;
            private readonly Portal2BoardsClient _client;

            public Portal2Boards(IConfiguration config)
            {
                _config = config;
                _client = new Portal2BoardsClient(_config["user_agent"]);
            }

            [Command("?"), Alias("info", "help")]
            public Task QuestionMark()
            {
                var embed = new EmbedBuilder()
                    .WithColor(Color.Blue)
                    .WithDescription("[Powered by Portal2Boards.Net (v2.1)](https://nekzor.github.io/Portal2Boards.Net)");

                return ReplyAndDeleteAsync(string.Empty, embed: embed.Build());
            }
            [Ratelimit(3, 1, Measure.Minutes)]
            [Command("leaderboard"), Alias("lb")]
            public async Task Leaderboard([Remainder] string mapName = null)
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
                                    $"by [{entry.Player.Name.ToRawText()}]({(entry.Player as SteamUser).Url})";

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
                                false // Allow other users to control the pages too
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
            [Command("changelog"), Alias("cl", "clog")]
            public async Task Changelog([Remainder] string mapName)
            {
                var map = Portal2Map.Search(mapName);
                if (map == null)
                {
                    await ReplyAndDeleteAsync("Invalid map name.", timeout: TimeSpan.FromSeconds(10));
                    return;
                }
                if (map.BestTimeId == null)
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
                        ((entry.Score.Improvement != default)
                            ? $" (-{entry.Score.Improvement.AsTimeToString()})"
                            : string.Empty) +
                        $" by [{entry.Player.Name.ToRawText()}]({(entry.Player as SteamUser).Url})";

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
                    false // Allow other users to control the pages too
                );
            }
            [Ratelimit(3, 1, Measure.Minutes)]
            [Command("profile"), Alias("pro", "user")]
            public async Task Profile([Remainder] string userNameOrSteamId64 = null)
            {
                var profile = default(IProfile);
                if (string.IsNullOrEmpty(userNameOrSteamId64))
                {
                    // Get user's linked SteamId64 when the DAPI supports this
                    // This will never happen right :(?
                    var nick = (Context.User as SocketGuildUser)?.Nickname;
                    var name = Context.User.Username;
                    if (nick != null)
                        profile = await _client.GetProfileAsync(nick);
                    if (profile == null)
                        profile = await _client.GetProfileAsync(name);
                }
                else
                {
                    profile = await _client.GetProfileAsync(userNameOrSteamId64);
                }

                if (profile != null)
                {
                    // Local funcion
                    Portal2Map GetMap(IDataScore score)
                    {
                        return Portal2Map.Search((score as DataScore).Id);
                    }
                    var user = profile as Profile;
                    var pages = new List<string>
                    {
                        $"\nSP WRs | {user.Times.SinglePlayerChapters.WorldRecords}" +
                            $"\nMP WRs | {user.Times.CooperativeChapters.WorldRecords}" +
                            $"\nTotal | {user.WorldRecords}",
                        $"\nSP Points | {user.Points.SinglePlayer.Score.FormatPointsToString()}" +
                            $"\nMP Points | {user.Points.Cooperative.Score.FormatPointsToString()}" +
                            $"\nTotal | {user.Points.Global.Score.FormatPointsToString()}",
                        $"\nSP Rank | {user.Times.SinglePlayer.PlayerRank.FormatRankToString()}" +
                            $"\nMP Rank | {user.Times.Cooperative.PlayerRank.FormatRankToString()}" +
                            $"\nOverall | {user.Times.Global.PlayerRank.FormatRankToString()}",
                        $"\nSP Avg Rank | {user.Times.SinglePlayerChapters.AveragePlace.FormatAveragePlaceToString()}" +
                            $"\nMP Avg Rank | {user.Times.CooperativeChapters.AveragePlace.FormatAveragePlaceToString()}" +
                            $"\nOverall | {user.GlobalAveragePlace.FormatAveragePlaceToString()}",
                        $"\nSP Best Rank | {user.Times.SinglePlayerChapters.BestScore.PlayerRank.FormatRankToString()} on {GetMap(user.Times.SinglePlayerChapters.BestScore).Alias}" +
                            $"\nMP Best Rank | {user.Times.CooperativeChapters.BestScore.PlayerRank.FormatRankToString()} on {GetMap(user.Times.CooperativeChapters.BestScore).Alias}",
                        $"\nSP Oldest Score | {user.Times.SinglePlayerChapters.OldestScore.Score.AsTimeToString()} on {GetMap(user.Times.SinglePlayerChapters.OldestScore).Alias}" +
                            $"\nMP Oldest Score | {user.Times.CooperativeChapters.OldestScore.Score.AsTimeToString()} on {GetMap(user.Times.CooperativeChapters.OldestScore).Alias}",
                        $"\nSP Newest Score | {user.Times.SinglePlayerChapters.NewestScore.Score.AsTimeToString()} on {GetMap(user.Times.SinglePlayerChapters.NewestScore).Alias}" +
                            $"\nMP Newest Score | {user.Times.CooperativeChapters.NewestScore.Score.AsTimeToString()} on {GetMap(user.Times.CooperativeChapters.NewestScore).Alias}"
                    };

                    var page = string.Empty;
                    var count = 0;

                    foreach (var map in Portal2.CampaignMaps)
                    {
                        var chamber = (user.Times as DataTimes).GetMapData(map);
                        if (chamber == null) continue;

                        if ((count % 5 == 0) && (count != 0))
                        {
                            pages.Add(page);
                            page = string.Empty;
                        }

                        page += $"\n[{map.Alias}]({map.Url}) | " +
                            $"{chamber.ScoreRank.FormatRankToString("WR")} | " +
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
                        false // Allow other users to control the pages too
                    );
                }
                else
                    await ReplyAndDeleteAsync("Invalid user name or id.", timeout: TimeSpan.FromSeconds(10));
            }
            [Ratelimit(3, 1, Measure.Minutes)]
            [Command("aggregated"), Alias("agg")]
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
                        $"\t[{entry.Player.Name}](https://board.iverb.me/profile/{(entry.Player as SteamUser).Id})";

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
                    false // Allow other users to control the pages too
                );
            }
        }
    }
}
