using NeKzBot.API;
using NeKzBot.Extensions;

namespace NeKzBot.Services.Notifications.Speedrun
{
    internal sealed class PostNotification : SpeedrunNotificationType
    {
        public PostNotification()
            : base(user: 0, game: 4, data: 2)
        {
        }

        public override string Description(SpeedrunNotification? nf)
        {
            return $"**Form Post**\n*[{Data?.ToRawText()}]({(nf?.Item?.Uri ?? string.Empty).ToRawText()})*";
        }
    }
}
