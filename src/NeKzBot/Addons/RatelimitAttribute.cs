using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;

namespace Discord.Addons.Preconditions
{
	public enum Measure
	{
		Days,
		Hours,
		Minutes
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public sealed class RatelimitAttribute : PreconditionAttribute
	{
		private readonly uint _invokeLimit;
		private readonly bool _noLimitInDMs;
		private readonly bool _noLimitForAdmins;
		private readonly TimeSpan _invokeLimitPeriod;
		private readonly Dictionary<ulong, CommandTimeout> _invokeTracker = new Dictionary<ulong, CommandTimeout>();

		public RatelimitAttribute(uint times, double period, Measure measure, bool noLimitInDMs = false, bool noLimitForAdmins = false)
		{
			_invokeLimit = times;
			_noLimitInDMs = noLimitInDMs;
			_noLimitForAdmins = noLimitForAdmins;

			switch (measure)
			{
				case Measure.Days:
					_invokeLimitPeriod = TimeSpan.FromDays(period);
					break;
				case Measure.Hours:
					_invokeLimitPeriod = TimeSpan.FromHours(period);
					break;
				case Measure.Minutes:
					_invokeLimitPeriod = TimeSpan.FromMinutes(period);
					break;
			}
		}

		public RatelimitAttribute(uint times, TimeSpan period, bool noLimitInDMs = false, bool noLimitForAdmins = false)
		{
			_invokeLimit = times;
			_noLimitInDMs = noLimitInDMs;
			_noLimitForAdmins = noLimitForAdmins;
			_invokeLimitPeriod = period;
		}

		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
		{
			if (_noLimitInDMs && context.Channel is IPrivateChannel)
				return Task.FromResult(PreconditionResult.FromSuccess());

			if (_noLimitForAdmins && context.User is IGuildUser gu && gu.GuildPermissions.Administrator)
				return Task.FromResult(PreconditionResult.FromSuccess());

			var now = DateTime.UtcNow;
			var timeout = (_invokeTracker.TryGetValue(context.User.Id, out var t)
				&& ((now - t.FirstInvoke) < _invokeLimitPeriod))
					? t : new CommandTimeout(now);

			timeout.TimesInvoked++;

			if (timeout.TimesInvoked <= _invokeLimit)
			{
				_invokeTracker[context.User.Id] = timeout;
				return Task.FromResult(PreconditionResult.FromSuccess());
			}
			else
			{
				return Task.FromResult(PreconditionResult.FromError("You are currently in Timeout."));
			}
		}

		private class CommandTimeout
		{
			public uint TimesInvoked { get; set; }
			public DateTime FirstInvoke { get; }

			public CommandTimeout(DateTime timeStarted)
			{
				FirstInvoke = timeStarted;
			}
		}
	}
}