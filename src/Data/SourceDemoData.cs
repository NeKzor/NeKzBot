using System;
using LiteDB;
using SourceDemoParser;

namespace NeKzBot.Data
{
    public class SourceDemoData
    {
        [BsonId(true)]
        public int Id { get; set; }
        public ulong UserId { get; set; }
        public string DownloadUrl { get; set; }
        public SourceDemo Demo { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
