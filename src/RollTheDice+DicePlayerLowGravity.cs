using CounterStrikeSharp.API.Core;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private Dictionary<string, string> DicePlayerLowGravity(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            Dictionary<string, object> config = GetDiceConfig("DicePlayerLowGravity");
            playerPawn.GravityScale = (float)config["gravity_scale"];
            return new Dictionary<string, string>
            {
                { "playerName", player.PlayerName }
            };
        }

        private Dictionary<string, object> DicePlayerLowGravityConfig()
        {
            var config = new Dictionary<string, object>();
            config["gravity_scale"] = (float)0.4f;
            return config;
        }
    }
}
