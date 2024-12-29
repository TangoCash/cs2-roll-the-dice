using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private List<CCSPlayerController> _playersCanInstantDefuse = new();
        private List<CCSPlayerController> _playersCanInstantPlant = new();

        private Dictionary<string, string> DiceFastBombAction(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            if (playerPawn.TeamNum == (int)CsTeam.Terrorist)
            {
                _playersCanInstantPlant.Add(player);
                return new Dictionary<string, string>
                {
                    {"_translation_player", "DiceFastBombActionTPlayer"},
                    {"_translation_other", "DiceFastBombActionT"},
                    { "playerName", player.PlayerName }
                };
            }
            else if (playerPawn.TeamNum == (int)CsTeam.CounterTerrorist)
            {
                _playersCanInstantDefuse.Add(player);
                return new Dictionary<string, string>
                {
                    {"_translation_player", "DiceFastBombActionCTPlayer"},
                    {"_translation_other", "DiceFastBombActionCT"},
                    { "playerName", player.PlayerName }
                };
            }
            else
            {
                return new Dictionary<string, string>
                {
                    {"_translation_player", "command.rollthedice.error"},
                    { "playerName", player.PlayerName }
                };
            }
        }

        private void DiceFastBombActionLoad()
        {
            RegisterEventHandler<EventBombBegindefuse>(DiceFastBombActionEventBeginDefuse);
            RegisterEventHandler<EventBombBeginplant>(DiceFastBombActionEventBeginPlant);
        }

        private void DiceFastBombActionUnload()
        {
            DiceFastBombActionReset();
        }

        private void DiceFastBombActionReset()
        {
            _playersCanInstantDefuse.Clear();
            _playersCanInstantPlant.Clear();
        }

        private HookResult DiceFastBombActionEventBeginDefuse(EventBombBegindefuse @event, GameEventInfo info)
        {
            var player = @event.Userid;
            if (player == null) return HookResult.Continue;
            if (!_playersCanInstantDefuse.Contains(player)) return HookResult.Continue;
            var playerPawn = player.PlayerPawn.Value;
            if (playerPawn == null) return HookResult.Continue;
            var c4 = Utilities.FindAllEntitiesByDesignerName<CPlantedC4>("planted_c4").First();
            Server.NextFrame(() =>
            {
                if (c4 == null) return;
                c4.DefuseCountDown = Server.CurrentTime;
            });
            return HookResult.Continue;
        }

        private HookResult DiceFastBombActionEventBeginPlant(EventBombBeginplant @event, GameEventInfo info)
        {
            var player = @event.Userid;
            if (player == null) return HookResult.Continue;
            if (!_playersCanInstantPlant.Contains(player)) return HookResult.Continue;
            var playerPawn = player.PlayerPawn.Value;
            if (playerPawn == null) return HookResult.Continue;
            var weaponService = playerPawn.WeaponServices?.ActiveWeapon;
            if (weaponService == null) return HookResult.Continue;
            var activeWeapon = weaponService.Value;
            if (activeWeapon == null) return HookResult.Continue;
            if (!activeWeapon.DesignerName.Contains("c4")) return HookResult.Continue;
            var c4 = new CC4(activeWeapon.Handle);
            Server.NextFrame(() =>
            {
                if (c4 == null) return;
                c4.ArmedTime = Server.CurrentTime;
            });
            return HookResult.Continue;
        }
    }
}
