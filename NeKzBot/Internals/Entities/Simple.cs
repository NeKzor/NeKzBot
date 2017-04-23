using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace NeKzBot.Internals.Entities
{
	[JsonObject("simple")]
	public class Simple : IMemory, IEnumerable<string>
	{
		[JsonProperty("value")]
		public List<string> Value { get; set; }

		public Simple()
			=> Value = new List<string>();
		public Simple(List<string> value)
			=> Value = value;

		IEnumerable<object> IMemory.Values => Value;

		public IEnumerator<string> GetEnumerator()
			=> Value.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		public string Search(string value)
			=> Value.Find(v => string.Equals(v, value, StringComparison.CurrentCultureIgnoreCase));
		public override string ToString()
			=> Value.FirstOrDefault();
		public string ToString(int index)
			=> Value.ElementAt(index);
	}
}