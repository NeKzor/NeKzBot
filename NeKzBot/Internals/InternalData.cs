using System.Threading.Tasks;

namespace NeKzBot.Internals
{
	// <summary>Used for managing data.</summary>
	public sealed class InternalData
	{
		public string Name { get; }
		public bool ReadingAllowed { get; }
		public bool WrittingAllowed { get; }
		public string FileName { get; }
		public object Data { get; private set; }

		public InternalData(string name, bool reading, bool writing, string filename, object data)
		{
			Name = name;
			ReadingAllowed = reading;
			WrittingAllowed = writing;
			FileName = filename;
			Data = data;
		}

		public Task ChangeData(object data)
			=> Task.FromResult(Data = data);
	}
}