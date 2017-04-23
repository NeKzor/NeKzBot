using System.Collections.Generic;

namespace NeKzBot.Internals.Entities
{
	public interface IMemory
	{
		IEnumerable<object> Values { get; }
	}
}