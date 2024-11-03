using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private string DicePlayerSuicide(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            playerPawn.CommitSuicide(true, true);
            return $"{ChatColors.Green}{player.PlayerName}{ChatColors.Default} exploded!";
        }
    }
}
