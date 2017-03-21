using NeKzBot.Server;

namespace NeKzBot.Classes
{
	public static class TwitchError
	{
		public static readonly string Generic = "**Error.**";
		public static readonly string Offline = "Streamer is offline.";
	}

	public static class DataError
	{
		public static readonly string Generic = "**Error.**";
		public static readonly string FileNotFound = "**Error** • File not found.";
		public static readonly string InvalidValues = "**Error** • Invalid values.";
		public static readonly string DataMissing = "**Error** • Data is missing.";
		public static readonly string NameAlreadyExists = "**Error** • Data already exists.";
		public static readonly string InvalidDimensions = "**Error** • Invalid dimensions. Did you forget a value?";
		public static readonly string Unknown = "**Error** • Something went wrong.";
		public static readonly string InvalidStream = "**Error** • Failed to write new data.";
		public static readonly string NameNotFound = "**Error** - Unique data name/id not found.";
	}

	public static class AudioError
	{
		public static readonly string None = string.Empty;
		public static readonly string Generic = "**Error.**";
		public static readonly string AlreadyConnected = "Already connected to a channel.";
		public static readonly string InvalidChannel = "Cannot join a channel on a different server.";
		public static readonly string BotNotConneted = $"Please connect the bot to a voice channel first. Try `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} connect`.";
		public static readonly string AlreadyPlaying = $"Already playing an audio stream. Try `{Configuration.Default.PrefixCmd + Configuration.Default.BotCmd} stop`.";
		public static readonly string InvalidRequest = "You cannot play an audio stream without connecting to a voice channel.";
		public static readonly string WrongChannel = "You cannot play an audio stream without connecting to the same voice channel of the bot.";
		public static readonly string FileMissing = "File could not be found. This is a server failure, sorry about that.";
	}
}