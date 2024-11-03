using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private List<CCSPlayerController> _playersWithIncreasedSpeed = new();

        private string DiceIncreaseSpeed(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            _playersWithIncreasedSpeed.Add(player);
            var random = new Random();
            var speedIncrease = random.NextDouble() * (1.5 - 1.1) + 1.1;
            playerPawn.VelocityModifier *= (float)speedIncrease;
            Utilities.SetStateChanged(playerPawn, "CCSPlayerPawn", "m_flVelocityModifier");
            var percentageIncrease = (speedIncrease - 1.0) * 100;
            return $"{ChatColors.Green}{player.PlayerName}{ChatColors.Default} increased their speed by {percentageIncrease:F2}%!";
        }

        private void ResetDiceIncreaseSpeed()
        {
            // iterate through all players
            foreach (var player in _playersWithIncreasedSpeed)
            {
                if (player == null || !player.PlayerPawn.IsValid || player.PlayerPawn.Value == null || player.LifeState != (byte)LifeState_t.LIFE_ALIVE) continue;
                // get player pawn
                var playerPawn = player.PlayerPawn.Value!;
                // reset player speed
                playerPawn.VelocityModifier = 1.0f;
                // set state changed
                Utilities.SetStateChanged(playerPawn, "CCSPlayerPawn", "m_flVelocityModifier");
            }
            _playersWithIncreasedSpeed.Clear();
        }
    }
}
