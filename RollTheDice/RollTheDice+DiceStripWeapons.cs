using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private string DiceStripWeapons(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            var playerWeapons = playerPawn.WeaponServices!;
            foreach (var weapon in playerWeapons.MyWeapons)
            {
                weapon.Value!.Remove();
            }
            player.GiveNamedItem("weapon_knife");
            return $"{ChatColors.Green}{player.PlayerName}{ChatColors.Default} lost all weapons!";
        }
    }
}
