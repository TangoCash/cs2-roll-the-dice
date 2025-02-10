﻿using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Config;
using CounterStrikeSharp.API.Modules.Extensions;
using System.IO.Enumeration;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RollTheDice
{
    public class GuiPositionConfig
    {
        [JsonPropertyName("message_font")] public string MessageFont { get; set; } = "";
        [JsonPropertyName("message_font_size")] public int MessageFontSize { get; set; } = 40;
        [JsonPropertyName("message_color")] public string MessageColor { get; set; } = "";
        [JsonPropertyName("message_shift_x")] public float MessageShiftX { get; set; } = 0.0f;
        [JsonPropertyName("message_shift_y")] public float MessageShiftY { get; set; } = 0.0f;
        [JsonPropertyName("status_font")] public string StatusFont { get; set; } = "";
        [JsonPropertyName("status_font_size")] public int StatusFontSize { get; set; } = 40;
        [JsonPropertyName("status_color")] public string StatusColor { get; set; } = "";
        [JsonPropertyName("status_shift_x")] public float StatusShiftX { get; set; } = 0.0f;
        [JsonPropertyName("status_shift_y")] public float StatusShiftY { get; set; } = 0.0f;
    }

    public class MapConfig
    {
        // disabled
        [JsonPropertyName("enabled")] public bool Enabled { get; set; } = true;
        // dices configuration
        [JsonPropertyName("dices")] public Dictionary<string, Dictionary<string, object>> Dices { get; set; } = new();
    }

    public class PluginConfig : BasePluginConfig
    {
        // disabled
        [JsonPropertyName("enabled")] public bool Enabled { get; set; } = true;
        // debug prints
        [JsonPropertyName("debug")] public bool Debug { get; set; } = false;
        // limit !rtd usage to every X rounds (only set one of both)
        [JsonPropertyName("cooldown_rounds")] public int CooldownRounds { get; set; } = 0;
        // limit !rtd usage to every X seconds (only set one of both)
        [JsonPropertyName("cooldown_seconds")] public int CooldownSeconds { get; set; } = 0;
        // sound to play on command usage
        [JsonPropertyName("sound_command")] public string CommandSound { get; set; } = "sounds/ui/coin_pickup_01.vsnd";
        // price to charge on command usage
        [JsonPropertyName("price_to_dice")] public int PriceToDice { get; set; } = 0;
        // allow re-dice after respawn
        [JsonPropertyName("allow_dice_after_respawn")] public bool AllowDiceAfterRespawn { get; set; } = false;
        // gui positions
        [JsonPropertyName("default_gui_position")] public string GUIPosition { get; set; } = "top_center";
        [JsonPropertyName("gui_positions")] public Dictionary<string, GuiPositionConfig> GUIPositions { get; set; } = new Dictionary<string, GuiPositionConfig>();
        // dices configuration
        [JsonPropertyName("dices")] public Dictionary<string, Dictionary<string, object>> Dices { get; set; } = new();
        // map configurations
        [JsonPropertyName("maps")] public Dictionary<string, MapConfig> MapConfigs { get; set; } = new Dictionary<string, MapConfig>();
    }

    public partial class RollTheDice : BasePlugin, IPluginConfig<PluginConfig>
    {
        public required PluginConfig Config { get; set; }
        private MapConfig[] _currentMapConfigs = Array.Empty<MapConfig>();

        public void OnConfigParsed(PluginConfig config) { Config = config; }
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

        private void UpdateConfig()
        {
            // iterate through all dices and add them to the configuration file
            foreach (var dice in _dices)
            {
                // create entry for dice if it does not exist
                if (!Config.Dices.ContainsKey(dice.Method.Name))
                {
                    Config.Dices[dice.Method.Name] = new Dictionary<string, object>();
                }
                // load current entries for dice
                var diceConfig = Config.Dices[dice.Method.Name];
                // Ensure "enabled" key exists
                if (!diceConfig.ContainsKey("enabled"))
                {
                    diceConfig["enabled"] = true;
                }
                // Check for further configuration of a dice and add it accordingly
                var methodName = $"{dice.Method.Name}Config";
                var method = GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
                if (method != null)
                {
                    var additionalConfig = method.Invoke(this, null) as Dictionary<string, object> ?? new();
                    // check if configuration does exist
                    foreach (var kvp in additionalConfig)
                    {
                        if (!diceConfig.ContainsKey(kvp.Key))
                        {
                            diceConfig[kvp.Key] = ConvertJsonElement(kvp.Value);
                        }
                    }
                    // Remove keys that should not exist anymore, ignoring the "enabled" key
                    var keysToRemove = diceConfig.Keys.Except(additionalConfig.Keys).Where(key => key != "enabled").ToList();
                    foreach (var key in keysToRemove)
                    {
                        diceConfig.Remove(key);
                    }
                    // sort keys by alphabet
                    var sortedKeys = diceConfig.Keys.OrderBy(key => key).ToList();
                    var sortedDiceConfig = new Dictionary<string, object>();
                    foreach (var key in sortedKeys)
                    {
                        sortedDiceConfig[key] = diceConfig[key];
                    }
                    Config.Dices[dice.Method.Name] = sortedDiceConfig;

                }
            }
            // delete all dices that do not exist anymore
            foreach (var key in Config.Dices.Keys)
            {
                if (!_dices.Any(dice => dice.Method.Name == key))
                {
                    Config.Dices.Remove(key);
                }
            }
            // check GUI config
            CheckGUIConfig();
        }

        private void SaveConfig()
        {
            var jsonString = JsonSerializer.Serialize(Config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Config.GetConfigPath(), jsonString);
        }

        private Dictionary<string, object> GetDiceConfig(string diceName)
        {
            // first try the map-specific configuration
            foreach (var mapConfig in _currentMapConfigs)
            {
                if (mapConfig.Dices.TryGetValue(diceName, out var config))
                {
                    return config.ToDictionary(kvp => kvp.Key, kvp => ConvertJsonElement(kvp.Value));
                }
            }
            // if not available, try the global configuration
            if (Config.Dices.TryGetValue(diceName, out var globalConfig))
            {
                return globalConfig.ToDictionary(kvp => kvp.Key, kvp => ConvertJsonElement(kvp.Value));
            }
            return new Dictionary<string, object>();
        }

        private object ConvertJsonElement(object element)
        {
            if (element is JsonElement jsonElement)
            {
                return jsonElement.ValueKind switch
                {
                    JsonValueKind.String => (object?)jsonElement.GetString() ?? string.Empty,
                    JsonValueKind.Number => jsonElement.TryGetSingle(out var number) ? (object)number : 0.0f,
                    JsonValueKind.True => (object)jsonElement.GetBoolean(),
                    JsonValueKind.False => (object)jsonElement.GetBoolean(),
                    JsonValueKind.Object => jsonElement.EnumerateObject().ToDictionary(property => property.Name, property => ConvertJsonElement(property.Value)),
                    JsonValueKind.Array => jsonElement.EnumerateArray().Select(element => ConvertJsonElement((object)element)).ToList(),
                    JsonValueKind.Undefined => (object)string.Empty,
                    _ => (object)string.Empty
                };
            }
            return element;
        }
    }
}
