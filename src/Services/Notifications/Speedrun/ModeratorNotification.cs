using NeKzBot.API;
using NeKzBot.Extensions;

namespace NeKzBot.Services.Notifications.Speedrun
{
    internal sealed class ModeratorNotification : SpeedrunNotificationType
    {
        public ModeratorNotification()
            : base(user: 0, game: 2, data: -1)
        {
            // Example: FatPerson115 has been added to Portal 2 as a moderator
            _splitPattern = "^([\\w]+)( has been added to )([\\w -]+)( as a moderator)$";
        }

        public override string Description(SpeedrunNotification? _)
        {
            return $"{Author?.ToRawText()} is now a moderator! :heart:";
        }
    }
}
