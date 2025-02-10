﻿using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Extensions;
using System.Reflection;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin, IPluginConfig<PluginConfig>
    {
        public override string ModuleName => "Roll The Dice";
        public override string ModuleAuthor => "Jon-Mailes Graeffe <mail@jonni.it> / Kalle <kalle@kandru.de>";

        private string _currentMap = "";
        private Dictionary<CCSPlayerController, Dictionary<string, object>> _playersThatRolledTheDice = new();
        private Dictionary<string, int> _countRolledDices = new();
        private Dictionary<CCSPlayerController, int> _PlayerCooldown = new();
        private List<Func<CCSPlayerController, CCSPlayerPawn, Dictionary<string, string>>> _dices = new();
        private bool _isDuringRound = false;
        Random _random = new Random(Guid.NewGuid().GetHashCode());

        public override void Load(bool hotReload)
        {
            // initialize dices
            InitializeDices();
            // initialize configuration
            UpdateConfig();
            SaveConfig();
            // initialize sounds
            InitializeEmitSound();
            // register listeners
            RegisterEventHandler<EventRoundStart>(OnRoundStart);
            RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
            RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
            RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
            RegisterListener<Listeners.OnMapStart>(OnMapStart);
            RegisterListener<Listeners.OnMapEnd>(OnMapEnd);
            RegisterListener<Listeners.OnServerPrecacheResources>(OnServerPrecacheResources);
            // print message if hot reload
            if (hotReload)
            {
                // set current map
                _currentMap = Server.MapName;
                // initialize configuration
                InitializeConfig(_currentMap);
                Console.WriteLine(Localizer["core.hotreload"]);
                SendGlobalChatMessage(Localizer["core.hotreload"]);
                // check if warmup and start round otherwise
                if (!(bool)GetGameRule("WarmupPeriod")!) _isDuringRound = true;
            }
        }

        public override void Unload(bool hotReload)
        {
            // reset dice rolls on unload
            ResetDices();
            RemoveAllGUIs();
            // unregister listeners
            DeregisterEventHandler<EventRoundStart>(OnRoundStart);
            DeregisterEventHandler<EventRoundEnd>(OnRoundEnd);
            RemoveListener<Listeners.OnMapStart>(OnMapStart);
            RemoveListener<Listeners.OnMapEnd>(OnMapEnd);
            RemoveListener<Listeners.OnServerPrecacheResources>(OnServerPrecacheResources);
            // iterate through all dices and call their unload method dynamically
            foreach (var dice in _dices)
            {
                var methodName = $"{dice.Method.Name}Unload";
                var method = GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
                if (method != null)
                {
                    DebugPrint($"Unloading dice: {methodName}");
                    method.Invoke(this, null);
                }
            }
            Console.WriteLine(Localizer["core.unload"]);
        }

        private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
        {
            DebugPrint("Round started");
            // reset players that rolled the dice
            _playersThatRolledTheDice.Clear();
            // reset dices (necessary after warmup)
            ResetDices();
            RemoveAllGUIs();
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
            DebugPrint("Round ended");
            ResetDices();
            RemoveAllGUIs();
            // disallow dice rolls
            _isDuringRound = false;
            // reduct cooldown if applicable
            if (Config.CooldownRounds > 0)
            {
                foreach (var kvp in _PlayerCooldown)
                {
                    // remove one round per player
                    if (_PlayerCooldown[kvp.Key] > 0) _PlayerCooldown[kvp.Key] -= 1;
                }
            }
            // continue event
            return HookResult.Continue;
        }

        private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
        {
            CCSPlayerController player = @event.Userid!;
            ResetDiceForPlayer(player);
            return HookResult.Continue;
        }

        private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
        {
            CCSPlayerController player = @event.Userid!;
            ResetDiceForPlayer(player);
            return HookResult.Continue;
        }

        private void OnMapStart(string mapName)
        {
            DebugPrint($"Map started: {mapName}");
            // set current map
            _currentMap = mapName;
            // update configuration
            Config.Reload();
            UpdateConfig();
            SaveConfig();
        }

        private void OnMapEnd()
        {
            DebugPrint($"Map ended: {_currentMap}");
            ResetDices();
            RemoveAllGUIs();
            // disallow dice rolls
            _isDuringRound = false;
            // reset cooldown
            _PlayerCooldown.Clear();
        }

        private void InitializeDices()
        {
            DebugPrint("Initializing dices");
            // create dynamic list containing functions to execute for each dice
            _dices = new List<Func<CCSPlayerController, CCSPlayerPawn, Dictionary<string, string>>>
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
                DiceFastMapAction,
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
                DiceNoExplosives,
                DiceChangePlayerModel,
                DicePlayerGlow,
                DiceShowPlayerHealthBar,
                DiceNoRecoil,
                DiceChangePlayerSize,
                DiceIncreaseMoney,
                DiceDecreaseMoney,
                DiceThirdPersonView,
            };
            // initialize dice counter
            foreach (var dice in _dices)
            {
                _countRolledDices[dice.Method.Name] = 0;
            }
            // run all dices' initialization methods
            // TODO: check after each map load and unload if dice is enabled
            // and run load and unload methods dynamically
            // iterate through all dices and call their reset method dynamically
            foreach (var dice in _dices)
            {
                var methodName = $"{dice.Method.Name}Load";
                var method = GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
                if (method != null)
                {
                    DebugPrint($"Loading dice: {methodName}");
                    method.Invoke(this, null);
                }
            }
        }

        private void ResetDices()
        {
            DebugPrint("Resetting dices");
            // iterate through all dices and call their reset method dynamically
            foreach (var dice in _dices)
            {
                var methodName = $"{dice.Method.Name}Reset";
                var method = GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
                if (method != null)
                {
                    DebugPrint($"Resetting dice: {methodName}");
                    method.Invoke(this, null);
                }
            }
        }

        private void ResetDiceForPlayer(CCSPlayerController player)
        {
            if (!Config.AllowDiceAfterRespawn) return;
            if (player == null || player.Pawn == null || !player.Pawn.IsValid || player.Pawn.Value == null) return;
            DebugPrint($"Resetting dices for {player.PlayerName}");
            if (!_playersThatRolledTheDice.ContainsKey(player)) return;
            // remove player from list
            _playersThatRolledTheDice.Remove(player);
            // remove gui from player
            RemoveGUI(player);
            // iterate through all dices and call their reset method dynamically
            foreach (var dice in _dices)
            {
                var methodName = $"{dice.Method.Name}ResetForPlayer";
                var method = GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
                if (method != null)
                {
                    DebugPrint($"Resetting dice: {methodName} for {player.PlayerName}");
                    method.Invoke(this, [player]);
                }
            }
        }
    }
}
