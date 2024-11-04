using CounterStrikeSharp.API.Core;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private List<CCSPlayerController> _playersDisguisedAsPlants = new();
        private Dictionary<CCSPlayerController, int> _playersDisguisedAsPlantsStates = new();
        private Dictionary<CCSPlayerController, string> _playersDisguisedAsPlantsOldModels = new();
        private Dictionary<CCSPlayerController, string> _playersDisguisedAsPlantsNewModels = new();
        private List<string> _playersDisguisedAsPlantsModels = new()
        {
            "models/props/cs_office/plant01.vmdl",
            "models/props_plants/plantairport01.vmdl"
        };

        private string DicePlayerDisguiseAsPlant(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            _playersDisguisedAsPlants.Add(player);
            _playersDisguisedAsPlantsStates[player] = 0;
            _playersDisguisedAsPlantsOldModels[player] = GetPlayerModel(playerPawn);
            _playersDisguisedAsPlantsNewModels[player] = _playersDisguisedAsPlantsModels[_random.Next(0, _playersDisguisedAsPlantsModels.Count)];
            return Localizer["DicePlayerDisguiseAsPlant"].Value
                .Replace("{playerName}", player.PlayerName);
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
                foreach (CCSPlayerController player in _playersDisguisedAsPlants)
                {
                    // sanity checks
                    if (player == null || player.Pawn == null || player.Pawn.Value == null
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
            });
        }
    }
}
