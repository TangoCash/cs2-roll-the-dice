using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private string DicePlayerRespawn(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            player.Respawn();
            return $"{ChatColors.Green}{player.PlayerName}{ChatColors.Default} got respawned!";
        }
    }
}
