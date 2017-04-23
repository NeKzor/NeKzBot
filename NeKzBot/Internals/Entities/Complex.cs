using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NeKzBot.Internals.Entities
{
	[JsonObject("complex")]
	public class Complex : IMemory, IEnumerable<Simple>
	{
		[JsonProperty("values")]
		public List<Simple> Values { get; set; }

		public Complex()
			=> Values = new List<Simple>();
		public Complex(List<Simple> value)
			=> Values = value;

		IEnumerable<object> IMemory.Values => Values;

		public IEnumerator<Simple> GetEnumerator()
			=> Values.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();

		public Simple Get(string value)
		{
			foreach (var item in Values)
				if (item.Search(value) != null)
					return item;
			return null;
		}
		public string GetValue(string value, int index)
		{
			foreach (var item in Values)
				if (item.Search(value) != null)
					return item.Value[index];
			return null;
		}
		public IEnumerable<string> Cast()
		{
			var output = new List<string>();
			foreach (var item in Values)
				output.Add(item.ToString());
			return output;
		}
		public IEnumerable<string> Cast(int index)
		{
			var output = new List<string>();
			foreach (var item in Values)
				output.Add(item.ToString(index));
			return output;
		}
	}
}