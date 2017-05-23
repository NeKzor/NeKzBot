using System;

namespace NeKzBot.Server
{
	public enum LogColor
	{
		Audio = ConsoleColor.DarkGreen,
		Caching = ConsoleColor.Red,
		Default = ConsoleColor.White,
		Dropbox = ConsoleColor.Blue,
		Error = ConsoleColor.Red,
		Init = ConsoleColor.DarkYellow,
		Leaderboard = ConsoleColor.DarkBlue,
		Speedrun = ConsoleColor.DarkRed,
		Twitch = ConsoleColor.DarkMagenta,
		Twitter = ConsoleColor.Green,
		Watch = ConsoleColor.Yellow
	}

	/// <summary>Units of time.</summary>
	public enum Time
	{
		Days,
		Hours,
		Minutes,
		Seconds,
		Milliseconds,
		Ticks
	}
}