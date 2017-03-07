using System.Threading.Tasks;
using Discord.Commands;
using NeKzBot.Resources;
using NeKzBot.Server;

namespace NeKzBot.Modules.Private.Owner
{
	public class DataBase : Commands
	{
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Data Base Module", LogColor.Init);
			await DataBaseCommands(Configuration.Default.BotCmd);
		}

		private static Task DataBaseCommands(string name)
		{
			CService.CreateGroup(name, GBuilder =>
			{
				GBuilder.CreateCommand("add")
						.Description($"Adds a new command to the database. Use the separator _{Utils.Separator}_ for data arrays.")
						.Parameter("name", ParameterType.Required)
						.Parameter("values", ParameterType.Unparsed)
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							if (await Data.DataExists(e.GetArg("name"), out var index))
							{
								if ((Data.Manager[index].ReadingAllowed)
								&& (Data.Manager[index].WrittingAllowed))
								{
									var result = await Utils.AddDataAsync(index, e.GetArg("values"));
									await e.Channel.SendMessage(result == string.Empty ? "Data has been added." : result);
								}
								else
									await e.Channel.SendMessage("This command doesn't allow to be changed.");
							}
							else
								await e.Channel.SendMessage($"Invalid data name. Try one of these: {await Utils.ListToList(await Data.GetDataNames(), "`")}");
						});

				GBuilder.CreateCommand("delete")
						.Alias("remove")
						.Description("Removes specified data with the given name. The parameter value is the first value in a data array.")
						.Parameter("name", ParameterType.Required)
						.Parameter("value", ParameterType.Required)
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							if (await Data.DataExists(e.GetArg("name"), out var index))
							{
								if ((Data.Manager[index].ReadingAllowed)
								&& (Data.Manager[index].WrittingAllowed))
								{
									var msg = await Utils.DeleteDataAsync(index, e.GetArg("value"));
									if (msg == string.Empty)
									{
										if (await Data.ReloadAsync(index))
											await e.Channel.SendMessage("Data deleted.");
										else
											await e.Channel.SendMessage("Data deleted but **failed** to reload data.");
									}
									else
										await e.Channel.SendMessage(msg);
								}
								else
									await e.Channel.SendMessage("This command doesn't allow to be changed.");
							}
							else
								await e.Channel.SendMessage($"Invalid data name. Try one of these: {await Utils.ListToList(await Data.GetDataNames(), "`")}");
						});

				GBuilder.CreateCommand("reload")
						.Alias("reloaddata")
						.Description("Reloads data and all commands.")
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await Data.InitAsync();
							await e.Channel.SendMessage("Reloaded data.");
						});

				GBuilder.CreateCommand("showdata")
						.Alias("debugdata")
						.Description("Shows the data of a certain data array.")
						.Parameter("name", ParameterType.Unparsed)
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							var index = 0;
							var found = false;
							var output = string.Empty;

							// Find command
							for (; index < Data.Manager.Count; index++)
							{
								if (e.Args[0] != Data.Manager[index].Name)
									continue;
								found = true;
								break;
							}

							if (found)
							{
								var obj = Data.Manager[index].Data;
								if (obj.GetType() == typeof(string[]))
								{
									foreach (var item in obj as string[])
										output += $"{item}, ";
								}
								else if (obj.GetType() == typeof(string[,]))
								{
									// Only show first dimension
									for (int i = 0; i < (obj as string[,]).GetLength(0); i++)
										output += $"{(obj as string[,])[i, 0]}, ";
								}
								else
									await e.Channel.SendMessage("**Error**");

								if (output != string.Empty)
									await e.Channel.SendMessage(await Utils.CutMessage(output.Substring(0, output.Length - 2).Replace("_", "\\_")));
							}
							else
								await e.Channel.SendMessage($"Invalid command parameter. Try one of these: {await Utils.ListToList(await Data.GetDataNames(), "`")}");
						});

				GBuilder.CreateCommand("datavars")
						.Alias("vars")
						.Description("Shows you a list of all data variables.")
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage($"**[Data Commands]**\n{await Utils.ListToList(await Data.GetDataNames(), "`", "\n")}");
						});
			});
			return Task.FromResult(0);
		}
	}
}