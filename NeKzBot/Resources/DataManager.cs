using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NeKzBot.Classes;
using NeKzBot.Internals;
using NeKzBot.Server;
using NeKzBot.Webhooks;

namespace NeKzBot.Resources
{
	public static partial class Data
	{
		public static List<object> Manager { get; private set; }

		public static Task<T> Get<T>(string name)
			where T : class, new()
		{
			var index = Manager.FindIndex(data => PatternMatching(data) == name);
			return Task.FromResult((index != -1)
										  ? (T)((InternalData<T>)Manager[index]).Memory
										  : null);
		}

		public static Task<IData> Get(string name)
		{
			var index = Manager.FindIndex(data => PatternMatching(data) == name);
			return Task.FromResult((index != -1)
										  ? (IData)Manager[index]
										  : null);
		}

		public static async Task InitAsync<T>(string name)
			where T : class, new()
		{
			await ((InternalData<T>)Manager[Manager.FindIndex(data => PatternMatching(data) == name)]).Init();
		}

		public static Task<List<string>> GetNames()
			=> Task.FromResult(Manager.Select(data => PatternMatching(data))
									  .ToList());

		private static Func<object, string> PatternMatching;

		public static async Task InitMangerAsync()
		{
			await Logger.SendAsync("Initializing Data Manger", LogColor.Init);
			PatternMatching = obj =>
			{
				if (obj is InternalData<Simple> simple)
					return simple.Name;
				if (obj is InternalData<Complex> complex)
					return complex.Name;
				if (obj is InternalData<Subscribers> subs)
					return subs.Name;
				if (obj is InternalData<Portal2Maps> p2map)
					return p2map.Name;
				else
					return null;
			};
			Manager = new List<object>
			{
				new InternalData<Simple>("cc", true, true, "consolecmds.dat"),
				new InternalData<Simple>("aa", false, false, "audioaliases.dat"),
				new InternalData<Simple>("games", true, true, "playingstatus.dat"),
				new InternalData<Simple>("credits", true, true, "credits.dat"),
				new InternalData<Simple>("streamers", true, true, "streamers.dat"),
				new InternalData<Simple>("vips", true, true, "vip.dat"),
				new InternalData<Complex>("scripts", false, false, "scripts.dat"),
				new InternalData<Complex>("memes", true, true, "memes.dat"),
				new InternalData<Complex>("tools", true, true, "tools.dat"),
				new InternalData<Complex>("links", true, true, "links.dat"),
				new InternalData<Complex>("projects", true, true, "runs.dat"),
				new InternalData<Complex>("quotes", true, true, "quotes.dat"),
				new InternalData<Complex>("sounds", false, false, "sounds.dat"),
				new InternalData<Complex>("exploits", true, true, "exploits.dat"),
				new InternalData<Complex>("p2cvars", true, true, "p2cvars.dat"),
				new InternalData<Subscribers>("p2hook", true, true, "p2subs.dat", Parsers.WebhookDataParser),
				new InternalData<Subscribers>("srcomsourcehook", true, true, "srsourcesubs.dat", Parsers.WebhookDataParser),
				new InternalData<Subscribers>("twtvhook", true, true, "twtvsubs.dat", Parsers.WebhookDataParser),
				new InternalData<Subscribers>("srcomportal2hook", true, true, "srportal2subs.dat", Parsers.WebhookDataParser),
				new InternalData<Portal2Maps>("p2maps", true, false, "p2maps.dat", Parsers.Portal2MapListParser)
			};
		}
	}

	public enum DataChangeMode
	{
		Add,
		Delete
	}
}