//#define CVARS
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
        Portal2,
        TheBeginnersGuide,
        TheStanleyParable
    }

    public class SourceCvarService
    {
        // Cache data to make things faster
        private ConcurrentDictionary<string, SourceCvarData> _hl2Cache;
        private ConcurrentDictionary<string, SourceCvarData> _p1Cache;
        private ConcurrentDictionary<string, SourceCvarData> _p2Cache;
        private ConcurrentDictionary<string, SourceCvarData> _tbgCache;
        private ConcurrentDictionary<string, SourceCvarData> _tspCache;

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
			_ = Import("hl2_windows.cvars", "hl2_linux.cvars", CvarGameType.HalfLife2);
            _ = Import("p1_windows.cvars", "p1_linux.cvars", CvarGameType.Portal);
            _ = Import("p2_windows.cvars", "p2_linux.cvars", CvarGameType.Portal2);
            _ = Import("tbg_windows.cvars", "tbg_linux.cvars", CvarGameType.TheBeginnersGuide);
            _ = Import("tsp_windows.cvars", "tsp_linux.cvars", CvarGameType.TheStanleyParable);
#endif
            _hl2Cache = new ConcurrentDictionary<string, SourceCvarData>();
            _p1Cache = new ConcurrentDictionary<string, SourceCvarData>();
            _p2Cache = new ConcurrentDictionary<string, SourceCvarData>();
            _tbgCache = new ConcurrentDictionary<string, SourceCvarData>();
            _tspCache = new ConcurrentDictionary<string, SourceCvarData>();

            var db = _dataBase
                .GetCollection<SourceCvarData>(nameof(SourceCvarService));

            foreach (var data in db.Find(d => d.Type == CvarGameType.HalfLife2))
                _hl2Cache.TryAdd(data.Name, data);
            foreach (var data in db.Find(d => d.Type == CvarGameType.Portal))
                _p1Cache.TryAdd(data.Name, data);
            foreach (var data in db.Find(d => d.Type == CvarGameType.Portal2))
                _p2Cache.TryAdd(data.Name, data);
            foreach (var data in db.Find(d => d.Type == CvarGameType.TheBeginnersGuide))
                _tbgCache.TryAdd(data.Name, data);
            foreach (var data in db.Find(d => d.Type == CvarGameType.TheStanleyParable))
                _tspCache.TryAdd(data.Name, data);

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
                case CvarGameType.TheBeginnersGuide:
                    _tbgCache.TryGetValue(cvar, out result);
                    break;
                case CvarGameType.TheStanleyParable:
                    _tspCache.TryGetValue(cvar, out result);
                    break;
            }
            return Task.FromResult(result);
        }
