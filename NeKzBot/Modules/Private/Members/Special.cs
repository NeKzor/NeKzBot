using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using NeKzBot.Extensions;
using NeKzBot.Server;
using NeKzBot.Utilities;

namespace NeKzBot.Modules.Private.Members
{
	public class Special : CommandModule
	{
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Special Module", LogColor.Init);
			await SpecialPermissionCommands(Configuration.Default.BotCmd);
		}

		private static Task SpecialPermissionCommands(string name)
		{
			CService.CreateGroup(name, GBuilder =>
			{
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
							{
								var bot = await Utils.GetBotUserObject(e.Channel);
								if (bot?.ServerPermissions.ChangeNickname == true)
									await bot.Edit(nickname: string.IsNullOrEmpty(e.GetArg("name")) ? Bot.Client.CurrentUser.Name : e.GetArg("name"));
								else
								{
									await e.Channel.SendIsTyping();
									await e.Channel.SendMessage("The permission to change nickname is required.");
								}
							}
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
							if ((e.User.ServerPermissions.ManageMessages)
							&& (e.User.GetPermissions(e.Channel).ManageMessages))
							{
								var bot = await Utils.GetBotUserObject(e.Channel);
								if ((bot?.ServerPermissions.ManageMessages == true)
								&& (bot?.GetPermissions(e.Channel).ManageMessages == true))
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

				GBuilder.CreateCommand("cleanup")
						.Alias("cleanuptime")
						.Description("Deletes the latest messages in the channel (max. 100 messages per cleanup and bulk delete mode only).")
						.Parameter("message_count", ParameterType.Required)
						.Parameter("message_id", ParameterType.Optional)
						.Hide()
						.Do(async e =>
						{
							if ((e.User.ServerPermissions.ManageMessages)
							&& (e.User.GetPermissions(e.Channel).ManageMessages))
							{
								// NOTE: bot would also need the permission to read message history
								var bot = await Utils.GetBotUserObject(e.Channel);
								if ((bot?.ServerPermissions.ManageMessages == true)
								&& (bot?.GetPermissions(e.Channel).ManageMessages == true))
								{
									if (uint.TryParse(e.GetArg("message_count"), out var count))
									{
										var id = e.Message.Id;
										await e.Message.Delete();
										if (!(string.IsNullOrEmpty(e.GetArg("message_id"))))
										{
											if (ulong.TryParse(e.GetArg("message_id"), out var result))
												id = result;
											else
											{
												await e.Message.Delete();
												return;
											}
										}
										await Utils.DeleteMessagesAsync(e, id, (int)count);
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

				GBuilder.CreateCommand("cleanuprange")
						.Description("Deletes the all messages between the ids (including themselves, max = 100).")
						.Parameter("first_id", ParameterType.Required)
						.Parameter("second_id", ParameterType.Required)
						.Hide()
						.Do(async e =>
						{
							if ((e.User.ServerPermissions.ManageMessages)
							&& (e.User.GetPermissions(e.Channel).ManageMessages))
							{
								// NOTE: bot would also need the permission to read message history
								var bot = await Utils.GetBotUserObject(e.Channel);
								if ((bot?.ServerPermissions.ManageMessages == true)
								&& (bot?.GetPermissions(e.Channel).ManageMessages == true))
								{
									if ((ulong.TryParse(e.GetArg("first_id"), out var firstid))
									&& (ulong.TryParse(e.GetArg("second_id"), out var secondid)))
									{
										var index = (await e.Channel.DownloadMessages(100, firstid, Relative.Before)).ToList()
																													 .FindIndex(m => m.Id == secondid);
										if ((index != -1) && (index <= 99))
											await Utils.DeleteMessagesAsync(e, firstid, index + 1);
									}
									await e.Message.Delete();
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