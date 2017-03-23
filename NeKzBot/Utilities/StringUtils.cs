using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NeKzBot.Extensions;

namespace NeKzBot.Utilities
{
	public static partial class Utils
	{
		private static readonly string[] _badChars = { "\\", "*", "_", "~", "`" };

		// Search
		public static Task<bool> SearchCollection(IEnumerable<string> collection, string tosearch)
			=> Task.FromResult(Array.FindIndex(collection.ToArray(), str => str == tosearch) != -1);

		public static Task<bool> SearchCollection(IEnumerable<string> Collection, string tosearch, out int index)
			=> Task.FromResult((index = Array.FindIndex(Collection.ToArray(), str => str == tosearch)) != -1);

		public static Task<bool> ValidateString(string s, string pattern, int maxlength)
		{
			if (s == string.Empty)
				return Task.FromResult(false);
			if (!(new Regex(pattern).IsMatch(s)))
				return Task.FromResult(false);
			return Task.FromResult(!(s.Length > maxlength));
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

		// Substring message text before sending it, so it doesn't exceed the char limit
		public static async Task<string> CutMessageAsync(string text, int minus = 0, bool badchars = true)
			=> ((text = (badchars) ? await AsRawText(text) : text).Length > DiscordConstants.MaximumCharsPerMessage)
																		  ? text.Substring(0, (int)DiscordConstants.MaximumCharsPerMessage)
																		  : text.Substring(0, text.Length - minus);

		public static async Task<string> CutMessageAsync(string text, int limit, string append, bool badchars = true)
			=> ((text = (badchars) ? await AsRawText(text) : text).Length > limit)
																		  ? text.Substring(0, limit) + append
																		  : text.Substring(0, text.Length);

		// Another copy...
		public static Task<string> AsRawText(string text)
		{
			if (string.IsNullOrEmpty(text))
				return Task.FromResult(text);
			foreach (var item in _badChars)
				text = text.Replace(item, $"\\{item}");
			return Task.FromResult(text);
		}
	}
}