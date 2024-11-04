using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private string DiceIncreaseHealth(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            var healthIncrease = _random.Next(10, 30);
            if (playerPawn.Health + healthIncrease > playerPawn.MaxHealth)
            {
                playerPawn.MaxHealth = playerPawn.Health + healthIncrease;
            }
            playerPawn.Health += healthIncrease;
            Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_iHealth");
            Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_iMaxHealth");
            return Localizer["DiceIncreaseHealth"].Value
                .Replace("{playerName}", player.PlayerName)
                .Replace("{healthIncrease}", healthIncrease.ToString());
        }
    }
}
