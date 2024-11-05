using CounterStrikeSharp.API.Core;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private string DicePlayerHighGravity(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            playerPawn.GravityScale = 1.5f;
            return Localizer["DicePlayerHighGravity"].Value
                .Replace("{playerName}", player.PlayerName);
        }
    }
}