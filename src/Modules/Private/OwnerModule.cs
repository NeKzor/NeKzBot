using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using NeKzBot.Services.Notifications;
using NeKzBot.Services.Notifications.Auditor;
using NeKzBot.Services.Notifications.Speedrun;

namespace NeKzBot.Modules.Public
{
    [RequireOwner]
    [Group("tasks"), Alias("bot")]
    public class OwnerModule : InteractiveBase<SocketCommandContext>
    {
        private AuditorNotificationService _auditor;
        private SpeedrunNotificationService _speedrun;

        protected OwnerModule(AuditorNotificationService auditor, SpeedrunNotificationService speedrun)
        {
            _auditor = auditor;
            _speedrun = speedrun;
        }

        private NotificationService? FindTask(string task) => task.ToLower() switch
        {
            "auditor"  => _auditor,
            "speedrun" => _speedrun,
            _          => default,
        };

        [Command("status"), Alias("?")]
        public async Task Status()
        {
            var status = $"Auditor: {(_auditor.IsRunning ? "started" : "stopped")}\n"
                       + $"Speedrun: {(_speedrun.IsRunning ? "started" : "stopped")}";

            var embed = new EmbedBuilder()
                .WithColor(await Context.User.GetRoleColor(Context.Guild))
                .WithTitle("Task Status")
                .WithDescription(status);

            await ReplyAndDeleteAsync(string.Empty, embed: embed.Build());
        }

        [Command("stop"), Alias("cancel", "end")]
        public async Task Stop(string taskName)
        {
            var task = FindTask(taskName);
            if (task is null)
            {
                await ReplyAndDeleteAsync("Unknown task name. Available tasks: auditor, speedrun.");
                return;
            }

            if (!task.IsRunning)
            {
                await ReplyAndDeleteAsync("Task is not running.");
                return;
            }

            await task.StopAsync();

            var embed = new EmbedBuilder()
                .WithColor(await Context.User.GetRoleColor(Context.Guild))
                .WithTitle("Task - " + task.Name)
                .WithDescription("Stopped");

            await ReplyAndDeleteAsync(string.Empty, embed: embed.Build());
        }

        [Command("start"), Alias("run")]
        public async Task Start(string taskName)
        {
            var task = FindTask(taskName);

            if (task is null)
            {
                await ReplyAndDeleteAsync("Unknown task name. Available tasks: auditor, speedrun.");
                return;
            }

            if (task.IsRunning)
            {
                await ReplyAndDeleteAsync("Task already running.");
                return;
            }

            _ = Task.Run(async () => await task.StartAsync());

            var embed = new EmbedBuilder()
                .WithColor(await Context.User.GetRoleColor(Context.Guild))
                .WithTitle("Task - " + task.Name)
                .WithDescription("Started");

            await ReplyAndDeleteAsync(string.Empty, embed: embed.Build());
        }

        [Command("restart"), Alias("reset")]
        public async Task Restart(string taskName)
        {
            var task = FindTask(taskName);

            if (task is null)
            {
                await ReplyAndDeleteAsync("Unknown task name. Available tasks: auditor, speedrun.");
                return;
            }

            await task.StopAsync();

            _ = Task.Run(async () => await task.StartAsync());

            var embed = new EmbedBuilder()
                .WithColor(await Context.User.GetRoleColor(Context.Guild))
                .WithTitle("Task - " + task.Name)
                .WithDescription("Restarted");

            await ReplyAndDeleteAsync(string.Empty, embed: embed.Build());
        }
    }
}
