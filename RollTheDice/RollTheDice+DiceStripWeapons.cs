using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private string DiceStripWeapons(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            var playerWeapons = playerPawn.WeaponServices!;
            foreach (var weapon in playerWeapons.MyWeapons)
            {
                // ignore unknown weapons
                if (weapon.Value == null || weapon.Value != null && weapon.Value.DesignerName == null) continue;
                // ignore C4
                if (weapon.Value!.DesignerName == $"weapon_{CsItem.C4.ToString().ToLower()}")
                    continue;
                weapon.Value!.Remove();
            }
            // give knife to player to force update of view
            player.GiveNamedItem(CsItem.Knife);
            return Localizer["DiceStripWeapons"].Value
                .Replace("{playerName}", player.PlayerName);
        }
    }
}
