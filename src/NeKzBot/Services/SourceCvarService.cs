using System;
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
			return Task.CompletedTask;
		}

		public Task<SourceCvarData> LookUpCvar(string cvar, CvarGameType type = CvarGameType.Portal2)
		{
			var db = _dataBase.GetCollection<SourceCvarData>();
			var data = db.FindOne(d => string.Equals(d.Cvar, cvar, StringComparison.CurrentCultureIgnoreCase));
			return Task.FromResult(data);
		}

		public Task Generate()
		{
			var db = _dataBase.GetCollection<SourceCvarData>();
			var data = new System.Collections.Generic.List<SourceCvarData>();
			using (var fs = System.IO.File.OpenRead("cvars-p2.txt"))
			using (var sr = new System.IO.StreamReader(fs))
			{
				while (!sr.EndOfStream)
				{
					var line = sr.ReadLine();
					var values = line.Split('|');
					data.Add(new SourceCvarData
					{
						Cvar = values[0],
						Description = values[1],
						Flags = values.Skip(2).Take(5),
						Type = CvarGameType.Portal2
					});
				}
			}
			db.Upsert(data);
			return Task.CompletedTask;
		}
	}
}