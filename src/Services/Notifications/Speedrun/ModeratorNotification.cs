using NeKzBot.API;
using NeKzBot.Extensions;

namespace NeKzBot.Services.Notifications.Speedrun
{
    internal sealed class ModeratorNotification : SpeedrunNotificationType
    {
        public ModeratorNotification()
            : base(user: 0, game: 2, data: -1)
        {
        }

        public override string Description(SpeedrunNotification? _)
        {
            return $"{Author?.ToRawText()} is now a moderator! :heart:"; ;
        }
    }
}
