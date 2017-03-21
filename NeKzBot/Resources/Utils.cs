using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using NeKzBot.Classes;
using NeKzBot.Extensions;
using NeKzBot.Internals;
using NeKzBot.Server;
using NeKzBot.Webhooks;

namespace NeKzBot.Resources
{
	public static class Utils
	{
		public static int CBuilderIndex { get; private set; }
		public static string CBuilderGroup { get; private set; } = string.Empty;
		private static int _temp = -1;
		private static readonly Random _rand = new Random(DateTime.Now.Millisecond);
		private static readonly string[] _badChars = { "\\", "*", "_", "~", "`" };

		public const char Separator = '|';
		private const int _luckynumber = 7;
		private const int _maxarraycount = 128;

		#region String search
		// Check if a specific value is in that data array/list
		public static Task<bool> SearchCollection(IEnumerable<string> collection, string tosearch)
			=> Task.FromResult(Array.FindIndex(collection.ToArray(), str => str == tosearch) != -1);

		public static Task<bool> SearchCollection(IEnumerable<string> Collection, string tosearch, out int index)
			=> Task.FromResult((index = Array.FindIndex(Collection.ToArray(), str => str == tosearch)) != -1);

		public static Task<bool> SearchArray(object[,] searchin, int dimension, string tosearch, out int index)
		{
			for (index = 0; index < searchin.GetLength(0); index++)
				if (tosearch.ToLower().Replace(" ", string.Empty) == searchin[index, dimension].ToString().ToLower().Replace(" ", string.Empty))
					return Task.FromResult(true);
			index = -1;
			return Task.FromResult(false);
		}

		public static Task<int> SearchInListClassPropertiesByName<T>(List<T> searchin, string name, string search)
			=> Task.FromResult(searchin.FindIndex(property => property.GetType()
																	  .GetProperty(name)
																	  .GetValue(property, null)
																	  .ToString()
																	  .ToLower()
																	  .Replace(" ", string.Empty) == search.ToLower()
																										   .Replace(" ", string.Empty)));
		#endregion

		#region To List
		// Turn collections to readable string list
		public static Task<string> CollectionToList(IEnumerable<IEnumerable<string>> s, int d, string formatting = "", string delimiter = ", ", string list = "", int customsub = 0)
		{
			var output = string.Empty;
			for (int i = 0; i < s.Count(); i++)
				output += list + formatting + s.ElementAt(i).ElementAt(d) + formatting + delimiter;
			return Task.FromResult(output.Substring(0, output.Length - ((customsub == 0) ? delimiter.Length : customsub)));
		}

		public static Task<string> CollectionToList(IEnumerable<string> s, string formatting = "", string delimiter = ", ", string list = "", int customsub = 0)
		{
			var output = string.Empty;
			for (int i = 0; i < s.Count(); i++)
				output += list + formatting + s.ElementAt(i) + formatting + delimiter;
			return Task.FromResult(output.Substring(0, output.Length - ((customsub == 0) ? delimiter.Length : customsub)));
		}
		#endregion

		#region Create Commands
		// Create multiple commands from array
		public static Task CommandBuilder(Action act, int dim, IEnumerable<string> str, bool hasaliases = false, IEnumerable<string> aliases = null)
		{
			if (hasaliases)
			{
				foreach (var item in aliases)   // Command has multiple aliases
				{
					CBuilderGroup = item;
					for (CBuilderIndex = 0; CBuilderIndex < str.ToArray().GetLength(dim); CBuilderIndex++)
						act?.Invoke();
				}
			}
			else
				for (CBuilderIndex = 0; CBuilderIndex < str.ToArray().GetLength(dim); CBuilderIndex++)
					act?.Invoke();
			return Task.FromResult(0);
		}
		#endregion

		#region Random Number Generation
		public static Task<int> Rng(int from, int to)
		{
			var numb = default(int);
			lock (new object())
			{
				do
					numb = _rand.Next(from, to);
				while ((numb == _temp) || ((numb != 0) && numb % _luckynumber == 0));
			}
			return Task.FromResult(_temp = numb);
		}

		public static async Task<int> RngAsync(int to)
			=> await Rng(0, to);

		public static async Task<T> RngAsync<T>(List<T> collection)
			=> collection[await Rng(0, collection.Count)];

