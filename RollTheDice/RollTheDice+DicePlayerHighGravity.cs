using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private List<CCSPlayerController> _playersWithHighGravity = new();

        private string DicePlayerHighGravity(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            playerPawn.GravityScale = 1.5f;
            return $"{ChatColors.Green}{player.PlayerName}{ChatColors.Default} got high gravity!";
        }
    }
}