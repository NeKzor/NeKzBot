using System.Collections.Generic;
using LiteDB;
using NeKzBot.API;

namespace NeKzBot.Data
{
    public class SpeedrunCacheData
    {
        [BsonId(true)]
        public int Id { get; set; }
        public IEnumerable<SpeedrunNotification> Notifications { get; set; }

        public SpeedrunCacheData()
            => Notifications = new List<SpeedrunNotification>();
    }
}
