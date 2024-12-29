using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System.Reflection;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin, IPluginConfig<PluginConfig>
    {
        public override string ModuleName => "Roll The Dice";
        public override string ModuleAuthor => "Jon-Mailes Graeffe <mail@jonni.it> / Kalle <kalle@kandru.de>";

        private string _currentMap = "";
        private List<CCSPlayerController> _playersThatRolledTheDice = new();
        private List<Func<CCSPlayerController, CCSPlayerPawn, Dictionary<string, string>>> _dices = new();
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
            // iterate through all dices and call their unload method dynamically
            foreach (var dice in _dices)
            {
                var methodName = $"{dice.Method.Name}Unload";
                var method = GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
                if (method != null) method.Invoke(this, null);
            }
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
            // run all dices' initialization methods
            // TODO: check after each map load and unload if dice is enabled
            // and run load and unload methods dynamically
            // iterate through all dices and call their reset method dynamically
            foreach (var dice in _dices)
            {
                var methodName = $"{dice.Method.Name}Load";
                var method = GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
                if (method != null) method.Invoke(this, null);
            }
        }

        private void ResetDices()
        {
            // iterate through all dices and call their reset method dynamically
            foreach (var dice in _dices)
            {
                var methodName = $"{dice.Method.Name}Reset";
                var method = GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
                if (method != null) method.Invoke(this, null);
            }
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
