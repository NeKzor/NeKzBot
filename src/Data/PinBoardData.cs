using LiteDB;

namespace NeKzBot.Data
{
    public class PinBoardData
    {
        [BsonId(true)]
        public ulong GuildId { get; set; }
        public ulong WebhookId { get; set; }
        public string? WebhookToken { get; set; }
        public uint MinimumReactions { get; set; }
        public uint DaysUntilMessageExpires { get; set; }
        public string? PinEmoji { get; set; }
        public System.DateTimeOffset CreatedAt { get; set; }
    }
}
