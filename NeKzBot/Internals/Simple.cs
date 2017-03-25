using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace NeKzBot.Internals
{
	[JsonObject("simple")]
	public class Simple
	{
		[JsonProperty("value")]
		public List<string> Value { get; set; }

		public Simple()
			=> Value = new List<string>();
		public Simple(List<string> value)
			=> Value = value;

		public string Search(string value)
			=> Value.Find(v => string.Equals(v, value, StringComparison.CurrentCultureIgnoreCase));
		public override string ToString()
			=> Value.FirstOrDefault();
		public string ToString(int index)
			=> Value.ElementAt(index);
	}
}