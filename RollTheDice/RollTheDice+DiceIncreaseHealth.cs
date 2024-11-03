using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private string DiceIncreaseHealth(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            var random = new Random();
            var healthIncrease = random.Next(10, 30);
            if (playerPawn.Health + healthIncrease > playerPawn.MaxHealth)
            {
                playerPawn.MaxHealth = playerPawn.Health + healthIncrease;
            }
            playerPawn.Health += healthIncrease;
            Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_iHealth");
            Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_iMaxHealth");
            return $"{ChatColors.Green}{player.PlayerName}{ChatColors.Default} increased their health by {healthIncrease}!";
        }
    }
}
