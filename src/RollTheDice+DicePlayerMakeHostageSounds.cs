using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private readonly Dictionary<CCSPlayerController, int> _playersWithHostageSounds = new();
        private readonly List<string> _hostageSounds = new List<string>
        {
            "Hostage.StartFollowCT",
            "Hostage.StartFollowCTGuardian",
            "Hostage.Pain"
        };

        private Dictionary<string, string> DicePlayerMakeHostageSounds(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            // create listener if not exists
            if (_playersWithHostageSounds.Count() == 0) RegisterListener<Listeners.OnTick>(EventDicePlayerMakeHostageSoundsOnTick);
            // add player to list
            _playersWithHostageSounds.Add(player, 0);
            return new Dictionary<string, string>
            {
                {"_translation_player", "DicePlayerMakeHostageSoundsPlayer"},
                {"_translation_other", "DicePlayerMakeHostageSounds"},
                { "playerName", player.PlayerName }
            };
        }

        private void ResetDicePlayerMakeHostageSounds()
        {
            // remove listener
            RemoveDicePlayerMakeHostageSoundsListener();
            // clear list
            _playersWithHostageSounds.Clear();
        }

        private void RemoveDicePlayerMakeHostageSoundsListener()
        {
            RemoveListener<Listeners.OnTick>(EventDicePlayerMakeHostageSoundsOnTick);
        }

        private void EventDicePlayerMakeHostageSoundsOnTick()
        {
            // remove listener if no players to save resources
            if (_playersWithHostageSounds.Count() == 0)
            {
                RemoveListener<Listeners.OnTick>(EventDicePlayerMakeHostageSoundsOnTick);
                return;
            }
            // worker
            Dictionary<CCSPlayerController, int> _playersWithHostageSoundsCopy = new(_playersWithHostageSounds);
            foreach (var (player, playerStatus) in _playersWithHostageSoundsCopy)
            {
                try
                {
                    // sanity checks
                    if (player == null
                    || player.PlayerPawn == null || !player.PlayerPawn.IsValid || player.PlayerPawn.Value == null
                    || player.PlayerPawn.Value.LifeState != (byte)LifeState_t.LIFE_ALIVE) continue;
                    if (player.Buttons == 0 && playerStatus == 0)
                    {
                        // emit sound
                        EmitSound(player, _hostageSounds[_random.Next(_hostageSounds.Count)]);
                        _playersWithHostageSounds[player] = (int)Server.CurrentTime + 1;
                    }
                    else if (player.Buttons != 0 && playerStatus <= (int)Server.CurrentTime)
                    {
                        _playersWithHostageSounds[player] = 0;
                    }
                }
                catch (Exception e)
                {
                    // remove player
                    _playersWithHostageSounds.Remove(player);
                    // log error
                    Console.WriteLine(Localizer["core.error"].Value.Replace("{error}", e.Message));
                }
            }
        }
    }
}
