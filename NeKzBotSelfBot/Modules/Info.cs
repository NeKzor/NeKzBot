using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NeKzBot.Classes;
using NeKzBot.Resources;
using NeKzBot.Server;

namespace NeKzBot.Modules
{
	public class Info : ModuleBase
	{
		[Command("info")]
		public async Task PrintInfo()
		{
			await Message.EditAsync(Context.Message, new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					Name = Context.User.Username,
					IconUrl = Context.User.GetAvatarUrl(),
					Url = "https://github.com/NeKzor"
				},
				Color = await Utils.GetUserColor(Context.User, Context.Guild),
				Title = Configuration.Default.AppName,
				Description = $"Version {Configuration.Default.AppVersion}",
				Url = Configuration.Default.AppUrl
			}
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Guilds";
				var guilds = (Context.Client as DiscordSocketClient).Guilds.Count;
				var ownguilds = (Context.Client as DiscordSocketClient).Guilds.Count(guild => guild.OwnerId == Context.User.Id);
				field.Value = $"Watching • {guilds - ownguilds}\nHosting • {ownguilds}\nTotal • {guilds}";
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Channels";
				var channels = (Context.Client as DiscordSocketClient).Guilds.Sum(guild => guild.Channels.Count);
				var textchannels = (Context.Client as DiscordSocketClient).Guilds.Sum(guild => guild.TextChannels.Count);
				var voicechannels = (Context.Client as DiscordSocketClient).Guilds.Sum(guild => guild.VoiceChannels.Count);
				field.Value = $"Text • {textchannels}\nVoice • {voicechannels}\nTotal • {channels}";
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Users";
				var users = (Context.Client as DiscordSocketClient).Guilds.Sum(guild => guild.Users.Count);
				var bots = (Context.Client as DiscordSocketClient).Guilds.SelectMany(guild => guild.Users).Count(user => user.IsBot);
				field.Value = $"People • {(users - bots).ToString("#,###,###.##")}\nBots • {bots.ToString("#,###,###.##")}\nTotal • {users.ToString("#,###,###.##")}";
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Latency";
				field.Value = $"{Bot.Client.Latency} ms";
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Heap Size";
				field.Value = $"{Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2)} MB";
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Threads";
				field.Value = Process.GetCurrentProcess().Threads.Count.ToString();
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Uptime";
				field.Value = (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"hh\:mm\:ss");
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Local Time";
				field.Value = DateTime.Now.ToUniversalTime().ToString("HH:mm:ss");
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Location";
				field.Value = "Graz, Austria";
			})
			.AddField(field =>
			{
				field.Name = "Library";
				field.Value = $"Discord.Net {DiscordConfig.Version}";
			})
			.AddField(field =>
			{
				field.Name = "Runtime";
				field.Value = RuntimeInformation.FrameworkDescription;
			})
			.AddField(field =>
			{
				field.Name = "Operating System";
				field.Value = RuntimeInformation.OSDescription;
			})
			.AddField(field =>
			{
				field.Name = "Modules";
				var output = string.Empty;
				foreach (var module in Bot.Handler.Service.Modules.OrderBy(module => module.Name))
					output += $"\n**{module.Name}** with {module.Commands.Count} commands";
				field.Value = (output != string.Empty)
									  ? output
									  : "None modules are loaded.";
			}));
		}

		[Command("modules")]
		public async Task ShowModules()
		{
			var embed = new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					Name = Context.User.Username,
					IconUrl = Context.User.GetAvatarUrl(),
					Url = "https://github.com/NeKzor"
				},
				Color = await Utils.GetUserColor(Context.User, Context.Guild),
				Title = Configuration.Default.AppName,
				Description = $"Loaded {Bot.Handler.Service.Modules.Count()} modules with at total of {Bot.Handler.Service.Commands.Count()} commands.",
				Url = Configuration.Default.AppUrl
			};

			foreach (var module in Bot.Handler.Service.Modules.Take((int)DiscordConstants.MaximumFieldsInEmbed))
			{
				embed.AddField(async field =>
				{
					field.Name = module.Name;
					var output = string.Empty;
					foreach (var command in module.Commands)
						output += $"`{command.Name}`, ";
					field.Value = await Utils.CutMessage(output.Substring(0, output.Length - 2), (int)DiscordConstants.MaximumCharsPerEmbedField, string.Empty);
				});
			}
			await Message.EditAsync(Context.Message, embed);
		}

		[Command("guilds")]
		public async Task ShowGuilds()
		{
			var guilds = (Context.Client as DiscordSocketClient).Guilds.OrderBy(guild => guild.Name);
			var rest = string.Empty;
			var embed = new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					Name = Context.User.Username,
					IconUrl = Context.User.GetAvatarUrl(),
					Url = "https://github.com/NeKzor"
				},
				Color = await Utils.GetUserColor(Context.User, Context.Guild),
				Title = "Guilds",
				Url = "https://discordapp.com/developers"
			}
			.AddField(field =>
			{
				field.Name = "Sorted By Name";
				var output = string.Empty;
				foreach (var guild in guilds)
				{
					var owner = guild.Owner;
					var temp = $"\n**{guild.Name}**"
							 + $"{(Debugger.IsAttached ? $"({guild.Id})" : string.Empty)}"
							 + $"({(owner != null ? $"{owner.Username}#{owner.Discriminator}" : "Not found")})"
							 + $"({guild.Users?.Count.ToString() ?? "Not found"})";
					if (output.Length + temp.Length > (int)DiscordConstants.MaximumCharsPerEmbedField)
						rest += temp;
					else
						output += temp;
				}
				field.Value = output;
			});

			// Ehem
			if ((rest != string.Empty)
			&& (rest.Length < (int)DiscordConstants.MaximumCharsPerEmbedField))
			{
				embed.AddField(field =>
				{
					field.Name = "v";
					field.Value = rest;
				});
			}
			await Message.EditAsync(Context.Message, embed);
		}

		[Command("guild")]
		public async Task GetGuildInfo(ulong id = 0)
		{
			var guild = (id == 0)
							? Context.Guild as SocketGuild
							: (Context.Client as DiscordSocketClient).Guilds.FirstOrDefault(gui => gui.Id == id);

			await Message.EditAsync(Context.Message, new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					Name = guild.Name,
					IconUrl = guild.IconUrl
				},
				Color = await Utils.GetUserColor(guild.Owner, Context.Guild),
				Title = "Guild Info",
				Url = "https://discordapp.com/developers"
			}
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Owner";
				field.Value = (guild.Owner != null)
										   ? $"{guild.Owner.Username}#{guild.Owner.Discriminator}\n{guild.OwnerId}"
										   : guild.OwnerId.ToString();
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Created At";
				field.Value = guild.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "ID";
				field.Value = guild.Id.ToString();
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Members";
				var total = guild.MemberCount;
				var users = guild.Users.Count;
				var bots = 0;
				foreach (var item in guild.Users)
					if (item.IsBot)
						bots++;
				field.Value = (users != total)
									 ? $"Users • {users - bots}\nBots • {bots}\nOnline • {users}\nOffline • {total - users}\nTotal • {total}"
									 : $"Users • {users - bots}\nBots • {bots}\nTotal • {total}";
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Channels";
				field.Value = $"Text • {guild.VoiceChannels.Count}\nVoice • {guild.TextChannels.Count}\nTotal • {guild.Channels.Count}";
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Features";
				var output = string.Empty;
				foreach (var item in guild.Features)
					output += $"\n`{item}`";
				field.Value = (output != string.Empty)
									  ? $"{guild.Features.Count}{output}"
									  : "None";
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Default Channel";
				field.Value = guild.DefaultChannel.Name;
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Verification Level";
				field.Value = guild.VerificationLevel.ToString();
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Links";
				field.Value = $"[Icon]({guild.IconUrl}){(guild.SplashUrl != null ? $"\n[Link]({guild.SplashUrl})" : string.Empty)}"
							+ $"\n[Banner](https://discordapp.com/api/guilds/{guild.Id}/embed.png?style=banner1)";
			})
			.AddField(field =>
			{
				field.Name = "Roles";
				var output = string.Empty;
				foreach (var item in guild.Roles.OrderByDescending(role => role.Position))
					output += $"`{item.Name}`, ";
				field.Value = $"{guild.Roles.Count}\n{ output.Substring(0, output.Length - 2)}";
			})
			.AddField(field =>
			{
				field.Name = "Emojis";
				var output = string.Empty;
				foreach (var item in guild.Emojis)
					output += (guild.Id == Context.Guild.Id)
										? $"<:{item.Name}:{item.Id}>  "
										: $"`{item.Name}`, ";
				field.Value = (output != string.Empty)
									  ? $"{guild.Emojis.Count}\n{output.Substring(0, output.Length - 2)}"
									  : "None";
			}));
		}

		[Command("hierarchy")]
		public async Task GetGuildHierarchy(ulong id = 0)
		{
			var guild = (id == 0)
							? Context.Guild as SocketGuild
							: (Context.Client as DiscordSocketClient).Guilds.FirstOrDefault(gui => gui.Id == id);

			await Message.EditAsync(Context.Message, new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					Name = guild.Name,
					IconUrl = guild.IconUrl
				},
				Color = await Utils.GetUserColor(guild.Owner, Context.Guild),
				Title = "Guild Hierarchy",
				Url = "https://discordapp.com/developers"
			}
			.AddField(async field =>
			{
				field.Name = "Roles";
				var output = string.Empty;
				var position = -1;
				var members = guild.Roles.OrderByDescending(role => role.Position);
				foreach (var member in members)
				{
					if (position != member.Position)
						output += $"\n{await Tasks.Leaderboard.FormatRank((members.FirstOrDefault().Position - member.Position + 1).ToString())} • `{member.Name}`";
					else
						output += $", `{member.Name}`";
					position = member.Position;
				}
				field.Value = output;
			}));
		}

		[Command("channel")]
		public async Task GetChannelInfo(ulong id = 0)
		{
			var channel = ((Context.Guild == null)
			&& (id == 0))
				   ? Context.Channel as SocketGuildChannel
				   : (Context.Guild as DiscordSocketClient).Guilds.FirstOrDefault(guild => guild.Channels.Any(cha => cha.Id == id)).Channels
																  .FirstOrDefault(cha => cha.Id == id);

			await Message.EditAsync(Context.Message, new EmbedBuilder
			{
				Author = new EmbedAuthorBuilder
				{
					Name = channel.Guild.Name,
					IconUrl = channel.Guild.IconUrl
				},
				Color = await Utils.GetUserColor(channel.Guild.Owner, channel.Guild),
				Title = "Channel Info",
				Url = "https://discordapp.com/developers"
			}
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Name";
				field.Value = channel.Name;
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "ID";
				field.Value = channel.Id.ToString();
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Created At";
				field.Value = channel.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Members";
				var members = channel.Users;
				var bots = 0;
				foreach (var item in members)
					if (item.IsBot)
						bots++;
				field.Value = $"Users • {members.Count - bots}\nBots • {bots}\nTotal • {members.Count}";
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Position";
				field.Value = channel.Position.ToString();
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Permissions";
				field.Value = channel.PermissionOverwrites.Count.ToString();
			}));
		}

		[Command("whois")]
		public async Task GetUserInfo(ulong id = 0)
		{
			var user = (id == 0)
						   ? ((Context.Guild as SocketGuild).Users).FirstOrDefault(guild => guild.Id == Context.User.Id)
						   : ((Context.Guild as SocketGuild).Users.Any(usr => usr.Id == id))
								   ? ((Context.Guild as SocketGuild).Users).FirstOrDefault(usr => usr.Id == id)
								   : (Context.Client as DiscordSocketClient).Guilds.FirstOrDefault(guild => guild.Users.Any(usr => usr.Id == id)).Users
																				   .FirstOrDefault(usr => usr.Id == id);

			var embed = new EmbedBuilder
			{
				Color = await Utils.GetUserColor(user, Context.Guild),
				Title = "User Info",
				Url = "https://discordapp.com/developers"
			}
			.WithAuthor(author =>
			{
				author.Name = user.Username;
				author.IconUrl = user.GetAvatarUrl();
				if (user.Id == Bot.Client.CurrentUser.Id)
					author.Url = "https://github.com/NeKzor";
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Created At";
				field.Value = user.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "ID";
				field.Value = user.Id.ToString();
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Discriminator";
				field.Value = user.Discriminator;
			})
			.AddField(field =>
			{
				field.IsInline = true;
				if (!(Context.IsPrivate))
				{
					field.Name = "Joined At";
					field.Value = user.JoinedAt.Value.ToString("yyyy-MM-dd hh:mm:ss");
				}
				else
				{
					field.Name = "First Contact At";
					field.Value = Context.Channel.CreatedAt.ToString("yyyy-MM-dd hh:mm:ss");
				}
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Status";
				field.Value = user.Status.ToString();
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Avatar Url";
				field.Value = $"[Link]({user.GetAvatarUrl()})";
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Game";
				field.Value = (user.Game != null)
										 ? $"{user.Game.Value.Name}\n{(user.Game.Value.StreamType != StreamType.NotStreaming ? $"Live on [{user.Game.Value.StreamType}]({user.Game.Value.StreamUrl})" : string.Empty)}"	// Sry
										 : "None";
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Nickname";
				field.Value = user.Nickname?.ToString() ?? "None";
			})
			.AddField(field =>
			{
				field.IsInline = true;
				field.Name = "Is Bot?";
				field.Value = (user.IsBot)
								   ? "Yes"
								   : "No";
			});

			if (user.Guild.Id == Context.Guild?.Id)
			{
				embed.AddField(async field =>
				{
					field.IsInline = true;
					field.Name = "Roles";
					var roles = await Context.Guild.GetUserAsync(user.Id);
					var output = string.Empty;
					foreach (var role in Context.Guild.Roles)
						if (roles.RoleIds.Contains(role.Id))
							output += $"`{role.Name}`, ";
					field.Value = output.Substring(0, output.Length - 2);
				});
			}
			else if (!(Context.IsPrivate))
				embed.WithFooter(footer => footer.Text = "Not on this server.");
			await Message.EditAsync(Context.Message, embed);
		}

		// This command is kinda obsolete
		[Command("idinfo")]
		public async Task GetIdInfo(string resource = "channel")
		{
			var users = default(IUser[]);
			var embed = new EmbedBuilder();
			if (string.Equals(resource, "channel", StringComparison.CurrentCultureIgnoreCase))
			{
				embed.Title = "Channel User ID Info";
				users = (await Context.Channel.GetUsersAsync().Flatten()).ToArray();
			}
			else if (string.Equals(resource, "server", StringComparison.CurrentCultureIgnoreCase))
			{
				embed.Title = "Server User ID Info";
				users = (await Context.Guild.GetUsersAsync()).ToArray();
			}

			if (users == null)
				return;

			var lowestid = users.FirstOrDefault();
			var highestid = users.FirstOrDefault();
			var lowestids = new List<IUser>();
			var highestids = new List<IUser>();
			var sumids = 0;
			foreach (var item in users.ToList())
			{
				// How and why?
				if (item.DiscriminatorValue == 0)
					continue;

				// Search lowest id
				if (lowestid.DiscriminatorValue > item.DiscriminatorValue)
				{
					lowestid = item;
					lowestids = new List<IUser> { item };
				}
				else if (lowestid.DiscriminatorValue == item.DiscriminatorValue)
					lowestids.Add(item);

				// Search highest id
				if (highestid.DiscriminatorValue < item.DiscriminatorValue)
				{
					highestid = item;
					highestids = new List<IUser> { item };
				}
				else if (highestid.DiscriminatorValue == item.DiscriminatorValue)
					highestids.Add(item);

				sumids += item.DiscriminatorValue;
			}

			embed.Author = new EmbedAuthorBuilder
			{
				Name = Context.Guild.Name,
				IconUrl = Context.Guild.IconUrl
			};
			embed.Color = await Utils.GetUserColor(Context.User, Context.Guild);
			embed.Url = "https://discordapp.com/developers";
			embed.AddField(field =>
			{
				field.Name = "Lowest ID";
				var value = string.Empty;
				foreach (var item in lowestids)
					value += $"{item.Username}#{item.Discriminator}\n";
				field.Value = value.Substring(0, value.Length - 1);
			})
			.AddField(field =>
			{
				field.Name = "Highest ID";
				var value = string.Empty;
				foreach (var item in highestids)
					value += $"{item.Username}#{item.Discriminator}\n";
				field.Value = value.Substring(0, value.Length - 1);
			})
			.AddField( field =>
			{
				field.Name = "Average ID";
				field.Value = $"#{Math.Round((decimal)sumids / users.Length, 0).ToString("D4")}";
			});
			await Message.EditAsync(Context.Message, embed);
		}
	}
}