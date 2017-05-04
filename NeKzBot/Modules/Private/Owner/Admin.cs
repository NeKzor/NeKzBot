using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using NeKzBot.Extensions;
using NeKzBot.Internals.Entities;
using NeKzBot.Resources;
using NeKzBot.Server;
using NeKzBot.Utilities;

namespace NeKzBot.Modules.Private.Owner
{
	public class Admin : CommandModule
	{
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Admin Module", LogColor.Init);
			await AdminCommands(Configuration.Default.BotCmd);
			await SpecialPermissionCommands(Configuration.Default.BotCmd);
		}

		private static Task AdminCommands(string name)
		{
			CService.CreateGroup(name, GBuilder =>
			{
				GBuilder.CreateCommand("newgame")
						.Alias("playnext", "nextgame", "ng")
						.Description("Sets a random playing game status.")
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e => Bot.Client.SetGame(await Utils.RngAsync((await Data.Get<Simple>("games")).Value)));

				GBuilder.CreateCommand("setgame")
						.Alias("play", "sg")
						.Description("Sets a new playing game status.")
						.Parameter("name", ParameterType.Unparsed)
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							if (!(string.IsNullOrEmpty(e.Args[0])))
							{
								if (e.Args[0] != Bot.Client.CurrentGame.Name)
								{
									Bot.Client.SetGame(e.Args[0]);
									await e.Channel.SendMessage($"Bot is now playing **{await Utils.AsRawText(e.Args[0])}**.");
								}
								else
									await e.Channel.SendMessage("Bot is already playing that game.");
							}
							else
								await e.Channel.SendMessage(await Utils.GetDescription(e.Command));
						});

