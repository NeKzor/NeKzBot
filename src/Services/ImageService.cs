using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace NeKzBot.Services
{
    public class ImageService
    {
        private List<string> _imageCache;

        private readonly IConfiguration _config;

        public ImageService(IConfiguration config)
        {
            _config = config;
            _imageCache = new List<string>();
        }

        public Task Initialize()
        {
            foreach (var file in Directory.GetFiles("private/resources/images"))
            {
                if (file.EndsWith(".png") || file.EndsWith(".jpg") || file.EndsWith(".gif"))
                    _imageCache.Add(file);
            }

            return Task.CompletedTask;
        }

        public string GetImage(string fileName)
        {
            return _imageCache.FirstOrDefault(img => img.EndsWith(fileName));
        }
        public string GetRandomImage()
        {
            var rand = new System.Random();
            return _imageCache[rand.Next(0, _imageCache.Count)];
        }
    }
}
