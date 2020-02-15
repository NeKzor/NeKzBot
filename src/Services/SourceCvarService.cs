using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NeKzBot.API;

namespace NeKzBot.Services
{
    public enum CvarGameType
    {
        HalfLife2,
        Portal,
        Portal2,
        SAR
    }

    public class SourceCvarService
    {
        // Cache data to make things faster
        private ConcurrentDictionary<string, Cvar>? _hl2Cache;
        private ConcurrentDictionary<string, Cvar>? _p1Cache;
        private ConcurrentDictionary<string, Cvar>? _p2Cache;
        private ConcurrentDictionary<string, Cvar>? _sarCache;

        private readonly IConfiguration _config;

        public SourceCvarService(IConfiguration config)
        {
            _config = config;
        }

        public async Task Initialize()
        {
            _hl2Cache = new ConcurrentDictionary<string, Cvar>();
            _p1Cache = new ConcurrentDictionary<string, Cvar>();
            _p2Cache = new ConcurrentDictionary<string, Cvar>();
            _sarCache = new ConcurrentDictionary<string, Cvar>();

            foreach (var data in await ImportAsync("half-life-2.json", CvarGameType.HalfLife2) ?? Enumerable.Empty<Cvar>())
                _hl2Cache.TryAdd(data.Name!.ToLower(), data);
            foreach (var data in await ImportAsync("portal.json", CvarGameType.Portal) ?? Enumerable.Empty<Cvar>())
                _p1Cache.TryAdd(data.Name!.ToLower(), data);
            foreach (var data in await ImportAsync("portal-2.json", CvarGameType.Portal2) ?? Enumerable.Empty<Cvar>())
                _p2Cache.TryAdd(data.Name!.ToLower(), data);
            foreach (var data in await ImportAsync("sar.json", CvarGameType.SAR) ?? Enumerable.Empty<Cvar>())
                _sarCache.TryAdd(data.Name!.ToLower(), data);
        }

        public Task<Cvar?> LookUpCvar(string cvar, CvarGameType type)
        {
            if (_hl2Cache is null || _p1Cache is null || _p2Cache is null || _sarCache is null)
                throw new System.Exception("Service is not initialized");

            var result = default(Cvar);
            switch (type)
            {
                case CvarGameType.HalfLife2:
                    _hl2Cache.TryGetValue(cvar.ToLower(), out result);
                    break;
                case CvarGameType.Portal:
                    _p1Cache.TryGetValue(cvar.ToLower(), out result);
                    break;
                case CvarGameType.Portal2:
                    _p2Cache.TryGetValue(cvar.ToLower(), out result);
                    break;
                case CvarGameType.SAR:
                    _sarCache.TryGetValue(cvar.ToLower(), out result);
                    break;
            }

            return Task.FromResult(result);
        }

        private async Task<IEnumerable<Cvar>?> ImportAsync(string file, CvarGameType type)
        {
            var path = "private/resources/cvars/" + file;

            if (!System.IO.File.Exists(path))
            {
                if (type == CvarGameType.SAR) return Enumerable.Empty<Cvar>();

                using var client = new CvarsApiClient(_config["user_agent"]);
                var cvars = await client.GetCvarsAsync(file);

                if (cvars is null)
                    throw new System.Exception($"Failed to fetch {file} cvars.");

                File.WriteAllText(path, JsonConvert.SerializeObject(cvars));
                return cvars.Data;
            }

            return JsonConvert.DeserializeObject<Cvars>(File.ReadAllText(path)).Data;
        }
    }
}
