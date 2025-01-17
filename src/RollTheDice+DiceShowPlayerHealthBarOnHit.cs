using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.UserMessages;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private List<CCSPlayerController> _playersWithHealthBarShown = new();

        private Dictionary<string, string> DiceShowPlayerHealthBarOnHit(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            // create listener if not exists
            if (_playersWithHealthBarShown.Count() == 0)
            {
                RegisterEventHandler<EventPlayerHurt>(EventDiceShowPlayerHealthBarOnHitOnPlayerHurt);
            }
            _playersWithHealthBarShown.Add(player);
            return new Dictionary<string, string>
            {
                {"_translation_player", "DiceShowPlayerHealthBarOnHitPlayer"},
                {"_translation_other", "DiceShowPlayerHealthBarOnHit"},
                { "playerName", player.PlayerName }
            };
        }

        private void DiceShowPlayerHealthBarOnHitUnload()
        {
            DiceShowPlayerHealthBarOnHitReset();
        }

        private void DiceShowPlayerHealthBarOnHitReset()
        {
            DeregisterEventHandler<EventPlayerHurt>(EventDiceShowPlayerHealthBarOnHitOnPlayerHurt);
            _playersWithHealthBarShown.Clear();
        }

        private HookResult EventDiceShowPlayerHealthBarOnHitOnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
        {
            CCSPlayerController? victim = @event.Userid;
            CCSPlayerController? attacker = @event.Attacker;
            if (victim == null
                || !victim.IsValid
                || victim.PlayerPawn == null
                || !victim.PlayerPawn.IsValid
                || victim.PlayerPawn.Value == null
                || attacker == null
                || !attacker.IsValid) return HookResult.Continue;
            if (victim == attacker) return HookResult.Continue;
            if (!_playersWithHealthBarShown.Contains(attacker)) return HookResult.Continue;
            var message = UserMessage.FromPartialName("UpdateScreenHealthBar");
            float oldHealth = @event.Health + @event.DmgHealth;
            float newHealth = @event.Health;
            if (oldHealth == newHealth) return HookResult.Continue;
            message.SetInt("entidx", (int)victim.PlayerPawn.Index);
            message.SetFloat("healthratio_old", oldHealth / victim.PlayerPawn.Value!.MaxHealth);
            message.SetFloat("healthratio_new", newHealth / victim.PlayerPawn.Value!.MaxHealth);
            message.SetInt("style", 0);
            message.Send(attacker);
            return HookResult.Continue;
        }
    }
}
