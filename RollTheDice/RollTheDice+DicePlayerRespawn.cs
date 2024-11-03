using CounterStrikeSharp.API.Core;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private string DicePlayerRespawn(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            player.Respawn();
            return Localizer["DicePlayerRespawn"].Value
                .Replace("{playerName}", player.PlayerName);
        }
    }
}
