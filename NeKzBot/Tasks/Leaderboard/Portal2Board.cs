using System;
using System.Threading.Tasks;
using NeKzBot.Classes;
using NeKzBot.Server;

namespace NeKzBot.Tasks.Leaderboard
{
	public static partial class Portal2Board
	{
		//private static Task<string> RankFromat(string s)
		//{
		//	var output = $"Rank • **{s}th**";
		//	if (Convert.ToInt16(s) > 10)
		//		output = $"Rank • **#{s}**";
		//	else if (s == "1")
		//		output = string.Empty;  // Trophy maybe?
		//	else if (s == "2")
		//		output = "Rank • **2nd**";
		//	else if (s == "3")
		//		output = "Rank • **3rd**";
		//	return Task.FromResult(output);
		//}

		//private static Task<string> AddDecimalPlace(string s)
		//	=> Task.FromResult((s.Contains(".")
		//					|| (s == "NO"))
		//						  ? s
		//						  : $"{s}.0");

		//private static Task<string> FormatRank(string s)
		//{
		//	if (s == "NO")
		//		return Task.FromResult(s);

		//	var output = $"{s}th";
		//	if ((s == "11")
		//	|| (s == "12")
		//	|| (s == "13"))
		//		return Task.FromResult(output);
		//	else if (s[s.Length - 1] == '1')
		//		output = $"{s}st";
		//	else if (s[s.Length - 1] == '2')
		//		output = $"{s}nd";
		//	else if (s[s.Length - 1] == '3')
		//		output = $"{s}rd";
		//	return Task.FromResult(output);
		//}

		//private static Task<string> FormatPoints(string s)
		//	=> Task.FromResult((s != "0")
		//						  ? int.Parse(s).ToString("#,###,###.##")
		//						  : s);

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