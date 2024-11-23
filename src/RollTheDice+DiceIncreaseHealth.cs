using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private Dictionary<string, string> DiceIncreaseHealth(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            var healthIncrease = _random.Next(10, 30);
            if (playerPawn.Health + healthIncrease > playerPawn.MaxHealth)
            {
                playerPawn.MaxHealth = playerPawn.Health + healthIncrease;
            }
            playerPawn.Health += healthIncrease;
            Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_iHealth");
            Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_iMaxHealth");
            return new Dictionary<string, string>
            {
                {"_translation", "DiceIncreaseHealth"},
                { "playerName", player.PlayerName },
                { "healthIncrease", healthIncrease.ToString() }
            };
        }
    }
}