#if CVARS
        [Flags]
        public enum FCVAR
        {
            NONE = 0,
            UNREGISTERED = (1 << 0),
            DEVELOPMENTONLY = (1 << 1),
            GAMEDLL = (1 << 2),
            CLIENTDLL = (1 << 3),
            HIDDEN = (1 << 4),
            PROTECTED = (1 << 5),
            SPONLY = (1 << 6),
            ARCHIVE = (1 << 7),
            NOTIFY = (1 << 8),
            USERINFO = (1 << 9),
            PRINTABLEONLY = (1 << 10),
            UNLOGGED = (1 << 11),
            NEVER_AS_STRING = (1 << 12),
            REPLICATED = (1 << 13),
            CHEAT = (1 << 14),
            SS = (1 << 14),
            DEMO = (1 << 16),
            DONTRECORD = (1 << 17),
            SS_ADDED = (1 << 18),
            RELEASE = (1 << 19),
            RELOAD_MATERIALS = (1 << 20),
            RELOAD_TEXTURES = (1 << 21),
            NOT_CONNECTED = (1 << 22),
            MATERIAL_SYSTEM_THREAD = (1 << 23),
            ARCHIVE_XBOX = (1 << 24),
            ACCESSIBLE_FROM_THREADS = (1 << 25),
            NETWORKSYSTEM = (1 << 26),
            VPHYSICS = (1 << 27),
            SERVER_CAN_EXECUTE = (1 << 28),
            SERVER_CANNOT_QUERY = (1 << 29),
            CLIENTCMD_CAN_EXECUTE = (1 << 30)
        }

        private Task Import(string fileWin, string fileLin, CvarGameType type)
        {
            var path = $"private/resources/cvars/";
            fileWin = path + fileWin;
            fileLin = path + fileLin;

            if (!System.IO.File.Exists(fileWin) || !System.IO.File.Exists(fileLin))
                return Task.CompletedTask;

            // Local function
            List<SourceCvarData> ReadFile(string file)
            {
                var result = new List<SourceCvarData>();
                using (var fs = System.IO.File.OpenRead(file))
                using (var sr = new System.IO.StreamReader(fs))
                {
                    var text = sr.ReadToEnd();
                    foreach (var cvar in text.Split(new string[] { "[end_of_cvar]" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var values = cvar.Split(new string[] { "[cvar_data]" }, StringSplitOptions.None);
                        if (values.Length != 4)
                            break;

                        var flags = new List<string>();
                        var flagsval = (FCVAR)int.Parse(values[2]);
                        if (flagsval.HasFlag(FCVAR.UNREGISTERED)) flags.Add("UNREGISTERED");
                        if (flagsval.HasFlag(FCVAR.DEVELOPMENTONLY)) flags.Add("DEVELOPMENTONLY");
                        if (flagsval.HasFlag(FCVAR.GAMEDLL)) flags.Add("GAMEDLL");
                        if (flagsval.HasFlag(FCVAR.CLIENTDLL)) flags.Add("CLIENTDLL");
                        if (flagsval.HasFlag(FCVAR.HIDDEN)) flags.Add("HIDDEN");
                        if (flagsval.HasFlag(FCVAR.PROTECTED)) flags.Add("PROTECTED");
                        if (flagsval.HasFlag(FCVAR.SPONLY)) flags.Add("SPONLY");
                        if (flagsval.HasFlag(FCVAR.ARCHIVE)) flags.Add("ARCHIVE");
                        if (flagsval.HasFlag(FCVAR.NOTIFY)) flags.Add("NOTIFY");
                        if (flagsval.HasFlag(FCVAR.USERINFO)) flags.Add("USERINFO");
                        if (flagsval.HasFlag(FCVAR.PRINTABLEONLY)) flags.Add("PRINTABLEONLY");
                        if (flagsval.HasFlag(FCVAR.UNLOGGED)) flags.Add("UNLOGGED");
                        if (flagsval.HasFlag(FCVAR.NEVER_AS_STRING)) flags.Add("NEVER_AS_STRING");
                        if (flagsval.HasFlag(FCVAR.REPLICATED)) flags.Add("REPLICATED");
                        if (flagsval.HasFlag(FCVAR.CHEAT)) flags.Add("CHEAT");
                        if (flagsval.HasFlag(FCVAR.SS)) flags.Add("SS");
                        if (flagsval.HasFlag(FCVAR.DEMO)) flags.Add("DEMO");
                        if (flagsval.HasFlag(FCVAR.DONTRECORD)) flags.Add("DONTRECORD");
                        if (flagsval.HasFlag(FCVAR.SS_ADDED)) flags.Add("SS_ADDED");
                        if (flagsval.HasFlag(FCVAR.RELEASE)) flags.Add("RELEASE");
                        if (flagsval.HasFlag(FCVAR.RELOAD_MATERIALS)) flags.Add("RELOAD_MATERIALS");
                        if (flagsval.HasFlag(FCVAR.RELOAD_TEXTURES)) flags.Add("RELOAD_TEXTURES");
                        if (flagsval.HasFlag(FCVAR.NOT_CONNECTED)) flags.Add("NOT_CONNECTED");
                        if (flagsval.HasFlag(FCVAR.MATERIAL_SYSTEM_THREAD)) flags.Add("MATERIAL_SYSTEM_THREAD");
                        if (flagsval.HasFlag(FCVAR.ARCHIVE_XBOX)) flags.Add("ARCHIVE_XBOX");
                        if (flagsval.HasFlag(FCVAR.ACCESSIBLE_FROM_THREADS)) flags.Add("ACCESSIBLE_FROM_THREADS");
                        if (flagsval.HasFlag(FCVAR.NETWORKSYSTEM)) flags.Add("NETWORKSYSTEM");
                        if (flagsval.HasFlag(FCVAR.VPHYSICS)) flags.Add("VPHYSICS");
                        if (flagsval.HasFlag(FCVAR.SERVER_CAN_EXECUTE)) flags.Add("SERVER_CAN_EXECUTE");
                        if (flagsval.HasFlag(FCVAR.SERVER_CANNOT_QUERY)) flags.Add("SERVER_CANNOT_QUERY");
                        if (flagsval.HasFlag(FCVAR.CLIENTCMD_CAN_EXECUTE)) flags.Add("CLIENTCMD_CAN_EXECUTE");

                        result.Add(new SourceCvarData()
                        {
                            Name = values[0],
                            DefaultValue = values[1],
                            FlagsValue = (int)flagsval,
                            Flags = flags.AsEnumerable(),
                            HelpText = values[3],
                            Os = OperatingSystem.Both,
                            Type = type
                        });
                    }
                }
                return result;
            }

            var cvars = ReadFile(fileWin);
            var cvars2 = ReadFile(fileLin);

            var unique = new List<SourceCvarData>();
            foreach (var match in cvars2)
            {
                if (cvars.FirstOrDefault(c => c.Name == match.Name) == null)
                {
                    Console.WriteLine($"[Linux] {match.Name}");
                    match.Os = OperatingSystem.Linux;
                    unique.Add(match);
                }
            }
            foreach (var match in cvars)
            {
                if (cvars2.FirstOrDefault(c => c.Name == match.Name) == null)
                {
                    Console.WriteLine($"[Windows] {match.Name}");
                    match.Os = OperatingSystem.Windows;
                }
            }

            cvars.AddRange(unique);
            Console.WriteLine($"Merged {unique.Count} cvars.");

            _dataBase
                .GetCollection<SourceCvarData>(nameof(SourceCvarService))
                .Upsert(cvars);

            return Task.CompletedTask;
        }
#endif
    }
}
