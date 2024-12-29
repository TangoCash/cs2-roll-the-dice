using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin, IPluginConfig<PluginConfig>
    {
        [ConsoleCommand("rollthedice", "Roll the Dice")]
        [ConsoleCommand("rtd", "Roll the Dice")]
        [ConsoleCommand("dice", "Roll the Dice")]
        [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
        public void CommandRollTheDice(CCSPlayerController player, CommandInfo command)
        {
            Config.MapConfigs.TryGetValue(_currentMap, out var mapConfig);
            if ((!Config.Enabled)
                || (Config.Enabled && mapConfig != null && !mapConfig.Enabled))
            {
                if (command.CallingContext == CommandCallingContext.Console) player.PrintToChat(Localizer["core.disabled"]);
                command.ReplyToCommand(Localizer["core.disabled"]);
                return;
            }
            if ((bool)GetGameRule("WarmupPeriod")!)
            {
                if (command.CallingContext == CommandCallingContext.Console) player.PrintToChat(Localizer["command.rollthedice.iswarmup"]);
                command.ReplyToCommand(Localizer["command.rollthedice.iswarmup"]);
                return;
            }
            if (!_isDuringRound)
            {
                if (command.CallingContext == CommandCallingContext.Console) player.PrintToChat(Localizer["command.rollthedice.noactiveround"]);
                command.ReplyToCommand(Localizer["command.rollthedice.noactiveround"]);
                return;
            }
            CCSPlayerPawn playerPawn = player!.PlayerPawn.Value!;
            if (player.PlayerPawn == null || !player.PlayerPawn.IsValid || player.PlayerPawn.Value == null || playerPawn.LifeState != (byte)LifeState_t.LIFE_ALIVE)
            {
                if (command.CallingContext == CommandCallingContext.Console) player.PrintToChat(Localizer["command.rollthedice.notalive"]);
                command.ReplyToCommand(Localizer["command.rollthedice.notalive"]);
                return;
            };
            if (_playersThatRolledTheDice.ContainsKey(player))
            {
                if (command.CallingContext == CommandCallingContext.Console) player.PrintToChat(Localizer["command.rollthedice.alreadyrolled"].Value
                                                                                .Replace("{dice}", _playersThatRolledTheDice[player]));
                command.ReplyToCommand(Localizer["command.rollthedice.alreadyrolled"].Value
                    .Replace("{dice}", _playersThatRolledTheDice[player])
                );
                return;
            }
            // get random dice
            var dice = GetRandomDice();
            if (dice == -1)
            {
                if (command.CallingContext == CommandCallingContext.Console) player.PrintToChat(Localizer["core.nodicesenabled"]);
                command.ReplyToCommand(Localizer["command.rollthedice.nodicesenabled"]);
                return;
            }
            // add player to list
            _playersThatRolledTheDice.Add(player, _dices[dice].Method.Name);
            // count dice roll
            _countRolledDices[_dices[dice].Method.Name]++;
            // execute dice function
            Dictionary<string, string> data = _dices[dice](player, playerPawn);
            // send message to all players
            if (data.TryGetValue("_translation_all", out var translationAll))
            {
                string message = Localizer[translationAll].Value;
                foreach (var kvp in data)
                {
                    message = message.Replace($"{{{kvp.Key}}}", kvp.Value);
                }
                SendGlobalChatMessage(message);
            }
            // send message to other players (and maybe player)
            else if (data.TryGetValue("_translation_other", out var translationOther))
            {
                // send message to others
                string message = Localizer[translationOther].Value;
                foreach (var kvp in data)
                {
                    message = message.Replace($"{{{kvp.Key}}}", kvp.Value);
                }
                SendGlobalChatMessage(message, player: player);
            }
            // if player should get a message
            if (data.TryGetValue("_translation_player", out var translationPlayer))
            {
                string message = Localizer[translationPlayer].Value;
                foreach (var kvp in data)
                {
                    message = message.Replace($"{{{kvp.Key}}}", kvp.Value);
                }
                player.PrintToCenter(message);
                player.PrintToChat(message);
            }
            // play sound
            player.ExecuteClientCommand("play sounds/ui/coin_pickup_01.vsnd");
        }
    }
}
