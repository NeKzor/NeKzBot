#if GEN || GEN_MD
using System;
using System.Collections.Generic;
#endif
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.Extensions.Configuration;
using NeKzBot.Data;

namespace NeKzBot.Services
{
	public enum CvarGameType
	{
		HalfLife2,
		Portal,
		Portal2
	}

	public class SourceCvarService
	{
		// Cache data to make things faster
		private ConcurrentDictionary<string, SourceCvarData> _hl2Cache;
		private ConcurrentDictionary<string, SourceCvarData> _p1Cache;
		private ConcurrentDictionary<string, SourceCvarData> _p2Cache;

		private readonly IConfiguration _config;
		private readonly LiteDatabase _dataBase;

		public SourceCvarService(IConfiguration config, LiteDatabase dataBase)
		{
			_config = config;
			_dataBase = dataBase;
		}
		
		public Task Initialize()
		{
#if GEN
			_dataBase.DropCollection(nameof(SourceCvarService));
			_ = Gen("private/resources/hl2-cvars", CvarGameType.HalfLife2);
			_ = Gen("private/resources/p1-cvars", CvarGameType.Portal);
			_ = Gen("private/resources/p2-cvars", CvarGameType.Portal2);
#endif
			_hl2Cache = new ConcurrentDictionary<string, SourceCvarData>();
			_p1Cache = new ConcurrentDictionary<string, SourceCvarData>();
			_p2Cache = new ConcurrentDictionary<string, SourceCvarData>();

			var db = _dataBase
				.GetCollection<SourceCvarData>(nameof(SourceCvarService));

			foreach (var data in db.Find(d => d.Type == CvarGameType.HalfLife2))
				_hl2Cache.TryAdd(data.Name, data);
			foreach (var data in db.Find(d => d.Type == CvarGameType.Portal))
				_p1Cache.TryAdd(data.Name, data);
			foreach (var data in db.Find(d => d.Type == CvarGameType.Portal2))
				_p2Cache.TryAdd(data.Name, data);

#if GEN_MD
			_ = GenMarkdown("hl2", CvarGameType.HalfLife2);
			_ = GenMarkdown("p1", CvarGameType.Portal);
			_ = GenMarkdown("p2", CvarGameType.Portal2);
#endif

			return Task.CompletedTask;
		}

		public Task<SourceCvarData> LookUpCvar(string cvar, CvarGameType type)
		{
			var result = default(SourceCvarData);
			switch (type)
			{
				case CvarGameType.HalfLife2:
					_hl2Cache.TryGetValue(cvar, out result);
					break;
				case CvarGameType.Portal:
					_p1Cache.TryGetValue(cvar, out result);
					break;
				case CvarGameType.Portal2:
					_p2Cache.TryGetValue(cvar, out result);
					break;
			}
			return Task.FromResult(result);
		}
#if GEN
		private Task Gen(string file, CvarGameType type)
		{
			var db = _dataBase.GetCollection<SourceCvarData>(nameof(SourceCvarService));
			var data = new List<SourceCvarData>();

			var dev = new List<string>();
			var hidden = new List<string>();

			using (var fs = System.IO.File.OpenRead(file +  ".txt"))
			using (var sr = new System.IO.StreamReader(fs))
			{
				while (!sr.EndOfStream)
				{
					var line = sr.ReadLine();
					if (line == "[cvar_list]") break;

					var values = line.Split(' ');
					var cvar = values[0];
					var flag = values[1];

					Console.WriteLine(cvar);

					if (flag == "[cvar_dev_hidden]")
					{
						dev.Add(cvar);
						hidden.Add(cvar);
					}
					else if (flag == "[cvar_dev]")
						dev.Add(cvar);
					else if (flag == "[cvar_hidden]")
						hidden.Add(cvar);
					else
						throw new Exception("Invalid cvar flag!");
				}

				var text = sr.ReadToEnd();
				foreach (var cvar in text.Split(new string[] { "[end_of_cvar]" }, StringSplitOptions.RemoveEmptyEntries))
				{
					var values = cvar.Split(new string[] { "[cvar_data]" }, StringSplitOptions.None);
					if (values.Length != 4)
						throw new Exception("Invalid cvar data!");

					var name = values[0].Trim();
					Console.WriteLine($"Name: {name}");

					var defaultvalue = values[1].Trim();
					Console.WriteLine($"Default: {defaultvalue}");

					var flags = values[2].Trim().Split(',').ToList();
					if (flags[0] == string.Empty)
						flags.RemoveAt(0);

					for (int i = 0; i < flags.Count; i++)
						flags[i] = flags[i].Replace("\"", string.Empty).Trim();

					if (dev.Contains(name))
						flags.Add("dev");
					else if (hidden.Contains(name))
						flags.Add("hidden");
					Console.WriteLine($"Flags: {string.Join("/", flags)}");

					var description = values[3].Trim();
					Console.WriteLine($"Description: {description}");
					Console.WriteLine("---------------------------------------------------");

					data.Add(new SourceCvarData()
					{
						Type = type,
						Name = name,
						DefaultValue = defaultvalue,
						Flags = flags.AsEnumerable(),
						HelpText = description
					});
				}
			}

			db.Upsert(data);
			return Task.CompletedTask;
		}
#endif
#if GEN_MD
		internal Task GenMarkdown(string file, CvarGameType type)
		{
			var cache = default(IEnumerable<SourceCvarData>);
			if (type == CvarGameType.HalfLife2)
				cache = _hl2Cache.Values.OrderBy(v => v.Name);
			else if (type == CvarGameType.Portal)
				cache = _p1Cache.Values.OrderBy(v => v.Name);
			else if (type == CvarGameType.Portal2)
				cache = _p2Cache.Values.OrderBy(v => v.Name);

			using (var fs = System.IO.File.OpenWrite(file +  ".md"))
			using (var sw = new System.IO.StreamWriter(fs))
			{
				sw.WriteLine("# " + type.ToString("G"));
				sw.WriteLine("Made with gen, a [NeKzBot project](https://github.com/NeKzor/NeKzBot/tree/master/src/gen).");
				sw.WriteLine(string.Empty);
				sw.WriteLine("| Name | Default | Flags | Help Text |");
				sw.WriteLine("| --- | --- | --- | --- |");
				foreach (var cvar in cache)
				{
					var flags = (cvar.Flags.Any())
						? string.Join("/",cvar.Flags)
						: "-";
					var description = (!string.IsNullOrEmpty(cvar.HelpText))
						? cvar.HelpText.Replace('\n', ' ').Replace('\t', ' ').Replace('\r', ' ')
						: "-";
					
					sw.WriteLine
					(
						$"| {cvar.Name} " +
						$"| {cvar.DefaultValue} " +
						$"| {flags} " +
						$"| {description} |"
					);
				}
			}
			return Task.CompletedTask;
		}
#endif
	}
}