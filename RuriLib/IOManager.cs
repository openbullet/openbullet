using Newtonsoft.Json;
using RuriLib.Models;
using RuriLib.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;

namespace RuriLib
{
    /// <summary>
    /// Static Class used to access serialization and deserialization of objects.
    /// </summary>
    public static class IOManager
    {
        private static JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

        /// <summary>
        /// Saves the RuriLib settings to a file.
        /// </summary>
        /// <param name="settingsFile">The file you want to save to</param>
        /// <param name="settings">The RuriLib settings object</param>
        public static void SaveSettings(string settingsFile, RLSettingsViewModel settings)
        {
            File.WriteAllText(settingsFile, JsonConvert.SerializeObject(settings, Formatting.Indented));
        }

        /// <summary>
        /// Loads the RuriLib settings from a file.
        /// </summary>
        /// <param name="settingsFile">The file you want to load from</param>
        /// <returns>An instance of RLSettingsViewModel</returns>
        public static RLSettingsViewModel LoadSettings(string settingsFile)
        {
            return JsonConvert.DeserializeObject<RLSettingsViewModel>(File.ReadAllText(settingsFile));
        }

        /// <summary>
        /// Serializes a block to a JSON string.
        /// </summary>
        /// <param name="block">The block to serialize</param>
        /// <returns>The JSON-encoded BlockBase object with TypeNameHandling on</returns>
        public static string SerializeBlock(BlockBase block)
        {
            return JsonConvert.SerializeObject(block, Formatting.None, settings);
        }

        /// <summary>
        /// Deserializes a Block from a JSON string.
        /// </summary>
        /// <param name="block">The JSON-encoded string with TypeNameHandling on</param>
        /// <returns>An instance of BlockBase</returns>
        public static BlockBase DeserializeBlock(string block)
        {
            return JsonConvert.DeserializeObject<BlockBase>(block, settings);
        }

        /// <summary>
        /// Serializes a list of blocks to a JSON string.
        /// </summary>
        /// <param name="blocks">The list of blocks to serialize</param>
        /// <returns>The JSON-encoded List of BlockBase objects with TypeNameHandling on</returns>
        public static string SerializeBlocks(List<BlockBase> blocks)
        {
            return JsonConvert.SerializeObject(blocks, Formatting.None, settings);
        }

        /// <summary>
        /// Deserializes a list of blocks from a JSON string.
        /// </summary>
        /// <param name="blocks">The JSON-encoded string with TypeNameHandling on</param>
        /// <returns>A list of instances of BlockBase</returns>
        public static List<BlockBase> DeserializeBlocks(string blocks)
        {
            return JsonConvert.DeserializeObject<List<BlockBase>>(blocks, settings);
        }

        /// <summary>
        /// Serializes a Config object to the loli-formatted string.
        /// </summary>
        /// <param name="config">The Config to serialize</param>
        /// <returns>The loli-formatted string</returns>
        public static string SerializeConfig(Config config)
        {
            StringWriter writer = new StringWriter();
            writer.WriteLine("[SETTINGS]");
            writer.WriteLine(JsonConvert.SerializeObject(config.Settings, Formatting.Indented));
            writer.WriteLine("");
            writer.WriteLine("[SCRIPT]");
            writer.Write(config.Script);
            return writer.ToString();
        }

        /// <summary>
        /// Deserializes a Config object from a loli-formatted string.
        /// </summary>
        /// <param name="config">The loli-formatted string</param>
        /// <returns>An instance of the Config object</returns>
        public static Config DeserializeConfig(string config)
        {
            var split = config.Split(new string[] { "[SETTINGS]", "[SCRIPT]" }, StringSplitOptions.RemoveEmptyEntries);
            return new Config(JsonConvert.DeserializeObject<ConfigSettings>(split[0]), split[1].TrimStart('\r', '\n'));
        }

        /// <summary>
        /// Serializes a list of proxies to a JSON string.
        /// </summary>
        /// <param name="proxies">The list of proxies to serialize</param>
        /// <returns>The JSON-encoded list of CProxy objects with TypeNameHandling on</returns>
        public static string SerializeProxies(List<CProxy> proxies)
        {
            return JsonConvert.SerializeObject(proxies, Formatting.None);
        }

        /// <summary>
        /// Deserializes a list of proxies from a JSON string.
        /// </summary>
        /// <param name="proxies">The JSON-encoded list of proxies with TypeNameHandling on</param>
        /// <returns>A list of CProxy objects</returns>
        public static List<CProxy> DeserializeProxies(string proxies)
        {
            return JsonConvert.DeserializeObject<List<CProxy>>(proxies);
        }

