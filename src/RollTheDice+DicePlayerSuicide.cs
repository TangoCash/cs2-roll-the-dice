using CounterStrikeSharp.API.Core;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private Dictionary<string, string> DicePlayerSuicide(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            playerPawn.CommitSuicide(true, true);
            return new Dictionary<string, string>
            {
                {"_translation_player", "DicePlayerSuicidePlayer"},
                {"_translation_other", "DicePlayerSuicide"},
                { "playerName", player.PlayerName }
            };
        }
    }
}
