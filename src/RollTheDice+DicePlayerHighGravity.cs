using CounterStrikeSharp.API.Core;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private Dictionary<string, string> DicePlayerHighGravity(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            playerPawn.GravityScale = 4f;
            return new Dictionary<string, string>
            {
                {"_translation", "DicePlayerHighGravity"},
                { "playerName", player.PlayerName }
            };
        }
    }
}