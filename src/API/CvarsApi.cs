using System.Collections.Generic;
using Newtonsoft.Json;

namespace NeKzBot.API
{
    [System.Flags]
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

    [JsonObject]
    public class Cvars
    {
        [JsonProperty("Cvars")]
        public IEnumerable<Cvar>? Data { get; set; }
    }

    public enum OperatingSystem
    {
        Windows,
        Linux,
        Both
    }

    [JsonObject]
    public class Cvar
    {
        [JsonProperty("name")]
        public string? Name { get; set; }
        [JsonProperty("default")]
        public string? DefaultValue { get; set; }
        [JsonProperty("flags")]
        public FCVAR? Flags { get; set; }
        [JsonProperty("system")]
        public OperatingSystem Os { get; set; }
        [JsonProperty("help")]
        public string? HelpText { get; set; }
        /* [JsonProperty("new")]
        public bool IsNew { get; set; } */

        // SAR
        [JsonProperty("games")]
        public List<string>? Games { get; set; }

        public IEnumerable<string> GetFlags()
        {
            var flags = new List<string>();
            if (Flags is null) return flags;

            if (Flags.Value.HasFlag(FCVAR.UNREGISTERED)) flags.Add("UNREGISTERED");
            if (Flags.Value.HasFlag(FCVAR.DEVELOPMENTONLY)) flags.Add("DEVELOPMENTONLY");
            if (Flags.Value.HasFlag(FCVAR.GAMEDLL)) flags.Add("GAMEDLL");
            if (Flags.Value.HasFlag(FCVAR.CLIENTDLL)) flags.Add("CLIENTDLL");
            if (Flags.Value.HasFlag(FCVAR.HIDDEN)) flags.Add("HIDDEN");
            if (Flags.Value.HasFlag(FCVAR.PROTECTED)) flags.Add("PROTECTED");
            if (Flags.Value.HasFlag(FCVAR.SPONLY)) flags.Add("SPONLY");
            if (Flags.Value.HasFlag(FCVAR.ARCHIVE)) flags.Add("ARCHIVE");
            if (Flags.Value.HasFlag(FCVAR.NOTIFY)) flags.Add("NOTIFY");
            if (Flags.Value.HasFlag(FCVAR.USERINFO)) flags.Add("USERINFO");
            if (Flags.Value.HasFlag(FCVAR.PRINTABLEONLY)) flags.Add("PRINTABLEONLY");
            if (Flags.Value.HasFlag(FCVAR.UNLOGGED)) flags.Add("UNLOGGED");
            if (Flags.Value.HasFlag(FCVAR.NEVER_AS_STRING)) flags.Add("NEVER_AS_STRING");
            if (Flags.Value.HasFlag(FCVAR.REPLICATED)) flags.Add("REPLICATED");
            if (Flags.Value.HasFlag(FCVAR.CHEAT)) flags.Add("CHEAT");
            if (Flags.Value.HasFlag(FCVAR.SS)) flags.Add("SS");
            if (Flags.Value.HasFlag(FCVAR.DEMO)) flags.Add("DEMO");
            if (Flags.Value.HasFlag(FCVAR.DONTRECORD)) flags.Add("DONTRECORD");
            if (Flags.Value.HasFlag(FCVAR.SS_ADDED)) flags.Add("SS_ADDED");
            if (Flags.Value.HasFlag(FCVAR.RELEASE)) flags.Add("RELEASE");
            if (Flags.Value.HasFlag(FCVAR.RELOAD_MATERIALS)) flags.Add("RELOAD_MATERIALS");
            if (Flags.Value.HasFlag(FCVAR.RELOAD_TEXTURES)) flags.Add("RELOAD_TEXTURES");
            if (Flags.Value.HasFlag(FCVAR.NOT_CONNECTED)) flags.Add("NOT_CONNECTED");
            if (Flags.Value.HasFlag(FCVAR.MATERIAL_SYSTEM_THREAD)) flags.Add("MATERIAL_SYSTEM_THREAD");
            if (Flags.Value.HasFlag(FCVAR.ARCHIVE_XBOX)) flags.Add("ARCHIVE_XBOX");
            if (Flags.Value.HasFlag(FCVAR.ACCESSIBLE_FROM_THREADS)) flags.Add("ACCESSIBLE_FROM_THREADS");
            if (Flags.Value.HasFlag(FCVAR.NETWORKSYSTEM)) flags.Add("NETWORKSYSTEM");
            if (Flags.Value.HasFlag(FCVAR.VPHYSICS)) flags.Add("VPHYSICS");
            if (Flags.Value.HasFlag(FCVAR.SERVER_CAN_EXECUTE)) flags.Add("SERVER_CAN_EXECUTE");
            if (Flags.Value.HasFlag(FCVAR.SERVER_CANNOT_QUERY)) flags.Add("SERVER_CANNOT_QUERY");
            if (Flags.Value.HasFlag(FCVAR.CLIENTCMD_CAN_EXECUTE)) flags.Add("CLIENTCMD_CAN_EXECUTE");
            return flags;
        }
    }
}
