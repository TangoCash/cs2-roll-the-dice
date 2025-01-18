using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private List<CCSPlayerController> _playersWithoutRecoil = new();

        private Dictionary<string, string> DiceNoRecoil(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            // create listener if not exists
            if (_playersWithoutRecoil.Count() == 0)
            {
                RegisterEventHandler<EventWeaponFire>(EventDiceNoRecoilOnWeaponFire);
            }
            _playersWithoutRecoil.Add(player);
            // no recoil
            playerPawn.AimPunchAngle.X = 0;
            playerPawn.AimPunchAngle.Y = 0;
            playerPawn.AimPunchAngle.Z = 0;
            playerPawn.AimPunchAngleVel.X = 0;
            playerPawn.AimPunchAngleVel.Y = 0;
            playerPawn.AimPunchAngleVel.Z = 0;
            playerPawn.AimPunchTickBase = -1;
            playerPawn.AimPunchTickFraction = 0;
            return new Dictionary<string, string>
            {
                {"_translation_player", "DiceNoRecoilPlayer"},
                {"_translation_other", "DiceNoRecoil"},
                { "playerName", player.PlayerName }
            };
        }

        private void DiceNoRecoilUnload()
        {
            DiceNoRecoilReset();
        }

        private void DiceNoRecoilReset()
        {
            DeregisterEventHandler<EventWeaponFire>(EventDiceNoRecoilOnWeaponFire);
            _playersWithoutRecoil.Clear();
        }

        private HookResult EventDiceNoRecoilOnWeaponFire(EventWeaponFire @event, GameEventInfo info)
        {
            CCSPlayerController? player = @event.Userid;
            if (player == null
                || player.PlayerPawn == null
                || !player.PlayerPawn.IsValid
                || player.PlayerPawn.Value == null
                || player.PlayerPawn.Value.WeaponServices == null
                || player.PlayerPawn.Value.WeaponServices.ActiveWeapon == null
                || !player.PlayerPawn.Value.WeaponServices.ActiveWeapon.IsValid
                || player.PlayerPawn.Value.WeaponServices.ActiveWeapon.Value == null) return HookResult.Continue;
            if (!_playersWithoutRecoil.Contains(player)) return HookResult.Continue;
            CBasePlayerWeapon weapon = player.PlayerPawn!.Value!.WeaponServices!.ActiveWeapon!.Value!;
            //decrease recoil
            weapon.As<CCSWeaponBase>().FlRecoilIndex = 0;
            //nospread
            weapon.As<CCSWeaponBase>().AccuracyPenalty = 0;
            return HookResult.Continue;
        }
    }
}
