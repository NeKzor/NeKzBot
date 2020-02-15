using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NeKzBot.Services
{
    public class Portal2Campaign
    {
        [JsonProperty("map_list")]
        public List<Portal2CampaignMap> MapList { get; set; }
        [JsonIgnore]
        public List<Discovery> Discoveries { get; set; }

        public Portal2Campaign()
        {
            MapList = new List<Portal2CampaignMap>();
            Discoveries = new List<Discovery>();
        }
    }
    public class Portal2CampaignMap
    {
        [JsonProperty("name")]
        public string? Name { get; set; }
        [JsonProperty("cm_name")]
        public string? ChallengeModeName { get; set; }
        [JsonProperty("best_time_id")]
        public string? BestTimeId { get; set; }
        [JsonProperty("best_portals_id")]
        public string? BestPortalsId { get; set; }
        [JsonProperty("three_letter_code")]
        public string? ThreeLetterCode { get; set; }
        [JsonProperty("elevator_timing")]
        public string? ElevatorTiming { get; set; }
        [JsonProperty("map_filter")]
        public int MapFilter { get; set; }
    }
    public class Discovery
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Category { get; set; }
        public string? Status { get; set; }
        public string? Showcase { get; set; }
    }

    public class Portal2CampaignService
    {
        private Portal2Campaign? _campaign;

        public Task Initialize()
        {
            var file = "public/resources/portal2campaign.json";
            if (File.Exists(file))
                _campaign = JsonConvert.DeserializeObject<Portal2Campaign>(File.ReadAllText(file));

            file = "public/resources/portal2exploits.csv";
            if (File.Exists(file) && _campaign != null)
            {
                using var fs = File.OpenRead(file);
                using var sr = new StreamReader(fs);

                sr.ReadLine();
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (line is null) break;

                    var values = line.Split(',');
                    _campaign.Discoveries.Add(new Discovery()
                    {
                        Name = values[0],
                        Type = values[1],
                        Category = values[2],
                        Status = values[3],
                        Showcase = values[4]
                    });
                }
            }

            return Task.CompletedTask;
        }

        public Portal2CampaignMap GetMap(string name)
        {
            if (_campaign is null)
                throw new System.Exception("Service is not initialized");

            return _campaign.MapList.FirstOrDefault(map =>
            {
                return (map.Name ?? string.Empty).Equals(name, System.StringComparison.CurrentCultureIgnoreCase)
                    || (map.ChallengeModeName ?? string.Empty).Equals(name, System.StringComparison.CurrentCultureIgnoreCase)
                    || (map.ThreeLetterCode ?? string.Empty).Equals(name, System.StringComparison.CurrentCultureIgnoreCase);
            });
        }
        public Discovery GetDiscovery(string name)
        {
            if (_campaign is null)
                throw new System.Exception("Service is not initialized");

            return _campaign.Discoveries.FirstOrDefault(discovery =>
            {
                return (discovery.Name ?? string.Empty).Equals(name, System.StringComparison.CurrentCultureIgnoreCase)
                    || (discovery.Name ?? string.Empty).StartsWith(name, System.StringComparison.CurrentCultureIgnoreCase)
                    || (discovery.Name ?? string.Empty).EndsWith(name, System.StringComparison.CurrentCultureIgnoreCase)
                    || (discovery.Name ?? string.Empty).IndexOf(name, System.StringComparison.CurrentCultureIgnoreCase) >= 0;
            });
        }

        public Portal2CampaignMap GetRandomMap()
        {
            if (_campaign is null)
                throw new System.Exception("Service is not initialized");

            var rand = new System.Random();
            return _campaign.MapList.ElementAt(rand.Next(0, _campaign.MapList.Count));
        }
        public Discovery GetRandomDiscovery()
        {
            if (_campaign is null)
                throw new System.Exception("Service is not initialized");

            var rand = new System.Random();
            return _campaign.Discoveries.ElementAt(rand.Next(0, _campaign.Discoveries.Count));
        }
    }
}
