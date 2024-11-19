﻿using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin, IPluginConfig<PluginConfig>
    {
        public override string ModuleName => "Roll The Dice";
        public override string ModuleAuthor => "Jon-Mailes Graeffe <mail@jonni.it> / Kalle <kalle@kandru.de>";

        private string _currentMap = "";
        private List<CCSPlayerController> _playersThatRolledTheDice = new();
        private List<Func<CCSPlayerController, CCSPlayerPawn, string>> _dices = new();
        private bool _isDuringRound = false;
        Random _random = new Random(Guid.NewGuid().GetHashCode());

        public override void Load(bool hotReload)
        {
            // initialize dices
            InitializeDices();
            // initialize configuration
            LoadConfig();
            UpdateConfig();
            SaveConfig();
            // initialize sounds
            InitializeEmitSound();
            // register listeners
            RegisterEventHandler<EventRoundStart>(OnRoundStart);
            RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
            RegisterListener<Listeners.OnMapStart>(OnMapStart);
            RegisterListener<Listeners.OnMapEnd>(OnMapEnd);
            RegisterListener<Listeners.OnServerPrecacheResources>(OnServerPrecacheResources);
            CreateDiceFastBombActionEventHandlers();
            CreateDicePlayerVampireEventHandler();
            CreateDicePlayerDisguiseAsPlantEventHandler();
            CreateDicePlayerRespawnEventHandler();
            CreateDicePlayerAsChickenEventHandler();
            CreateDiceBigTaserBatteryEventHandler();
            // print message if hot reload
            if (hotReload)
            {
                // set current map
                _currentMap = Server.MapName;
                // initialize configuration
                InitializeConfig(_currentMap);
                Console.WriteLine(Localizer["core.hotreload"]);
                SendGlobalChatMessage(Localizer["core.hotreload"]);
            }
        }

        public override void Unload(bool hotReload)
        {
            // reset dice rolls on unload
            ResetDices();
            // unregister listeners
            DeregisterEventHandler<EventRoundStart>(OnRoundStart);
            DeregisterEventHandler<EventRoundEnd>(OnRoundEnd);
            RemoveListener<Listeners.OnMapStart>(OnMapStart);
            RemoveListener<Listeners.OnMapEnd>(OnMapEnd);
            RemoveListener<Listeners.OnServerPrecacheResources>(OnServerPrecacheResources);
            RemoveDiceFastBombActionEventHandlers();
            RemoveDicePlayerVampireEventHandler();
            RemoveDicePlayerDisguiseAsPlantListener();
            RemoveDicePlayerRespawnListener();
            RemoveDicePlayerAsChickenListeners();
            RemoveDicePlayerMakeHostageSoundsListener();
            RemoveDicePlayerMakeFakeGunSoundsListener();
            RemoveDiceBigTaserBatteryListeners();
            RemoveDicePlayerCloakListeners();
            RemoveDiceNoExplosivesEventHandler();
            Console.WriteLine(Localizer["core.unload"]);
        }

        private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
        {
            // reset players that rolled the dice
            _playersThatRolledTheDice.Clear();
            // reset dices (necessary after warmup)
            ResetDices();
            // abort if warmup
            if ((bool)GetGameRule("WarmupPeriod")!) return HookResult.Continue;
            // announce round start
            SendGlobalChatMessage(Localizer["core.announcement"]);
            // allow dice rolls
            _isDuringRound = true;
            // continue event
            return HookResult.Continue;
        }

        private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
        {
            ResetDices();
            // disallow dice rolls
            _isDuringRound = false;
            // continue event
            return HookResult.Continue;
        }

        private void OnMapStart(string mapName)
        {
            // set current map
            _currentMap = mapName;
            // update configuration
            LoadConfig();
            UpdateConfig();
            SaveConfig();
        }

        private void OnMapEnd()
        {
            ResetDices();
            // disallow dice rolls
            _isDuringRound = false;
        }

        private void InitializeDices()
        {
            // create dynamic list containing functions to execute for each dice
            _dices = new List<Func<CCSPlayerController, CCSPlayerPawn, string>>
            {
                DiceIncreaseHealth,
                DiceDecreaseHealth,
                DiceIncreaseSpeed,
                DiceChangeName,
                DicePlayerInvisible,
                DicePlayerSuicide,
                DicePlayerRespawn,
                DiceStripWeapons,
                DiceChickenLeader,
                DiceFastBombAction,
                DicePlayerVampire,
                DicePlayerLowGravity,
                DicePlayerHighGravity,
                DicePlayerOneHP,
                DicePlayerDisguiseAsPlant,
                DicePlayerAsChicken,
                DicePlayerMakeHostageSounds,
                DicePlayerMakeFakeGunSounds,
                DiceBigTaserBattery,
                DicePlayerCloak,
                DiceGiveHealthShot,
                DiceNoExplosives
            };
        }

        private void ResetDices()
        {
            ResetDicePlayerInvisible();
            ResetDiceIncreaseSpeed();
            ResetDiceChangeName();
            ResetDiceFastBombAction();
            ResetDicePlayerVampire();
            ResetDicePlayerDisguiseAsPlant();
            ResetDicePlayerRespawn();
            ResetDicePlayerAsChicken();
            ResetDicePlayerMakeHostageSounds();
            ResetDicePlayerMakeFakeGunSounds();
            ResetDiceBigTaserBattery();
            ResetDicePlayerCloak();
            ResetDiceNoExplosives();
        }

        private int GetRandomDice()
        {
            // Filter enabled dices based on map-specific and global configuration
            var enabledDiceIndices = _dices
                .Select((dice, index) => new { dice, index })
                .Where(diceInfo =>
                {
                    var diceName = diceInfo.dice.Method.Name;
                    // Check map-specific configuration
                    if (Config.MapConfigs.TryGetValue(_currentMap, out var mapConfig) && mapConfig.Features.TryGetValue(diceName, out var isEnabled))
                    {
                        return isEnabled;
                    }
                    // Check global configuration
                    if (Config.Features.TryGetValue(diceName, out isEnabled))
                    {
                        return isEnabled;
                    }
                    // Default to enabled if not found in either configuration
                    return true;
                })
                .Select(diceInfo => diceInfo.index)
                .ToList();
            if (enabledDiceIndices.Count == 0) return -1;
            // Get random dice from enabled dices
            return enabledDiceIndices[_random.Next(enabledDiceIndices.Count)];
        }
    }
}
