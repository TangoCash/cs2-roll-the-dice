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
            uint weaponRaw = 0;
            foreach (var weapon in playerWeapons.MyWeapons)
            {
                // ignore unknown weapons
                if (weapon.Value == null || weapon.Value != null && weapon.Value.DesignerName == null) continue;
                // ignore knife and C4
                if (weapon.Value!.DesignerName == $"weapon_{CsItem.C4.ToString().ToLower()}"
                    || weapon.Value!.DesignerName == "weapon_knife" // necessary because CsItem.Knife is not always this value
                    || weapon.Value!.DesignerName == $"weapon_{CsItem.Knife.ToString().ToLower()}"
                    || weapon.Value!.DesignerName == $"weapon_{CsItem.KnifeCT.ToString().ToLower()}"
                    || weapon.Value!.DesignerName == $"weapon_{CsItem.KnifeT.ToString().ToLower()}"
                    || weapon.Value!.DesignerName == $"weapon_{CsItem.DefaultKnifeCT.ToString().ToLower()}"
                    || weapon.Value!.DesignerName == $"weapon_{CsItem.DefaultKnifeT.ToString().ToLower()}")
                {
                    // save weapon raw
                    weaponRaw = weapon.Raw;
                    continue;
                }
                // change weapon to currently active weapon
                playerPawn.WeaponServices.ActiveWeapon.Raw = weapon.Raw;
                // drop active weapon
                player.DropActiveWeapon();
                // delete it
                weapon.Value.Remove();
            }
            // put knife in hand of player
            playerPawn.WeaponServices.ActiveWeapon.Raw = weaponRaw;
            return Localizer["DiceStripWeapons"].Value
                .Replace("{playerName}", player.PlayerName);
        }
    }
}
