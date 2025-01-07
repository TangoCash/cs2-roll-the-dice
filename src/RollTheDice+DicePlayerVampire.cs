using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private List<CCSPlayerController> _playerVampires = new();

        private Dictionary<string, string> DicePlayerVampire(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            // create listener if not exists
            if (_playerVampires.Count() == 0) RegisterEventHandler<EventPlayerHurt>(EventDicePlayerVampireOnPlayerHurt);
            _playerVampires.Add(player);
            return new Dictionary<string, string>
            {
                {"_translation_player", "DicePlayerVampirePlayer"},
                {"_translation_other", "DicePlayerVampire"},
                { "playerName", player.PlayerName }
            };
        }

        private void DicePlayerVampireUnload()
        {
            DicePlayerVampireReset();
        }

        private void DicePlayerVampireReset()
        {
            DeregisterEventHandler<EventPlayerHurt>(EventDicePlayerVampireOnPlayerHurt);
            _playerVampires.Clear();
        }

        private HookResult EventDicePlayerVampireOnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
        {
            var attacker = @event.Attacker;
            var victim = @event.Userid;
            if (attacker == null || victim == null) return HookResult.Continue;
            if (!_playerVampires.Contains(attacker)) return HookResult.Continue;
            var playerPawn = attacker.PlayerPawn.Value;
            if (playerPawn == null) return HookResult.Continue;
            playerPawn.Health += (int)float.Round(@event.DmgHealth);
            if (playerPawn.Health > 200) playerPawn.Health = 200;
            attacker.PrintToCenterAlert($"+{(int)float.Round(@event.DmgHealth)} health!");
            Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_iHealth");
            return HookResult.Continue;
        }
    }
}
