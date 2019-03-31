using Newtonsoft.Json;
using OpenBullet.ViewModels;
using System.IO;

namespace OpenBullet
{
    public class OBIOManager
    {
        private static JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

        public static void SaveSettings(string settingsFile, OBSettingsViewModel settings)
        {
            File.WriteAllText(settingsFile, JsonConvert.SerializeObject(settings, Formatting.Indented));
        }

        public static OBSettingsViewModel LoadSettings(string settingsFile)
        {
            return JsonConvert.DeserializeObject<OBSettingsViewModel>(File.ReadAllText(settingsFile));
        }
    }
}
