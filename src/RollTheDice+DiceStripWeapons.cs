using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private Dictionary<string, string> DiceStripWeapons(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            if (playerPawn.WeaponServices == null)
                return new Dictionary<string, string>
                {
                    {"_translation_player", "command.rollthedice.error"},
                    { "playerName", player.PlayerName }
                };
            var playerWeapons = playerPawn.WeaponServices!;
            bool hasC4 = false;
            foreach (var weapon in playerWeapons.MyWeapons)
            {
                // ignore unknown weapons
                if (weapon.Value == null || weapon.Value != null && weapon.Value.DesignerName == null) continue;
                if (weapon.Value!.DesignerName == CsItem.C4.ToString().ToLower()
                    || weapon.Value!.DesignerName == CsItem.Bomb.ToString().ToLower()
                    || weapon.Value!.DesignerName == $"weapon_{CsItem.C4.ToString().ToLower()}"
                    || weapon.Value!.DesignerName == $"weapon_{CsItem.Bomb.ToString().ToLower()}")
                {
                    hasC4 = true;
                }
            }
            player.RemoveWeapons();
            player.GiveNamedItem(CsItem.Knife);
            if (hasC4)
                player.GiveNamedItem(CsItem.C4);
            return new Dictionary<string, string>
            {
                {"_translation_player", "DiceStripWeaponsPlayer"},
                {"_translation_other", "DiceStripWeapons"},
                { "playerName", player.PlayerName }
            };
        }
    }
}
