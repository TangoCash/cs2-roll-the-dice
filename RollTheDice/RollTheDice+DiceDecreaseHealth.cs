using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

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
            return Localizer["DiceDecreaseHealth"].Value
                .Replace("{playerName}", player.PlayerName)
                .Replace("{healthDecrease}", healthDecrease.ToString());
        }
    }
}
