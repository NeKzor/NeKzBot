using System.Text.RegularExpressions;

namespace NeKzBot.Extensions
{
	public static class StringExtensions
    {
		private static readonly string[] _badChars = { "\\", "*", "_", "~", "`" };

		public static string ToRawText(this string text)
		{
			if (!string.IsNullOrEmpty(text))
				foreach (var item in _badChars)
					text = text.Replace(item, $"\\{item}");
			return text;
		}

		public static bool Validate(this string text, string pattern, int maxLength)
		{
			if ((!string.IsNullOrEmpty(text)) && (new Regex(pattern).IsMatch(text)))
				return !(text.Length > maxLength);
			return false;
		}
	}
}