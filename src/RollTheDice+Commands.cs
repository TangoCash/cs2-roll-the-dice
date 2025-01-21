using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using System.Drawing;

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
            }
            if (_playersThatRolledTheDice.ContainsKey(player) && _playersThatRolledTheDice[player].ContainsKey("message"))
            {
                if (command.CallingContext == CommandCallingContext.Console)
                    player.PrintToChat(Localizer["command.rollthedice.alreadyrolled"].Value
                        .Replace("{dice}", (string)_playersThatRolledTheDice[player]["message"]));
                command.ReplyToCommand(Localizer["command.rollthedice.alreadyrolled"].Value
                    .Replace("{dice}", (string)_playersThatRolledTheDice[player]["message"]));
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
            // debug print
            DebugPrint($"Player {player.PlayerName} rolled the dice and got {_dices[dice].Method.Name}");
            // add player to list
            _playersThatRolledTheDice.Add(player, new Dictionary<string, object> { { "dice", _dices[dice].Method.Name } });
            // count dice roll
            _countRolledDices[_dices[dice].Method.Name]++;
            // execute dice function
            Dictionary<string, string> data = _dices[dice](player, playerPawn);
            // check for error
            if (data.ContainsKey("error"))
            {
                string message = Localizer[data["error"]];
                // send message to player
                player.PrintToCenter(message);
                player.PrintToChat(message);
                // change player message
                _playersThatRolledTheDice[player]["message"] = message;
                return;
            }
            // send message to all players
            if (!Localizer[$"{_dices[dice].Method.Name}_all"].ResourceNotFound)
            {
                string message = Localizer[$"{_dices[dice].Method.Name}_all"].Value;
                foreach (var kvp in data)
                {
                    message = message.Replace($"{{{kvp.Key}}}", kvp.Value);
                }
                SendGlobalChatMessage(message);
            }
            // send message to other players (and maybe player)
            else if (!Localizer[$"{_dices[dice].Method.Name}_other"].ResourceNotFound)
            {
                // send message to others
                string message = Localizer[$"{_dices[dice].Method.Name}_other"].Value;
                foreach (var kvp in data)
                {
                    message = message.Replace($"{{{kvp.Key}}}", kvp.Value);
                }
                SendGlobalChatMessage(message, player: player);
            }
            // if player should get a message
            if (!Localizer[$"{_dices[dice].Method.Name}_player"].ResourceNotFound)
            {
                string message = Localizer[$"{_dices[dice].Method.Name}_player"].Value;
                foreach (var kvp in data)
                {
                    message = message.Replace($"{{{kvp.Key}}}", kvp.Value);
                }
                player.PrintToCenter(message);
                player.PrintToChat(message);
                // change player message (if any)
                _playersThatRolledTheDice[player]["message"] = message;
            }
            // if player should get a GUI message
            if (!Localizer[$"{_dices[dice].Method.Name}_gui"].ResourceNotFound)
            {
                string message = Localizer["command.prefix"].Value
                    + " "
                    + Localizer[$"{_dices[dice].Method.Name}_gui"].Value;
                foreach (var kvp in data)
                {
                    message = message.Replace($"{{{kvp.Key}}}", kvp.Value);
                }
                // create gui entity
                CPointWorldText? playerGUIMessage = CreateGUI(
                    player: player,
                    text: message,
                    size: 40,
                    color: Color.Purple,
                    font: "Verdana",
                    shiftX: -2.9f,
                    shiftY: 4.4f
                );
                if (playerGUIMessage != null) _playersThatRolledTheDice[player]["gui_message"] = playerGUIMessage;
                // create (empty) status gui entity
                CPointWorldText? playerGUIStatus = CreateGUI(
                    player: player,
                    text: "",
                    size: 30,
                    color: Color.Red,
                    font: "Verdana",
                    shiftX: -2.75f,
                    shiftY: 4.0f
                );
                if (playerGUIStatus != null) _playersThatRolledTheDice[player]["gui_status"] = playerGUIStatus;
            }
            // play sound
            player.ExecuteClientCommand("play sounds/ui/coin_pickup_01.vsnd");
        }
    }
}
