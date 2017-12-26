using System.Text.RegularExpressions;

namespace NeKzBot.Extensions
{
	public static class StringExtensions
    {
		// Validation
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

		// Numbers to string
		public static string FormatPointsToString(this uint? points)
		{
			if (points != 0)
				return points?.ToString("#,###,###.##");
			return points.ToString();
		}
		public static string FormatAveragePlaceToString(this float? place)
		{
			if (place == default)
				return "NO";
			return place?.ToString("N1");
		}
		public static string FormatRankToString(this uint? rank, string wr = null)
		{
			if (rank == default)
				return "NO";
			if ((rank == 1) && (wr != null))
				return wr;
			if ((rank == 11) || (rank == 12) || (rank == 13))
				return $"{rank}th";
			if (rank % 10 == 1)
				return $"{rank}st";
			if (rank % 10 == 2)
				return $"{rank}nd";
			if (rank % 10 == 3)
				return $"{rank}rd";
			return $"{rank}th";
		}
	}
}