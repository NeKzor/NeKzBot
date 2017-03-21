namespace NeKzBot.Resources
{
	public interface IData
	{
		string Name { get; }
		bool ReadingAllowed { get; }
		bool WrittingAllowed { get; }
		bool ReadWriteAllowed { get; }
		string FileName { get; }
		object Memory { get; }
	}
}