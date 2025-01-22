using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private CPointWorldText? CreateGUI(CCSPlayerController player, string text, int size = 100, Color? color = null, string font = "", float shiftX = 0f, float shiftY = 0f)
        {
            if (player.PlayerPawn == null
                || !player.PlayerPawn.IsValid)
                return null;
            CCSPlayerPawn playerPawn = player?.PlayerPawn.Value!;
            var handle = new CHandle<CCSGOViewModel>((IntPtr)(playerPawn.ViewModelServices!.Handle + Schema.GetSchemaOffset("CCSPlayer_ViewModelServices", "m_hViewModel") + 4));
            if (!handle.IsValid)
            {
                CCSGOViewModel viewmodel = Utilities.CreateEntityByName<CCSGOViewModel>("predicted_viewmodel")!;
                viewmodel.DispatchSpawn();
                handle.Raw = viewmodel.EntityHandle.Raw;
                Utilities.SetStateChanged(playerPawn, "CCSPlayerPawnBase", "m_pViewModelServices");
            }
            CPointWorldText worldText = Utilities.CreateEntityByName<CPointWorldText>("point_worldtext")!;
            worldText.MessageText = text;
            worldText.Enabled = true;
            worldText.FontSize = size;
            worldText.Fullbright = true;
            worldText.Color = color ?? Color.Aquamarine;
            worldText.WorldUnitsPerPx = 0.01f;
            worldText.FontName = font;
            worldText.JustifyHorizontal = PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_LEFT;
            worldText.JustifyVertical = PointWorldTextJustifyVertical_t.POINT_WORLD_TEXT_JUSTIFY_VERTICAL_TOP;
            worldText.ReorientMode = PointWorldTextReorientMode_t.POINT_WORLD_TEXT_REORIENT_NONE;
            QAngle eyeAngles = playerPawn.EyeAngles;
            Vector forward = new(), right = new(), up = new();
            NativeAPI.AngleVectors(eyeAngles.Handle, forward.Handle, right.Handle, up.Handle);
            Vector eyePosition = new();
            eyePosition += forward * 7;
            eyePosition += right * shiftX;
            eyePosition += up * shiftY;
            QAngle angles = new()
            {
                Y = eyeAngles.Y + 270,
                Z = 90 - eyeAngles.X,
                X = 0
            };
            worldText.DispatchSpawn();
            worldText.Teleport(playerPawn.AbsOrigin! + eyePosition + new Vector(0, 0, playerPawn.ViewOffset.Z), angles, null);
            Server.NextFrame(() =>
            {
                if (worldText == null
                    || handle == null) return;
                worldText.AcceptInput("SetParent", handle.Value, null, "!activator");
            });
            return worldText;
        }

        private void RemoveGUI(CCSPlayerController player)
        {
            if (!_playersThatRolledTheDice.ContainsKey(player)) return;
            // remove gui message
            if (_playersThatRolledTheDice[player].ContainsKey("gui_message")
                && (CPointWorldText)_playersThatRolledTheDice[player]["gui_message"] != null)
            {
                CPointWorldText worldText = (CPointWorldText)_playersThatRolledTheDice[player]["gui_message"];
                worldText.AcceptInput("Kill");
            }
            // remove gui status
            if (_playersThatRolledTheDice[player].ContainsKey("gui_status")
                && (CPointWorldText)_playersThatRolledTheDice[player]["gui_status"] != null)
            {
                CPointWorldText worldText = (CPointWorldText)_playersThatRolledTheDice[player]["gui_status"];
                worldText.AcceptInput("Kill");
            }
        }

        private void CheckGUIConfig()
        {
            var positions = new Dictionary<string, (string MessageFont, int MessageFontSize, string MessageColor, float MessageShiftX, float MessageShiftY, string StatusFont, int StatusFontSize, string StatusColor, float StatusShiftX, float StatusShiftY)>
            {
                { "top_center", ("Verdana", 40, "Purple",-2.9f, 4.4f, "Verdana", 30, "Red", -2.75f, 4.0f) },
            };

            foreach (var position in positions)
            {
                if (!Config.GUIPositions.ContainsKey(position.Key))
                {
                    Config.GUIPositions[position.Key] = new GuiPositionConfig
                    {
                        MessageFont = position.Value.MessageFont,
                        MessageFontSize = position.Value.MessageFontSize,
                        MessageColor = position.Value.MessageColor,
                        MessageShiftX = position.Value.MessageShiftX,
                        MessageShiftY = position.Value.MessageShiftY,
                        StatusFont = position.Value.StatusFont,
                        StatusFontSize = position.Value.StatusFontSize,
                        StatusColor = position.Value.StatusColor,
                        StatusShiftX = position.Value.StatusShiftX,
                        StatusShiftY = position.Value.StatusShiftY,
                    };
                }
            }
        }
    }
}
