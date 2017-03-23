using System.Linq;
using System.Threading.Tasks;
using NeKzBot.Resources;
using NeKzBot.Server;
using NeKzBot.Utilities;

namespace NeKzBot.Modules.Public.Others
{
	public class RaspberryPi : CommandModule
	{
		public static async Task LoadAsync()
		{
			await Logger.SendAsync("Loading RaspberryPi Module", LogColor.Init);
			await GetServerInfo("rpi");
		}

		private static Task GetServerInfo(string s)
		{
			CService.CreateGroup(s, GBuilder =>
			{
				GBuilder.CreateCommand("specs")
						.Description("Shows some hardware information about the server.")
						.AddCheck(Permissions.LinuxOnly)
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage(Data.ServerSpecs);
						});

				GBuilder.CreateCommand("date")
						.Description("Shows time and date of the server.")
						.AddCheck(Permissions.LinuxOnly)
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage((await Utils.GetCommandOutputAsync("date")).Replace("  ", " "));
						});

				GBuilder.CreateCommand("uptime")
						.Description("Shows how long the server is running for.")
						.AddCheck(Permissions.LinuxOnly)
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							await e.Channel.SendMessage($"{(await Utils.GetCommandOutputAsync("uptime")).Split(',')[0].Replace("up", "(up for")})");
						});

				GBuilder.CreateCommand("temperature")
						.Alias("temp", "howhot?")
						.Description("Shows the current temperature of the SoC.")
						.AddCheck(Permissions.LinuxOnly)
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							var temp = await Utils.GetCommandOutputAsync("vcgencmd", "measure_temp");
							await e.Channel.SendMessage($"SoC Temperature = **{temp.Substring(5).Replace("'", "°")}**");
						});

				GBuilder.CreateCommand("os")
						.Description("Gives you more information about the server's operating system.")
						.AddCheck(Permissions.LinuxOnly)
						.Do(async e =>
						{
							await e.Channel.SendIsTyping();
							var os = await Utils.GetCommandOutputAsync("cat", "/etc/os-release");
							var output = string.Empty;
							foreach (var item in os.Split('\n'))
								output += $"{item.Split('=').Last()}\n";
							await e.Channel.SendMessage(output.Substring(0, output.Length - 1).Replace("\"", string.Empty));
						});
			});
			return Task.FromResult(0);
		}
	}
}