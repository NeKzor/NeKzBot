using System;
using NeKzBot.API;
using NeKzBot.Extensions;

namespace NeKzBot.Services.Notifications.Speedrun
{
    internal sealed class ResourceNotification : SpeedrunNotificationType
    {
        public ResourceNotification()
        {
            Add(user: -1, game: 5, data: 3, keyword: "patch", key: 1);
            Add(user: -1, game: 5, data: 3, keyword: "save", key: 1);
            Add(user: -1, game: 5, data: 3, keyword: "splits", key: 1);
        }

        public override string Description(SpeedrunNotification? nf)
        {
            var link = $"\n*[{Data?.ToRawText()}]({(nf?.Item?.Uri ?? string.Empty).ToRawText()})*";

            return _meta.Keyword switch
            {
                "patch"  => $"**New Patch Added**" + link,
                "save"   => $"**New Save Added**" + link,
                "splits" => $"**New Splits Added**" + link,
                _        => throw new Exception("Unknown keyword")
            };
        }
    }
}
