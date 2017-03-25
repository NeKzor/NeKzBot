using System.Threading.Tasks;
using NeKzBot.Resources;
using NeKzBot.Utilities;

namespace NeKzBot.Internals
{
	public sealed class InternalData<T> : IData
		where T : class, new()
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

		public InternalData(string name, bool reading, bool writing, string filename, bool initnow = true)
		{
			Name = name;
			ReadingAllowed = reading;
			WrittingAllowed = writing;
			FileName = filename;
			Memory = new T();

			if (initnow)
				InitAsync().GetAwaiter().GetResult();
		}

		public async Task InitAsync()
			=> Memory = await Utils.ReadJson<T>(FileName + ".json");
		public Task Get()
			=> Task.FromResult(Memory as T);
		public Task Change(object data)
			=> Task.FromResult(Memory = data);
		public async Task<bool> ExportAsync()
			=> await Utils.WriteJson(Memory, FileName + ".json");
	}
}