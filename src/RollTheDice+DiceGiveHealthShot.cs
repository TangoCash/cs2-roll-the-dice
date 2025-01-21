using CounterStrikeSharp.API.Core;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private Dictionary<string, string> DiceGiveHealthShot(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            int amount = _random.Next(1, 5);
            for (int i = 0; i < amount; i++)
            {
                player.GiveNamedItem("weapon_healthshot");
            }
            return new Dictionary<string, string>
            {
                { "playerName", player.PlayerName },
                { "amount", amount.ToString() }
            };
        }
    }
}
