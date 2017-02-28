using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NeKzBot.Classes;
using NeKzBot.Resources;

namespace NeKzBot.Modules
{
	[Group("Stats"), Alias("top")]
	public class Stats : ModuleBase
	{
		[Command("id"), Alias("ids")]
		public async Task ShowTopUserIds(ulong id = 0, string sort = "", int top = 10)
		{
			if (top < 2)
				return;

			id = (id == 0)
					 ? Context.Guild.Id
					 : id;
			var guild = (Context.Client as DiscordSocketClient).Guilds.FirstOrDefault(gui => gui.Id == id);
			top = (top > guild.Users.Count)
					   ? guild.Users.Count
					   : top;

			var sortedby = string.Empty;
			var users = default(IEnumerable<SocketGuildUser>);
			if (string.Equals(sort, "v", StringComparison.CurrentCultureIgnoreCase))
			{
				sortedby = "Descending Order";
				users = guild.Users.OrderByDescending(user => user.Id)
								   .Take(top);
			}
			else
			{
				sortedby = "Ascending Order";
				users = guild.Users.OrderBy(user => user.Id)
								   .Take(top);
			}

			var count = 0;
			var output = string.Empty;
			foreach (var user in users)
			{
				var temp = $"\n{user.Id} = {user.Username}";
				if (output.Length + temp.Length > DiscordConstants.MaximumCharsPerEmbedField)
					break;
				output += temp;
				count++;
			}

			await Message.EditAsync(Context.Message, new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					Name = guild.Name,
					IconUrl = guild.IconUrl
				},
				Color = await Utils.GetUserColor(Context.User, Context.Guild),
				Title = $"Top {count} User IDs",
				Url = "https://discordapp.com/developers"
			}
			.AddField(field =>
			{
				field.Name = sortedby;
				field.Value = output;
			}));
		}

		[Command("disc"), Alias("di", "discs")]
		public async Task ShowTopUserDiscriminators(ulong id = 0, string sort = "", int top = 10)
		{
			if (top < 2)
				return;

			id = (id == 0)
					 ? Context.Guild.Id
					 : id;
			var guild = (Context.Client as DiscordSocketClient).Guilds.FirstOrDefault(gui => gui.Id == id);
			top = (top > guild.Users.Count)
					   ? guild.Users.Count
					   : top;

			var sortedby = string.Empty;
			var users = default(IEnumerable<SocketGuildUser>);
			if (string.Equals(sort, "v", StringComparison.CurrentCultureIgnoreCase))
			{
				sortedby = "Descending Order";
				users = guild.Users.OrderByDescending(user => user.DiscriminatorValue)
								   .Take(top);
			}
			else
			{
				sortedby = "Ascending Order";
				users = guild.Users.OrderBy(user => user.DiscriminatorValue)
								   .Take(top);
			}

			var count = 0;
			var output = string.Empty;
			foreach (var user in users)
			{
				var temp = $"\n{user.Discriminator} = {user.Username}";
				if (output.Length + temp.Length > DiscordConstants.MaximumCharsPerEmbedField)
					break;
				output += temp;
				count++;
			}

			await Message.EditAsync(Context.Message, new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					Name = guild.Name,
					IconUrl = guild.IconUrl
				},
				Color = await Utils.GetUserColor(Context.User, Context.Guild),
				Title = $"Top {count} User Discriminators",
				Url = "https://discordapp.com/developers"
			}
			.AddField(field =>
			{
				field.Name = sortedby;
				field.Value = output;
			}));
		}

		[Command("joined")]
		public async Task ShowTopUserJoinedAt(ulong id = 0, string sort = "", int top = 10)
		{
			if (top < 2)
				return;

			id = (id == 0)
					 ? Context.Guild.Id
					 : id;
			var guild = (Context.Client as DiscordSocketClient).Guilds.FirstOrDefault(gui => gui.Id == id);
			top = (top > guild.Users.Count)
					   ? guild.Users.Count
					   : top;

			var sortedby = string.Empty;
			var users = default(IEnumerable<SocketGuildUser>);
			if (string.Equals(sort, "v", StringComparison.CurrentCultureIgnoreCase))
			{
				sortedby = "Descending Order";
				users = guild.Users.OrderByDescending(user => user.JoinedAt)
								   .Take(top);
			}
			else
			{
				sortedby = "Ascending Order";
				users = guild.Users.OrderBy(user => user.JoinedAt)
								   .Take(top);
			}

			var count = 0;
			var output = string.Empty;
			foreach (var user in users)
			{
				var temp = $"\n{user.CreatedAt.ToString(@"yyyy\-MM\-dd hh\:mm\:ss")} = {user.Username}";
				if (output.Length + temp.Length > DiscordConstants.MaximumCharsPerEmbedField)
					break;
				output += temp;
				count++;
			}

			await Message.EditAsync(Context.Message, new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					Name = guild.Name,
					IconUrl = guild.IconUrl
				},
				Color = await Utils.GetUserColor(Context.User, Context.Guild),
				Title = $"Top {count} User Joined Dates",
				Url = "https://discordapp.com/developers"
			}
			.AddField(field =>
			{
				field.Name = sortedby;
				field.Value = output;
			}));
		}

		[Command("created")]
		public async Task ShowTopUserCreated(ulong id = 0, string sort = "", int top = 10)
		{
			if (top < 2)
				return;

			id = (id == 0)
					 ? Context.Guild.Id
					 : id;
			var guild = (Context.Client as DiscordSocketClient).Guilds.FirstOrDefault(gui => gui.Id == id);
			top = (top > guild.Users.Count)
					   ? guild.Users.Count
					   : top;

			var sortedby = string.Empty;
			var users = default(IEnumerable<SocketGuildUser>);
			if (string.Equals(sort, "v", StringComparison.CurrentCultureIgnoreCase))
			{
				sortedby = "Descending Order";
				users = guild.Users.OrderByDescending(user => user.CreatedAt)
								   .Take(top);
			}
			else
			{
				sortedby = "Ascending Order";
				users = guild.Users.OrderBy(user => user.CreatedAt)
								   .Take(top);
			}

			var count = 0;
			var output = string.Empty;
			foreach (var user in users)
			{
				var temp = $"\n{user.CreatedAt.ToString(@"yyyy\-MM\-dd hh\:mm\:ss")} = {user.Username}";
				if (output.Length + temp.Length > DiscordConstants.MaximumCharsPerEmbedField)
					break;
				output += temp;
				count++;
			}

			await Message.EditAsync(Context.Message, new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					Name = guild.Name,
					IconUrl = guild.IconUrl
				},
				Color = await Utils.GetUserColor(Context.User, Context.Guild),
				Title = $"Top {count} User Created Dates",
				Url = "https://discordapp.com/developers"
			}
			.AddField(field =>
			{
				field.Name = sortedby;
				field.Value = output;
			}));
		}

		[Command("score"), Alias("scores")]
		public async Task ShowTopUserScores(ulong id = 0, string sort = "", int top = 10)
		{
			if (top < 2)
				return;

			id = (id == 0)
					 ? Context.Guild.Id
					 : id;
			var guild = (Context.Client as DiscordSocketClient).Guilds.FirstOrDefault(gui => gui.Id == id);
			top = (top > guild.Users.Count)
					   ? guild.Users.Count
					   : top;

			// Not sure if this algorithm is fair: appending user discriminator with user id
			var sortedby = string.Empty;
			var users = default(IEnumerable<SocketGuildUser>);
			if (string.Equals(sort, "v", StringComparison.CurrentCultureIgnoreCase))
			{
				sortedby = "Descending Order";
				users = guild.Users.OrderByDescending(user => double.Parse(user.DiscriminatorValue.ToString() + user.Id.ToString()))
								   .Take(top);
			}
			else
			{
				sortedby = "Ascending Order";
				users = guild.Users.OrderBy(user => double.Parse(user.DiscriminatorValue.ToString() + user.Id.ToString()))
								   .Take(top);
			}

			var count = 0;
			var output = string.Empty;
			foreach (var user in users)
			{
				var temp = $"\n{Math.Round(Math.Log(double.Parse(user.DiscriminatorValue.ToString() + user.Id.ToString())), 3).ToString("N3")} = {user.Username}";
				if (output.Length + temp.Length > DiscordConstants.MaximumCharsPerEmbedField)
					break;
				output += temp;
				count++;
			}

			await Message.EditAsync(Context.Message, new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					Name = guild.Name,
					IconUrl = guild.IconUrl
				},
				Color = await Utils.GetUserColor(Context.User, Context.Guild),
				Title = $"Top {count} User Scores",
				Url = "https://discordapp.com/developers"
			}
			.AddField(field =>
			{
				field.Name = sortedby;
				field.Value = output;
			}));
		}
	}
}