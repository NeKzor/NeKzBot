using System;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Addons.Preconditions;
using Discord.Commands;
using NeKzBot.Extensions;
using NeKzBot.Services;

namespace NeKzBot.Modules.Public
{
    public class FunModule : InteractiveBase<SocketCommandContext>
    {
        private ImageService _imageService;

        public FunModule(ImageService ImageService)
        {
            _imageService = ImageService;
        }

        [Ratelimit(6, 1, Measure.Minutes)]
        [Command("ris")]
        public async Task RegionalIndicatorSymbol([Remainder] string text)
        {
            var message = string.Empty;
            var lines = 1;
            var numbers = new string[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };

            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];
                var temp = string.Empty;

                if (c == ' ')
                    temp = "          ";
                else if (c == '\n')
                    temp = "\n";
                else if ($"{c}".Validate("^[a-zA-Z]", 1))
                    temp = $":regional_indicator_{$"{c}".ToLower()}:";
                else if ($"{c}".Validate("^[0-9]", 1))
                    temp = $":{(numbers[Convert.ToInt16($"{c}")])}:";
                else if (c == '!')
                    temp = ":exclamation:";
                else if (c == '?')
                    temp = ":question:";
                else
                    continue;

                if ((temp == "\n") && (++lines > 4))
                    continue;

                // Only append if it doesn't exceed Discord char limit
                if (message.Length + temp.Length > DiscordConfig.MaxMessageSize)
                    break;
                else
                    message += temp;
            }
            if (!string.IsNullOrEmpty(message))
                await ReplyAsync(message);
        }
        [Ratelimit(6, 1, Measure.Minutes)]
        [Command("meme")]
        public async Task Meme(string imageName = null)
        {
            if (!string.IsNullOrEmpty(imageName)
                && !string.IsNullOrEmpty(imageName = _imageService.GetImage(imageName)))
                await Context.Channel.SendFileAsync(imageName);
            else
                await Context.Channel.SendFileAsync(_imageService.GetRandomImage());
        }
    }
}
