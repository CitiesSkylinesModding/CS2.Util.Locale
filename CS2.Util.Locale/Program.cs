using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

using CS2.Util.Locale.Core;

namespace CS2.Util.Locale
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var gamePath = Environment.GetEnvironmentVariable("CSII_INSTALLATIONPATH", EnvironmentVariableTarget.User);
            ArgumentNullException.ThrowIfNull(gamePath, nameof(gamePath));
            var dataDir = Path.Combine(gamePath, "Cities2_Data", "StreamingAssets", "Data~");
            var eng = Path.Combine(dataDir, "en-US.loc");
            var engFile = LocalizationFile.FromFile(eng);
            var untranslated = new DirectoryInfo(dataDir).GetFiles("*.loc", SearchOption.TopDirectoryOnly)
                .Where(info => info.Name != "en-US.loc")
                .Select(info => (info, LocalizationFile.FromFile(info.FullName)))
                .Select(tup => (tup.info, tup.Item2, GetUntranslatedEntry(engFile.Localizations, tup.Item2.Localizations)));

            foreach (var (file, info, entry) in untranslated)
            {
                if (entry.Count <= 0) continue;
                Console.WriteLine($"{info.Name}({info.NameInEnglish}) has untranslated entry.");
                //foreach (var (key, value) in entry)
                //{
                //    Console.WriteLine($"\t{key}: {value}");
                //}
                File.WriteAllText(Path.GetFileNameWithoutExtension(file.Name) + ".json", JsonSerializer.Serialize(entry, new JsonSerializerOptions() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, WriteIndented = true, TypeInfoResolver = DictContext.Default.DictionaryStringString.OriginatingResolver }));
            }
        }

        private static Dictionary<string, string> GetUntranslatedEntry(Dictionary<string, string> original, Dictionary<string, string> translated)
        {
            var dict = new Dictionary<string, string>();
            foreach (var (key, value) in original)
            {
                if (translated.TryGetValue(key, out var text))
                {
                    if (text == value)
                    {
                        dict.Add(key, value);
                    }
                }
                else
                {
                    dict.Add(key, value);
                }
            }

            return dict;
        }
    }

    [JsonSerializable(typeof(Dictionary<string, string>))]
    [JsonSourceGenerationOptions(WriteIndented = true)]
    public partial class DictContext : JsonSerializerContext
    {

    }
}
