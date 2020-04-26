using LiteDB;

namespace NeKzBot.Data
{
    public class PinMessageData
    {
        [BsonId(true)]
        public ulong Id { get; set; }
        public ulong ChannelId { get; set; }
        public ulong GuildId { get; set; }
    }
}
