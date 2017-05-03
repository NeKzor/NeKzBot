using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using NeKzBot.Internals.Entities;
using NeKzBot.Resources;
using NeKzBot.Server;
using NeKzBot.Utilities;

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
							var message = default(string);
							switch (await Utils.ChangeDataAsync(e.GetArg("name"), e.GetArg("values"), DataChangeMode.Add))
							{
								case DataChangeResult.Error:
									message = "**Error.**";
									break;
								case DataChangeResult.Success:
									message = "Data has been added.";
									break;
								case DataChangeResult.InvalidInput:
									message = "Found invalid character to parse this type of data.";
									break;
								case DataChangeResult.Dupulicate:
									message = "Value already exists.";
									break;
								case DataChangeResult.ExportFailed:
									message = "An error occurred while exporting the new data.";
									break;
								case DataChangeResult.SeparatorNotFound:
									// Useful when you don't know how many values the list takes
									message = "Could not find the input separator for parsing this kind of data.\n" +
											  $"Data array needs {Enumerable.First((await Data.Get<Complex>(e.GetArg("name"))).Values).Value.Count} values.";
									break;
								case DataChangeResult.InvalidCount:
									message = "Could not parse input.\n" +
											  $"Data array needs {Enumerable.First((await Data.Get<Complex>(e.GetArg("name"))).Values).Value.Count} values.";
									break;
								case DataChangeResult.IncorrectType:
									message = "Subscription values need the correct type: `Ulong: id`, `String: token`, `Ulong: guildid`, `Ulong: userid`.";
									break;
								case DataChangeResult.IncorrectValues:
									message = "Subscription needs four values in this order: `id`, `token`, `guildid`, `userid`.";
									break;
								case DataChangeResult.NotAllowed:
									message = "This data doesn't allow to be changed.";
									break;
								case DataChangeResult.NotImplemented:
									message = "This type of data cannot be changed.";
									break;
								case DataChangeResult.NameNotFound:
									message = $"Invalid data name. Try one of these: {await Utils.CollectionToList(await Data.GetNames(), "`")}";
									break;
							}
							await e.Channel.SendMessage(message);
						});

				GBuilder.CreateCommand("delete")
						.Alias("remove")
						.Description("Removes specified data with the given name. The parameter value is the first value in a data list.")
						.Parameter("name", ParameterType.Required)
						.Parameter("value", ParameterType.Required)
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							var message = default(string);
							switch (await Utils.ChangeDataAsync(e.GetArg("name"), e.GetArg("value"), DataChangeMode.Delete))
							{
								case DataChangeResult.Error:
									message = "**Error.**";
									break;
								case DataChangeResult.Success:
									message = "Data has been deleted.";
									break;
								case DataChangeResult.ExportFailed:
									message = "An error occurred while exporting the new data.";
									break;
								case DataChangeResult.IncorrectType:
									message = "The webhook id of this subscription should be the type ulong.";
									break;
								case DataChangeResult.NotAllowed:
									message = "This data doesn't allow to be changed.";
									break;
								case DataChangeResult.NotImplemented:
									message = "This type of data cannot be changed.";
									break;
								case DataChangeResult.NameNotFound:
									message = $"Invalid data name. Try one of these: {await Utils.CollectionToList(await Data.GetNames(), "`")}";
									break;
								case DataChangeResult.NoMatch:
									message = "Value could not be matched with the data.";
									break;
							}
							await e.Channel.SendMessage(message);
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
						.Description("Shows the data of an internal data collection. Index parameter is only required to show a specific data list.")
						.Parameter("name", ParameterType.Required)
						.Parameter("index", ParameterType.Optional)
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
									// Parse optional index parameter (default means that the first value of each list item will be shown)
									if (uint.TryParse(e.GetArg("index"), out var index))
									{
										if (index >= complex.Values.Count)
										{
											await e.Channel.SendMessage($"Invalid index. Memory object only has {complex.Values.Count} items.");
											return;
										}

										foreach (var item in complex.Values[(int)index].Value)
											output += $"{item}, ";
									}
									else
									{
										// Try to find the index instead (if the uint index is actually a string)
										var found = complex.Values.FindIndex(s => s.Value.First() == e.GetArg("index"));
										if (found != -1)
										{
											foreach (var item in complex.Values[found].Value)
												output += $"{item}, ";
										}
										else
										{
											foreach (var item in complex.Values)
												output += $"{item.Value[0]}, ";
										}
									}
								}
								else if (memory is Subscription sub)
								{
									foreach (var hook in sub.Subscribers)
										output += $"{hook.GuildId}, ";
								}
								else
									await e.Channel.SendMessage("Type not supported.");
								await e.Channel.SendMessage((output != string.Empty)
																	? await Utils.CutMessageAsync(output.Substring(0, output.Length - 2))
																	: "Data is empty.");
							}
							else
								await e.Channel.SendMessage($"Invalid data name. Try one of these: {await Utils.CollectionToList(await Data.GetNames(), "`")}");
						});

				GBuilder.CreateCommand("datavars")
						.Alias("vars")
						.Description("Lists all data variables in the data manager.")
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage($"Data names in data manager:\n{await Utils.CollectionToList((await Data.GetNames()).OrderBy(name => name), "`", ", ")}");
						});

				GBuilder.CreateCommand("datastats")
						.Alias("datainfo")
						.Description("Shows how many items each data variable holds in its memory.")
						.AddCheck(Permissions.BotOwnerOnly)
						.Hide()
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							var output = string.Empty;
							foreach (IData data in Data.Manager)
							{
								var memory = data.Memory;
								if (memory is Simple simple)
									output += $"\nObject Simple: `{data.Name}` with {simple.Value.Count} item{((simple.Value.Count == 1) ? string.Empty : "s")}.";
								else if (memory is Complex complex)
								{
									var listcount = complex.Values.Count;
									var itemscount = complex.Values[0].Value.Count;
									var total = listcount * itemscount;
									output += $"\nObject Complex: `{data.Name}` with {listcount} x {itemscount} = {total} item{((total == 1) ? string.Empty : "s")}.";
								}
								else if (memory is Subscription subscription)
									output += $"\nObject Subscription: `{data.Name}` with {subscription.Subscribers.Count} subscriber{((subscription.Subscribers.Count == 1) ? string.Empty : "s")}.";
								else if (memory is Portal2Maps maplist)
									output += $"\nObject Portal2Maps: `{data.Name}` with {maplist.Maps.Count} map{((maplist.Maps.Count == 1) ? string.Empty : "s")}.";
							}
							await e.Channel.SendMessage(await Utils.CutMessageAsync($"Loaded **{Data.Manager.Count}** internal data chunks:{output}", badchars: false));
						});
			});
			return Task.FromResult(0);
		}
	}
}