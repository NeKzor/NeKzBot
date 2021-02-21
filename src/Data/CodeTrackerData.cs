using LiteDB;

namespace NeKzBot.Data
{
    public class CodeTrackerData
    {
        [BsonId(true)]
        public ulong UserId { get; set; }
        public ulong MessageId { get; set; }
        public ulong ReplyId { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
    }
}
