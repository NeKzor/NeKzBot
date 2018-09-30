using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace NeKzBot.Services
{
    public class Portal2Campaign
    {
        [JsonProperty("map_list")]
        public List<Portal2CampaignMap> MapList { get; set; }
    }
    public class Portal2CampaignMap
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("cm_name")]
        public string ChallengeModeName { get; set; }
        [JsonProperty("best_time_id")]
        public string BestTimeId { get; set; }
        [JsonProperty("best_portals_id")]
        public string BestPortalsId { get; set; }
        [JsonProperty("three_letter_code")]
        public string ThreeLetterCode { get; set; }
        [JsonProperty("elevator_timing")]
        public string ElevatorTiming { get; set; }
        [JsonProperty("map_filter")]
        public int MapFilter { get; set; }
    }

    public class Portal2CampaignService
    {
        private Portal2Campaign _campaign;

        private readonly IConfiguration _config;

        public Portal2CampaignService(IConfiguration config)
        {
            _config = config;
        }

        public Task Initialize()
        {
            var file = "private/resources/portal2campaign.json";
            if (File.Exists(file))
                _campaign = JsonConvert.DeserializeObject<Portal2Campaign>(File.ReadAllText(file));

            return Task.CompletedTask;
        }

        public Portal2CampaignMap GetMap(string name)
        {
            return _campaign.MapList.FirstOrDefault(map =>
            {
                return map.Name.Equals(name, System.StringComparison.CurrentCultureIgnoreCase)
                    || map.ChallengeModeName.Equals(name, System.StringComparison.CurrentCultureIgnoreCase)
                    || map.ThreeLetterCode.Equals(name, System.StringComparison.CurrentCultureIgnoreCase);
            });
        }
    }
}
