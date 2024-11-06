using CounterStrikeSharp.API.Core;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private Dictionary<CCSPlayerController, Dictionary<string, string>> _playersAsChicken = new();
        private readonly string _playersAsChickenModel = "models/chicken/chicken.vmdl";

        private string DicePlayerAsChicken(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            if (_playersAsChicken.ContainsKey(player)) return Localizer["command.rollthedice.error"].Value.Replace("{playerName}", player.PlayerName);
            _playersAsChicken.Add(player, new Dictionary<string, string>());
            _playersAsChicken[player]["old_model"] = GetPlayerModel(playerPawn);
            _playersAsChicken[player]["prop"] = SpawnProp(player, _playersAsChickenModel, 5.0f).ToString();
            MakePlayerInvisible(player);
            return Localizer["DicePlayerAsChicken"].Value
                .Replace("{playerName}", player.PlayerName);
        }

        private void ResetDicePlayerAsChicken()
        {
            foreach (CCSPlayerController player in _playersAsChicken.Keys)
            {
                if (player == null || player.Pawn == null || player.Pawn.Value == null) continue;
                RemoveProp(int.Parse(_playersAsChicken[player]["prop"]));
                MakePlayerVisible(player);
            }
            _playersAsChicken.Clear();
        }

        private void CreateDicePlayerAsChickenListener()
        {
            RegisterListener<Listeners.OnTick>(EventDicePlayerAsChickenOnTick);
            RegisterEventHandler<EventPlayerDeath>(EventDicePlayerAsChickenOnPlayerDeath);
        }

        private void RemoveDicePlayerAsChickenListener()
        {
            RemoveListener<Listeners.OnTick>(EventDicePlayerAsChickenOnTick);
        }

        private void EventDicePlayerAsChickenOnTick()
        {
            Dictionary<CCSPlayerController, Dictionary<string, string>> _playersAsChickenCopy = new(_playersAsChicken);
            foreach (var (player, playerData) in _playersAsChickenCopy)
            {
                try
                {
                    // sanity checks
                    if (player == null
                    || player.Pawn == null
                    || player.Pawn.Value == null
                    || player.PlayerPawn == null || !player.PlayerPawn.IsValid || player.PlayerPawn.Value == null
                    || player.PlayerPawn.Value.LifeState != (byte)LifeState_t.LIFE_ALIVE
                    || !playerData.ContainsKey("prop")) continue;
                    UpdateProp(
                        player,
                        int.Parse(playerData["prop"])
                    );
                }
                catch (Exception e)
                {
                    // remove player
                    _playersAsChicken.Remove(player);
                    // log error
                    Console.WriteLine(Localizer["core.error"].Value.Replace("{error}", e.Message));
                }
            }
        }

        private HookResult EventDicePlayerAsChickenOnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
        {
            CCSPlayerController player = @event.Userid!;
            if (_playersAsChicken.ContainsKey(player))
            {
                RemoveProp(int.Parse(_playersAsChicken[player]["prop"]));
                MakePlayerVisible(player);
                _playersAsChicken.Remove(player);
            }
            return HookResult.Continue;
        }
    }
}
