using System;
using System.Timers;
using NeKzBot.Server;

namespace NeKzBot.Internals
{
	/// <summary>Used to start a new instance of a class at a certain time.</summary>
	public sealed class InternalMaintenance
	{
		private Type _class { get; }
		private string _method { get; }
		private Timer _event { get; set; }

		public InternalMaintenance(Type tclass, string method)
		{
			if (!(tclass.IsClass))
				throw new Exception("You can only use classes for the type.");

			_class = tclass;
			_method = method;
		}

		public void Init(uint time, Time type = Time.Milliseconds)
		{
			var factor = default(int);
			switch (type)
			{
				case Time.Days:
					factor = 24 * 60 * 60 * 1000;
					break;
				case Time.Hours:
					factor = 60 * 60 * 1000;
					break;
				case Time.Minutes:
					factor = 60 * 1000;
					break;
				case Time.Seconds:
					factor = 1 * 1000;
					break;
				case Time.Milliseconds:
					factor = 1;
					break;
				case Time.Ticks:
					throw new Exception("You cannot set the type to ticks.");
			}

			_event = new Timer(time * factor)
			{
				AutoReset = true
			};
			_event.Elapsed += OnElapsed;
		}

		public void Start()
		{
			if (_event == null)
				throw new Exception("You did not invoke the required Init() of this class.");
			_event.Start();
		}

		private void OnElapsed(object source, ElapsedEventArgs e)
		{
			if (GetType().GetMethod(_method, Type.EmptyTypes) == null)
				throw new Exception("This method does not exist.");

			GetType().GetMethod(_method, Type.EmptyTypes)
					 .MakeGenericMethod(_class)
					 .Invoke(this, null);
		}
	}
}