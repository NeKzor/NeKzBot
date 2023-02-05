using NeKzBot.API;
using NeKzBot.Extensions;

namespace NeKzBot.Services.Notifications.Speedrun
{
    internal sealed class ModeratorNotification : SpeedrunNotificationType
    {
        // Example: FatPerson115 has been added to Portal 2 as a moderator
        _splitPattern = "^([\\w]+)( has been added to )([\\w -]+)( as a moderator)$";

        public ModeratorNotification()
            : base(user: 0, game: 2, data: -1)
        {
        }

        public override string Description(SpeedrunNotification? _)
        {
            return $"{Author?.ToRawText()} is now a moderator! :heart:";
        }
    }
}
