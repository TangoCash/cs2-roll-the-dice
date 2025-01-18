using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Core.Attributes.Registration;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private List<CCSPlayerController> _playersCanInstantDefuse = new();
        private List<CCSPlayerController> _playersCanInstantPlant = new();
        private List<CCSPlayerController> _playersCanInstantRescueHostages = new();

        private Dictionary<string, string> DiceFastMapAction(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            // check for map entities
            var bombEntities = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("weapon_c4").ToArray();
            var hostageEntities = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("hostage_entity").ToArray();
            // if bomb map
            if (bombEntities.Length > 0)
            {
                // create listener if not exists
                if (_playersCanInstantDefuse.Count() == 0 && _playersCanInstantPlant.Count() == 0)
                {
                    RegisterEventHandler<EventBombBegindefuse>(DiceFastBombActionEventBeginDefuse);
                    RegisterEventHandler<EventBombBeginplant>(DiceFastBombActionEventBeginPlant);
                }
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
            else if (hostageEntities.Length > 0)
            {
                if (playerPawn.TeamNum == (int)CsTeam.CounterTerrorist)
                {
                    _playersCanInstantRescueHostages.Add(player);
                    return new Dictionary<string, string>
                    {
                        {"_translation_player", "DiceFastHostageActionCTPlayer"},
                        {"_translation_other", "DiceFastHostageActionCT"},
                        { "playerName", player.PlayerName }
                    };
                }
                else
                {
                    return new Dictionary<string, string>
                    {
                        {"_translation_player", "command.rollthedice.unlucky"},
                        { "playerName", player.PlayerName }
                    };
                }
            }
            else
            {
                return new Dictionary<string, string>
                {
                    {"_translation_player", "command.rollthedice.unlucky"},
                    { "playerName", player.PlayerName }
                };
            }
        }

        private void DiceFastMapActionUnload()
        {
            DiceFastMapActionReset();
        }

        private void DiceFastMapActionReset()
        {
            DeregisterEventHandler<EventBombBegindefuse>(DiceFastBombActionEventBeginDefuse);
            DeregisterEventHandler<EventBombBeginplant>(DiceFastBombActionEventBeginPlant);
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

        // Hook HostageEntity grab begin
        [EntityOutputHook("hostage_entity", "OnHostageBeginGrab")]
        public HookResult OnPickup(CEntityIOOutput output, string name, CEntityInstance activator, CEntityInstance caller, CVariant value, float delay)
        {
            // TODO: set variables:
            // m_iProgressBarDuration -> 0.1f
            // m_flProgressBarStartTime -> gameTime
            // m_flGrabSuccessTime -> gameTime + 0.1f
            // set changed state to propagate properly to user
            if (!_playersCanInstantRescueHostages.Contains(activator)) return HookResult.Continue;
            return HookResult.Continue;
        }
    }
}
