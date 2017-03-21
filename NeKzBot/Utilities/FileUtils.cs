using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NeKzBot.Classes;
using NeKzBot.Internals;
using NeKzBot.Resources;
using NeKzBot.Server;
using NeKzBot.Webhooks;

namespace NeKzBot.Utilities
{
	public static partial class Utils
	{
		public const char DataSeparator = '|';
		private const int _maxarraycount = 128;

		public static async Task<object> ReadFromFileAsync(string name)
		{
			var file = Path.Combine(await GetAppPath(), Configuration.Default.DataPath, name);
			if (!(File.Exists(file)))
				return null;

			var input = new string[_maxarraycount];
			var array = default(string[,]);

			try
			{
				using (var fs = new FileStream(file, FileMode.Open))
				using (var sr = new StreamReader(fs))
				{
					while (!sr.EndOfStream)
						input = (await sr.ReadToEndAsync()).Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

					if (input[0].Contains(DataSeparator))
						array = new string[input.Length, input[0].Split(DataSeparator).Length];
					else
						return input;

					for (int i = 0; i < input.Length; i++)
						for (int j = 0; j < input[i].Split(DataSeparator).Length; j++)
							array[i, j] = input[i].Split(DataSeparator)[j];
				}
			}
			catch
			{
				return null;
			}
			return array;
		}

		public static async Task<string> ChangeDataAsync(IData data, string value, DataChangeMode mode)
		{
			// Fail safe
			var file = data.FileName;
			var filepath = Path.Combine(await GetAppPath(), Configuration.Default.DataPath, file);
			if (!(File.Exists(filepath)))
				return DataError.FileNotFound;

			var values = default(List<string>);
			if (mode == DataChangeMode.Add)
			{
				values = value.Split(DataSeparator)
							  .ToList();
				foreach (var item in values)
					if (item == string.Empty)
						return DataError.InvalidValues;
			}

			// Pattern matching
			var dimensions = 1;
			var collection = default(List<string>);
			var foundindex = default(int);
			var memory = data.Memory;
			if (memory == null)
				return DataError.DataMissing;
			if (memory is Complex complex)
			{
				var count = 0;
				for (; count < complex.Values.Count; count++)
					if (await SearchCollection(complex.Values[count].Value, value, out foundindex))
						break;

				if ((foundindex != -1)
				&& (mode == DataChangeMode.Delete))
				{
					foundindex += count;
					var temp = await ReadFromFileAsync(file) as string[,];
					dimensions = temp.GetLength(1);
					collection = temp.Cast<string>()
									 .ToList();
					for (int i = 0; i < dimensions; i++)
						collection.RemoveAt(foundindex * dimensions);
				}
				else if ((foundindex == -1)
				&& (mode == DataChangeMode.Add))
				{
					var temp = await ReadFromFileAsync(file) as string[,];
					if (temp.GetLength(1) != values.Count)
						return DataError.InvalidDimensions;
					collection = temp.Cast<string>()
									 .ToList();
					foreach (var item in values)
						collection.Add(item);
				}
				else
					return DataError.NameNotFound;
				var result = await WriteToFileAsync(collection, dimensions, filepath);
				if (!(string.IsNullOrEmpty(result)))
					return result;
				await Data.InitAsync<Complex>(data.Name);
			}
			else if (memory is Simple simple)
			{
				await SearchCollection(simple.Value, value, out foundindex);
				if ((foundindex != -1)
				&& (mode == DataChangeMode.Delete))
				{
					collection = (await ReadFromFileAsync(file) as string[]).ToList();
					for (int i = 0; i < dimensions; i++)
						collection.RemoveAt(foundindex * dimensions);
				}
				else if ((foundindex == -1)
				&& (mode == DataChangeMode.Add))
				{
					collection = (await ReadFromFileAsync(file) as string[]).ToList();
					foreach (var item in values)
						collection.Add(item);
				}
				else
					return DataError.NameNotFound;
				var result = await WriteToFileAsync(collection, dimensions, filepath);
				if (!(string.IsNullOrEmpty(result)))
					return result;
				await Data.InitAsync<Simple>(data.Name);
			}
			else if (memory is Subscribers sub)
			{
				if (((foundindex = sub.Subs.FindIndex(s => s.Id.ToString() == value)) != -1)
				&& (mode == DataChangeMode.Delete))
				{
					var temp = await ReadFromFileAsync(file) as string[,];
					dimensions = temp.GetLength(1);
					collection = temp.Cast<string>()
									 .ToList();
					for (int i = 0; i < dimensions; i++)
						collection.RemoveAt(foundindex * dimensions);
				}
				else if ((foundindex == -1)
				&& (mode == DataChangeMode.Add))
				{
					var temp = await ReadFromFileAsync(file) as string[,];
					if (temp.GetLength(1) != values.Count)
						return DataError.InvalidDimensions;
					collection = temp.Cast<string>()
									 .ToList();
					foreach (var item in values)
						collection.Add(item);
				}
				else
					return DataError.NameNotFound;
				var result = await WriteToFileAsync(collection, dimensions, filepath);
				if (!(string.IsNullOrEmpty(result)))
					return result;
				await Data.InitAsync<Subscribers>(data.Name);
			}
			else
				return DataError.Unknown;
			return null;
		}

		private static async Task<string> WriteToFileAsync(List<string> collection, int dimensions, string filepath)
		{
			try
			{
				using (var fs = new FileStream(filepath, FileMode.Create))
				using (var sw = new StreamWriter(fs))
				{
					for (int i = 0; i < collection.Count; i += dimensions)
					{
						for (int j = 0; j < dimensions; j++)
						{
							await sw.WriteAsync(collection[i + j]);
							if (j + 1 != dimensions)
								await sw.WriteAsync(DataSeparator);
						}
						if (i + dimensions != collection.Count)
							await sw.WriteAsync("\n");
					}
				}
			}
			catch
			{
				return DataError.InvalidStream;
			}
			return null;
		}
	}
}