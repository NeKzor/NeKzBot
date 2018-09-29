using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace NeKzBot.Modules.Public
{
    public static class DiscordExtensions
    {
        public static Task<Color> GetRoleColor(this IUser user, IGuild guild = null)
        {
            if ((user != null) && (guild != null))
                foreach (var role in guild.Roles.Skip(1).OrderByDescending(r => r.Position))
                    if ((user as SocketGuildUser)?.Roles.Contains(role) == true)
                        return Task.FromResult(role.Color);
            return Task.FromResult(new Color(14, 186, 83));
        }
    }
}
