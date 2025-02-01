using CounterStrikeSharp.API.Core;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private Dictionary<CCSPlayerController, int> _playersWithBigTaserBattery = new();

        private Dictionary<string, string> DiceBigTaserBattery(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            Dictionary<string, object> config = GetDiceConfig("DiceBigTaserBattery");
            // create listener if not exists
            if (_playersWithBigTaserBattery.Count() == 0)
            {
                RegisterEventHandler<EventWeaponFire>(EventDiceBigTaserBatteryOnWeaponFire);
            }
            int battery = _random.Next(
                Convert.ToInt32(config["min_batteries"]),
                Convert.ToInt32(config["max_batteries"]) + 1
            );
            _playersWithBigTaserBattery.Add(player, battery);
            player.GiveNamedItem("weapon_taser");
            return new Dictionary<string, string>
            {
                { "playerName", player.PlayerName },
                { "batterySize", battery.ToString() }
            };
        }

        private void DiceBigTaserBatteryUnload()
        {
            DiceBigTaserBatteryReset();
        }

        private void DiceBigTaserBatteryReset()
        {
            DeregisterEventHandler<EventWeaponFire>(EventDiceBigTaserBatteryOnWeaponFire);
            _playersWithBigTaserBattery.Clear();
        }

        private void DiceBigTaserBatteryResetForPlayer(CCSPlayerController player)
        {
            if (!_playersWithBigTaserBattery.ContainsKey(player)) return;
            _playersWithBigTaserBattery.Remove(player);
        }

        private HookResult EventDiceBigTaserBatteryOnWeaponFire(EventWeaponFire @event, GameEventInfo info)
        {
            CCSPlayerController player = @event.Userid!;
            if (!_playersWithBigTaserBattery.ContainsKey(player)) return HookResult.Continue;
            if (player == null || player.Pawn == null || !player.Pawn.IsValid || player.Pawn.Value == null) return HookResult.Continue;
            if (player.Pawn.Value.WeaponServices == null || player.Pawn.Value.WeaponServices.ActiveWeapon == null) return HookResult.Continue;
            if (player.Pawn.Value.WeaponServices.ActiveWeapon == null || player.Pawn.Value.WeaponServices.ActiveWeapon.Value == null) return HookResult.Continue;
            if (player.Pawn.Value.WeaponServices.ActiveWeapon.Value!.DesignerName != "weapon_taser") return HookResult.Continue;
            if (_playersWithBigTaserBattery[player] <= 0) return HookResult.Continue;
            player.Pawn.Value.WeaponServices.ActiveWeapon.Value!.Clip1 = 2;
            _playersWithBigTaserBattery[player]--;
            return HookResult.Continue;
        }

        private Dictionary<string, object> DiceBigTaserBatteryConfig()
        {
            var config = new Dictionary<string, object>();
            config["min_batteries"] = (int)2;
            config["max_batteries"] = (int)10;
            return config;
        }
    }
}
