using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private Dictionary<string, string> DiceIncreaseHealth(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            Dictionary<string, object> config = GetDiceConfig("DiceIncreaseHealth");
            var healthIncrease = _random.Next(
                Convert.ToInt32(config["min_health"]),
                Convert.ToInt32(config["max_health"]) + 1
            );
            if (playerPawn.Health + healthIncrease > playerPawn.MaxHealth)
            {
                playerPawn.MaxHealth = playerPawn.Health + healthIncrease;
            }
            playerPawn.Health += healthIncrease;
            Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_iHealth");
            Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_iMaxHealth");
            return new Dictionary<string, string>
            {
                { "playerName", player.PlayerName },
                { "healthIncrease", healthIncrease.ToString() }
            };
        }

        private Dictionary<string, object> DiceIncreaseHealthConfig()
        {
            var config = new Dictionary<string, object>();
            config["min_health"] = (int)10;
            config["max_health"] = (int)30;
            return config;
        }
    }
}
