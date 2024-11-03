using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private string DicePlayerOneHP(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            playerPawn.Health = 1;
            Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_iHealth");
            return $"{ChatColors.Green}{player.PlayerName}{ChatColors.Default} has only 1HP left!";
        }
    }
}
