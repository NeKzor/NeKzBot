using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using NeKzBot.Server;
using NeKzBot.Modules;

namespace NeKzBot.Resources
{
	public class Utils
	{
		public static int index = 0;
		public static string group = string.Empty;
		private static int temp = -1;
		private static readonly object obj = new object();
		private static Random rand = new Random(DateTime.Now.Millisecond);

		public const char seperator = '|';
		private const int luckynumber = 7;
		private const int maxarraycount = 128;
		private const int maxmessagecount = 2000;

		#region Searching
		// Check if a specific value is in that data array/list
		public static bool SearchArray(string[] searchin, string tosearch)
		{
			for (int i = 0; i < searchin.GetLength(0); i++)
				if (tosearch.ToLower().Replace(" ", string.Empty) == searchin[i].ToLower().Replace(" ", string.Empty))
					return true;
			return false;
		}

		public static bool SearchArray(string[] searchin, string tosearch, out int index)
		{
			for (index = 0; index < searchin.GetLength(0); index++)
				if (tosearch.ToLower().Replace(" ", string.Empty) == searchin[index].ToLower().Replace(" ", string.Empty))
					return true;
			return false;
		}

		public static bool SearchArray(string[,] searchin, int dimension, string tosearch, out int index)
		{
			for (index = 0; index < searchin.GetLength(0); index++)
				if (tosearch.ToLower().Replace(" ", string.Empty) == searchin[index, dimension].ToLower().Replace(" ", string.Empty))
					return true;
			return false;
		}

		public static bool SearchArray(object[,] searchin, int dimension, string tosearch, out int index)
		{
			for (index = 0; index < searchin.GetLength(dimension); index++)
				if (tosearch.ToLower().Replace(" ", string.Empty) == searchin[index, dimension].ToString().ToLower().Replace(" ", string.Empty))
					return true;
			return false;
		}

		public static bool SearchList(List<string> searchin, string tosearch, out int index)
		{
			for (index = 0; index < searchin.Count(); index++)
				if (tosearch.ToLower().Replace(" ", string.Empty) == searchin[index].ToLower().Replace(" ", string.Empty))
					return true;
			return false;
		}

		public static bool SearchListOneDmim(List<string[]> searchin, string tosearch, out int index)
		{
			index = 0;
			foreach (var item in searchin)
				for (; index < item.Count(); index++)
					if (tosearch.ToLower().Replace(" ", string.Empty) == item[index].ToLower().Replace(" ", string.Empty))
						return true;
			return false;
		}

		public static bool SearchListMultiDmin(List<string[,]> searchin, string tosearch, out int index, int dimension = 0)
		{
			index = 0;
			foreach (var item in searchin)
				for (; index < item.GetLength(dimension); index++)
					if (tosearch.ToLower().Replace(" ", string.Empty) == item[index, dimension].ToLower().Replace(" ", string.Empty))
						return true;
			return false;
		}
		#endregion

		#region To List
		// Turn the array/list dimension into a list
		public static string ArrayToList(string[,] s, int d, string formatting = "", string delimiter = ", ")
		{
			var output = string.Empty;
			for (int i = 0; i < s.GetLength(0); i++)
				output += formatting + s[i, d] + formatting + delimiter;
			return output.Substring(0, output.Length - delimiter.Length);
		}

		public static string ArrayToList(string[] s, string formatting = "", string delimiter = ", ", string list = "")
		{
			var output = string.Empty;
			for (int i = 0; i < s.GetLength(0); i++)
				output += list + s[i] + delimiter;
			return output.Substring(0, output.Length - delimiter.Length);
		}

		public static string ListToList(List<string> s, string formatting = "", string delimiter = ", ")
		{
			var output = string.Empty;
			for (int i = 0; i < s.Count; i++)
				output += formatting + s[i] + formatting + delimiter;
			return output.Substring(0, output.Length - delimiter.Length);
		}
		#endregion

