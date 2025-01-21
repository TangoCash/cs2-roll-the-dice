using CounterStrikeSharp.API.Core;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private Dictionary<string, string> DicePlayerLowGravity(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            playerPawn.GravityScale = 0.4f;
            return new Dictionary<string, string>
            {
                { "playerName", player.PlayerName }
            };
        }
    }
}
