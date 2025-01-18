using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.UserMessages;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private List<CCSPlayerController> _playersWithHealthBarShown = new();

        private Dictionary<string, string> DiceShowPlayerHealthBar(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            // create listener if not exists
            if (_playersWithHealthBarShown.Count() == 0)
            {
                RegisterListener<Listeners.OnTick>(DiceShowPlayerHealthBarOnTick);
                RegisterEventHandler<EventPlayerHurt>(EventDiceShowPlayerHealthBarOnPlayerHurt);
            }
            _playersWithHealthBarShown.Add(player);
            return new Dictionary<string, string>
            {
                {"_translation_player", "DiceShowPlayerHealthBarPlayer"},
                {"_translation_other", "DiceShowPlayerHealthBar"},
                { "playerName", player.PlayerName }
            };
        }

        private void DiceShowPlayerHealthBarUnload()
        {
            DiceShowPlayerHealthBarReset();
        }

        private void DiceShowPlayerHealthBarReset()
        {
            RemoveListener<Listeners.OnTick>(DiceShowPlayerHealthBarOnTick);
            DeregisterEventHandler<EventPlayerHurt>(EventDiceShowPlayerHealthBarOnPlayerHurt);
            _playersWithHealthBarShown.Clear();
        }

        private HookResult EventDiceShowPlayerHealthBarOnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
        {
            CCSPlayerController? victim = @event.Userid;
            CCSPlayerController? attacker = @event.Attacker;
            if (victim == null
                || !victim.IsValid
                || victim.PlayerPawn == null
                || !victim.PlayerPawn.IsValid
                || victim.PlayerPawn.Value == null
                || attacker == null
                || !attacker.IsValid
                || victim.TeamNum == attacker.TeamNum) return HookResult.Continue;
            if (victim == attacker) return HookResult.Continue;
            if (!_playersWithHealthBarShown.Contains(attacker)) return HookResult.Continue;
            float oldHealth = @event.Health + @event.DmgHealth;
            float newHealth = @event.Health;
            if (oldHealth == newHealth) return HookResult.Continue;
            // send message
            var message = UserMessage.FromPartialName("UpdateScreenHealthBar");
            message.SetInt("entidx", (int)victim.PlayerPawn.Index);
            message.SetFloat("healthratio_old", oldHealth / victim.PlayerPawn.Value!.MaxHealth);
            message.SetFloat("healthratio_new", newHealth / victim.PlayerPawn.Value!.MaxHealth);
            message.SetInt("style", 0);
            message.Send(attacker);
            return HookResult.Continue;
        }

        private void DiceShowPlayerHealthBarOnTick()
        {
            if (_playersWithHealthBarShown.Count() == 0) return;
            // only every 32 ticks (roughly one second)
            if (Server.TickCount % 8 != 0) return;
            // worker
            List<CCSPlayerController> _playersWithHealthBarShownCopy = new(_playersWithHealthBarShown);
            foreach (CCSPlayerController player in _playersWithHealthBarShownCopy)
            {
                try
                {
                    // sanity checks
                    if (player == null
                    || player.PlayerPawn == null
                    || !player.PlayerPawn.IsValid
                    || player.PlayerPawn.Value == null
                    || player.PlayerPawn.Value.LifeState != (byte)LifeState_t.LIFE_ALIVE) continue;
                    // check if player is aiming at another player
                    CCSPlayerPawn? playerTarget = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules")?
                        .FirstOrDefault()?
                        .GameRules?
                        .FindPickerEntity<CCSPlayerPawn>(player);
                    if (playerTarget == null
                        || !playerTarget.IsValid
                        || playerTarget.LifeState != (byte)LifeState_t.LIFE_ALIVE
                        || playerTarget.TeamNum == player.TeamNum) continue;
                    // send message
                    var message = UserMessage.FromPartialName("UpdateScreenHealthBar");
                    message.SetInt("entidx", (int)playerTarget.Index);
                    message.SetFloat("healthratio_old", playerTarget.Health / 100);
                    message.SetFloat("healthratio_new", playerTarget.Health / 100);
                    message.SetInt("style", 0);
                    message.Send(player);
                }
                catch (Exception e)
                {
                    // remove player
                    _playersWithCloak.Remove(player);
                    // log error
                    Console.WriteLine(Localizer["core.error"].Value.Replace("{error}", e.Message));
                }
            }
        }
    }
}
