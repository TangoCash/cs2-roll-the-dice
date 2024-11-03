using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private string DicePlayerSuicide(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            playerPawn.CommitSuicide(true, true);
            return Localizer["DicePlayerSuicide"].Value
                .Replace("{playerName}", player.PlayerName);
        }
    }
}
