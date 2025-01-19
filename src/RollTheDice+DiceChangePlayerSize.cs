using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private List<CCSPlayerPawn> _playersWithChangedModelSize = new();

        private Dictionary<string, string> DiceChangePlayerSize(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            _playersWithChangedModelSize.Add(playerPawn);
            float playerSize = (float)(_random.NextDouble() * (1.5 - 0.5) + 0.5);
            var playerSceneNode = playerPawn.CBodyComponent?.SceneNode;
            if (playerSceneNode == null)
                return new Dictionary<string, string>
                {
                    {"_translation_player", "command.rollthedice.error"},
                    { "playerName", player.PlayerName }
                };
            playerSceneNode.GetSkeletonInstance().Scale = playerSize;
            playerPawn.AcceptInput("SetScale", null, null, playerSize.ToString());
            Server.NextFrame(() =>
            {
                if (playerPawn == null) return;
                Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_CBodyComponent");
            });

            return new Dictionary<string, string>
            {
                {"_translation_player", "DiceChangePlayerSizePlayer"},
                {"_translation_other", "DiceChangePlayerSize"},
                { "playerName", player.PlayerName },
                { "playerSize", playerSize.ToString() }
            };
        }

        private void DiceChangePlayerSizeUnload()
        {
            DiceChangePlayerSizeReset();
        }

        private void DiceChangePlayerSizeReset()
        {
            foreach (CCSPlayerPawn playerPawn in _playersWithChangedModelSize)
            {
                if (playerPawn == null) continue;
                var playerSceneNode = playerPawn.CBodyComponent?.SceneNode;
                if (playerSceneNode == null) continue;
                playerSceneNode.GetSkeletonInstance().Scale = 1.0f;
                playerPawn.AcceptInput("SetScale", null, null, "1.0");
                Server.NextFrame(() =>
                {
                    if (playerPawn == null) return;
                    Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_CBodyComponent");
                });
            }
            _playersWithChangedModelSize.Clear();
        }
    }
}
