using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NeKzBot.Classes;

namespace NeKzBot.Resources
{
	public static class Utils
	{
		private static int _temp = -1;
		private static readonly Random _rand = new Random();

		private const int _luckynumber = 7;

		public static Task<bool> SearchInArray(object[,] searchin, int dimension, string tosearch, out int index)
		{
			for (index = 0; index < searchin.GetLength(0); index++)
				if (tosearch.ToLower().Replace(" ", string.Empty) == searchin[index, dimension].ToString().ToLower().Replace(" ", string.Empty))
					return Task.FromResult(true);
			return Task.FromResult(false);
		}

		// Search values of properties in a class
		public static Task<bool> SearchInClassByName<T>(List<T> searchin, Type typeclass, string name, string search, out int index)
		{
			index = -1;
			if (searchin.FirstOrDefault().GetType() == typeclass)
			{
				var tosearch = search.ToLower()
									 .Replace(" ", string.Empty);
				for (index = 0; index < searchin.Count; index++)
				{
					var tocomp = searchin[index].GetType()
												.GetProperty(name)
												.GetValue(searchin[index], null)
												.ToString()
												.ToLower()
												.Replace(" ", string.Empty);
					if (tosearch == tocomp)
						return Task.FromResult(true);
				}
			}
			return Task.FromResult(false);
		}

		public static Task<string> CutMessage(string msg, int limit, string append)
			=> Task.FromResult((msg.Length > limit)
										   ? msg.Substring(0, limit) + append
										   : msg.Substring(0, msg.Length));

		public static Task<int> RNG(int from, int to)
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

		public static async Task<object> RNGObject(object[,] array, int index = 0)
			=> array[await RNG(0, array.GetLength(0)), index];

		public static async Task<object> RNGObject<T>(List<T> list)
			=> list[await RNG(0, list.Count)];

		public static async Task<int> RNGInt<T>(List<T> list)
			=> await RNG(0, list.Count);

		// File I/O
		public static async Task<object> ReadDataAsync(string path, string separator = "|")
		{
			var data = default(object[]);
			var lines = default(object[]);
			var file = Path.Combine(await GetPath(), path);
			if (!(File.Exists(file)))
				new object();

			using (var fs = new FileStream(file, FileMode.Open))
			using (var sr = new StreamReader(fs))
				lines = (await sr.ReadToEndAsync()).Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
			if ((lines as string[]).FirstOrDefault().Contains(separator))
			{
				data = data ?? new object[(lines.Length)];
				for (int i = 0; i < data.Length; i++)
					data[i] = (lines[i] as string).Split(new string[] { separator }, StringSplitOptions.None);
			}
			else
				data = lines;
			return data;
		}

		// Small information when caching data
		public static Task<int> StringInBytes(string s)
			=> Task.FromResult(Encoding.UTF8.GetByteCount(s));

		public static async Task<string> StringInBytesAsync(params string[] s)
		{
			var size = 0;
			foreach (var item in s)
				size += await StringInBytes(item);
			return size.ToString();
		}

		// Fun formatting
		public static async Task<string> ToRISAsync(string s)
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

		public static async Task<List<string>> GetEmojisAsRISAsync(string s)
		{
			var output = string.Empty;
			var emojis = new List<string>();
			var numbers = new string[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };
			for (int i = 0; i < s.Length; i++)
			{
				var c = s[i];
				var temp = string.Empty;
				if (await ValidateString(c.ToString(), "^[a-zA-Z]", 1))
					temp = $":regional_indicator_{c.ToString().ToLower()}:";
				else if (await ValidateString(c.ToString(), "^[0-9]", 1))
					temp = $":{numbers[Convert.ToInt16(c.ToString())]}:";
				else if (c == '!')
					temp = ":grey_exclamation:";
				else if (c == '?')
					temp = ":grey_question:";
				else
					continue;

				// Don't add double emojis
				if (emojis.Contains(temp))
					continue;
				else
					emojis.Add(temp);
			}
			return emojis;
		}

		// Find channel of server
		public static Task<SocketChannel> GetChannel(ulong server, ulong channel)
			=> Task.FromResult(Bot.Client?.GetGuild(server)?.GetChannel(channel) as SocketChannel);

		// Return the role color of a user
		public static Task<Color> GetUserColor(IUser user, IGuild guild)
		{
			if ((user != null)
			&& (guild != null))
				foreach (var role in guild.Roles)
					if ((user as SocketGuildUser).Roles.Contains(role))
						return Task.FromResult(role.Color);
			return Task.FromResult(Data.BasicColor);
		}

		// Get message of a channel
		public static async Task<IMessage> GetMessageAsync(CommandContext context, ulong id)
		{
			var message = default(IMessage);
			try
			{
				message = (await context.Channel.GetMessagesAsync(context.Message, Direction.Before).Flatten()).FirstOrDefault(msg => msg.Id == id);
			}
			catch
			{
				return null;
			}
			return message;
		}

		// String utilities
		public static Task<string> GetLocalTime()
			=> Task.FromResult(DateTime.Now.ToString("HH:mm:ss"));

		public static Task<string> GetPath()
			=> Task.FromResult(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));

		// Others
		public static Task<bool> ValidateString(string s, string pattern, int maxlength)
			=> (s != string.Empty)
			&& (new Regex(pattern).IsMatch(s))
				? Task.FromResult(!(s.Length > maxlength))
				: Task.FromResult(false);

		public static Task<bool> IsImage(string extension)
			=> Task.FromResult((extension == ".png")
							|| (extension == ".bmp")
							|| (extension == ".gif")
							|| (extension == ".png")
							|| (extension == ".svg")
							|| (extension == ".jpg")
							|| (extension == ".jpeg")
							|| (extension == ".tiff"));

		public static Task<string> GetDuration(DateTime time)
		{
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
			return Task.FromResult(output);
		}
	}
}