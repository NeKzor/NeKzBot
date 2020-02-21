using System;
using NeKzBot.API;
using NeKzBot.Extensions;

namespace NeKzBot.Services.Notifications.Speedrun
{
    internal sealed class RunNotification : SpeedrunNotificationType
    {
        public RunNotification()
        {
            Add(user: 0, game: 2, data: 4, keyword: "beat the WR in", key: 1, multiUsers: true);
            Add(user: 0, game: 2, data: 4, keyword: "got a new PB in", key: 1, multiUsers: true);
        }

        public override string Description(SpeedrunNotification? nf)
        {
            var link = $"{Category.ToRawText()} in [{Data?.ToRawText()}]({nf?.Item?.Uri ?? string.Empty})";

            return _meta.Keyword switch
            {
                "beat the WR in"  => $"**New World Record**\n" + link,
                "got a new PB in" => $"**New Personal Best**\n" + link,
                _                 => throw new Exception("Unknown keyword"),
            };
        }
    }
}
