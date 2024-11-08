using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private List<CCSPlayerController> _playersCanInstantDefuse = new();
        private List<CCSPlayerController> _playersCanInstantPlant = new();

        private string DiceFastBombAction(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            if (playerPawn.TeamNum == (int)CsTeam.Terrorist)
            {
                _playersCanInstantPlant.Add(player);
                return Localizer["DiceFastBombActionT"].Value
                    .Replace("{playerName}", player.PlayerName);

            }
            else if (playerPawn.TeamNum == (int)CsTeam.CounterTerrorist)
            {
                _playersCanInstantDefuse.Add(player);
                return Localizer["DiceFastBombActionCT"].Value
                    .Replace("{playerName}", player.PlayerName);
            }
            else
            {
                return Localizer["command.rollthedice.error"].Value
                    .Replace("{playerName}", player.PlayerName);
            }
        }

        private void ResetDiceFastBombAction()
        {
            _playersCanInstantDefuse.Clear();
            _playersCanInstantPlant.Clear();
        }

        private void CreateDiceFastBombActionEventHandlers()
        {
            DiceFastBombActionEventBeginDefuse();
            DiceFastBombActionEventBeginPlant();
        }

        private void DiceFastBombActionEventBeginDefuse()
        {
            RegisterEventHandler<EventBombBegindefuse>((@event, info) =>
            {
                var player = @event.Userid;
                if (player == null) return HookResult.Continue;
                var playerPawn = player.PlayerPawn.Value;
                if (playerPawn == null) return HookResult.Continue;
                if (!_playersCanInstantDefuse.Contains(player)) return HookResult.Continue;
                var bomb = Utilities.FindAllEntitiesByDesignerName<CPlantedC4>("planted_c4").First();
                Server.NextFrame(() =>
                {
                    bomb.DefuseCountDown = Server.CurrentTime;
                });
                return HookResult.Continue;
            });
        }

        private void DiceFastBombActionEventBeginPlant()
        {
            RegisterEventHandler<EventBombBeginplant>((@event, info) =>
            {
                var player = @event.Userid;
                if (player == null) return HookResult.Continue;
                var playerPawn = player.PlayerPawn.Value;
                if (playerPawn == null) return HookResult.Continue;
                if (!_playersCanInstantPlant.Contains(player)) return HookResult.Continue;
                var weaponService = playerPawn.WeaponServices?.ActiveWeapon;
                if (weaponService == null) return HookResult.Continue;
                var activeWeapon = weaponService.Value;
                if (activeWeapon == null) return HookResult.Continue;
                if (!activeWeapon.DesignerName.Contains("c4")) return HookResult.Continue;
                var c4 = new CC4(activeWeapon.Handle);
                Server.NextFrame(() =>
                {
                    c4.ArmedTime = Server.CurrentTime;
                });
                return HookResult.Continue;
            });
        }
    }
}
