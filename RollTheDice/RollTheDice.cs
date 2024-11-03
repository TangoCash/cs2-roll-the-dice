using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin, IPluginConfig<PluginConfig>
    {
        public override string ModuleName => "Map Modifiers Plugin";
        public override string ModuleAuthor => "Jon-Mailes Graeffe <mail@jonni.it> / Kalle <kalle@kandru.de>";
        public override string ModuleVersion => "0.0.11";

        private string _currentMap = "";
        private List<CCSPlayerController> _playersThatRolledTheDice = new();
        private List<Func<CCSPlayerController, CCSPlayerPawn, string>> _dices = new();

        public override void Load(bool hotReload)
        {
            // initialize configuration
            LoadConfig();
            // initialize dices
            InitializeDices();
            // register listeners
            RegisterEventHandler<EventRoundStart>(OnRoundStart);
            RegisterListener<Listeners.OnServerPrecacheResources>(OnServerPrecacheResources);
            CreateDiceFastBombActionListener();
            CreateDicePlayerVampireListener();
            // print message if hot reload
            if (hotReload)
            {
                // set current map
                _currentMap = Server.MapName;
                // initialize configuration
                InitializeConfig(_currentMap);
                Console.WriteLine("[MapModifiers] Hot reload detected, restart map for all changes to take effect!");
            }
        }

        public override void Unload(bool hotReload)
        {
            Console.WriteLine("[RollTheDice] Unloaded Plugin!");
        }

        private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
        {
            // reset players that rolled the dice
            _playersThatRolledTheDice.Clear();
            // reset dice rolls on round start
            ResetDicePlayerInvisible();
            ResetDiceIncreaseSpeed();
            ResetDiceChangeName();
            ResetDiceFastBombAction();
            ResetDicePlayerVampire();
            // continue event
            return HookResult.Continue;
        }

        private void InitializeDices()
        {
            // create dynamic list containing functions to execute for each dice
            _dices = new List<Func<CCSPlayerController, CCSPlayerPawn, string>>
            {
                // add functions for each dice
                (player, playerPawn) => DiceIncreaseHealth(player, playerPawn),
                (player, playerPawn) => DiceDecreaseHealth(player, playerPawn),
                (player, playerPawn) => DiceIncreaseSpeed(player, playerPawn),
                (player, playerPawn) => DiceChangeName(player, playerPawn),
                (player, playerPawn) => DicePlayerInvisible(player, playerPawn),
                (player, playerPawn) => DicePlayerSuicide(player, playerPawn),
                (player, playerPawn) => DicePlayerRespawn(player, playerPawn),
                (player, playerPawn) => DiceStripWeapons(player, playerPawn),
                (player, playerPawn) => DiceChickenLeader(player, playerPawn),
                (player, playerPawn) => DiceFastBombAction(player, playerPawn),
                (player, playerPawn) => DicePlayerVampire(player, playerPawn),
                (player, playerPawn) => DicePlayerLowGravity(player, playerPawn),
                (player, playerPawn) => DicePlayerHighGravity(player, playerPawn),
            };
        }

        private int GetRandomDice()
        {
            // get random dice
            var random = new Random();
            return random.Next(0, _dices.Count);
        }
    }
}
