using NeKzBot.Server;
using System.Diagnostics;
using System.Threading.Tasks;

namespace NeKzBot.Internals
{
	/// <summary>Used for accurate task delays.</summary>
	public sealed class InternalWatch
	{
		private Stopwatch _watch { get; set; }

		public InternalWatch()
			=> _watch = new Stopwatch();

		/// <summary>Returns the current running state of the stopwatch.</summary>
		public bool IsRunning
			=> _watch.IsRunning;

		/// <summary>Returns the last measured time of the stop watch.</summary>
		public int LastCheckedTimeValue { get; internal set; } = 0;

		/// <summary>Returns the elapsed time of the stopwatch.</summary>
		/// <param name="unit">Value with the given unit.</param>
		/// <param name="stop">If true, stops the internal timer.</param>
		/// <param name="message">If not null or empty, logs the message text with the value.</param>
		/// <returns></returns>
		public async Task<int> GetElapsedTimeAsync(Time unit = Time.Seconds, bool stop = true, string message = "")
		{
			if ((_watch == null)
			|| !((bool)_watch?.IsRunning))
				return 0;

			if (stop)
				_watch.Stop();

			var iresult = 0;
			var sresult = default(string);
			switch (unit)
			{
				case Time.Days:
					iresult = _watch.Elapsed.Days;
					sresult = $"{iresult}d";
					break;
				case Time.Hours:
					iresult = _watch.Elapsed.Hours;
					sresult = $"{iresult}h";
					break;
				case Time.Minutes:
					iresult = _watch.Elapsed.Minutes;
					sresult = $"{iresult}m";
					break;
				case Time.Seconds:
					iresult = _watch.Elapsed.Seconds;
					sresult = $"{iresult}s";
					break;
				case Time.Milliseconds:
					iresult = (int)_watch.ElapsedMilliseconds;
					sresult = $"{iresult}ms";
					break;
				case Time.Ticks:
					iresult = (int)_watch.ElapsedTicks;
					sresult = $"{iresult}t";
					break;
			}
			if (!(string.IsNullOrEmpty(message)))
				await Logger.SendAsync(message + sresult, LogColor.Watch);
			return LastCheckedTimeValue = iresult;
		}

		/// <summary>Starts and stops the stopwatch.</summary>
		public async Task RestartAsync()
		{
			await Stop();
			await Start();
		}

		private Task Start()
		{
			if (_watch == null)
				_watch = Stopwatch.StartNew();
			else
				_watch.Start();
			return Task.FromResult(0);
		}

		private Task Stop()
		{
			if (_watch != null)
				_watch.Stop();
			return Task.FromResult(0);
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
}