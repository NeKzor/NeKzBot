using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using NeKzBot.Internals;
using NeKzBot.Resources;
using NeKzBot.Server;
using NeKzBot.Utilities;
using NeKzBot.Webhooks;

namespace NeKzBot.Modules.Private.Owner
{
	public class DataBase : CommandModule
	{
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading Data Base Module", LogColor.Init);
			await DataBaseCommands(Configuration.Default.BotCmd);
		}

		private static Task DataBaseCommands(string n)
		{
			CService.CreateGroup(n, GBuilder =>
			{
				GBuilder.CreateCommand("add")
						.Description($"Adds a new command to the database. Use the separator '{Utils.DataSeparator}' for data arrays.")
						.Parameter("name", ParameterType.Required)
						.Parameter("values", ParameterType.Unparsed)
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							if (await Data.Get(e.GetArg("name")) is IData data)
							{
								if (data.ReadWriteAllowed)
								{
									var result = await Utils.ChangeDataAsync(data, e.GetArg("values"), DataChangeMode.Add);
									await e.Channel.SendMessage((string.IsNullOrEmpty(result)) ? "Data has been added." : result);
								}
								else
									await e.Channel.SendMessage("This command doesn't allow to be changed.");
							}
							else
								await e.Channel.SendMessage($"Invalid data name. Try one of these: {await Utils.CollectionToList(await Data.GetNames(), "`")}");
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
							// Patter matching <3
							if (await Data.Get(e.GetArg("name")) is IData data)
							{
								if (data.ReadWriteAllowed)
								{
									var result = await Utils.ChangeDataAsync(data, e.GetArg("value"), DataChangeMode.Delete);
									await e.Channel.SendMessage((string.IsNullOrEmpty(result)) ? "Data deleted." : result);
								}
								else
									await e.Channel.SendMessage("This command doesn't allow to be changed.");
							}
							else
								await e.Channel.SendMessage($"Invalid data name. Try one of these: {await Utils.CollectionToList(await Data.GetNames(), "`")}");
						});

				GBuilder.CreateCommand("reload")
						.Alias("reloaddata")
						.Description("Reloads data and all commands.")
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await Data.InitMangerAsync();
							await e.Channel.SendMessage("Reloaded data.");
						});

				GBuilder.CreateCommand("showdata")
						.Alias("debugdata")
						.Description("Shows the data of an internal data collection.")
						.Parameter("name", ParameterType.Required)
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							var output = string.Empty;

							if (await Data.Get(e.GetArg("name")) is IData data)
							{
								var memory = data.Memory;
								if (memory is Simple simple)
								{
									foreach (var item in simple.Value)
										output += $"{item}, ";
								}
								else if (memory is Complex complex)
								{
									// Only show first dimension
									for (int i = 0; i < complex.Values.Count; i++)
										output += $"{complex.Values[i].Value[0]}, ";
								}
								else if (memory is Subscribers sub)
								{
									foreach (var hook in sub.Subs)
										output += $"{hook.GuildId}, ";
								}
								else
									await e.Channel.SendMessage("Type not supported.");
								await e.Channel.SendMessage((output != string.Empty)
																	? await Utils.CutMessageAsync(output.Substring(0, output.Length - 2))
																	: "Data is empty.");
							}
							else
								await e.Channel.SendMessage($"Invalid command parameter. Try one of these: {await Utils.CollectionToList(await Data.GetNames(), "`")}");
						});

				GBuilder.CreateCommand("datavars")
						.Alias("vars")
						.Description("Shows you a list of all data variables.")
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage($"**[Data Commands]**\n{await Utils.CollectionToList((await Data.GetNames()).OrderBy(name => name), "`", "\n")}");
						});
			});
			return Task.FromResult(0);
		}
	}
}