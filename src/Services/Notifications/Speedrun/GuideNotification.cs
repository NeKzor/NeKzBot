using NeKzBot.API;
using NeKzBot.Extensions;

namespace NeKzBot.Services.Notifications.Speedrun
{
    internal sealed class GuideNotification : SpeedrunNotificationType
    {
        public GuideNotification(bool notifyUpdated)
            : base(user: 0, game: 2, data: 4)
        {
            if (notifyUpdated)
                Add(user: -1, game: 3, data: 1, keyword: "has been updated", key: 4);
        }

        public override string Description(SpeedrunNotification? nf)
        {
            var link = $"*[{Data?.ToRawText()}]({(nf?.Item?.Uri ?? string.Empty).ToRawText()})*";

            return _meta.Keyword switch
            {
                "has been updated" => "**Guide Updated**\n" + link,
                _                  => "**Guide Added**\n" + link,
            };
        }
    }
}
