using CounterStrikeSharp.API.Core;

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
