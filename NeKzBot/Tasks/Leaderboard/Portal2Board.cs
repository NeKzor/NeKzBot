using System;
using System.Threading.Tasks;
using NeKzBot.Classes;
using NeKzBot.Server;

namespace NeKzBot.Tasks.Leaderboard
{
	public static partial class Portal2Board
	{
		public static string FormatPointsToString(this uint? points)
			=> (points != 0)
					   ? points?.ToString("#,###,###.##")
					   : points.ToString();

		public static string FormatAveragePlaceToString(this float? place)
			=> place == default(float?)
					 ? "NO"
					 : place?.ToString("N1");

		public static string FormatRankToString(this uint? rank)
		{
			if (rank == default(uint?))
				return "NO";

			if ((rank == 11)
			|| (rank == 12)
			|| (rank == 13))
				return $"{rank}th";
			if (rank % 10 == 1)
				return $"{rank}st";
			if (rank % 10 == 2)
				return $"{rank}nd";
			if (rank % 10 == 3)
				return $"{rank}rd";
			return $"{rank}th";
		}

		private static async Task<string> FormatMainTweetAsync(string msg, params string[] stuff)
		{
			var output = msg;
			try
			{
				// Check if this (somehow) exceed the character limit
				if (Twitter.TweetLimit - output.Length < 0)
					return string.Empty;

				// Only append stuff if it fits
				foreach (var item in stuff)
				{
					if (item == string.Empty)
						continue;
					if (Twitter.TweetLimit - output.Length - item.Length < 0)
						return output;
					output += "\n" + item;
				}
				return output;
			}
			catch (Exception e)
			{
				await Logger.SendToChannelAsync("Portal2.FormatMainTweetAsync Error", e);
			}
			return string.Empty;
		}

		private static async Task<string> FormatReplyTweetAsync(string player, string comment)
		{
			var newline = $"@Portal2Records {player}: ";
			var output = string.Empty;
			try
			{
				// There isn't always a comment
				if (string.IsNullOrEmpty(comment))
					return comment;

				// Check what's left
				const string cut = "...";
				var left = Twitter.TweetLimit - output.Length - newline.Length - comment.Length;
				if ((-left + cut.Length == comment.Length)
				|| (newline.Length >= Twitter.TweetLimit - output.Length))
					return output;

				// It's safe
				if (left >= 0)
					return newline + comment;

				// Cut comment and append "..." at the end
				return newline + comment.Substring(0, Twitter.TweetLimit - output.Length - newline.Length - cut.Length) + cut;
			}
			catch (Exception e)
			{
				await Logger.SendToChannelAsync("Portal2.FormatReplyTweetAsync Error", e);
			}
			return string.Empty;
		}
	}
}