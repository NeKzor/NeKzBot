using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace NeKzBot.Modules.Public
{
    public static class DiscordExtensions
    {
        private static readonly Color DefaultRoleColor = new Color(14, 186, 83);

        public static Task<Color> GetRoleColor(this IUser user, IGuild? guild = null)
        {
            if ((user is null) || (guild is null))
                return Task.FromResult(DefaultRoleColor);

            foreach (var role in guild.Roles.Skip(1).OrderByDescending(r => r.Position))
            {
                if ((user as SocketGuildUser)?.Roles.Contains(role) == true)
                    return Task.FromResult(role.Color);
            }

            return Task.FromResult(DefaultRoleColor);
        }
    }
}