				GBuilder.CreateCommand("echo")
						.Alias("say")
						.Description("Returns the given message.")
						.Parameter("message", ParameterType.Unparsed)
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage(await Utils.CutMessageAsync(e.GetArg("message"), badchars: false));
						});

				GBuilder.CreateCommand("send")
						.Description("Sends the given message to a specific channel of a guild. Use the keyword _this_ to skip the guild id parameter.")
						.Parameter("guild_id", ParameterType.Required)
						.Parameter("channel", ParameterType.Required)
						.Parameter("message", ParameterType.Unparsed)
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							if (string.IsNullOrEmpty(e.GetArg("message")))
							{
								await e.Channel.SendMessage("Message text can't be empty.");
								return;
							}

							var server = default(Discord.Server);
							if (ulong.TryParse(e.GetArg("guild_id"), out var guiid))
								server = await Utils.FindGuild(guiid);
							else
								server = e.Server;

							var channel = default(Channel);
							if (server != null)
							{
								channel = await Utils.FindTextChannel(e.GetArg("channel"), server);
								var permissions = e.Channel.Users.FirstOrDefault(x => x.Id == Bot.Client.CurrentUser.Id).GetPermissions(channel);
								if (!(permissions.ReadMessages))
								{
									await e.Channel.SendMessage("Cannot send a message to a hidden channel.");
									return;
								}
								if (!(permissions.SendMessages))
								{
									await e.Channel.SendMessage("Cannot send messages without a permission.");
									return;
								}

								if (channel == null)
								{
									await e.Channel.SendMessage("Could not find the channel.");
									return;
								}
							}
							else
							{
								await e.Channel.SendIsTyping();
								await e.Channel.SendMessage(await Utils.CutMessageAsync(e.GetArg("message"), badchars: false));
								return;
							}

							await channel.SendIsTyping();
							await channel.SendMessage(await Utils.CutMessageAsync(e.GetArg("message"), badchars: false));
						});
			});
			return Task.FromResult(0);
		}

		private static Task SpecialPermissionCommands(string name)
		{
			CService.CreateGroup(name, GBuilder =>
			{
				// I don't know why but some emojis don't work
				GBuilder.CreateCommand("react")
						.Description("Reacts to a message with the give emoji.")
						.Parameter("channel_id", ParameterType.Required)
						.Parameter("message_id", ParameterType.Required)
						.Parameter("emoji", ParameterType.Required)
						.Hide()
						.Do(async e =>
						{
							// Check permission
							if (Utils.CheckRolePermissionsAsync(e.User, DiscordConstants.AddReactionsFlag).GetAwaiter().GetResult())
							{
								if (await Utils.CheckRolePermissionsAsync(await Utils.GetBotUserObject(e.Channel), DiscordConstants.AddReactionsFlag))
								{
									var channelid = (ulong.TryParse(e.GetArg("channel_id"), out var result))
														  ? result
														  : e.Channel.Id;
									var messageid = (ulong.TryParse(e.GetArg("message_id"), out result))
														  ? result
														  : e.Message.Id;
									await Bot.SendAsync(CustomRequest.AddReaction(channelid, messageid, e.GetArg("emoji")), null);
								}
								else
								{
									await e.Channel.SendIsTyping();
									await e.Channel.SendMessage("The permission to add reactions is required.");
								}
							}
							else
							{
								await e.Channel.SendIsTyping();
								await e.Channel.SendMessage("You don't have a role which allows to add reactions to messages.");
							}
						});

				GBuilder.CreateCommand("setnickname")
						.Alias("setnick", "changenickname", "changenick")
						.Description("Sets the nickname of the bot.")
						.Parameter("name", ParameterType.Unparsed)
						.Hide()
						.Do(async e =>
						{
							if (e.User.ServerPermissions.ManageNicknames)
								await (await Utils.GetBotUserObject(e.Channel))?.Edit(nickname: string.IsNullOrEmpty(e.GetArg("name")) ? Bot.Client.CurrentUser.Name : e.GetArg("name"));
							else
							{
								await e.Channel.SendIsTyping();
								await e.Channel.SendMessage("You are not allowed to manage nicknames.");
							}
						});

				GBuilder.CreateCommand("pin")
						.Description("Pins a message.")
						.Parameter("channel_id", ParameterType.Required)
						.Parameter("message_id", ParameterType.Required)
						.Hide()
						.Do(async e =>
						{
							// Check permission
							if (e.User.ServerPermissions.ManageMessages)
							{
								if ((await Utils.GetBotUserObject(e.Channel)).GetPermissions(e.Channel).ManageMessages)
								{
									var channelid = (ulong.TryParse(e.GetArg("channel_id"), out var result))
														  ? result
														  : e.Channel.Id;
									var messageid = (ulong.TryParse(e.GetArg("message_id"), out result))
														  ? result
														  : e.Message.Id;
									await Bot.SendAsync(CustomRequest.PinMessage(channelid, messageid), null);
								}
								else
								{
									await e.Channel.SendIsTyping();
									await e.Channel.SendMessage("The permission to manage messages is required.");
								}
							}
							else
							{
								await e.Channel.SendIsTyping();
								await e.Channel.SendMessage("You don't have the permission to manage messages.");
							}
						});

				// TODO: change permission settings variable (document permissions too)
				GBuilder.CreateCommand("cleanup")
						.Alias("cleanuptime")
						.Description("Deletes the latest messages in the channel (max. 33 messages per cleanup and bulk delete mode only).")
						.Parameter("message_count", ParameterType.Required)
						.Hide()
						.Do(async e =>
						{
							if (e.User.ServerPermissions.ManageMessages)
							{
								if ((await Utils.GetBotUserObject(e.Channel)).GetPermissions(e.Channel).ManageMessages)
								{
									if (uint.TryParse(e.GetArg("message_count"), out var count))
									{
										// Bulk delete only
										var messages = (await e.Channel.DownloadMessages((int)count, e.Message.Id, Relative.Before, false))
											// Am I doing this right?
											.Where(m => (DateTime.UtcNow - m.Timestamp.Date.ToUniversalTime()).TotalMilliseconds < (1000 * 60 * 60 * 24 * 7 * 2));
										await e.Message.Delete();
										var msgcount = messages.Count();
										if (msgcount > 0)
										{
											await e.Channel.DeleteMessages(messages.Take(33).ToArray());
											await Logger.SendAsync($"{e.User.Name}#{e.User.Discriminator.ToString("D4")} (ID {e.User.Id}) deleted {msgcount} message{((msgcount == 1) ? string.Empty : "s")} in channel #{e.Channel.Name} (ID {e.Channel.Id}).");
										}
									}
									else
									{
										await e.Channel.SendIsTyping();
										await e.Channel.SendMessage("Parameter should be an unsigned integer.");
									}
								}
								else
								{
									await e.Channel.SendIsTyping();
									await e.Channel.SendMessage("The permission to manage messages is required.");
								}
							}
							else
							{
								await e.Channel.SendIsTyping();
								await e.Channel.SendMessage("You are not allowed to manage messages.");
							}
						});
			});
			return Task.FromResult(0);
		}
	}
}