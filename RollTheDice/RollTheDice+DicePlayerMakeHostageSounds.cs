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

        private string DicePlayerMakeHostageSounds(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            _playersWithHostageSounds.Add(player, 0);
            return Localizer["DicePlayerMakeHostageSounds"].Value
                .Replace("{playerName}", player.PlayerName);
        }

        private void ResetDicePlayerMakeHostageSounds()
        {
            _playersWithHostageSounds.Clear();
        }

        private void CreateDicePlayerMakeHostageSoundsListener()
        {
            RegisterListener<Listeners.OnTick>(EventDicePlayerMakeHostageSoundsOnTick);
        }

        private void RemoveDicePlayerMakeHostageSoundsListener()
        {
            RemoveListener<Listeners.OnTick>(EventDicePlayerMakeHostageSoundsOnTick);
        }

        private void EventDicePlayerMakeHostageSoundsOnTick()
        {
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
                        EmitSound(player, _hostageSounds[Random.Shared.Next(_hostageSounds.Count)]);
                        _playersWithHostageSounds[player] = (int)Server.CurrentTime + 5;
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