        /// <summary>
        /// Loads a Config object from a .loli file.
        /// </summary>
        /// <param name="fileName">The config file</param>
        /// <returns>A Config object</returns>
        public static Config LoadConfig(string fileName)
        {
            return DeserializeConfig(File.ReadAllText(fileName));
        }

        /// <summary>
        /// Saves a Config object to a .loli file.
        /// </summary>
        /// <param name="config">The viewmodel of the config to save</param>
        /// <param name="fileName">The path of the file where the Config will be saved</param>
        /// <returns>Whether the file has been saved successfully</returns>
        public static bool SaveConfig(Config config, string fileName)
        {
            try
            {
                if ((config.Settings.Name + ".loli") != fileName.Replace("Configs\\", ""))
                {
                    File.Move(fileName, "Configs\\" + config.Settings.Name + ".loli");
                    fileName = "Configs\\" + config.Settings.Name + ".loli";
                }
                File.WriteAllText(fileName, SerializeConfig(config));
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// Clones a Config object by serializing and deserializing it.
        /// </summary>
        /// <param name="config">The object to clone</param>
        /// <returns>The cloned Config object</returns>
        public static Config CloneConfig(Config config)
        {
            return DeserializeConfig(SerializeConfig(config));
        }

        /// <summary>
        /// Clones a BlockBase object by serializing and deserializing it.
        /// </summary>
        /// <param name="block">The object to clone</param>
        /// <returns>The cloned BlockBase object</returns>
        public static BlockBase CloneBlock(BlockBase block)
        {
            return DeserializeBlock(SerializeBlock(block));
        }

        /// <summary>
        /// Clones a list of proxies by serializing and deserializing it.
        /// </summary>
        /// <param name="proxies">The list of proxies to clone</param>
        /// <returns>The cloned list of proxies</returns>
        public static List<CProxy> CloneProxies(List<CProxy> proxies)
        {
            return DeserializeProxies(SerializeProxies(proxies));
        }

        /// <summary>
        /// Parses the EnvironmentSettings from a file.
        /// </summary>
        /// <param name="envFile">The .ini file of the settings</param>
        /// <returns>The loaded EnvironmentSettings object</returns>
        public static EnvironmentSettings ParseEnvironmentSettings(string envFile)
        {
            var env = new EnvironmentSettings();
            var lines = File.ReadAllLines(envFile).Where(l => !string.IsNullOrEmpty(l)).ToArray();
            for (int i = 0; i < lines.Count(); i++)
            {
                var line = lines[i];
                if (line.StartsWith("#")) continue;
                else if (line.StartsWith("["))
                {
                    Type type;
                    var header = line;
                    switch (line.Trim())
                    {
                        case "[WLTYPE]": type = typeof(WordlistType); break;
                        case "[CUSTOMKC]": type = typeof(CustomKeychain); break;
                        case "[EXPFORMAT]": type = typeof(ExportFormat); break;
                        default: throw new Exception("Unrecognized ini header");
                    }

                    var parameters = new List<string>();
                    int j = i + 1;
                    for (; j < lines.Count(); j++)
                    {
                        line = lines[j];
                        if (line.StartsWith("[")) break;
                        else if (line.Trim() == "" || line.StartsWith("#")) continue;
                        else parameters.Add(line);
                    }

                    switch (header)
                    {
                        case "[WLTYPE]": env.WordlistTypes.Add((WordlistType)ParseObjectFromIni(type, parameters)); break;
                        case "[CUSTOMKC]": env.CustomKeychains.Add((CustomKeychain)ParseObjectFromIni(type, parameters)); break;
                        case "[EXPFORMAT]": env.ExportFormats.Add((ExportFormat)ParseObjectFromIni(type, parameters)); break;
                        default: break;
                    }

                    i = j - 1;
                }
            }

            return env;
        }

        private static object ParseObjectFromIni(Type type, List<string> parameters)
        {
            object obj = Activator.CreateInstance(type);
            foreach (var pair in parameters
                .Where(p => !string.IsNullOrEmpty(p))
                .Select(p => p.Split(new char[] { '=' }, 2)))
            {
                var prop = type.GetProperty(pair[0]);
                var propObj = prop.GetValue(obj);
                dynamic value = null;
                var ts = new TypeSwitch()
                    .Case((String x) => value = pair[1])
                    .Case((Int32 x) => value = Int32.Parse(pair[1]))
                    .Case((Boolean x) => value = Boolean.Parse(pair[1]))
                    .Case((List<String> x) => value = pair[1].Split(',').ToList())
                    .Case((Color x) => value = Color.FromRgb(
                        System.Drawing.Color.FromName(pair[1]).R,
                        System.Drawing.Color.FromName(pair[1]).G,
                        System.Drawing.Color.FromName(pair[1]).B
                    ))
                ;

                ts.Switch(propObj);
                prop.SetValue(obj, value);
            }
            return obj;
        }
    }
}
