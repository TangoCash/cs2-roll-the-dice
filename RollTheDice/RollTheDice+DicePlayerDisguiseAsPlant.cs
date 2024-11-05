using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using System.Drawing;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private Dictionary<CCSPlayerController, Dictionary<string, string>> _playersDisguisedAsPlants = new();
        private readonly Dictionary<string, Dictionary<string, object>> _playersDisguisedAsPlantsModels = new()
        {
            { "Office/Plant", new Dictionary<string, object> { { "model", "models/props/cs_office/plant01.vmdl" } } },
            { "Trafficcone", new Dictionary<string, object> { { "model", "models/props/de_vertigo/trafficcone_clean.vmdl" }, { "offset_z", "15" } } },
            { "Barstool", new Dictionary<string, object> { { "model", "models/generic/barstool_01/barstool_01.vmdl" } } },
            { "Fireextinguisher", new Dictionary<string, object> { { "model", "models/generic/fire_extinguisher_01/fire_extinguisher_01.vmdl" } } },
            { "Hostage", new Dictionary<string, object> { { "model", "models/hostage/hostage.vmdl" } } },
            { "Pottery", new Dictionary<string, object> { { "model", "models/ar_shoots/shoots_pottery_02.vmdl" } } },
            { "AnubisInfoPanel", new Dictionary<string, object> { { "model", "models/anubis/signs/anubis_info_panel_01.vmdl" } } },
            { "Chicken", new Dictionary<string, object> { { "model", "models/chicken/chicken.vmdl" } } },
            { "Italy/Chair", new Dictionary<string, object> { { "model", "models/cs_italy/seating/chair/wood_chair_1.vmdl" }, { "offset_angle", "180" } } },
            { "FileCabinet", new Dictionary<string, object> { { "model", "models/props_office/file_cabinet_03.vmdl" } } },
            { "Airport/Plant", new Dictionary<string, object> { { "model", "models/props_plants/plantairport01.vmdl" } } },
            { "MailDropbox", new Dictionary<string, object> { { "model", "models/props_street/mail_dropbox.vmdl" } } },
        };
        private string DicePlayerDisguiseAsPlant(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            if (_playersDisguisedAsPlants.ContainsKey(player)) return Localizer["command.rollthedice.error"].Value.Replace("{playerName}", player.PlayerName);
            _playersDisguisedAsPlants.Add(player, new Dictionary<string, string>());
            _playersDisguisedAsPlants[player]["status"] = "player";
            var randomKey = _playersDisguisedAsPlantsModels.Keys.ElementAt(_random.Next(0, _playersDisguisedAsPlantsModels.Count));
            _playersDisguisedAsPlants[player]["prop"] = spawnPlant(
                player,
                _playersDisguisedAsPlantsModels[randomKey]["model"].ToString()!
            ).ToString();
            _playersDisguisedAsPlants[player]["offset_z"] = _playersDisguisedAsPlantsModels[randomKey].ContainsKey("offset_z") ? (string)_playersDisguisedAsPlantsModels[randomKey]["offset_z"] : "0";
            _playersDisguisedAsPlants[player]["offset_angle"] = _playersDisguisedAsPlantsModels[randomKey].ContainsKey("offset_angle") ? (string)_playersDisguisedAsPlantsModels[randomKey]["offset_angle"] : "0";
            return Localizer["DicePlayerDisguiseAsPlant"].Value
                .Replace("{playerName}", player.PlayerName)
                .Replace("{model}", randomKey);
        }

        private void ResetDicePlayerDisguiseAsPlant()
        {
            foreach (CCSPlayerController player in _playersDisguisedAsPlants.Keys)
            {
                if (player == null || player.Pawn == null || player.Pawn.Value == null) continue;
                removePlant(int.Parse(_playersDisguisedAsPlants[player]["prop"]));
                makePlayerVisible(player);
            }
            _playersDisguisedAsPlants.Clear();
        }

        private void CreateDicePlayerDisguiseAsPlantListener()
        {
            RegisterListener<Listeners.OnTick>(EventDicePlayerDisguiseAsPlantOnTick);
            RegisterEventHandler<EventPlayerDeath>(EventDicePlayerDisguiseAsPlantOnPlayerDeath);
        }

        private void RemoveDicePlayerDisguiseAsPlantListener()
        {
            RemoveListener<Listeners.OnTick>(EventDicePlayerDisguiseAsPlantOnTick);
        }

        private void EventDicePlayerDisguiseAsPlantOnTick()
        {
            Dictionary<CCSPlayerController, Dictionary<string, string>> _playersDisguisedAsPlantsCopy = new(_playersDisguisedAsPlants);
            foreach (var (player, playerData) in _playersDisguisedAsPlantsCopy)
            {
                try
                {
                    // sanity checks
                    if (player == null
                    || player.Pawn == null
                    || player.Pawn.Value == null
                    || player.PlayerPawn == null || !player.PlayerPawn.IsValid || player.PlayerPawn.Value == null
                    || player.PlayerPawn.Value.LifeState != (byte)LifeState_t.LIFE_ALIVE
                    || !_playersDisguisedAsPlants.ContainsKey(player)) continue;
                    // change player model if player is not pressing any buttons
                    if (player.Buttons == 0 && playerData["status"] == "player")
                    {
                        _playersDisguisedAsPlants[player]["status"] = "plant";
                        makePlayerInvisible(player);
                        updatePlant(
                            player,
                            int.Parse(playerData["prop"]),
                            int.Parse(_playersDisguisedAsPlants[player]["offset_z"]),
                            int.Parse(_playersDisguisedAsPlants[player]["offset_angle"])
                        );
                    }
                    else if (player.Buttons != 0 && playerData["status"] == "plant")
                    {
                        _playersDisguisedAsPlants[player]["status"] = "player";
                        makePlayerVisible(player);
                        removePlant(int.Parse(playerData["prop"]), true);
                    }
                    else if (playerData["status"] == "plant")
                    {
                        updatePlant(
                            player,
                            int.Parse(playerData["prop"]),
                            int.Parse(_playersDisguisedAsPlants[player]["offset_z"]),
                            int.Parse(_playersDisguisedAsPlants[player]["offset_angle"])
                        );
                    }
                }
                catch (Exception e)
                {
                    // remove player
                    _playersDisguisedAsPlants.Remove(player);
                    // log error
                    Console.WriteLine(Localizer["core.error"].Value.Replace("{error}", e.Message));
                }
            }
        }

        private HookResult EventDicePlayerDisguiseAsPlantOnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
        {
            CCSPlayerController player = @event.Userid!;
            if (_playersDisguisedAsPlants.ContainsKey(player))
            {
                removePlant(int.Parse(_playersDisguisedAsPlants[player]["prop"]));
                makePlayerVisible(player);
                _playersDisguisedAsPlants.Remove(player);
            }
            return HookResult.Continue;
        }

        private int spawnPlant(CCSPlayerController player, string model)
        {
            // sanity checks
            if (player == null
            || player.PlayerPawn == null || !player.PlayerPawn.IsValid || player.PlayerPawn.Value == null
            || player.PlayerPawn.Value.LifeState != (byte)LifeState_t.LIFE_ALIVE
            || !_playersDisguisedAsPlants.ContainsKey(player)) return -1;
            // create dynamic prop
            CDynamicProp prop;
            prop = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic_override")!;
            // set attributes
            prop.Collision.SolidType = SolidType_t.SOLID_NONE;
            prop.Collision.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_NONE;
            // spawn it
            prop.DispatchSpawn();
            prop.SetModel(model);
            prop.Teleport(new Vector(-999, -999, -999));
            return (int)prop.Index;
        }

        private void updatePlant(CCSPlayerController player, int index, int offset_z = 0, int offset_angle = 0)
        {
            var prop = Utilities.GetEntityFromIndex<CDynamicProp>((int)index);
            // sanity checks
            if (prop == null
            || player == null
            || player.Pawn == null
            || player.Pawn.Value == null
            || player.PlayerPawn == null || !player.PlayerPawn.IsValid || player.PlayerPawn.Value == null
            || player.PlayerPawn.Value.LifeState != (byte)LifeState_t.LIFE_ALIVE
            || !_playersDisguisedAsPlants.ContainsKey(player)) return;
            // get player pawn
            var playerPawn = player!.PlayerPawn!.Value;
            // teleport it to player
            Vector playerOrigin = new Vector(
                (float)Math.Round(playerPawn.AbsOrigin!.X, 3),
                (float)Math.Round(playerPawn.AbsOrigin!.Y, 3),
                (float)Math.Round(playerPawn.AbsOrigin!.Z, 3) + offset_z
            );
            Vector propOrigin = new Vector(
                (float)Math.Round(prop.AbsOrigin!.X, 3),
                (float)Math.Round(prop.AbsOrigin!.Y, 3),
                (float)Math.Round(prop.AbsOrigin!.Z, 3)
            );
            QAngle playerRotation = new QAngle(
                0,
                (float)Math.Round(playerPawn.AbsRotation!.Y, 3) + offset_angle,
                0
            );
            QAngle propRotation = new QAngle(
                0,
                (float)Math.Round(prop.AbsRotation!.Y, 3),
                0
            );
            if (playerOrigin.X == propOrigin.X
                && playerOrigin.Y == propOrigin.Y
                && playerOrigin.Z == propOrigin.Z
                && playerRotation.Y == propRotation.Y) return;
            prop.Teleport(playerOrigin, playerRotation);
        }

        private void removePlant(int index, bool softRemove = false)
        {
            var prop = Utilities.GetEntityFromIndex<CDynamicProp>((int)index);
            // remove plant entity
            if (prop == null)
                return;
            if (softRemove)
                prop.Teleport(new Vector(-999, -999, -999));
            else
                prop.Remove();
        }

        private void makePlayerInvisible(CCSPlayerController player)
        {
            // sanity checks
            if (player == null
            || player.PlayerPawn == null || !player.PlayerPawn.IsValid || player.PlayerPawn.Value == null
            || player.PlayerPawn.Value.LifeState != (byte)LifeState_t.LIFE_ALIVE) return;
            // get player pawn
            var playerPawn = player!.PlayerPawn!.Value;
            playerPawn.Render = Color.FromArgb(0, 255, 255, 255);
            Utilities.SetStateChanged(playerPawn, "CBaseModelEntity", "m_clrRender");
            if (playerPawn.WeaponServices?.MyWeapons != null)
            {
                foreach (var gun in playerPawn.WeaponServices!.MyWeapons)
                {
                    var weapon = gun.Value;
                    if (weapon != null)
                    {
                        weapon.Render = Color.FromArgb(0, 255, 255, 255);
                        weapon.ShadowStrength = 0.0f;
                        Utilities.SetStateChanged(weapon, "CBaseModelEntity", "m_clrRender");
                    }
                }
            }
        }

        private void makePlayerVisible(CCSPlayerController player)
        {
            // sanity checks
            if (player == null
            || player.PlayerPawn == null || !player.PlayerPawn.IsValid || player.PlayerPawn.Value == null) return;
            // get player pawn
            var playerPawn = player!.PlayerPawn!.Value;
            playerPawn.Render = Color.FromArgb(255, 255, 255, 255);
            Utilities.SetStateChanged(playerPawn, "CBaseModelEntity", "m_clrRender");
            if (playerPawn.WeaponServices!.MyWeapons != null)
            {
                foreach (var gun in playerPawn.WeaponServices!.MyWeapons)
                {
                    var weapon = gun.Value;
                    if (weapon != null)
                    {
                        weapon.Render = Color.FromArgb(255, 255, 255, 255);
                        weapon.ShadowStrength = 0.0f;
                        Utilities.SetStateChanged(weapon, "CBaseModelEntity", "m_clrRender");
                    }
                }
            }
        }
    }
}
