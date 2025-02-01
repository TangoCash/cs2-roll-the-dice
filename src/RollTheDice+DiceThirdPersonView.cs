using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private List<CCSPlayerController> _playersWithThirdPersonView = new();
        public static Dictionary<CCSPlayerController, CDynamicProp> ThirdPersonPool = new Dictionary<CCSPlayerController, CDynamicProp>();
        public static Dictionary<CCSPlayerController, CPhysicsPropMultiplayer> ThirdPersonPoolSmooth = new Dictionary<CCSPlayerController, CPhysicsPropMultiplayer>();
        private Dictionary<string, string> DiceThirdPersonView(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {

            CDynamicProp? _cameraProp = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic");

            if (_cameraProp == null)
            {
                return new Dictionary<string, string> { };
            }

            if (_playersWithThirdPersonView.Count() == 0)
            {
                RegisterListener<Listeners.OnTick>(DiceThirdPersonViewOnTick);
            }

            _cameraProp.DispatchSpawn();
            _cameraProp.Render = Color.FromArgb(0, 255, 255, 255);

            Utilities.SetStateChanged(_cameraProp, "CBaseModelEntity", "m_clrRender");

            _cameraProp.Teleport(CalculatePositionInFront(player, -110, 90), player.PlayerPawn.Value!.V_angle, new Vector());

            player.PlayerPawn!.Value!.CameraServices!.ViewEntity.Raw = _cameraProp.EntityHandle.Raw;

            Utilities.SetStateChanged(player.PlayerPawn!.Value!, "CBasePlayerPawn", "m_pCameraServices");

            ThirdPersonPool.Add(player, _cameraProp);

            RollTheDice.Instance!.AddTimer(0.5f, () =>
            {
                _cameraProp.Teleport(CalculatePositionInFront(player, -110, 90), player.PlayerPawn.Value.V_angle, new Vector());
            });
            return new Dictionary<string, string>
            {
                { "playerName", player.PlayerName }
            };
        }

        private void DiceThirdPersonViewUnload()
        {
            DiceThirdPersonViewReset();
        }

        private void DiceThirdPersonViewReset()
        {
            // iterate through all players
            foreach (var player in _playersWithThirdPersonView)
            {
                if (player == null || player.PlayerPawn == null || !player.PlayerPawn.IsValid || player.PlayerPawn.Value == null || player.LifeState != (byte)LifeState_t.LIFE_ALIVE) continue;
                player!.PlayerPawn!.Value!.CameraServices!.ViewEntity.Raw = uint.MaxValue;
                RollTheDice.Instance!.AddTimer(0.3f, () =>
                    Utilities.SetStateChanged(player.PlayerPawn!.Value!, "CBasePlayerPawn", "m_pCameraServices")
                );

                if (ThirdPersonPool[player] != null && ThirdPersonPool[player].IsValid)
                {
                    ThirdPersonPool[player].Remove();
                }

                ThirdPersonPool.Remove(player);
                ThirdPersonPoolSmooth.Remove(player);
            }
            RemoveListener<Listeners.OnTick>(DiceThirdPersonViewOnTick);
            _playersWithThirdPersonView.Clear();
        }
        public static void DiceThirdPersonViewOnTick()
        {
             foreach (var player in ThirdPersonPool.Keys)
                {
                    UpdateCamera(ThirdPersonPool[player], player);
                }

             foreach (var player in ThirdPersonPoolSmooth.Keys)
                {
                    UpdateCameraSmooth(ThirdPersonPoolSmooth[player], player);
                }
        }
    }
}
