using CounterStrikeSharp.API.Core;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private string DicePlayerLowGravity(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            playerPawn.GravityScale = 0.4f;
            return Localizer["DicePlayerLowGravity"].Value
                .Replace("{playerName}", player.PlayerName);
        }
    }
}
