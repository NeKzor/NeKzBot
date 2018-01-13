#if GEN
using System;
using System.Collections.Generic;
#endif
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
			_ = Gen("hl2-cvars.txt", CvarGameType.HalfLife2);
			_ = Gen("p1-cvars.txt", CvarGameType.Portal);
			_ = Gen("p2-cvars.txt", CvarGameType.Portal2);
#endif
			return Task.CompletedTask;
		}

		public Task<SourceCvarData> LookUpCvar(string cvar, CvarGameType type)
		{
			var db = _dataBase.GetCollection<SourceCvarData>(nameof(SourceCvarService));
			var data = db.Find(d => d.Type == type);
			return Task.FromResult(data.FirstOrDefault(c => c.Name == cvar));
		}
#if GEN
		private Task Gen(string file, CvarGameType type)
		{
			var db = _dataBase.GetCollection<SourceCvarData>(nameof(SourceCvarService));
			var data = new List<SourceCvarData>();

			var dev = new List<string>();
			var hidden = new List<string>();

			using (var fs = System.IO.File.OpenRead(file))
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

					data.Add(new SourceCvarData
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
	}
}