using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

namespace CS2.Util.Locale.Core
{
    public class LocalizationFile
    {
        public string? FilePath { get; set; }
        public LocalizationFileType Extension { get; private set; }

        public string? LocaleId { get; private set; }
        public string? Name { get; private set; }
        public string? NameInEnglish { get; private set; }

        public readonly Dictionary<string, string> Localizations = new();
        public readonly Dictionary<string, int> Index = new();

        public LocalizationFile(string path)
        {
            FilePath = path;
            Extension = Path.GetExtension(path) switch
            {
                ".loc" => LocalizationFileType.InGame,
                ".json" => LocalizationFileType.Json,
                _ => LocalizationFileType.Unknown
            };

            switch (Extension)
            {
                case LocalizationFileType.InGame:
                    ReadInGameFile();
                    break;
                case LocalizationFileType.Json:
                    ReadJson();
                    break;
                default:
                    break;
            }
        }

        public static LocalizationFile FromFile(string path)
        {
            var instance = new LocalizationFile(path);
            return instance;
        }

        /// <summary>
        /// Inspired by suluknumoh/TranslateCS2.
        /// https://github.com/suluknumoh/TranslateCS2
        /// </summary>
        private void ReadInGameFile()
        {
            if (FilePath is null)
            {
                ArgumentNullException.ThrowIfNull(FilePath, nameof(FilePath));
            }

            using var stream = File.OpenRead(FilePath);
            using var reader = new BinaryReader(stream, Encoding.UTF8);

            _ = reader.ReadUInt16();
            NameInEnglish = reader.ReadString();
            LocaleId = reader.ReadString();
            Name = reader.ReadString();

            var localizationCount = reader.ReadInt32();
            while (localizationCount-- > 0)
            {
                var key = reader.ReadString();
                var value = reader.ReadString();
                Localizations.Add(key, value);
            }

            var indexCount = reader.ReadInt32();
            while (indexCount-- > 0)
            {
                var key = reader.ReadString();
                var value = reader.ReadInt32();
                Index.Add(key, value);
            }
        }

        private void ReadJson()
        {
            if (FilePath is null)
            {
                ArgumentNullException.ThrowIfNull(FilePath, nameof(FilePath));
            }

            foreach (var (key, value) in JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(FilePath))!)
            {
                Localizations.Add(key, value);
            }
        }

        public void SaveAs(string path)
        {
            var ext = Path.GetExtension(path) switch
            {
                ".loc" => LocalizationFileType.InGame,
                ".json" => LocalizationFileType.Json,
                _ => LocalizationFileType.Unknown
            };

            if (ext == LocalizationFileType.Json)
            {
                File.WriteAllText(path, JsonSerializer.Serialize(Localizations, new JsonSerializerOptions { WriteIndented = true }));
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }

    public enum LocalizationFileType
    {
        Unknown,
        InGame,
        Json
    }
}
