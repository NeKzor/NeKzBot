using LiteDB;

namespace NeKzBot.Data
{
    public class PinMessageData
    {
        [BsonId(true)]
        public ulong MessageId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong GuildId { get; set; }
        public PinUser? Champion { get; set; }
        public System.Collections.Generic.List<PinUser>? Pinners { get; set; }
    }

    public class PinUser
    {
        public ulong UserId { get; set; }
        public string? Name { get; set; }
        public string? Nickname { get; set; }

        public static PinUser Create(Discord.IUser user)
        {
            return new PinUser()
            {
                UserId = user.Id,
                Name = user.Username,
                Nickname = (user as Discord.IGuildUser)?.Nickname,
            };
        }
    }
}
