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
		public static readonly string Unkown = "**Error** • Something went wrong.";
		public static readonly string InvalidStream = "**Error** • Failed to write new data.";
		public static readonly string Reload = "**Error** • DataManager failed to reload data.";
		public static readonly string NameNotFound = "**Error** - Unique data name/id not found.";
	}
}