		public static async Task<object> RngAsync(object[] array)
			=> array[await Rng(0, array.Length)];

		public static async Task<object> RngAsync(object[,] array, int index = 0)
			=> array[await Rng(0, array.GetLength(0)), index];

		public static async Task<string> RngStringAsync(params string[] s)
			=> s[await Rng(0, s.Length)];

		public static async Task<string> RngStringAsync(List<string> s)
			=> s[await Rng(0, s.Count)];
		#endregion

		#region DATA I/O
		public static async Task<object> ReadFromFileAsync(string name)
		{
			var file = Path.Combine(await GetAppPath(), Configuration.Default.DataPath, name);
			if (!(File.Exists(file)))
				return null;

			var input = new string[_maxarraycount];
			var array = default(string[,]);

			try
			{
				using (var fs = new FileStream(file, FileMode.Open))
				using (var sr = new StreamReader(fs))
				{
					while (!sr.EndOfStream)
						input = (await sr.ReadToEndAsync()).Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

					if (input[0].Contains(Separator))
						array = new string[input.Length, input[0].Split(Separator).Length];
					else
						return input;

					for (int i = 0; i < input.Length; i++)
						for (int j = 0; j < input[i].Split(Separator).Length; j++)
							array[i, j] = input[i].Split(Separator)[j];
				}
			}
			catch
			{
				return null;
			}
			return array;
		}

		public static async Task<string> ChangeDataAsync(IData data, string value, DataChangeMode mode)
		{
			// Fail safe
			var file = data.FileName;
			var filepath = Path.Combine(await GetAppPath(), Configuration.Default.DataPath, file);
			if (!(File.Exists(filepath)))
				return DataError.FileNotFound;

			var values = default(List<string>);
			if (mode == DataChangeMode.Add)
			{
				values = value.Split(Separator)
							  .ToList();
				foreach (var item in values)
					if (item == string.Empty)
						return DataError.InvalidValues;
			}

			// Pattern matching
			var dimensions = 1;
			var collection = default(List<string>);
			var foundindex = default(int);
			var memory = data.Memory;
			if (memory == null)
				return DataError.DataMissing;
			if (memory is Complex complex)
			{
				var count = 0;
				for (; count < complex.Values.Count; count++)
					if (await SearchCollection(complex.Values[count].Value, value, out foundindex))
						break;

				if ((foundindex != -1)
				&& (mode == DataChangeMode.Delete))
				{
					foundindex += count;
					var temp = await ReadFromFileAsync(file) as string[,];
					dimensions = temp.GetLength(1);
					collection = temp.Cast<string>()
									 .ToList();
					for (int i = 0; i < dimensions; i++)
						collection.RemoveAt(foundindex * dimensions);
				}
				else if ((foundindex == -1)
				&& (mode == DataChangeMode.Add))
				{
					var temp = await ReadFromFileAsync(file) as string[,];
					if (temp.GetLength(1) != values.Count)
						return DataError.InvalidDimensions;
					collection = temp.Cast<string>()
									 .ToList();
					foreach (var item in values)
						collection.Add(item);
				}
				else
					return DataError.NameNotFound;
				var result = await WriteToFileAsync(collection, dimensions, filepath);
				if (!(string.IsNullOrEmpty(result)))
					return result;
				await Data.InitAsync<Complex>(data.Name);
			}
			else if (memory is Simple simple)
			{
				await SearchCollection(simple.Value, value, out foundindex);
				if ((foundindex != -1)
				&& (mode == DataChangeMode.Delete))
				{
					collection = (await ReadFromFileAsync(file) as string[]).ToList();
					for (int i = 0; i < dimensions; i++)
						collection.RemoveAt(foundindex * dimensions);
				}
				else if ((foundindex == -1)
				&& (mode == DataChangeMode.Add))
				{
					collection = (await ReadFromFileAsync(file) as string[]).ToList();
					foreach (var item in values)
						collection.Add(item);
				}
				else
					return DataError.NameNotFound;
				var result = await WriteToFileAsync(collection, dimensions, filepath);
				if (!(string.IsNullOrEmpty(result)))
					return result;
				await Data.InitAsync<Simple>(data.Name);
			}
			else if (memory is Subscribers sub)
			{
				if (((foundindex = sub.Subs.FindIndex(s => s.Id.ToString() == value)) != -1)
				&& (mode == DataChangeMode.Delete))
				{
					var temp = await ReadFromFileAsync(file) as string[,];
					dimensions = temp.GetLength(1);
					collection = temp.Cast<string>()
									 .ToList();
					for (int i = 0; i < dimensions; i++)
						collection.RemoveAt(foundindex * dimensions);
				}
				else if ((foundindex == -1)
				&& (mode == DataChangeMode.Add))
				{
					var temp = await ReadFromFileAsync(file) as string[,];
					if (temp.GetLength(1) != values.Count)
						return DataError.InvalidDimensions;
					collection = temp.Cast<string>()
									 .ToList();
					foreach (var item in values)
						collection.Add(item);
				}
				else
					return DataError.NameNotFound;
				var result = await WriteToFileAsync(collection, dimensions, filepath);
				if (!(string.IsNullOrEmpty(result)))
					return result;
				await Data.InitAsync<Subscribers>(data.Name);
			}
			else
				return DataError.Unknown;
			return null;
		}

