using LiteDB;

namespace NeKzBot.Data
{
    public class PinMessageData
    {
        [BsonId(true)]
        public ulong MessageId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong GuildId { get; set; }
    }
}