		#region Create Commands
		// Create multiple commands from array
		public static Task CommandCreator(Action act, int dim, string[,] str, bool hasaliases = false, string[] aliases = null)
		{
			if (hasaliases)
				foreach (var item in aliases)   // Command has multiple aliases
				{
					group = item;
					for (index = 0; index < str.GetLength(dim); index++)
						act();
				}
			else
				for (index = 0; index < str.GetLength(dim); index++)
					act();
			return Task.FromResult(0);
		}

		public static Task CommandCreator(Action act, int from, int to, bool hasaliases = false, string[] aliases = null)
		{
			if (hasaliases)
				foreach (var item in aliases)
				{
					group = item;
					for (index = from; index < to; index++)
						act();
				}
			else
				for (index = from; index < to; index++)
					act.Invoke();
			return Task.FromResult(0);
		}
		#endregion

		#region RANDOM NUMBER GENERATION
		public static int RNG(int to)
		{
			int numb;
			lock (obj)
			{
				do
					numb = rand.Next(0, to);
				while (numb == temp || numb % luckynumber == 0);
			}
			return temp = numb;
		}

		public static int RNG(int from, int to)
		{
			int numb;
			lock (obj)
			{
				do
					numb = rand.Next(from, to);
				while (numb == temp || numb % luckynumber == 0);
			}
			return temp = numb;
		}

		public static string RNGString(params string[] s) => s[RNG(0, s.Count())];
		#endregion

		#region DATA I/O
		public static async Task<object> ReadFromFile(string filepath)
		{
			var file = GetPath() + Settings.Default.DataPath + filepath;

			if (!FileFound(file))
				return null;

			string[] input = new string[maxarraycount];
			string[,] array = null;

			try
			{
				using (var fs = new FileStream(file, FileMode.Open))
				using (var sr = new StreamReader(fs))
				{
					while (!sr.EndOfStream)
						input = (await sr.ReadToEndAsync()).Replace("\r", string.Empty).Split('\n');

					if (input[0].Contains(seperator))
						array = new string[input.Count(), input[0].Split(seperator).Count()];
					else
						return input;

					for (int i = 0; i < input.Count(); i++)
						for (int j = 0; j < input[i].Split(seperator).Count(); j++)
							array[i, j] = input[i].Split(seperator)[j];
				}
			}
			catch
			{
				return null;
			}
			return array;
		}

		public static async Task<string> AddData(int index, string value)
		{
			var filepath = (string)DataManager.dataCommands[index, 2];
			var file = AppDomain.CurrentDomain.BaseDirectory + Settings.Default.DataPath + filepath;
			if (!FileFound(file))
				return "**Error -** File not found.";

			var values = value.Split(seperator).ToList();
			foreach (var item in values)
				if (item == string.Empty)
					return "**Error -** Invalid values.";

			var data = new List<string>();
			if (DataManager.dataCommands[index, 3] == null)
				return "**Error - ** Data is missing.";
			if (DataManager.dataCommands[index, 3].GetType() == typeof(string[,]))
			{
				if (SearchArray((string[,])DataManager.dataCommands[index, 3], 0, values[0], out index))
					return "**Error -** Command already exists.";
				var temp = (string[,])(await ReadFromFile(filepath));
				if (temp.Rank != values.Count())
					return "**Error -** Invalid dimensions. Did you forget a value?";
				data = temp.Cast<string>().ToList();
			}
			else if (DataManager.dataCommands[index, 3].GetType() == typeof(string[]))
			{
				if (SearchArray((string[])DataManager.dataCommands[index, 3], values[1], out index))
					return "**Error -** Data already exists.";
				values.RemoveAt(0);
				data = ((string[])(await ReadFromFile(filepath))).ToList();
			}
			else
				return "**Error -** Something went wrong.";

			// Add new data
			foreach (var item in values)
				data.Add(item);

			// Write new data
			try
			{
				using (var fs = new FileStream(file, FileMode.Create))
				using (var sw = new StreamWriter(fs))
				{
					for (int i = 0; i < data.Count(); i += values.Count)
					{
						for (int j = 0; j < values.Count; j++)
						{
							await sw.WriteAsync(data[i + j]);
							if (j + 1 != values.Count)
								await sw.WriteAsync(seperator);
						}
						if (i + values.Count != data.Count())
							await sw.WriteAsync("\n");
					}
				}
			}
			catch
			{
				return "**Error -** Failed to write new data.";
			}
			if (!(await DataManager.Reload(index)))
				return "**Error -** DataManager failed to reload data.";
			return "New data added.";
		}

