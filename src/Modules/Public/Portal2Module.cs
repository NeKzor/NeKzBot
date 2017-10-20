using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using NeKzBot.Extensions;
using Portal2Boards.Net;
using Portal2Boards.Net.Extensions;

namespace NeKzBot.Modules.Public
{
	[Group("Portal2Boards"), Alias("p2b", "p2")]
	public class Portal2Module : InteractiveBase<SocketCommandContext>
	{
		private readonly IConfiguration _config;
		private readonly Portal2BoardsClient _client;

		public Portal2Module(IConfiguration config)
		{
			_config = config;
			var http = new HttpClient();
			http.DefaultRequestHeaders.UserAgent.ParseAdd(_config["user_agent"]);
			_client = new Portal2BoardsClient(http);
		}

		[Command("?"), Alias("info", "help")]
		public Task QuestionMark()
		{
			return ReplyAndDeleteAsync("Powered by Portal2Boards.Net (v1.1)", timeout: TimeSpan.FromSeconds(60));
		}

		[Command("Leaderboard")]
		public async Task Leaderboard([Remainder] string mapName)
		{
			if (!string.IsNullOrEmpty(mapName))
			{
				var map = await Portal2.GetMapByName(mapName);
				if (map != null)
				{
					var board = await _client.GetLeaderboardAsync(map);
					var result = $"[Top Five Rankings For {map.Alias}]";
					foreach (var entry in board.Take(5))
						result += $"\n**{entry.PlayerRank}** -> {entry.Player.Name.ToRawText()} with {entry.Score.AsTimeToString()}";
					await ReplyAndDeleteAsync("Test", timeout: TimeSpan.FromSeconds(60));
				}
				else
					await ReplyAndDeleteAsync($"Could not find a map named *{mapName.ToRawText()}*.", timeout: TimeSpan.FromSeconds(60));
			}
			else
				await ReplyAndDeleteAsync("Invalid map name.", timeout: TimeSpan.FromSeconds(60));
		}

		[Command("Changelog")]
		public Task Changelog()
		{
			return ReplyAndDeleteAsync("Todo", timeout: TimeSpan.FromSeconds(60));
		}

		[Command("Profile")]
		public Task Profile()
		{
			return ReplyAndDeleteAsync("Todo", timeout: TimeSpan.FromSeconds(60));
		}

		[Command("Aggregated")]
		public Task Aggregated()
		{
			return ReplyAndDeleteAsync("Todo", timeout: TimeSpan.FromSeconds(60));
		}
	}
}