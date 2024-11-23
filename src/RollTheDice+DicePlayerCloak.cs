using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System.Drawing;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private Dictionary<CCSPlayerController, int> _playersWithCloak = new();

        private Dictionary<string, string> DicePlayerCloak(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            if (_playersWithCloak.ContainsKey(player))
                return new Dictionary<string, string>
                {
                    {"_translation", "command.rollthedice.error"},
                    { "playerName", player.PlayerName }
                };
            // create listener if not exists
            if (_playersWithCloak.Count() == 0)
            {
                RegisterListener<Listeners.OnTick>(EventDicePlayerCloakOnTick);
            }
            // add player to list
            _playersWithCloak.Add(player, 255);
            return new Dictionary<string, string>
            {
                {"_translation", "DicePlayerCloak"},
                { "playerName", player.PlayerName }
            };
        }

        private void RemoveDicePlayerCloakListeners()
        {
            RemoveListener<Listeners.OnTick>(EventDicePlayerCloakOnTick);
        }

        private void ResetDicePlayerCloak()
        {
            // iterate through all players
            foreach (var (player, visibility) in _playersWithCloak)
            {
                if (player == null || player.PlayerPawn == null || !player.PlayerPawn.IsValid || player.PlayerPawn.Value == null || player.LifeState != (byte)LifeState_t.LIFE_ALIVE) continue;
                // get player pawn
                var playerPawn = player.PlayerPawn.Value!;
                // reset player render color
                playerPawn.Render = Color.FromArgb(255, 255, 255, 255);
                // set state changed
                Utilities.SetStateChanged(playerPawn, "CBaseModelEntity", "m_clrRender");
            }
            _playersWithCloak.Clear();
        }

        private void EventDicePlayerCloakOnTick()
        {
            // remove listener if no players to save resources
            if (_playersWithCloak.Count() == 0)
            {
                RemoveListener<Listeners.OnTick>(EventDicePlayerCloakOnTick);
                return;
            }
            // worker
            Dictionary<CCSPlayerController, int> _playersWithCloakCopy = new(_playersWithCloak);
            foreach (var (player, visibility) in _playersWithCloakCopy)
            {
                try
                {
                    // sanity checks
                    if (player == null
                    || player.Pawn == null
                    || player.Pawn.Value == null
                    || player.PlayerPawn == null || !player.PlayerPawn.IsValid || player.PlayerPawn.Value == null
                    || player.PlayerPawn.Value.LifeState != (byte)LifeState_t.LIFE_ALIVE) continue;
                    // check if player does not move
                    if (player.Buttons == 0 && visibility > 0)
                    {
                        // update visibility variable
                        _playersWithCloak[player]--;
                        // reset player render color
                        player.PlayerPawn.Value.Render = Color.FromArgb(visibility - 1, 255, 255, 255);
                        // set state changed
                        Utilities.SetStateChanged(player.PlayerPawn.Value, "CBaseModelEntity", "m_clrRender");
                    }
                    else if (player.Buttons != 0 && visibility < 255)
                    {
                        // update visibility variable
                        _playersWithCloak[player] = 255;
                        // reset player render color
                        player.PlayerPawn.Value.Render = Color.FromArgb(255, 255, 255, 255);
                        // set state changed
                        Utilities.SetStateChanged(player.PlayerPawn.Value, "CBaseModelEntity", "m_clrRender");
                    }
                }
                catch (Exception e)
                {
                    // remove player
                    _playersWithCloak.Remove(player);
                    // log error
                    Console.WriteLine(Localizer["core.error"].Value.Replace("{error}", e.Message));
                }
            }
        }
    }
}
