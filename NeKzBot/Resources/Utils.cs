using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace NeKzBot
{
	public class Utils
	{
		public static int index = 0;
		public static string group = string.Empty;
		private static Random rand;

		public const char seperator = '|';
		private const int maxarraycount = 128;

		// Check if a specific value is in that data array/list
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

		// Check if user has the role to make an operation
		public static bool RoleCheck(Discord.User checkUser, params string[] roles)
		{
			var userRoles = checkUser.Roles.Select(x => x.ToString()).ToArray();

			foreach (var role in roles)
				foreach (var userRole in userRoles)
					if (role == userRole)
						return true;
			return false;
		}

		public static bool RoleCheck(Discord.User checkUser, System.Collections.Specialized.StringCollection roles)
		{
			var userRoles = checkUser.Roles.Select(x => x.ToString()).ToArray();
			foreach (var role in roles)
				foreach (var userRole in userRoles)
					if (role == userRole)
						return true;
			return false;
		}

		public static bool RoleCheck(Discord.User checkUser, System.Collections.Specialized.StringCollection roles, params string[] proles)
		{
			var userRoles = checkUser.Roles.Select(x => x.ToString()).ToArray();
			foreach (var role in roles)
				foreach (var userRole in userRoles)
					if (role == userRole)
						return true;

			foreach (var role in proles)
				foreach (var userRole in userRoles)
					if (role == userRole)
						return true;
			return false;
		}

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

		public static string ArrayToList(object[,] s, int dimension = 0, string formatting = "", string delimiter = ", ", string list = "")
		{
			var output = string.Empty;
			for (int i = 0; i < s.GetLength(0); i++)
				output += list + formatting + s[i, dimension].ToString() + formatting + delimiter;
			return output.Substring(0, output.Length - delimiter.Length);
		}

		public static string[] ArrayToArray(string[,] s, int d)
		{
			var output = new string[s.GetLength(0)];
			for (int i = 0; i < s.GetLength(0); i++)
				output[i] += s[i, d];
			return output;
		}

		public static string CollectionToList(System.Collections.Specialized.StringCollection s, string formatting = "", string delimiter = ", ")
		{
			var output = string.Empty;
			for (int i = 0; i < s.Count; i++)
				output += formatting + s[i] + formatting + delimiter;
			return output.Substring(0, output.Length - delimiter.Length);
		}

		public static string ListToList(List<string> s, string formatting = "", string delimiter = ", ")
		{
			var output = string.Empty;
			for (int i = 0; i < s.Count; i++)
				output += formatting + s[i] + formatting + delimiter;
			return output.Substring(0, output.Length - delimiter.Length);
		}

		// Create multiple commands from array
		public static void CommandCreator(Action act, int dim, string[,] str, bool hasaliases = false, string[] aliases = null)
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
		}

		public static void CommandCreator(Action act, int from, int to, bool hasaliases = false, string[] aliases = null)
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
		}

		// Text formatting
		public static string AsBold(string s) => $"**{s}**";
		public static string AsItalitc(string s) => $"*{s}*";
		public static string AsCommand(string s) => $"`{s}`";

		// Checking names (name = Name -> true)
		public static bool StringMatch(string s, string c) => (s.ToLower() != c.ToLower()) ? true : false;

		// Parse after par1 (!cmd par1 par2)
		public static string GetRest(string[] s, int from, int to = 0, string sep = "", bool firstreplace = false)
		{
			if (to == 0)
				to = s.Count();

			var output = string.Empty;
			for (; from < to; from++)
			{
				output += s[from];
				if (from + 1 != to)
					output += sep;
			}
			return firstreplace ? ReplaceFirst(output, sep, seperator.ToString()) : output;
		}

		public static string ReplaceFirst(string text, string search, string replace)
		{
			var pos = text.IndexOf(search);
			return pos < 0 ? text : text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
		}

		// Check with regex pattern
		public static bool ValidateString(string s, string pattern, int maxlength)
		{
			if (s == string.Empty)
				return false;
			if (!new System.Text.RegularExpressions.Regex(pattern).IsMatch(s))
				return false;
			return s.Length > maxlength ? false : true;
		}

		// Small information when caching data
		public static string StringInBytes(string s) => (s.Length * sizeof(char)).ToString();

		public static string StringInBytes(params string[] s)
		{
			var size = 0;
			foreach (var item in s)
				size += (item.Length * sizeof(char));
			return size.ToString();
		}

		// Generates random integers
		public static int RNG(int to)
		{
			rand = new Random();
			return rand.Next(0, to);
		}

		public static int RNG(int from, int to)
		{
			rand = new Random();
			return rand.Next(from, to);
		}

		// Fun formatting with regional indicator symbols
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

		// Read, write data
		public static object ReadFromFile(string filepath)
		{
			var file = Properties.Settings.Default.ApplicationPath + Properties.Settings.Default.DataPath + filepath;
			if (System.Diagnostics.Debugger.IsAttached)
				file = AppDomain.CurrentDomain.BaseDirectory + Properties.Settings.Default.DataPath + filepath;

			if (!FileFound(file))
				return null;

			string[] input = new string[maxarraycount];
			string[,] array = null;

			try
			{
				var fs = new FileStream(file, FileMode.Open);
				var sr = new StreamReader(fs);

				while (!sr.EndOfStream)
					input = sr.ReadToEnd().Replace("\r", string.Empty).Split('\n');

				if (input[0].Contains(seperator))
					array = new string[input.Count(), input[0].Split(seperator).Count()];
				else
				{
					sr.Close();
					fs.Close();
					return ReadFromFileS(filepath);
				}

				for (int i = 0; i < input.Count(); i++)
					for (int j = 0; j < input[i].Split(seperator).Count(); j++)
						array[i, j] = input[i].Split(seperator)[j];

				sr.Close();
				fs.Close();
			}
			catch
			{
				return null;
			}
			return array;
		}

		public static string[] ReadFromFileS(string filepath)
		{
			var file = Properties.Settings.Default.ApplicationPath + Properties.Settings.Default.DataPath + filepath;
			if (System.Diagnostics.Debugger.IsAttached)
				file = AppDomain.CurrentDomain.BaseDirectory + Properties.Settings.Default.DataPath + filepath;

			if (!FileFound(file))
				return null;

			string[] input = new string[maxarraycount];

			try
			{
				var fs = new FileStream(file, FileMode.Open);
				var sr = new StreamReader(fs);

				while (!sr.EndOfStream)
					input = sr.ReadToEnd().Replace("\r", string.Empty).Split('\n');

				sr.Close();
				fs.Close();
			}
			catch
			{
				return null;
			}
			return input;
		}

		public static string AddData(string filepath, string value, out bool success)
		{
			success = false;
			var file = AppDomain.CurrentDomain.BaseDirectory + Properties.Settings.Default.DataPath + filepath;
			if (!FileFound(file))
				return "**Error -** File not found.";

			string[] line = value.Split(seperator);
			foreach (var item in line)
				if (item.Replace(" ", string.Empty) == string.Empty)	// I know this doesn't check everything but who wants to exploit that?
					return "**Error -** Invalid values.";

			// Check if command already exits
			object obj = null;
			for (int i = 0; i < CmdManager.dataCommands.GetLength(0); i++)
			{
				obj = CmdManager.dataCommands[i, 3];
				if (obj.GetType() == typeof(string[,]))
				{
					if (SearchArray((string[,])CmdManager.dataCommands[i, 3], 0, line[0], out index))
						return "**Error -** Command already exists.";
				}
				else if (obj.GetType() == typeof(string[]))
				{
					if (SearchArray((string[])CmdManager.dataCommands[i, 3], line[0], out index))
						return "**Error -** Command already exists.";
				}
				else
					return "**Error -** Something went wrong.";
			}

			// First read old data
			List<string> ls;
			if (line.Count() == 1)
				ls = ReadFromFileS(filepath).Cast<string>().ToList();
			else if (line.Count() > 1)
			{
				string[,] temp = (string[,])ReadFromFile(filepath);
				// Check if dimension count is valid
				if (temp.Rank != line.Count())
					return "**Error -** Invalid dimensions. Did you forget a value?";
				ls = temp.Cast<string>().ToList();
			}
			else
				return "**Error -** Did you forget a value?";

			// Add new data
			for (int i = 0; i < line.Length; i++)
				ls.Add(line[i]);

			// Write new data
			try
			{
				var fs = new FileStream(file, FileMode.Create);
				var sw = new StreamWriter(fs);

				for (int i = 0; i < ls.Count(); i += line.Length)
				{
					for (int j = 0; j < line.Length; j++)
					{
						sw.Write(ls[i + j]);
						if (j + 1 != line.Length)
							sw.Write(seperator);
					}
					if (i + line.Length != ls.Count())
						sw.Write("\n");
				}
				sw.Close();
				fs.Close();
			}
			catch
			{
				return "**Error -** Failed to write new data.";
			}
			success = true;
			return "New command added.";
		}

		public static string DeleteData(string filepath, string value)
		{
			var file = AppDomain.CurrentDomain.BaseDirectory + Properties.Settings.Default.DataPath + filepath;
			if (!FileFound(file))
				return "**Error -** File not found.";

			string[] line = value.Split(seperator);
			foreach (var item in line)
				if (item.Replace(" ", string.Empty) == string.Empty)
					return "**Error -** Failed to parse values.";

			// Check if command does actually exit
			var found = false;
			int index = 0, dimensions = 1;
			object obj = null;
			List<string> ls = null;
			for (int i = 0; i < CmdManager.dataCommands.GetLength(0); i++)
			{
				obj = CmdManager.dataCommands[i, 3];
				if (obj.GetType() == typeof(string[,]))
				{
					if (SearchArray((string[,])CmdManager.dataCommands[i, 3], 0, line[0], out index))
					{
						string[,] temp = (string[,])ReadFromFile(filepath);
						dimensions = temp.Rank;
						ls = temp.Cast<string>().ToList();
						found = true;
						break;
					}
				}
				else if (obj.GetType() == typeof(string[]))
				{
					if (SearchArray((string[])CmdManager.dataCommands[i, 3], line[0], out index))
					{
						ls = ReadFromFileS(filepath).Cast<string>().ToList();
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
				var fs = new FileStream(file, FileMode.Create);
				var sw = new StreamWriter(fs);

				for (int i = 0; i < ls.Count(); i += dimensions)
				{
					for (int j = 0; j < dimensions; j++)
					{
						sw.Write(ls[i + j]);
						if (j + 1 != dimensions)
							sw.Write(seperator);
					}
					if (i + dimensions != ls.Count())
						sw.Write("\n");
				}
				sw.Close();
				fs.Close();
			}
			catch
			{
				return "**Error -** Failed to write new data.";
			}
			return "Command deleted. Changes take effect on next server restart.";
		}

		// System process
		public static string GetCommandOutput(string command, string parameters = "")
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
			catch
			{
				return "Error";
			}
		}

		// Others
		public static bool FileFound(string f) => File.Exists(f) ? true : false;

		public static string UpperString(string s, bool b = true) => b ? s.ToUpper() : s;

		public static string GetRestAfter(string s, char l) => s.Split(l).Last();
	}
}