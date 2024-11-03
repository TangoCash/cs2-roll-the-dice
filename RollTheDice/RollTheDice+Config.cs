using System.IO.Enumeration;
using System.Text.Json;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace RollTheDice
{
    public class MapConfig
    {
        // disabled features
        [JsonPropertyName("disabled_dices")] public List<string> DisabledDices { get; set; } = new();
    }

    public class PluginConfig : BasePluginConfig
    {
        // disabled features
        [JsonPropertyName("disabled_dices")] public List<string> DisabledDices { get; set; } = new();
        // map configurations
        [JsonPropertyName("maps")] public Dictionary<string, MapConfig> MapConfigs { get; set; } = new Dictionary<string, MapConfig>();
    }

    public partial class RollTheDice : BasePlugin, IPluginConfig<PluginConfig>
    {
        public PluginConfig Config { get; set; } = null!;
        private MapConfig[] _currentMapConfigs = Array.Empty<MapConfig>();
        private string _configPath = "";

        private void LoadConfig()
        {
            _configPath = Path.Combine(ModuleDirectory, $"../../configs/plugins/RollTheDice/RollTheDice.json");
        }

        private void InitializeConfig(string mapName)
        {
            // select map configs whose regexes (keys) match against the map name
            _currentMapConfigs = (from mapConfig in Config.MapConfigs
                                  where FileSystemName.MatchesSimpleExpression(mapConfig.Key, mapName)
                                  select mapConfig.Value).ToArray();

            if (_currentMapConfigs.Length > 0)
            {
                if (Config.MapConfigs.TryGetValue("default", out var config))
                {
                    // add default configuration
                    _currentMapConfigs = new[] { config };
                    Console.WriteLine(Localizer["core.defaultconfig"].Value.Replace("{mapName}", mapName));
                }
                else
                {
                    // there is no config to apply
                    Console.WriteLine(Localizer["core.noconfig"].Value.Replace("{mapName}", mapName));
                }
            }
            else
            {
                Console.WriteLine(Localizer["core.defaultconfig"].Value.Replace("{mapName}", mapName));
                // create default configuration
                Config.MapConfigs.Add(mapName, new MapConfig());
            }
            Console.WriteLine(Localizer["core.foundconfig"].Value.Replace("{count}", _currentMapConfigs.Length.ToString()).Replace("{mapName}", mapName));
        }

        public void OnConfigParsed(PluginConfig config)
        {
            Config = config;
            Console.WriteLine("[RollTheDice] Initialized map configuration!");
        }

        private void SaveConfig()
        {
            var jsonString = JsonSerializer.Serialize(Config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configPath, jsonString);
        }
    }
}
