using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using System.Drawing;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private int GetRandomDice()
        {
            // Filter enabled dices based on map-specific and global configuration
            var enabledDiceIndices = _dices
                .Select((dice, index) => new { dice, index })
                .Where(diceInfo =>
                {
                    var diceName = diceInfo.dice.Method.Name;
                    // Check map-specific configuration
                    if (Config.MapConfigs.TryGetValue(_currentMap, out var mapConfig) && mapConfig.Features.TryGetValue(diceName, out var isEnabled))
                    {
                        return isEnabled;
                    }
                    // Check global configuration
                    if (Config.Features.TryGetValue(diceName, out isEnabled))
                    {
                        return isEnabled;
                    }
                    // Default to enabled if not found in either configuration
                    return true;
                })
                .Select(diceInfo => diceInfo.index)
                .ToList();
            if (enabledDiceIndices.Count == 0) return -1;
            // subset of enabled dices from dice counter
            var countRolledDicesEnabled = _countRolledDices
                .Where(diceCounter => enabledDiceIndices.Contains(_dices.FindIndex(dice => dice.Method.Name == diceCounter.Key)))
                .ToDictionary(diceCounter => diceCounter.Key, diceCounter => diceCounter.Value);
            // get the lowest count from enabled dices
            var countRolledDicesLowestCount = countRolledDicesEnabled.Values.Min();
            var countRolledDicesLowest = countRolledDicesEnabled
                .Where(diceCounter => diceCounter.Value == countRolledDicesLowestCount)
                .Select(diceCounter => _dices.FindIndex(dice => dice.Method.Name == diceCounter.Key))
                .ToList();
            // Get random dice from lowest used enabled dices
            return countRolledDicesLowest[_random.Next(countRolledDicesLowest.Count)];
        }

        public void SendGlobalChatMessage(string message, float delay = 0, CCSPlayerController? player = null)
        {
            foreach (CCSPlayerController entry in Utilities.GetPlayers())
            {
                if (entry.IsBot || entry == player) continue;
                AddTimer(delay, () => entry.PrintToChat(message));
            }
        }

        public void SendGlobalCenterMessage(string message, float delay = 0, bool alert = false, CCSPlayerController? player = null)
        {
            foreach (CCSPlayerController entry in Utilities.GetPlayers())
            {
                if (entry.IsBot || entry == player) continue;
                if (alert) AddTimer(delay, () => entry.PrintToCenterAlert(message));
                else AddTimer(delay, () => entry.PrintToCenterHtml(message));
            }
        }

        private void ChangeGameRule(string rule, object value)
        {
            var ents = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules");
            foreach (var ent in ents)
            {
                var gameRules = ent.GameRules;
                if (gameRules == null) continue;

                var property = gameRules.GetType().GetProperty(rule);
                if (property != null && property.CanWrite)
                {
                    property.SetValue(gameRules, Convert.ChangeType(value, property.PropertyType));
                }
            }
        }

        private object? GetGameRule(string rule)
        {
            var ents = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules");
            foreach (var ent in ents)
            {
                var gameRules = ent.GameRules;
                if (gameRules == null) continue;

                var property = gameRules.GetType().GetProperty(rule);
                if (property != null && property.CanRead)
                {
                    return property.GetValue(gameRules);
                }
            }
            return null;
        }

        private int SpawnProp(CCSPlayerController player, string model, float scale = 1.0f)
        {
            // sanity checks
            if (player == null
            || player.PlayerPawn == null || !player.PlayerPawn.IsValid || player.PlayerPawn.Value == null
            || player.PlayerPawn.Value.LifeState != (byte)LifeState_t.LIFE_ALIVE) return -1;
            // create dynamic prop
            CDynamicProp prop;
            prop = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic_override")!;
            // set attributes
            prop.MoveType = MoveType_t.MOVETYPE_NOCLIP;
            prop.Collision.SolidType = SolidType_t.SOLID_NONE;
            prop.Collision.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_NONE;
            prop.Collision.CollisionAttribute.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_NONE;
            // spawn it
            prop.DispatchSpawn();
            prop.SetModel(model);
            prop.Teleport(new Vector(-999, -999, -999));
            prop.AnimGraphUpdateEnabled = false;
            prop.CBodyComponent!.SceneNode!.Scale = scale;
            return (int)prop.Index;
        }

        private void UpdateProp(CCSPlayerController player, int index, int offset_z = 0, int offset_angle = 0)
        {
            var prop = Utilities.GetEntityFromIndex<CDynamicProp>((int)index);
            // sanity checks
            if (prop == null
            || player == null
            || player.Pawn == null
            || player.Pawn.Value == null
            || player.PlayerPawn == null || !player.PlayerPawn.IsValid || player.PlayerPawn.Value == null
            || player.PlayerPawn.Value.LifeState != (byte)LifeState_t.LIFE_ALIVE) return;
            // get player pawn
            var playerPawn = player!.PlayerPawn!.Value;
            // teleport it to player
            Vector playerOrigin = new Vector(
                (float)Math.Round(playerPawn.AbsOrigin!.X, 5),
                (float)Math.Round(playerPawn.AbsOrigin!.Y, 5),
                (float)Math.Round(playerPawn.AbsOrigin!.Z, 5) + offset_z
            );
            Vector propOrigin = new Vector(
                (float)Math.Round(prop.AbsOrigin!.X, 5),
                (float)Math.Round(prop.AbsOrigin!.Y, 5),
                (float)Math.Round(prop.AbsOrigin!.Z, 5)
            );
            QAngle playerRotation = new QAngle(
                0,
                (float)Math.Round(playerPawn.AbsRotation!.Y, 5) + offset_angle,
                0
            );
            QAngle propRotation = new QAngle(
                0,
                (float)Math.Round(prop.AbsRotation!.Y, 5),
                0
            );
            if (playerOrigin.X == propOrigin.X
                && playerOrigin.Y == propOrigin.Y
                && playerOrigin.Z == propOrigin.Z
                && playerRotation.Y == propRotation.Y) return;
            prop.Teleport(playerOrigin, playerRotation);
        }

        private void RemoveProp(int index, bool softRemove = false)
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

        private void MakePlayerInvisible(CCSPlayerController player)
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

        private void MakePlayerVisible(CCSPlayerController player)
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

        private static string GetPlayerModel(CCSPlayerPawn playerPawn)
        {
            return playerPawn.CBodyComponent?.SceneNode?.GetSkeletonInstance().ModelState.ModelName ?? string.Empty;
        }
    }
}