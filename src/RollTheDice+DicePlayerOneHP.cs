using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private string DicePlayerOneHP(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            playerPawn.Health = 1;
            Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_iHealth");
            return Localizer["DicePlayerOneHP"].Value
                .Replace("{playerName}", player.PlayerName);
        }
    }
}
