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
            // create listener if not exists
            if (_playersWithIncreasedSpeed.Count() == 0)
            {
                RegisterEventHandler<EventPlayerHurt>(EventDiceIncreaseSpeedOnPlayerHurt);
            }
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

        private void DiceIncreaseSpeedUnload()
        {
            DiceIncreaseSpeedReset();
        }

        private void DiceIncreaseSpeedReset()
        {
            DeregisterEventHandler<EventPlayerHurt>(EventDiceIncreaseSpeedOnPlayerHurt);
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
            AddTimer(0f, () =>
            {
                if (victim == null || !victim.IsValid
                    || victim.PlayerPawn == null || !victim.PlayerPawn.IsValid) return;
                CCSPlayerPawn playerPawn = victim.PlayerPawn.Value!;
                // set player speed
                playerPawn.VelocityModifier = _playersWithIncreasedSpeedValue[victim];
                // set state changed
                Utilities.SetStateChanged(playerPawn, "CCSPlayerPawn", "m_flVelocityModifier");
            });
            return HookResult.Continue;
        }
    }
}
