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
	public class StatsModule : InteractiveBase<SocketCommandContext>
	{
		[Command("guild"), Alias("server")]
		public async Task Guild()
		{
			await ReplyAndDeleteAsync(string.Empty, embed: new EmbedBuilder
			{
				Color = await Context.User.GetRoleColor(Context.Guild)
			}
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Owner";
				field.Value = (Context.Guild.Owner != null)
					? $"{Context.Guild.Owner.Username}#{Context.Guild.Owner.Discriminator}" +
					  $"\n{Context.Guild.OwnerId}"
					: $"{Context.Guild.OwnerId}";
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Created At";
				field.Value = Context.Guild.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "ID";
				field.Value = $"{Context.Guild.Id}";
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Members";
				var total = Context.Guild.MemberCount;
				var users = Context.Guild.Users.Count;
				var bots = 0;
				foreach (var item in Context.Guild.Users)
					if (item.IsBot)
						bots++;
				field.Value = (users != total)
					? $"Users • {users - bots}\n" +
					  $"Bots • {bots}\n" +
					  $"Online • {users}\n" +
					  $"Offline • {total - users}\n" +
					  $"Total • {total}"
					: $"Users • {users - bots}\n" +
					  $"Bots • {bots}\n" +
					  $"Total • {total}";
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Channels";
				field.Value =
					$"Text • {Context.Guild.VoiceChannels.Count}\n" +
					$"Voice • {Context.Guild.TextChannels.Count}\n" +
					$"Total • {Context.Guild.Channels.Count}";
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Features";
				var result = string.Empty;
				foreach (var item in Context.Guild.Features)
					result += $"\n`{item}`";
				field.Value = (result != string.Empty)
					? $"{Context.Guild.Features.Count}{result}"
					: "None";
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Default Channel";
				field.Value = Context.Guild.DefaultChannel.Name;
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Verification Level";
				field.Value = $"{Context.Guild.VerificationLevel}";
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Links";
				field.Value =
					$"[Icon]({Context.Guild.IconUrl})" +
					(!string.IsNullOrEmpty(Context.Guild.SplashUrl)
						? $"\n[Link]({Context.Guild.SplashUrl})"
						: string.Empty)
					+ $"\n[Banner](https://discordapp.com/api/guilds/{Context.Guild.Id}/embed.png?style=banner1)";
			})
			.Build(),
			timeout: TimeSpan.FromSeconds(60));
		}
		[Command("hierarchy")]
		public async Task Hierarchy()
		{
			var result = string.Empty;
			var position = -1;
			var members = Context.Guild.Roles.OrderByDescending(role => role.Position);
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
			
			await ReplyAndDeleteAsync(string.Empty, embed: new EmbedBuilder
			{
				Color = await Context.User.GetRoleColor(Context.Guild),
				Title = "Guild Hierarchy",
				Description = result
			}
			.Build(),
			timeout: TimeSpan.FromSeconds(60));
		}
		[Command("channel")]
		public async Task Channel()
		{
			await ReplyAndDeleteAsync(string.Empty, embed: new EmbedBuilder
			{
				Color = await Context.User.GetRoleColor(Context.Guild)
			}
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Name";
				field.Value = Context.Channel.Name;
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "ID";
				field.Value = $"{Context.Channel.Id}";
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Created At";
				field.Value = Context.Channel.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Members";
				var members = (Context.Channel as SocketChannel).Users;
				var bots = 0;
				foreach (var item in members)
					if (item.IsBot)
						bots++;
				field.Value =
					$"Users • {members.Count - bots}\n" +
					$"Bots • {bots}\n" +
					$"Total • {members.Count}";
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Position";
				field.Value = $"{(Context.Channel as SocketGuildChannel).Position}";
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Permissions";
				field.Value = $"{(Context.Channel as SocketGuildChannel).PermissionOverwrites.Count}";
			})
			.Build(),
			timeout: TimeSpan.FromSeconds(60));
		}
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

			await ReplyAndDeleteAsync(string.Empty, embed: new EmbedBuilder
			{
				Color = await Context.User.GetRoleColor(Context.Guild),
				Title = $"Top {count} User IDs {order}",
				Description = result
			}
			.Build(),
			timeout: TimeSpan.FromSeconds(60));
		}
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

			await ReplyAndDeleteAsync(string.Empty, embed: new EmbedBuilder
			{
				Color = await Context.User.GetRoleColor(Context.Guild),
				Title = $"Top {count} User Discriminators {order}",
				Description = result
			}
			.Build(),
			timeout: TimeSpan.FromSeconds(60));
		}
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

			await ReplyAndDeleteAsync(string.Empty, embed: new EmbedBuilder
			{
				Color = await Context.User.GetRoleColor(Context.Guild),
				Title = $"Top {count} User Joined Dates {order}",
				Description = result
			}
			.Build(),
			timeout: TimeSpan.FromSeconds(60));
		}
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

			await ReplyAndDeleteAsync(string.Empty, embed: new EmbedBuilder
			{
				Color = await Context.User.GetRoleColor(Context.Guild),
				Title = $"Top {count} User Created Dates {order}",
				Description = result
			}
			.Build(),
			timeout: TimeSpan.FromSeconds(60));
		}
		[Command("score")]
		public async Task Score(bool ascending = true)
		{
			// Not sure if this algorithm is fair:
			// Appending user discriminator with user id
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

			await ReplyAndDeleteAsync(string.Empty, embed: new EmbedBuilder
			{
				Color = await Context.User.GetRoleColor(Context.Guild),
				Title = $"Top {count} User Scores {order}",
				Description = result
			}
			.Build(),
			timeout: TimeSpan.FromSeconds(60));
		}
	}
}