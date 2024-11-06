using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private readonly Dictionary<CCSPlayerController, int> _playersWithFakeGunSounds = new();
        private readonly List<(string, string, int, float)> _fakeGunSounds = new()
        {
            ("Deagle", "Weapon_DEagle.Single", 5, 2.0f),
            ("M249", "Weapon_M249.Single", 15, 1.5f),
            ("AWP", "Weapon_AWP.Single", 1, 1f),
            ("Bizon", "Weapon_bizon.Single", 10, 1.5f),
            ("P90", "Weapon_P90.Single", 15, 1.5f),
        };

        private string DicePlayerMakeFakeGunSounds(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            // create listener if not exists
            if (_playersWithFakeGunSounds.Count() == 0) RegisterListener<Listeners.OnTick>(EventDicePlayerMakeFakeGunSoundsOnTick);
            // add player to list
            _playersWithFakeGunSounds.Add(player, (int)Server.CurrentTime + Random.Shared.Next(3, 10));
            return Localizer["DicePlayerMakeFakeGunSounds"].Value
                .Replace("{playerName}", player.PlayerName);
        }

        private void ResetDicePlayerMakeFakeGunSounds()
        {
            _playersWithFakeGunSounds.Clear();
        }

        private void RemoveDicePlayerMakeFakeGunSoundsListener()
        {
            RemoveListener<Listeners.OnTick>(EventDicePlayerMakeFakeGunSoundsOnTick);
        }

        private void EventDicePlayerMakeFakeGunSoundsOnTick()
        {
            // remove listener if no players to save resources
            if (_playersWithFakeGunSounds.Count() == 0)
            {
                RemoveListener<Listeners.OnTick>(EventDicePlayerMakeFakeGunSoundsOnTick);
                return;
            }
            // worker
            Dictionary<CCSPlayerController, int> _playersWithFakeGunSoundsCopy = new(_playersWithFakeGunSounds);
            foreach (var (player, last_sound) in _playersWithFakeGunSoundsCopy)
            {
                try
                {
                    // sanity checks
                    if (player == null
                    || player.PlayerPawn == null || !player.PlayerPawn.IsValid || player.PlayerPawn.Value == null
                    || last_sound > (int)Server.CurrentTime
                    || player.Buttons != 0
                    || player.PlayerPawn.Value.LifeState != (byte)LifeState_t.LIFE_ALIVE) continue;
                    // get random gun sound entry
                    var (weaponName, soundName, playTotal, soundLength) = _fakeGunSounds[Random.Shared.Next(_fakeGunSounds.Count)];
                    EmitFakeGunSounds(player.Handle, soundName, soundLength, playTotal);
                    // let everyone know
                    SendGlobalChatMessage(Localizer["DicePlayerMakeFakeGunSoundsWeapon"].Value
                        .Replace("{playerName}", player.PlayerName)
                        .Replace("{weapon}", weaponName));
                    // reset timer
                    _playersWithFakeGunSounds[player] = (int)Server.CurrentTime + Random.Shared.Next(playTotal * (int)soundLength + 5, (playTotal * (int)soundLength) + 10);
                }
                catch (Exception e)
                {
                    // remove player
                    _playersWithFakeGunSounds.Remove(player);
                    // log error
                    Console.WriteLine(Localizer["core.error"].Value.Replace("{error}", e.Message));
                }
            }
        }

        private void EmitFakeGunSounds(nint playerHandle, string soundName, float soundLength, int playTotal, int playCount = 0)
        {
            playCount += 1;
            CCSPlayerController? player = new CCSPlayerController(playerHandle);
            if (player == null) return;
            EmitSound(player, soundName);
            if (playCount >= playTotal) return;
            AddTimer(soundLength, () =>
            {
                float randomDelay = (float)(Random.Shared.NextDouble() * (soundLength / 4)) + (soundLength / 3);
                EmitFakeGunSounds(playerHandle, soundName, randomDelay, playTotal, playCount);
            });
        }
    }
}