		private static async Task<string> WriteToFileAsync(List<string> collection, int dimensions, string filepath)
		{
			try
			{
				using (var fs = new FileStream(filepath, FileMode.Create))
				using (var sw = new StreamWriter(fs))
				{
					for (int i = 0; i < collection.Count; i += dimensions)
					{
						for (int j = 0; j < dimensions; j++)
						{
							await sw.WriteAsync(collection[i + j]);
							if (j + 1 != dimensions)
								await sw.WriteAsync(Separator);
						}
						if (i + dimensions != collection.Count)
							await sw.WriteAsync("\n");
					}
				}
			}
			catch
			{
				return DataError.InvalidStream;
			}
			return null;
		}
		#endregion

		#region System Process
		public static async Task<string> GetCommandOutputAsync(string command, string parameters = "")
		{
			try
			{
				return await Process.Start(new ProcessStartInfo
				{
					FileName = command,
					Arguments = parameters,
					UseShellExecute = false,
					RedirectStandardOutput = true
				})
				.StandardOutput.ReadToEndAsync();
			}
			catch (Exception e)
			{
				await Logger.SendToChannelAsync("Utils.GetCommandOutputAsync Error", e);
			}
			return "**Error.**";
		}
		#endregion

		#region Discord
		// Find channel
		public static Task<Channel> FindTextChannelByName(string channelName, Discord.Server guild = null)
			=> Task.FromResult(((guild == null)
									  ? Bot.Client?.Servers?.FirstOrDefault(server => server.Id == Credentials.Default.DiscordMainServerId)
									  : Bot.Client?.Servers?.FirstOrDefault(server => server.Name == guild.Name))?.TextChannels?
															.FirstOrDefault(channel => channel.Name == channelName));

		public static Task<Channel> FindTextChannelById(ulong id, Discord.Server guild = null)
			=> Task.FromResult((guild == null
									  ? Bot.Client?.Servers?.FirstOrDefault(server => server.Id == Credentials.Default.DiscordMainServerId)
									  : Bot.Client?.Servers?.FirstOrDefault(server => server.Name == guild.Name))?.TextChannels?
															.FirstOrDefault(channel => channel.Id == id));

		// Find server
		public static Task<Discord.Server> FindServerByName(string name)
			=> Task.FromResult(Bot.Client?.Servers.FirstOrDefault(server => server.Name == name));

		public static Task<Discord.Server> FindServerById(ulong id)
			=> Task.FromResult(Bot.Client?.Servers?.FirstOrDefault(server => server.Id == id));

		// Find channels of every connected server
		public static Task<List<Channel>> GetChannels(string name)
		{
			var list = new List<Channel>();
			foreach (var server in Bot.Client?.Servers)
			{
				var channel = server?.TextChannels?.FirstOrDefault(cha => cha.Name == name);
				if (channel != null)
					list.Add(channel);
			}
			return Task.FromResult((list.Count > 0)
											   ? list
											   : null);
		}

		// Find command and return its description
		public static async Task<string> FindDescriptionAsync(string name)
		{
			if (string.IsNullOrEmpty(name))
				return Data.ListModules + Data.MoreInformation;
			foreach (var command in CommandModule.CService.AllCommands)
			{
				if ((name == command.Text)
				|| (command.Aliases.Contains(name)))
					return await GetDescription(command);
			}
			return "This command does not exist.";
		}

