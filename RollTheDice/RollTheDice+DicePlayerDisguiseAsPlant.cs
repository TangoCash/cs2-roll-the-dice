using CounterStrikeSharp.API.Core;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private List<CCSPlayerController> _playersDisguisedAsPlants = new();
        private Dictionary<CCSPlayerController, int> _playersDisguisedAsPlantsStates = new();
        private Dictionary<CCSPlayerController, string> _playersDisguisedAsPlantsOldModels = new();
        private Dictionary<CCSPlayerController, string> _playersDisguisedAsPlantsNewModels = new();
        private readonly Dictionary<string, string> _playersDisguisedAsPlantsModels = new()
        {
            //{"Office/Plant", "models/props/cs_office/plant01.vmdl"},
            //{"Trafficcone", "models/props/de_vertigo/trafficcone_clean.vmdl"},
            //{"Barstool", "models/generic/barstool_01/barstool_01.vmdl"},
            //{"Fireextinguisher", "models/generic/fire_extinguisher_01/fire_extinguisher_01.vmdl"},
            {"Hostage", "models/hostage/hostage.vmdl"},
            //{"Pottery", "models/ar_shoots/shoots_pottery_02.vmdl"},
            //{"AnubisInfoPanel", "models/anubis/signs/anubis_info_panel_01.vmdl"}

        };
        private string DicePlayerDisguiseAsPlant(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            if (_playersDisguisedAsPlants.Contains(player)) return Localizer["command.rollthedice.error"].Value.Replace("{playerName}", player.PlayerName);
            _playersDisguisedAsPlants.Add(player);
            _playersDisguisedAsPlantsStates[player] = 0;
            _playersDisguisedAsPlantsOldModels[player] = GetPlayerModel(playerPawn);
            var randomKey = _playersDisguisedAsPlantsModels.Keys.ElementAt(_random.Next(0, _playersDisguisedAsPlantsModels.Count));
            _playersDisguisedAsPlantsNewModels[player] = _playersDisguisedAsPlantsModels[randomKey];
            return Localizer["DicePlayerDisguiseAsPlant"].Value
                .Replace("{playerName}", player.PlayerName)
                .Replace("{model}", randomKey);
        }

        private void ResetDicePlayerDisguiseAsPlant()
        {
            foreach (CCSPlayerController player in _playersDisguisedAsPlants)
            {
                if (player == null || player.Pawn == null || player.Pawn.Value == null) continue;
                player.Pawn.Value.SetModel(_playersDisguisedAsPlantsOldModels[player]);
            }
            _playersDisguisedAsPlants.Clear();
            _playersDisguisedAsPlantsStates.Clear();
            _playersDisguisedAsPlantsOldModels.Clear();
            _playersDisguisedAsPlantsNewModels.Clear();
        }

        private void CreateDicePlayerDisguiseAsPlantListener()
        {
            RegisterListener<Listeners.OnTick>(() =>
            {
                List<CCSPlayerController> _playersDisguisedAsPlantsCopy = new(_playersDisguisedAsPlants);
                foreach (CCSPlayerController player in _playersDisguisedAsPlantsCopy)
                {
                    try
                    {
                        // sanity checks
                        if (player == null
                        || player.Pawn == null
                        || player.Pawn.Value == null
                        || player.PlayerPawn == null || !player.PlayerPawn.IsValid || player.PlayerPawn.Value == null
                        || player.PlayerPawn.Value.LifeState != (byte)LifeState_t.LIFE_ALIVE) continue;
                        // change player model if player is not pressing any buttons
                        if (player.Buttons == 0 && _playersDisguisedAsPlantsStates[player] != 1)
                        {
                            _playersDisguisedAsPlantsStates[player] = 1;
                            player.Pawn.Value.SetModel(_playersDisguisedAsPlantsNewModels[player]);
                        }
                        else if (player.Buttons != 0 && _playersDisguisedAsPlantsStates[player] != 0)
                        {
                            _playersDisguisedAsPlantsStates[player] = 0;
                            if (_playersDisguisedAsPlantsOldModels[player] != "") player.Pawn.Value.SetModel(_playersDisguisedAsPlantsOldModels[player]);
                        }
                    }
                    catch (Exception e)
                    {
                        // remove player
                        _playersDisguisedAsPlants.Remove(player);
                        _playersDisguisedAsPlantsStates.Remove(player);
                        _playersDisguisedAsPlantsOldModels.Remove(player);
                        _playersDisguisedAsPlantsNewModels.Remove(player);
                        // log error
                        Console.WriteLine(Localizer["core.error"].Value.Replace("{error}", e.Message));
                    }
                }
            });
        }
    }
}
