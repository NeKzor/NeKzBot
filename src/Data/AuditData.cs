using System.Collections.Generic;
using LiteDB;

namespace NeKzBot.Data
{
    public class AuditData
    {
        [BsonId(true)]
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public IEnumerable<ulong> AuditIds { get; set; }

        public AuditData()
            => AuditIds = new List<ulong>();
    }
}
