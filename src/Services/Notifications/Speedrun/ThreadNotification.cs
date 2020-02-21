using NeKzBot.API;
using NeKzBot.Extensions;

namespace NeKzBot.Services.Notifications.Speedrun
{
    internal sealed class ThreadNotification : SpeedrunNotificationType
    {
        public ThreadNotification()
            : base(user: 0, game: 2, data: 4)
        {
        }

        public override string Description(SpeedrunNotification? nf)
        {
            return $"**Thread Response**\n*[{Data?.ToRawText()}]({(nf?.Item?.Uri ?? string.Empty).ToRawText()})*";
        }
    }
}
