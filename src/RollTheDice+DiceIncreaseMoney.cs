using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private Dictionary<string, string> DiceIncreaseMoney(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            Dictionary<string, object> config = GetDiceConfig("DiceIncreaseMoney");
            var moneyIncrease = _random.Next(
                Convert.ToInt32(config["min_money"]),
                Convert.ToInt32(config["max_money"]) + 1
            );
            player.InGameMoneyServices!.Account += moneyIncrease;
            Utilities.SetStateChanged(player, "CCSPlayerController", "m_pInGameMoneyServices");
            return new Dictionary<string, string>
            {
                { "playerName", player.PlayerName },
                { "money", moneyIncrease.ToString() }
            };
        }

        private Dictionary<string, object> DiceIncreaseMoneyConfig()
        {
            var config = new Dictionary<string, object>();
            config["min_money"] = (int)100;
            config["max_money"] = (int)1000;
            return config;
        }
    }
}
