using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;

namespace NeKzBot.Modules.Public
{
	// Really old code here...
	public class StatsModule : InteractiveBase<SocketCommandContext>
	{
		[RequireContext(ContextType.Guild)]
		[Command("guild"), Alias("server")]
		public async Task Guild()
		{
			var owner = (Context.Guild.Owner != null)
				? $"{Context.Guild.Owner.Username}#{Context.Guild.Owner.Discriminator}\n{Context.Guild.OwnerId}"
				: $"{Context.Guild.OwnerId}";
			
			var members = Context.Guild.MemberCount;
			var users = Context.Guild.Users.Count;
			var bots = Context.Guild.Users.Count(u => u.IsBot);
			
			var features = string.Empty;
			foreach (var feature in Context.Guild.Features)
				features += $"\n`{feature}`";

			var splash = (!string.IsNullOrEmpty(Context.Guild.SplashUrl))
				? $"\n[Link]({Context.Guild.SplashUrl})"
				: string.Empty;
			
			var embed = new EmbedBuilder()
				.WithColor(await Context.User.GetRoleColor(Context.Guild))
				// Fields
				.AddField("Owner", owner, true)
				.AddField("Created At", Context.Guild.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"), true)
				.AddField("ID", $"{Context.Guild.Id}", true)
				.AddField("Members", (users != members)
					? $"Users • {users - bots}\n" +
						$"Bots • {bots}\n" +
						$"Online • {users}\n" +
						$"Offline • {members - users}\n" +
						$"Total • {members}"
					: $"Users • {users - bots}\n" +
						$"Bots • {bots}\n" +
						$"Total • {members}",
					  true)
				.AddField("Channels",
					$"Text • {Context.Guild.VoiceChannels.Count}\n" +
					$"Voice • {Context.Guild.TextChannels.Count}\n" +
					$"Total • {Context.Guild.Channels.Count}",
					true)
				.AddField("Features", (features != string.Empty)
					? $"{Context.Guild.Features.Count}{features}"
					: "None",
					true)
				.AddField("Default Channel", Context.Guild.DefaultChannel.Name, true)
				.AddField("Verification Level", $"{Context.Guild.VerificationLevel}", true)
				.AddField("Links", $"[Icon]({Context.Guild.IconUrl})" +
					splash +
					$"\n[Banner](https://discordapp.com/api/guilds/{Context.Guild.Id}/embed.png?style=banner1)",
					true);
				
			await ReplyAndDeleteAsync(string.Empty, embed: embed.Build());
		}
		[RequireContext(ContextType.Guild)]
		[Command("hierarchy")]
		public async Task Hierarchy()
		{
			var members = Context.Guild.Roles
				.OrderByDescending(r => r.Position);
			
			var result = string.Empty;
			var position = -1;

			foreach (var member in members)
			{
				var temp = string.Empty;
				if (position != member.Position)
					temp = $"\n{members.FirstOrDefault().Position - member.Position + 1}. • `{member.Name}`";
				else
					temp = $", `{member.Name}`";
				
				if (result.Length + temp.Length > 2048)
					break;
				
				result += temp;
				position = member.Position;
			}

			var embed = new EmbedBuilder()
				.WithTitle("Guild Hierarchy")
				.WithColor(await Context.User.GetRoleColor(Context.Guild))
				.WithDescription(result);

			await ReplyAndDeleteAsync(string.Empty, embed: embed.Build());
		}
		[RequireContext(ContextType.Guild)]
		[Command("channel")]
		public async Task Channel()
		{
			var users = (Context.Channel as SocketChannel).Users;
			var bots = users.Count(m => m.IsBot);

			var embed = new EmbedBuilder()
				.WithColor(await Context.User.GetRoleColor(Context.Guild))
				// Fields
				.AddField("Name", Context.Channel.Name, true)
				.AddField("Created At", Context.Channel.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"), true)
				.AddField("ID", $"{Context.Channel.Id}", true)
				.AddField("Members",
					$"Users • {users.Count - bots}\n" +
					$"Bots • {bots}\n" +
					$"Total • {users.Count}",
					true)
				.AddField("Position", $"{(Context.Channel as SocketGuildChannel).Position}", true)
				.AddField("Permissions", $"{(Context.Channel as SocketGuildChannel).PermissionOverwrites.Count}", true);
				
			
			await ReplyAndDeleteAsync(string.Empty, embed: embed.Build());
		}
		[RequireContext(ContextType.Guild)]
		[Command("id")]
		public async Task Id(bool ascending = true)
		{
			var order = string.Empty;
			var users = default(IEnumerable<SocketGuildUser>);

			if (ascending)
			{
				order = "(asc.)";
				users = Context.Guild.Users
					.OrderBy(u =>u.Id)
					.Take(10);
			}
			else
			{
				order = "(desc.)";
				users = Context.Guild.Users
					.OrderByDescending(u =>u.Id)
					.Take(10);
			}

			var count = 0;
			var result = string.Empty;

			foreach (var user in users)
			{
				var temp = $"\n{user.Id} = {user.Username}";
				if (result.Length + temp.Length > 2048)
					break;
				
				result += temp;
				count++;
			}

			var embed = new EmbedBuilder()
				.WithColor(await Context.User.GetRoleColor(Context.Guild))
				.WithTitle($"Top {count} User IDs {order}")
				.WithDescription(result);

			await ReplyAndDeleteAsync(string.Empty, embed: embed.Build());
		}
		[RequireContext(ContextType.Guild)]
		[Command("disc"), Alias("discriminator")]
		public async Task Disc(bool ascending = true)
		{
			var order = string.Empty;
			var users = default(IEnumerable<SocketGuildUser>);

			if (ascending)
			{
				order = "(asc.)";
				users = Context.Guild.Users
					.OrderBy(u =>u.DiscriminatorValue)
					.Take(10);
			}
			else
			{
				order = "(desc.)";
				users = Context.Guild.Users
					.OrderByDescending(u =>u.DiscriminatorValue)
					.Take(10);
			}

			var count = 0;
			var result = string.Empty;

			foreach (var user in users)
			{
				var temp = $"\n{user.Discriminator} = {user.Username}";
				if (result.Length + temp.Length > 2048)
					break;
				
				result += temp;
				count++;
			}

			var embed = new EmbedBuilder()
				.WithColor(await Context.User.GetRoleColor(Context.Guild))
				.WithTitle($"Top {count} User Discriminators {order}")
				.WithDescription(result);

			await ReplyAndDeleteAsync(string.Empty, embed: embed.Build());
		}
		[RequireContext(ContextType.Guild)]
		[Command("joined")]
		public async Task Joined(bool ascending = true)
		{
			var order = string.Empty;
			var users = default(IEnumerable<SocketGuildUser>);

			if (ascending)
			{
				order = "(asc.)";
				users = Context.Guild.Users
					.OrderBy(u =>u.JoinedAt)
					.Take(10);
			}
			else
			{
				order = "(desc.)";
				users = Context.Guild.Users
					.OrderByDescending(u =>u.JoinedAt)
					.Take(10);
			}

			var count = 0;
			var result = string.Empty;

			foreach (var user in users)
			{
				var temp = $"\n{user.CreatedAt.ToString(@"yyyy\-MM\-dd hh\:mm\:ss")} = {user.Username}";
				if (result.Length + temp.Length > 2048)
					break;
				
				result += temp;
				count++;
			}

			var embed = new EmbedBuilder()
				.WithColor(await Context.User.GetRoleColor(Context.Guild))
				.WithTitle($"Top {count} User Joined Dates {order}")
				.WithDescription(result);
			
			await ReplyAndDeleteAsync(string.Empty, embed: embed.Build());
		}
		[RequireContext(ContextType.Guild)]
		[Command("created")]
		public async Task Created(bool ascending = true)
		{
			var order = string.Empty;
			var users = default(IEnumerable<SocketGuildUser>);

			if (ascending)
			{
				order = "(asc.)";
				users = Context.Guild.Users
					.OrderBy(u =>u.CreatedAt)
					.Take(10);
			}
			else
			{
				order = "(desc.)";
				users = Context.Guild.Users
					.OrderByDescending(u =>u.CreatedAt)
					.Take(10);
			}

			var count = 0;
			var result = string.Empty;

			foreach (var user in users)
			{
				var temp = $"\n{user.CreatedAt.ToString(@"yyyy\-MM\-dd hh\:mm\:ss")} = {user.Username}";
				if (result.Length + temp.Length > 2048)
					break;
				
				result += temp;
				count++;
			}

			var embed = new EmbedBuilder()
				.WithColor(await Context.User.GetRoleColor(Context.Guild))
				.WithTitle($"Top {count} User Created Dates {order}")
				.WithDescription(result);
			
			await ReplyAndDeleteAsync(string.Empty, embed: embed.Build());
		}
		// Not sure if this algorithm is fair:
		// Appending user discriminator with user id
		[RequireContext(ContextType.Guild)]
		[Command("score")]
		public async Task Score(bool ascending = true)
		{
			var order = string.Empty;
			var users = default(IEnumerable<SocketGuildUser>);

			if (ascending)
			{
				order = "(asc.)";
				users = Context.Guild.Users
					.OrderBy(u => double.Parse($"{u.DiscriminatorValue}{u.Id}"))
					.Take(10);
			}
			else
			{
				order = "(desc.)";
				users = Context.Guild.Users
					.OrderByDescending(u => double.Parse($"{u.DiscriminatorValue}{u.Id}"))
					.Take(10);
			}

			var count = 0;
			var result = string.Empty;

			foreach (var user in users)
			{
				var score = Math.Round(Math.Log(double.Parse($"{user.DiscriminatorValue}{user.Id}")), 3);
				var temp = $"\n{score.ToString("N3")} = {user.Username}";

				if (result.Length + temp.Length > 2048)
					break;
				
				result += temp;
				count++;
			}

			var embed = new EmbedBuilder()
				.WithColor(await Context.User.GetRoleColor(Context.Guild))
				.WithTitle($"Top {count} User Scores {order}")
				.WithDescription(result);
			
			await ReplyAndDeleteAsync(string.Empty, embed: embed.Build());
		}
	}
}