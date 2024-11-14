using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private string DiceStripWeapons(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            if (playerPawn.WeaponServices == null) return Localizer["command.rollthedice.error"].Value.Replace("{playerName}", player.PlayerName);
            var playerWeapons = playerPawn.WeaponServices!;
            foreach (var weapon in playerWeapons.MyWeapons)
            {
                // ignore unknown weapons
                if (weapon.Value == null || weapon.Value != null && weapon.Value.DesignerName == null) continue;
                if (weapon.Value!.DesignerName == $"weapon_{CsItem.C4.ToString().ToLower()}"
                    || weapon.Value!.DesignerName == $"weapon_{CsItem.Bomb.ToString().ToLower()}")
                {
                    // change weapon to currently active weapon
                    playerPawn.WeaponServices.ActiveWeapon.Raw = weapon.Raw;
                    // drop active weapon
                    player.DropActiveWeapon();
                }
            }
            AddTimer(0.1f, () =>
            {
                if (player == null || !player.IsValid) return;
                player.RemoveWeapons();
                player.GiveNamedItem("weapon_knife");
            });

            return Localizer["DiceStripWeapons"].Value
                .Replace("{playerName}", player.PlayerName);
        }
    }
}
