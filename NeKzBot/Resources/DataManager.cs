using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NeKzBot.Internals;
using NeKzBot.Internals.Entities;
using NeKzBot.Server;

namespace NeKzBot.Resources
{
	public static partial class Data
	{
		public static List<object> Manager { get; private set; }

		public static Task<T> Get<T>(string name)
			where T : class, IMemory, new()
		{
			var index = Manager.FindIndex(data => (data as IData).Name == name);
			return Task.FromResult((index != -1)
										  ? (T)((InternalData<T>)Manager[index]).Memory
										  : default(T));
		}
		public static Task<IData> Get(string name)
		{
			var index = Manager.FindIndex(data => (data as IData).Name == name);
			return Task.FromResult((index != -1)
										  ? (IData)Manager[index]
										  : default(IData));
		}
		public static async Task InitAsync<T>(string name)
			where T : class, IMemory, new()
		{
			await ((InternalData<T>)Manager[Manager.FindIndex(data => (data as IData).Name == name)])?.InitAsync();
		}
		public static async Task ChangeAsync<T>(string name, object newdata)
			where T : class, IMemory, new()
		{
			await ((InternalData<T>)Manager[Manager.FindIndex(data => (data as IData).Name == name)])?.Change(newdata);
		}
		public static async Task<bool> ExportAsync<T>(string name)
			where T : class, IMemory, new()
		{
			return await ((InternalData<T>)Manager[Manager.FindIndex(data => (data as IData).Name == name)])?.ExportAsync();
		}

		public static Task<IEnumerable<string>> GetNames()
			=> Task.FromResult(Manager.Select(data => (data as IData).Name));

		public static async Task InitMangerAsync()
		{
			await Logger.SendAsync("Initializing Data Manger", LogColor.Init);
			Manager = new List<object>
			{
				new InternalData<Simple>("cc", true, true, "consolecmds"),
				new InternalData<Simple>("aa", true, false, "audioaliases"),
				new InternalData<Simple>("games", true, true, "playingstatus"),
				new InternalData<Simple>("credits", true, true, "credits"),
				new InternalData<Simple>("streamers", true, true, "streamers"),
				new InternalData<Simple>("vips", true, true, "vip"),
				new InternalData<Complex>("scripts", true, true, "scripts"),
				new InternalData<Complex>("memes", true, true, "memes"),
				new InternalData<Complex>("tools", true, true, "tools"),
				new InternalData<Complex>("links", true, true, "links"),
				new InternalData<Complex>("projects", true, true, "runs"),
				new InternalData<Complex>("quotes", true, true, "quotes"),
				new InternalData<Complex>("sounds", true, true, "sounds"),
				new InternalData<Complex>("exploits", true, true, "exploits"),
				new InternalData<Complex>("p2cvars", true, true, "p2cvars"),
				new InternalData<Subscription>("p2hook", true, true, "p2subs"),
				new InternalData<Subscription>("srcomsourcehook", true, true, "srsourcesubs"),
				new InternalData<Subscription>("twtvhook", true, true, "twtvsubs"),
				new InternalData<Subscription>("srcomportal2hook", true, true, "srportal2subs"),
				new InternalData<Portal2Maps>("p2maps", true, false, "p2maps"),
				new InternalData<Submissions>("contest", true, true, "contest")
			};
		}
	}

	public enum DataChangeMode
	{
		Add,
		Delete
	}
}