		public static Task<string> GetDescription(Command cmd)
		{
			var output = $"`{Configuration.Default.PrefixCmd}{cmd.Text}";
			if (cmd.Parameters.Any())
			{
				foreach (var parameter in cmd.Parameters)
				{
					if (parameter.Type == ParameterType.Multiple)
						output += $" <{parameter.Name}> <etc.>";
					else if (parameter.Type == ParameterType.Optional)
						output += $" ({parameter.Name})";
					else
						output += $" <{parameter.Name}>";
				}
			}
			output += (string.IsNullOrEmpty(cmd.Description))
							 ? "`\nNo description."
							 : $"`\n{cmd.Description}";

			var aliases = "\n\nKnown aliases:";
			if (cmd.Aliases.Any())
				foreach (var alias in cmd.Aliases)
					aliases += $" `{alias}`, ";
			return CutMessageAsync((aliases != "\n\nKnown aliases:")
									   ? output += aliases.Substring(0, aliases.Length - 2)
									   : output, badchars: false);
		}

		public static Task<Command> FindCommandByName(string name)
		{
			foreach (var command in CommandModule.CService.AllCommands)
			{
				if ((name == command.Text)
				|| (command.Aliases.Contains(name)))
					return Task.FromResult(command);
			}
			return Task.FromResult(default(Command));
		}
		#endregion

		#region String Utilities
		public static Task<bool> ValidateString(string s, string pattern, int maxlength)
		{
			if (s == string.Empty)
				return Task.FromResult(false);
			if (!(new Regex(pattern).IsMatch(s)))
				return Task.FromResult(false);
			return Task.FromResult(!(s.Length > maxlength));
		}

		public static Task<string> GetRest(string[] s, int from, int to = 0, string sep = "")
		{
			to = (to == 0)
					 ? s.Length
					 : to;
			var output = string.Empty;
			for (; from < to; from++)
			{
				output += s[from];
				if (from + 1 != to)
					output += sep;
			}
			return Task.FromResult(output);
		}

		public static Task<string> StringInBytes(params string[] s)
		{
			var size = 0;
			foreach (var item in s)
				size += Encoding.UTF8.GetByteCount(item);
			return Task.FromResult(size.ToString());
		}

		public static async Task<string> RisAsync(string s)
		{
			var output = string.Empty;
			var numbers = new string[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };
			for (int i = 0; i < s.Length; i++)
			{
				var c = s[i];
				var temp = string.Empty;
				if (c == ' ')
					temp = "          ";
				else if (c == '\n')
					temp = "\n";
				else if (await ValidateString(c.ToString(), "^[a-zA-Z]", 1))
					temp = $":regional_indicator_{c.ToString().ToLower()}:";
				else if (await ValidateString(c.ToString(), "^[0-9]", 1))
					temp = $":{numbers[Convert.ToInt16(c.ToString())]}:";
				else if (c == '!')
					temp = ":exclamation:";
				else if (c == '?')
					temp = ":question:";
				else
					continue;

				// Only append if it doesn't exceed Discord char limit
				if (output.Length + temp.Length > DiscordConstants.MaximumCharsPerMessage)
					break;
				else
					output += temp;
			}
			return output;
		}

		// Another copy...
		public static Task<string> FormatRawText(string text)
		{
			foreach (var item in _badChars)
				text = text.Replace(item, $"\\{item}");
			return Task.FromResult(text);
		}
		#endregion

		#region Others
		public static Task<bool> ValidFileName(string file)
			=> Task.FromResult((!(string.IsNullOrEmpty(file))) && (file?.IndexOfAny(Path.GetInvalidFileNameChars()) == -1));

		public static Task<bool> ValidPathName(string path)
			=> Task.FromResult((!(string.IsNullOrEmpty(path))) && (path?.IndexOfAny(Path.GetInvalidPathChars()) == -1));

		public static Task<string> UpperString(string s, bool yes = true)
			=> Task.FromResult((yes)
								 ? s.ToUpper()
								 : s);

		public static Task<string> GetRestAfter(string s, char l)
			=> Task.FromResult(s.Split(l).Last());

		public static Task<string> GetLocalTime()
			=> Task.FromResult(DateTime.Now.ToString("HH:mm:ss"));

