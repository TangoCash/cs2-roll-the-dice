using CounterStrikeSharp.API.Core;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private Dictionary<string, string> DiceGiveHealthShot(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            Dictionary<string, object> config = GetDiceConfig("DiceGiveHealthShot");
            int amount = _random.Next(
                Convert.ToInt32(config["min_healthshots"]),
                Convert.ToInt32(config["max_healthshots"]) + 1
            );
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

        private Dictionary<string, object> DiceGiveHealthShotConfig()
        {
            var config = new Dictionary<string, object>();
            config["min_healthshots"] = (int)1;
            config["max_healthshots"] = (int)5;
            return config;
        }
    }
}
