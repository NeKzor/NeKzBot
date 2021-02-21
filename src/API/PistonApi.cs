using System.Collections.Generic;
using Newtonsoft.Json;

namespace NeKzBot.API
{
    [JsonObject]
    public class PistonVersion
    {
        [JsonProperty("name")]
        public string? Name { get; set; }
        [JsonProperty("aliases")]
        public List<string>? Aliases { get; set; }
        [JsonProperty("version")]
        public string? Version { get; set; }
    }

    [JsonObject]
    public class PistonCode
    {
        [JsonProperty("language")]
        public string? Language { get; set; }
        [JsonProperty("source")]
        public string? Source { get; set; }
        [JsonProperty("stdin")]
        public string? StdIn { get; set; }
        [JsonProperty("args")]
        public List<string>? Args { get; set; }
    }

    [JsonObject]
    public class PistonResult
    {
        // 200: OK
        [JsonProperty("ran")]
        public bool? Ran { get; set; }
        [JsonProperty("language")]
        public string? Language { get; set; }
        [JsonProperty("version")]
        public string? Version { get; set; }
        [JsonProperty("output")]
        public string? Output { get; set; }
        [JsonProperty("stdout")]
        public string? StdOut { get; set; }
        [JsonProperty("stderr")]
        public string? StdErr { get; set; }

        // 400: Bad Request
        [JsonProperty("message")]
        public string? Message { get; set; }
    }
}
