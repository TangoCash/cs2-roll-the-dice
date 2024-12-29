using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private List<CCSPlayerController> _playersWithIncreasedSpeed = new();
        private Dictionary<CCSPlayerController, float> _playersWithIncreasedSpeedValue = new();

        private Dictionary<string, string> DiceIncreaseSpeed(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            _playersWithIncreasedSpeed.Add(player);
            var speedIncrease = _random.NextDouble() * (2.0 - 1.5) + 1.5;
            playerPawn.VelocityModifier *= (float)speedIncrease;
            _playersWithIncreasedSpeedValue[player] = (float)playerPawn.VelocityModifier;
            Utilities.SetStateChanged(playerPawn, "CCSPlayerPawn", "m_flVelocityModifier");
            var percentageIncrease = (speedIncrease - 1.0) * 100;
            return new Dictionary<string, string>
            {
                {"_translation_player", "DiceIncreaseSpeedPlayer"},
                {"_translation_other", "DiceIncreaseSpeed"},
                { "playerName", player.PlayerName },
                { "percentageIncrease", Math.Round(percentageIncrease, 2).ToString() }
            };
        }

        private void DiceIncreaseSpeedLoad()
        {
            RegisterEventHandler<EventPlayerHurt>(EventDiceIncreaseSpeedOnPlayerHurt);
        }

        private void DiceIncreaseSpeedUnload()
        {
            DiceIncreaseSpeedReset();
        }

        private void DiceIncreaseSpeedReset()
        {
            // iterate through all players
            foreach (var player in _playersWithIncreasedSpeed)
            {
                if (player == null || player.PlayerPawn == null || !player.PlayerPawn.IsValid || player.PlayerPawn.Value == null || player.LifeState != (byte)LifeState_t.LIFE_ALIVE) continue;
                // get player pawn
                var playerPawn = player.PlayerPawn.Value!;
                // reset player speed
                playerPawn.VelocityModifier = 1.0f;
                // set state changed
                Utilities.SetStateChanged(playerPawn, "CCSPlayerPawn", "m_flVelocityModifier");
            }
            _playersWithIncreasedSpeed.Clear();
            _playersWithIncreasedSpeedValue.Clear();
        }

        private HookResult EventDiceIncreaseSpeedOnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
        {
            var victim = @event.Userid;
            if (victim == null) return HookResult.Continue;
            if (!_playersWithIncreasedSpeed.Contains(victim)) return HookResult.Continue;
            if (victim == null || victim.PlayerPawn == null || !victim.PlayerPawn.IsValid || victim.PlayerPawn.Value == null || victim.LifeState != (byte)LifeState_t.LIFE_ALIVE) return HookResult.Continue;
            var playerPawn = victim.PlayerPawn.Value!;
            AddTimer(0f, () =>
            {
                if (playerPawn == null || !playerPawn.IsValid) return;
                // set player speed
                playerPawn.VelocityModifier = _playersWithIncreasedSpeedValue[victim];
                // set state changed
                Utilities.SetStateChanged(playerPawn, "CCSPlayerPawn", "m_flVelocityModifier");
            });
            return HookResult.Continue;
        }
    }
}
