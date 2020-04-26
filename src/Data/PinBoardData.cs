using LiteDB;

namespace NeKzBot.Data
{
    public class PinBoardData
    {
        [BsonId(true)]
        public int Id { get; set; }
        public ulong WebhookId { get; set; }
        public string? WebhookToken { get; set; }
        public ulong GuildId { get; set; }
        public uint MinimumReactions { get; set; }
        public string? PinEmoji { get; set; }
    }
}