		// TODO: rewrite?
		public static async Task<string> DeleteData(string filepath, string value)
		{
			var file = AppDomain.CurrentDomain.BaseDirectory + Settings.Default.DataPath + filepath;
			if (!FileFound(file))
				return "**Error -** File not found.";

			string[] line = value.Split(seperator);
			foreach (var item in line)
				if (item.Replace(" ", string.Empty) == string.Empty)
					return "**Error -** Failed to parse values.";

			// Check if command does actually exit
			var index = 0;
			var dimensions = 1;
			var found = false;
			object obj = null;
			List<string> ls = null;
			for (int i = 0; i < DataManager.dataCommands.GetLength(0); i++)
			{
				obj = DataManager.dataCommands[i, 3];
				if (obj.GetType() == typeof(string[,]))
				{
					if (SearchArray((string[,])DataManager.dataCommands[i, 3], 0, line[0], out index))
					{
						string[,] temp = (string[,])(await ReadFromFile(filepath));
						dimensions = temp.Rank;
						ls = temp.Cast<string>().ToList();
						found = true;
						break;
					}
				}
				else if (obj.GetType() == typeof(string[]))
				{
					if (SearchArray((string[])DataManager.dataCommands[i, 3], line[0], out index))
					{
						ls = ((string[])(await ReadFromFile(filepath))).ToList();
						found = true;
						break;
					}
				}
			}
			if (!found)
				return "**Error -** Command does not exist.";

			for (int i = 0; i < dimensions; i++)
				ls.RemoveAt(index * dimensions);

			try
			{
				using (var fs = new FileStream(file, FileMode.Create))
				using (var sw = new StreamWriter(fs))
				{
					for (int i = 0; i < ls.Count(); i += dimensions)
					{
						for (int j = 0; j < dimensions; j++)
						{
							await sw.WriteAsync(ls[i + j]);
							if (j + 1 != dimensions)
								await sw.WriteAsync(seperator);
						}
						if (i + dimensions != ls.Count())
							await sw.WriteAsync("\n");
					}
				}
			}
			catch
			{
				return "**Error -** Failed to write new data.";
			}
			return string.Empty;
		}
		#endregion

		// System process
		public static async Task<string> GetCommandOutput(string command, string parameters = "")
		{
			try
			{
				var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
				{
					FileName = command,
					Arguments = parameters,
					UseShellExecute = false,
					RedirectStandardOutput = true
				});
				return process.StandardOutput.ReadToEnd();
			}
			catch (Exception ex)
			{
				await Logging.CHA("GetCommandOutput error", ex);
				return "**Error.**";
			}
		}

		// Find channel
		public static Task<Discord.Channel> GetChannel(string channelName, Discord.Server mainServer = null)
		{
			if (mainServer != null)
				if (mainServer.Id != Credentials.Default.DiscordMainServerID)
					return null;
			var server = Bot.dClient?.Servers?.First(x => x.Id == Credentials.Default.DiscordMainServerID);
			var channel = server?.FindChannels(channelName, Discord.ChannelType.Text, false);
			return Task.FromResult(channel.Any() ? channel.First() : null);
		}

