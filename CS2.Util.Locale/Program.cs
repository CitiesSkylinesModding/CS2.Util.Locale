using System.IO.Compression;
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
            var dataFile = Path.Combine(gamePath, "Cities2_Data", "Content", "Game", "Locale.cok");
            using var fs = File.OpenRead(dataFile);
            var zip = new ZipArchive(fs);
            var localizationFiles = zip.Entries.Where(e => e.Name.EndsWith(".loc")).Select(e => LocalizationFile.FromFile(e.Open())).ToList();
            
            foreach (var localizationFile in localizationFiles)
            {
                localizationFile.SaveAs($"{localizationFile.LocaleId}.json");
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
