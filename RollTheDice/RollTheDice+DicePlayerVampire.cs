using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private List<CCSPlayerController> _playerVampires = new();

        private string DicePlayerVampire(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            _playerVampires.Add(player);
            return Localizer["DicePlayerVampire"].Value
                .Replace("{playerName}", player.PlayerName);
        }

        private void ResetDicePlayerVampire()
        {
            _playerVampires.Clear();
        }

        private void CreateDicePlayerVampireListener()
        {
            RegisterEventHandler<EventPlayerHurt>((@event, _) =>
            {
                var attacker = @event.Attacker;
                if (attacker == null) return HookResult.Continue;
                var playerPawn = attacker.PlayerPawn.Value;
                if (playerPawn == null) return HookResult.Continue;
                if (!_playerVampires.Contains(attacker)) return HookResult.Continue;
                playerPawn.Health += (int)float.Round(@event.DmgHealth);
                if (playerPawn.Health > 200) playerPawn.Health = 200;
                attacker.PrintToCenterAlert($"+{(int)float.Round(@event.DmgHealth)} health!");
                Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_iHealth");
                return HookResult.Continue;
            });
        }
    }
}
