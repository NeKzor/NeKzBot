using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NeKzBot.Resources;

namespace NeKzBot.Internals
{
	public class Simple
	{
		public List<string> Value { get; set; }
		public Simple()
			=> Value = new List<string>();
		public Simple(List<string> value)
			=> Value = value;
		public override string ToString()
			=> Value.FirstOrDefault();
		public string ToString(int index)
			=> Value.ElementAt(index);
		public string Search(string value)
			=> Value.Find(v => string.Equals(v, value, StringComparison.CurrentCultureIgnoreCase));
	}

	public class Complex
	{
		public List<Simple> Values { get; set; }
		public Complex()
			=> Values = new List<Simple>();
		public Complex(List<Simple> value)
			=> Values = value;
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

	public sealed class InternalData<T> : IData where T : class, new()
	{
		public string Name { get; }
		public bool ReadingAllowed { get; }
		public bool WrittingAllowed { get; }
		public bool ReadWriteAllowed
		{
			get => ReadingAllowed && WrittingAllowed;
		}
		public string FileName { get; }
		public object Memory { get; private set; }
		public Func<string, object> SelfInit { get; set; }

		public InternalData(string name, bool reading, bool writing, string filename, Func<string, object> parser = default(Func<string, object>), bool initnow = true)
		{
			Name = name;
			ReadingAllowed = reading;
			WrittingAllowed = writing;
			FileName = filename;
			Memory = new T();
			SelfInit = (parser == default(Func<string, object>))
							   ? Parsers.CrossParser
							   : parser;
			if (initnow)
				Init();
		}

		public Task Init()
			=> Task.FromResult(Memory = SelfInit(FileName));

		public Task Get()
			=> Task.FromResult(Memory as T);

		public Task Change(object data)
			=> Task.FromResult(Memory = data);
	}
}