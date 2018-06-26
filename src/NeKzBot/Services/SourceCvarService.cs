#if CVARS
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
#if CVARS
			_dataBase.DropCollection(nameof(SourceCvarService));
			_ = Import("private/resources/hl2.cvars", CvarGameType.HalfLife2);
			_ = Import("private/resources/p1.cvars", CvarGameType.Portal);
			_ = Import("private/resources/p2.cvars", CvarGameType.Portal2);
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
#if CVARS
		[Flags]
		public enum FCVAR
		{
			NONE					= 0,
			UNREGISTERED			= (1<<0),
			DEVELOPMENTONLY			= (1<<1),
			GAMEDLL					= (1<<2),
			CLIENTDLL				= (1<<3),
			HIDDEN					= (1<<4),
			PROTECTED				= (1<<5),
			SPONLY					= (1<<6),
			ARCHIVE					= (1<<7),
			NOTIFY					= (1<<8),
			USERINFO				= (1<<9),
			PRINTABLEONLY			= (1<<10),
			UNLOGGED				= (1<<11),
			NEVER_AS_STRING			= (1<<12),
			REPLICATED				= (1<<13),
			CHEAT					= (1<<14),
			DEMO					= (1<<16),
			DONTRECORD				= (1<<17),
			NOT_CONNECTED			= (1<<22),
			ARCHIVE_XBOX			= (1<<24),
			SERVER_CAN_EXECUTE		= (1<<28),
			SERVER_CANNOT_QUERY		= (1<<29),
			CLIENTCMD_CAN_EXECUTE	= (1<<30)
		}

		private Task Import(string file, CvarGameType type)
		{
			if (!System.IO.File.Exists(file))
				return Task.CompletedTask;

			var db = _dataBase.GetCollection<SourceCvarData>(nameof(SourceCvarService));
			var data = new List<SourceCvarData>();

			var dev = new List<string>();
			var hidden = new List<string>();

			using (var fs = System.IO.File.OpenRead(file))
			using (var sr = new System.IO.StreamReader(fs))
			{
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

					var flags = new List<string>();
					var flagsval = int.Parse(values[2].Trim());
					if ((flagsval & (int)FCVAR.UNREGISTERED) != 0) flags.Add("UNREGISTERED");
					if ((flagsval & (int)FCVAR.DEVELOPMENTONLY) != 0) flags.Add("DEVELOPMENTONLY");
					if ((flagsval & (int)FCVAR.GAMEDLL) != 0) flags.Add("GAMEDLL");
					if ((flagsval & (int)FCVAR.CLIENTDLL) != 0) flags.Add("CLIENTDLL");
					if ((flagsval & (int)FCVAR.HIDDEN) != 0) flags.Add("HIDDEN");
					if ((flagsval & (int)FCVAR.PROTECTED) != 0) flags.Add("PROTECTED");
					if ((flagsval & (int)FCVAR.SPONLY) != 0) flags.Add("SPONLY");
					if ((flagsval & (int)FCVAR.ARCHIVE) != 0) flags.Add("ARCHIVE");
					if ((flagsval & (int)FCVAR.NOTIFY) != 0) flags.Add("NOTIFY");
					if ((flagsval & (int)FCVAR.USERINFO) != 0) flags.Add("USERINFO");
					if ((flagsval & (int)FCVAR.PRINTABLEONLY) != 0) flags.Add("PRINTABLEONLY");
					if ((flagsval & (int)FCVAR.UNLOGGED) != 0) flags.Add("UNLOGGED");
					if ((flagsval & (int)FCVAR.NEVER_AS_STRING) != 0) flags.Add("NEVER_AS_STRING");
					if ((flagsval & (int)FCVAR.REPLICATED) != 0) flags.Add("REPLICATED");
					if ((flagsval & (int)FCVAR.CHEAT) != 0) flags.Add("CHEAT");
					if ((flagsval & (int)FCVAR.DEMO) != 0) flags.Add("DEMO");
					if ((flagsval & (int)FCVAR.DONTRECORD) != 0) flags.Add("DONTRECORD");
					if ((flagsval & (int)FCVAR.NOT_CONNECTED) != 0) flags.Add("NOT_CONNECTED");
					if ((flagsval & (int)FCVAR.ARCHIVE_XBOX) != 0) flags.Add("ARCHIVE_XBOX");
					if ((flagsval & (int)FCVAR.SERVER_CAN_EXECUTE) != 0) flags.Add("SERVER_CAN_EXECUTE");
					if ((flagsval & (int)FCVAR.SERVER_CANNOT_QUERY) != 0) flags.Add("SERVER_CANNOT_QUERY");
					if ((flagsval & (int)FCVAR.CLIENTCMD_CAN_EXECUTE) != 0) flags.Add("CLIENTCMD_CAN_EXECUTE");
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
	}
}