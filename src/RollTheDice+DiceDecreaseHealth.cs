using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private Dictionary<string, string> DiceDecreaseHealth(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            Dictionary<string, object> config = GetDiceConfig("DiceDecreaseHealth");
            var healthDecrease = _random.Next(
                Convert.ToInt32(config["min_health"]),
                Convert.ToInt32(config["max_health"]) + 1
            );
            playerPawn.Health -= healthDecrease;
            Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_iHealth");
            return new Dictionary<string, string>
            {
                { "playerName", player.PlayerName },
                { "healthDecrease", healthDecrease.ToString() }
            };
        }

        private Dictionary<string, object> DiceDecreaseHealthConfig()
        {
            var config = new Dictionary<string, object>();
            config["min_health"] = (int)10;
            config["max_health"] = (int)30;
            return config;
        }
    }
}
