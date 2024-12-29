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
            ("M249", "Weapon_M249.Single", 15, 0.7f),
            ("AWP", "Weapon_AWP.Single", 1, 1f),
            ("Bizon", "Weapon_bizon.Single", 10, 1.5f),
            ("P90", "Weapon_P90.Single", 15, 1.1f),
            ("G3SG1", "Weapon_G3SG1.Single", 11, 1.1f),
            ("Negev", "Weapon_Negev.Single", 35, 0.7f),
            ("Nova", "Weapon_Nova.Single", 3, 2.5f),
            ("AUG", "Weapon_AUG.Single", 30, 1.1f),
            ("M4A1", "Weapon_M4A1.Single", 25, 0.9f)
        };

        private Dictionary<string, string> DicePlayerMakeFakeGunSounds(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            // create listener if not exists
            if (_playersWithFakeGunSounds.Count() == 0) RegisterListener<Listeners.OnTick>(EventDicePlayerMakeFakeGunSoundsOnTick);
            // add player to list
            _playersWithFakeGunSounds.Add(player, (int)Server.CurrentTime + _random.Next(3, 10));
            return new Dictionary<string, string>
            {
                {"_translation_player", "DicePlayerMakeFakeGunSoundsPlayer"},
                {"_translation_other", "DicePlayerMakeFakeGunSounds"},
                { "playerName", player.PlayerName }
            };
        }

        private void DicePlayerMakeFakeGunSoundsUnload()
        {
            DicePlayerMakeFakeGunSoundsReset();
        }

        private void DicePlayerMakeFakeGunSoundsReset()
        {
            RemoveListener<Listeners.OnTick>(EventDicePlayerMakeFakeGunSoundsOnTick);
            _playersWithFakeGunSounds.Clear();
        }

        private void EventDicePlayerMakeFakeGunSoundsOnTick()
        {
            // remove listener if no players to save resources
            if (_playersWithFakeGunSounds.Count() == 0)
            {
                DicePlayerMakeFakeGunSoundsReset();
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
                    var (weaponName, soundName, playTotal, soundLength) = _fakeGunSounds[_random.Next(_fakeGunSounds.Count)];
                    EmitFakeGunSounds(player.Handle, soundName, soundLength, playTotal);
                    // let the player know
                    player.PrintToCenter(Localizer["DicePlayerMakeFakeGunSoundsWeaponPlayer"].Value
                        .Replace("{weapon}", weaponName));
                    // let everyone else know
                    SendGlobalChatMessage(Localizer["DicePlayerMakeFakeGunSoundsWeapon"].Value
                        .Replace("{playerName}", player.PlayerName)
                        .Replace("{weapon}", weaponName),
                        player: player);
                    // reset timer
                    _playersWithFakeGunSounds[player] = (int)Server.CurrentTime + _random.Next(playTotal * (int)soundLength + 2, (playTotal * (int)soundLength));
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
            if (player == null
                || player.PlayerPawn == null || !player.PlayerPawn.IsValid || player.PlayerPawn.Value == null
                || player.PlayerPawn.Value.LifeState != (byte)LifeState_t.LIFE_ALIVE) return;
            EmitSound(player, soundName);
            if (playCount >= playTotal) return;
            AddTimer(soundLength, () =>
            {
                float randomDelay = (float)(_random.NextDouble() * (soundLength / 4)) + (soundLength / 3);
                EmitFakeGunSounds(playerHandle, soundName, randomDelay, playTotal, playCount);
            });
        }
    }
}
