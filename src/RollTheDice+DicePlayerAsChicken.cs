using System.Numerics;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Entities.Constants;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private Dictionary<CCSPlayerController, Dictionary<string, string>> _playersAsChicken = new();
        private readonly string _playersAsChickenModel = "models/chicken/chicken.vmdl";
        private readonly List<string> _chickenSounds = new List<string>
        {
            "Chicken.Idle",
            "Chicken.Panic",
        };

        private Dictionary<string, string> DicePlayerAsChicken(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            // create listener if not exists
            if (_playersAsChicken.Count() == 0)
            {
                RegisterEventHandler<EventPlayerDeath>(EventDicePlayerAsChickenOnPlayerDeath);
                RegisterListener<Listeners.OnTick>(EventDicePlayerAsChickenOnTick);
                RegisterListener<Listeners.CheckTransmit>(EventDicePlayerAsChickenCheckTransmit);
                RegisterEventHandler<EventWeaponFire>(EventDicePlayerAsChickenOnWeaponFire);
                RegisterEventHandler<EventBombPlanted>(EventDicePlayerAsChickenBombPlanted);
                RegisterEventHandler<EventPlayerHurt>(EventDicePlayerAsChickenOnPlayerHurt);
            }
            // add player to list
            playerPawn.Health = 250;
            //Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_iHealth");
            playerPawn.GravityScale = 0.5f;
            playerPawn.VelocityModifier *= 1.5f;
            //Utilities.SetStateChanged(playerPawn, "CCSPlayerPawn", "m_flVelocityModifier");
            if (playerPawn.WeaponServices == null)
                return new Dictionary<string, string>
                {
                    {"error", "command.rollthedice.error"}
                };
            var playerWeapons = playerPawn.WeaponServices!;
            bool hasC4 = false;
            foreach (var weapon in playerWeapons.MyWeapons)
            {
                // ignore unknown weapons
                if (weapon.Value == null || weapon.Value != null && weapon.Value.DesignerName == null) continue;
                if (weapon.Value!.DesignerName == CsItem.C4.ToString().ToLower()
                    || weapon.Value!.DesignerName == CsItem.Bomb.ToString().ToLower()
                    || weapon.Value!.DesignerName == $"weapon_{CsItem.C4.ToString().ToLower()}"
                    || weapon.Value!.DesignerName == $"weapon_{CsItem.Bomb.ToString().ToLower()}")
                {
                    hasC4 = true;
                }
            }
            player.RemoveWeapons();
            player.GiveNamedItem(CsItem.Knife);
            if (hasC4)
                player.GiveNamedItem(CsItem.C4);
            _playersAsChicken.Add(player, new Dictionary<string, string>());
            _playersAsChicken[player]["old_model"] = GetPlayerModel(playerPawn);
            _playersAsChicken[player]["next_sound"] = $"{(int)Server.CurrentTime + 2}";
            _playersAsChicken[player]["prop"] = SpawnProp(player, _playersAsChickenModel, 1.5f).ToString();
            _playersAsChicken[player]["speed"] = 1.5f.ToString();
            MakePlayerInvisible(player);
            RefreshUI(player);
            return new Dictionary<string, string>
            {
                { "playerName", player.PlayerName }
            };
        }

        private void DicePlayerAsChickenUnload()
        {
            DicePlayerAsChickenReset();
        }

        private void DicePlayerAsChickenReset()
        {
            DeregisterEventHandler<EventPlayerDeath>(EventDicePlayerAsChickenOnPlayerDeath);
            RemoveListener<Listeners.OnTick>(EventDicePlayerAsChickenOnTick);
            RemoveListener<Listeners.CheckTransmit>(EventDicePlayerAsChickenCheckTransmit);
            DeregisterEventHandler<EventWeaponFire>(EventDicePlayerAsChickenOnWeaponFire);
            DeregisterEventHandler<EventBombPlanted>(EventDicePlayerAsChickenBombPlanted);
            DeregisterEventHandler<EventPlayerHurt>(EventDicePlayerAsChickenOnPlayerHurt);
            // iterate through all players
            Dictionary<CCSPlayerController, Dictionary<string, string>> _playersAsChickenCopy = new(_playersAsChicken);
            foreach (CCSPlayerController player in _playersAsChickenCopy.Keys)
            {
                if (player == null || player.Pawn == null || player.Pawn.Value == null) continue;
                // reset player speed
                var playerPawn = player.PlayerPawn.Value!;
                playerPawn.VelocityModifier = 1.0f;
                playerPawn.GravityScale = 1.0f;
                // set state changed
                //Utilities.SetStateChanged(playerPawn, "CCSPlayerPawn", "m_flVelocityModifier");
                RemoveProp(int.Parse(_playersAsChicken[player]["prop"]));
                MakePlayerVisible(player);
                RefreshUI(player);
            }
            _playersAsChicken.Clear();
        }

        private void DicePlayerAsChickenResetForPlayer(CCSPlayerController player)
        {
            if (!_playersAsChicken.ContainsKey(player)) return;
            // get prop
            int prop = int.Parse(_playersAsChicken[player]["prop"]);
            // remove player first to avoid infinite loop
            _playersAsChicken.Remove(player);
            // remove prop
            RemoveProp(prop);
            MakePlayerVisible(player);
        }

        private void EventDicePlayerAsChickenOnTick()
        {
            // remove listener if no players to save resources
            if (_playersAsChicken.Count() == 0) return;
            // worker
            Dictionary<CCSPlayerController, Dictionary<string, string>> _playersAsChickenCopy = new(_playersAsChicken);
            foreach (var (player, playerData) in _playersAsChickenCopy)
            {
                try
                {
                    // sanity checks
                    if (player == null
                    || player.Pawn == null
                    || player.Pawn.Value == null
                    || player.PlayerPawn == null || !player.PlayerPawn.IsValid || player.PlayerPawn.Value == null
                    || player.PlayerPawn.Value.LifeState != (byte)LifeState_t.LIFE_ALIVE
                    || !playerData.ContainsKey("prop")) continue;
                    // update prop every tick to ensure synchroneity
                    UpdateProp(
                        player,
                        int.Parse(playerData["prop"]),
                        (player.Buttons & PlayerButtons.Duck) != 0 ? -18 : 0
                    );
                    // make sound if time
                    if (int.Parse(_playersAsChickenCopy[player]["next_sound"]) <= (int)Server.CurrentTime)
                    {
                        EmitSound(player, _chickenSounds[_random.Next(_chickenSounds.Count)]);
                        _playersAsChickenCopy[player]["next_sound"] = $"{(int)Server.CurrentTime + _random.Next(2, 5)}";
                    }
                }
                catch (Exception e)
                {
                    // remove player
                    _playersAsChicken.Remove(player);
                    // log error
                    Console.WriteLine(Localizer["core.error"].Value.Replace("{error}", e.Message));
                }
            }
        }

        private void EventDicePlayerAsChickenCheckTransmit(CCheckTransmitInfoList infoList)
        {
            // remove listener if no players to save resources
            if (_playersAsChicken.Count() == 0) return;
            // worker
            foreach ((CCheckTransmitInfo info, CCSPlayerController? player) in infoList)
            {
                if (player == null) continue;
                if (!_playersAsChicken.ContainsKey(player)) continue;
                var prop = Utilities.GetEntityFromIndex<CDynamicProp>(int.Parse(_playersAsChicken[player]["prop"]));
                if (prop == null) continue;
                info.TransmitEntities.Remove(prop);
            }
        }

        private HookResult EventDicePlayerAsChickenOnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
        {
            CCSPlayerController player = @event.Userid!;
            if (_playersAsChicken.ContainsKey(player))
            {
                RemoveProp(int.Parse(_playersAsChicken[player]["prop"]));
                MakePlayerVisible(player);
                _playersAsChicken.Remove(player);
            }
            return HookResult.Continue;
        }
        private HookResult EventDicePlayerAsChickenOnWeaponFire(EventWeaponFire @event, GameEventInfo info)
        {
            CCSPlayerController player = @event.Userid!;
            if (_playersAsChicken.ContainsKey(player))
            {
                var act_weapon = player.PlayerPawn.Value?.WeaponServices!.ActiveWeapon;
                if (act_weapon != null)
                {
                    while (!(act_weapon.Value.DesignerName.Contains("bayonet") || act_weapon.Value.DesignerName.Contains("knife") || act_weapon.Value.DesignerName.Contains("weapon_c4")))
                    {
                        player.DropActiveWeapon();
                        act_weapon = player.PlayerPawn.Value?.WeaponServices!.ActiveWeapon;
                        EmitSound(player, _chickenSounds[_random.Next(_chickenSounds.Count)]);
                        if (act_weapon == null) break;
                    }
                }
            }
            return HookResult.Continue;
        }
        private HookResult EventDicePlayerAsChickenBombPlanted(EventBombPlanted @event, GameEventInfo info)
        {
            CCSPlayerController player = @event.Userid!;
            if (_playersAsChicken.ContainsKey(player))
            {
                var ElementPlantedC4 = GetPlantedC4();
                Server.NextFrame(() =>
                {
                    if (ElementPlantedC4 != null)
                    {
                        ElementPlantedC4.SetModel("models/props/de_dust/hr_dust/dust_soccerball/dust_soccer_ball001.vmdl");
                    }
                });
                EmitSound(player, _chickenSounds[_random.Next(_chickenSounds.Count)]);
            }
            return HookResult.Continue;
        }
        private HookResult EventDicePlayerAsChickenOnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
        {
            CCSPlayerController? victim = @event.Userid;
            CCSPlayerController? attacker = @event.Attacker;
            if (victim == null) return HookResult.Continue;
            if (!_playersAsChicken.ContainsKey(victim)) return HookResult.Continue;
            if (victim == null || victim.PlayerPawn == null || !victim.PlayerPawn.IsValid || victim.PlayerPawn.Value == null || victim.LifeState != (byte)LifeState_t.LIFE_ALIVE) return HookResult.Continue;
            if (victim == attacker) return HookResult.Continue;
            Server.NextFrame(() =>
            {
                if (victim == null
                    || !victim.IsValid
                    || victim.PlayerPawn == null
                    || !victim.PlayerPawn.IsValid
                    || !_playersAsChicken.ContainsKey(victim)) return;
                CCSPlayerPawn playerPawn = victim.PlayerPawn.Value!;
                // set player speed
                playerPawn.VelocityModifier *= float.Parse(_playersAsChicken[victim]["speed"]);
                // set state changed
                Utilities.SetStateChanged(playerPawn, "CCSPlayerPawn", "m_flVelocityModifier");
            });
            return HookResult.Continue;
        }
    }
}
