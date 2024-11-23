using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private Dictionary<string, string> DiceDecreaseHealth(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            var healthDecrease = _random.Next(10, 30);
            playerPawn.Health -= healthDecrease;
            Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_iHealth");
            return new Dictionary<string, string>
            {
                {"_translation", "DiceDecreaseHealth"},
                { "playerName", player.PlayerName },
                { "healthDecrease", healthDecrease.ToString() }
            };
        }
    }
}
