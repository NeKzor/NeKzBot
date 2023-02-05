using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NeKzBot.API;

namespace NeKzBot.Services.Notifications.Speedrun
{
    internal abstract class SpeedrunNotificationType
    {
        public IEnumerable<string>? Users { get; private set; }
        public string? GameAndCategory { get; private set; }
        public string? Data { get; private set; }

        public string Author => Users.FirstOrDefault() ?? string.Empty;
        public string Game => GameAndCategory?.Split('-')?.ElementAtOrDefault(0) ?? string.Empty;
        public string Category => string.Join('-', GameAndCategory?.Split('-')?.Skip(1) ?? Enumerable.Empty<string>());

        protected string[]? _fields;
        protected string? _splitPattern;

        protected List<(int User, int Game, int Data, string Keyword, int Key, bool MultiUsers)> _metaData
            = new List<(int User, int Game, int Data, string Keyword, int Key, bool MultiUsers)>();

        protected (int User, int Game, int Data, string Keyword, int Key, bool MultiUsers) _meta;

        public SpeedrunNotificationType()
        {
        }
        public SpeedrunNotificationType(int user, int game, int data, string keyword = "", int key = -1, bool multiUsers = false)
        {
            Add(user, game, data, keyword, key, multiUsers);
        }

        public void Add(int user, int game, int data, string keyword = "", int key = -1, bool multiUsers = false)
        {
            _metaData.Add((user, game, data, keyword, key, multiUsers));
        }

        public (string, string, string) Get(SpeedrunNotification? nf)
        {
            if (nf?.Text is null) return default;

            var pattern = new Regex(_splitPattern);

            _fields = pattern.Split(nf.Text)
                .Select((x) => x.Trim())
                .Where((x) => x != string.Empty);

            void FindMeta()
            {
                foreach (var meta in _metaData)
                {
                    if (meta.Key != -1)
                    {
                        if (_fields.ElementAtOrDefault(meta.Key)?.IndexOf(meta.Keyword) != -1)
                        {
                            _meta = meta;
                            return;
                        }
                    }
                }

                _meta = _metaData.FirstOrDefault();
            }

            FindMeta();

            Users = (_meta.MultiUsers)
                ? _fields.ElementAtOrDefault(_meta.User)?.Split(new string[] { " , ", " and " }, StringSplitOptions.RemoveEmptyEntries)
                : new string[] { _fields.ElementAtOrDefault(_meta.User) };

            GameAndCategory = _fields.ElementAtOrDefault(_meta.Game);
            Data = _fields.ElementAtOrDefault(_meta.Data);

            return (
                Author,
                Game,
                Description(nf)
            );
        }

        public abstract string Description(SpeedrunNotification? nf);
    }
}
