using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private Dictionary<string, string> DicePlayerOneHP(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            playerPawn.Health = 1;
            Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_iHealth");
            return new Dictionary<string, string>
            {
                { "playerName", player.PlayerName }
            };
        }
    }
}