		public static Task<List<Discord.Channel>> GetChannels(string channelName)
		{
			var list = new List<Discord.Channel>();
			foreach (var server in Bot.dClient?.Servers)
				list.Add(server?.FindChannels(channelName, Discord.ChannelType.Text, false).First());
			return Task.FromResult(list.Any() ? list : null);
		}

		// Find command and return its description
		public static Task<string> FindDescription(string msg, bool exact = false)
		{
			var commands = Commands.cmd.AllCommands;
			var list = new List<string>();
			var count = 0;
			foreach (var item in commands)
			{
				if (!item.IsHidden)
					count++;
				var command = item.Text;
				if (command.Split(' ').Count() == 1)
				{
					if (item.Parameters.Any())
						list.Add(command + " <p>");
					else
						list.Add(command);
				}
			}
			var output = exact ? string.Empty : $"There are {count} ({commands.Count()}) commands you can use:\n\n{ListToList(list, "`")}\n\nTry `{Settings.Default.PrefixCmd}help <command>` for more information.";
			foreach (var command in commands)
			{
				if (command.Text == msg || SearchArray(command.Aliases?.ToArray(), msg))
				{
					output = command.Description;
					if (command.Aliases.Any())
					{
						output += "\nKnown aliases: ";
						foreach (var alias in command.Aliases)
							output += $"`{alias}`, ";
						output = output.Substring(0, output.Length - 2);
					}
					break;
				}
			}
			// Cut the message, just to make sure
			return Task.FromResult(CutMessage(output));
		}

		// Others
		public static bool ValidateString(string s, string pattern, int maxlength)
		{
			if (s == string.Empty)
				return false;
			if (!new System.Text.RegularExpressions.Regex(pattern).IsMatch(s))
				return false;
			return !(s.Length > maxlength);
		}

		public static string GetRest(string[] s, int from, int to = 0, string sep = "", bool firstreplace = false)
		{
			to = to == 0 ? s.Count() : to;
			var output = string.Empty;
			for (; from < to; from++)
			{
				output += s[from];
				if (from + 1 != to)
					output += sep;
			}
			return !firstreplace ?
				output : ReplaceFirst(output, sep, seperator.ToString());
		}

		public static string ReplaceFirst(string text, string search, string replace)
		{
			var pos = text.IndexOf(search);
			return pos < 0 ?
				text : text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
		}

		public static string StringInBytes(params string[] s)
		{
			var size = 0;
			foreach (var item in s)
				size += System.Text.Encoding.UTF8.GetByteCount(item);
			return size.ToString();
		}

		public static string RIS(string s)
		{
			var output = string.Empty;
			for (int i = 0; i < s.Length; i++)
			{
				if (s[i].ToString() == " ")
					output += "          ";
				else if (s[i].ToString() == "\n")
					output += "\n";
				else if (ValidateString(s[i].ToString(), "^[a-zA-Z]", 1))
					output += $":regional_indicator_{s[i].ToString().ToLower()}:";
			}
			return output;
		}

		public static bool FileFound(string f) =>
			File.Exists(f);

		public static string UpperString(string s, bool b = true) =>
			!b ? s : s.ToUpper();

		public static string GetRestAfter(string s, char l) =>
			s.Split(l).Last();

		public static bool ValidFileName(string path) =>
			string.IsNullOrEmpty(path) || path?.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0;

		public static bool ValidPathName(string path) =>
			string.IsNullOrEmpty(path) || path?.IndexOfAny(Path.GetInvalidPathChars()) >= 0;

		public static string GetLocalTime() =>
			DateTime.Now.ToString("HH:mm:ss");

		public static string GetPath() =>
			System.Diagnostics.Debugger.IsAttached ?
		    AppDomain.CurrentDomain.BaseDirectory : Settings.Default.ApplicationPath;

		public static string CutMessage(string msg, int minus = 0) =>
			msg.Length > maxmessagecount ?
			msg.Substring(0, maxmessagecount) : msg.Substring(0, msg.Length - minus);
	}
}