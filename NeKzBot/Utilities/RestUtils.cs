using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace NeKzBot.Utilities
{
	public static partial class Utils
	{
		public static Task<bool> ValidFileName(string file)
			=> Task.FromResult((!(string.IsNullOrEmpty(file))) && (file?.IndexOfAny(Path.GetInvalidFileNameChars()) == -1));

		public static Task<bool> ValidPathName(string path)
			=> Task.FromResult((!(string.IsNullOrEmpty(path))) && (path?.IndexOfAny(Path.GetInvalidPathChars()) == -1));

		public static Task<string> GetLocalTime(bool actualtime = false)
		{
			var result = default(string);
			if (actualtime)
			{
				var zone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
				var offset = zone.GetUtcOffset(DateTime.UtcNow).Hours;
				result = $"{TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zone).ToString("HH:mm:ss")}" +
						 $" (UTC{((offset == 0) ? " " : (offset < 0) ? "-" : "+")}{offset})";
			}
			else
				result = DateTime.UtcNow.ToString("HH:mm:ss");
			return Task.FromResult(result);
		}

		public static Task<string> GetAppPath()
			=> Task.FromResult(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));

		public static Task<TimeSpan> GetUptime()
			=> Task.FromResult(DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime());

		public static Task<bool> IsLinux()
			=> Task.FromResult((Environment.OSVersion.Platform == PlatformID.Unix)
							|| (Environment.OSVersion.Platform == PlatformID.MacOSX)
							|| ((int)Environment.OSVersion.Platform == 128));

		// I copied this from Voltana c:
		public static Task<bool> FlagIsSet(uint value, byte flag)
			=> Task.FromResult(((value >> flag) & 1U) == 1);
	}
}