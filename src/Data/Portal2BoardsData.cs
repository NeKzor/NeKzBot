using System.Collections.Generic;
using LiteDB;

namespace NeKzBot.Data
{
    public class Portal2BoardsData
    {
        [BsonId(true)]
        public int Id { get; set; }
        public IEnumerable<uint> EntryIds { get; set; }

        public Portal2BoardsData()
            => EntryIds = new List<uint>();
    }
}
