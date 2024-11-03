using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private string DiceDecreaseHealth(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            var random = new Random();
            var healthDecrease = random.Next(10, 30);
            playerPawn.Health -= healthDecrease;
            Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_iHealth");
            return $"{ChatColors.Green}{player.PlayerName}{ChatColors.Default} decreased their health by {healthDecrease}!";
        }
    }
}