		public static Task<string> GetAppPath()
			=> Task.FromResult(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));

		public static async Task<string> CutMessageAsync(string text, int minus = 0, bool badchars = true)
			=> ((text = (badchars) ? await FormatRawText(text) : text).Length > DiscordConstants.MaximumCharsPerMessage)
											? text.Substring(0, (int)DiscordConstants.MaximumCharsPerMessage)
											: text.Substring(0, text.Length - minus);

		public static async Task<string> CutMessageAsync(string text, int limit, string append, bool badchars = true)
			=> ((text = (badchars) ? await FormatRawText(text) : text).Length > limit)
										   ? text.Substring(0, limit) + append
										   : text.Substring(0, text.Length);

		public static Task<TimeSpan> GetUptime()
			=> Task.FromResult(DateTime.Now - Process.GetCurrentProcess().StartTime);

		public static Task<bool> IsLinux()
			=> Task.FromResult(((int)Environment.OSVersion.Platform == 4)
							|| ((int)Environment.OSVersion.Platform == 6)
							|| ((int)Environment.OSVersion.Platform == 128));

		public static Task<string> GetDuration(DateTime time)
		{
			if (time == default(DateTime))
				return Task.FromResult(default(string));

			var duration = DateTime.UtcNow - time;
			var output = (duration.Days > 0)
										? $"{duration.Days} Day{(duration.Days == 1 ? string.Empty : "s")} "
										: string.Empty;
			output += (duration.Hours > 0)
									  ? $"{duration.Hours} Hour{(duration.Hours == 1 ? string.Empty : "s")} "
									  : string.Empty;
			output += (duration.Minutes > 0)
										? $"{duration.Minutes} Minute{(duration.Minutes == 1 ? string.Empty : "s")} "
										: string.Empty;
			output += (duration.Seconds > 0)
										? $"{duration.Seconds} Second{(duration.Seconds == 1 ? string.Empty : "s")}"
										: string.Empty;
			output += ((duration.Days > 0)
			&& (duration.TotalDays > 365))
								   ? $" (about {Math.Round((decimal)duration.TotalDays / 365, 1)} Year{(Math.Round((decimal)duration.TotalDays / 365, 1) == (decimal)1.0 ? string.Empty : "s")})"
								   : string.Empty;
			return Task.FromResult((output != string.Empty)
										   ? output
										   : default(string));
		}

		public static Task<User> GetBotUserObject(Discord.Server gui)
			=> Task.FromResult(gui.Users.FirstOrDefault(x => x.Id == Bot.Client.CurrentUser.Id));

		public static Task<User> GetBotUserObject(Channel cha)
			=> Task.FromResult(cha.Users.FirstOrDefault(x => x.Id == Bot.Client.CurrentUser.Id));

		public static async Task<bool> CheckRolesHasPermissionAsync(User usr, byte flag)
		{
			foreach (var role in usr.Roles)
				if (await FlagIsSet(role.Permissions.RawValue, flag))
					return true;
			return false;
		}

		// I copied this from Voltana c:
		public static Task<bool> FlagIsSet(uint value, byte flag)
			=> Task.FromResult(((value >> flag) & 1U) == 1);

		public static async Task<string> GenerateModuleListAsync(string title, string description = default(string), string footer = default(string), string specialprefix = default(string), params string[] commands)
		{
			var output = string.Empty;
			foreach (var name in commands)
			{
				var command = await FindCommandByName((string.IsNullOrEmpty(specialprefix)) ? name : specialprefix + name);
				if (command == null)
					continue;
				output += $"\n• `{Configuration.Default.PrefixCmd}{command.Text}";
				if (command.Parameters.Any())
				{
					foreach (var parameter in command.Parameters)
					{
						if (parameter.Type == ParameterType.Multiple)
							output += $" <{parameter.Name}> <etc.>";
						else if (parameter.Type == ParameterType.Optional)
							output += $" ({parameter.Name})";
						else
							output += $" <{parameter.Name}>";
					}
				}
				output += "`";
			}
			var desc = (string.IsNullOrEmpty(description))
							  ? string.Empty
							  : $"\n{description}";
			return (string.IsNullOrEmpty(footer))
						  ? $"**[{title}]**{desc}{output}"
						  : $"**[{title}]**{desc}{output}\n{footer}";
		}
		#endregion
	}
}