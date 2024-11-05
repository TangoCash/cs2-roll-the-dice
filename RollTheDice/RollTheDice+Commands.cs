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
        public void CommandRollTheDice(CCSPlayerController? player, CommandInfo command)
        {
            CCSPlayerPawn playerPawn = player!.PlayerPawn.Value!;
            if (!_isDuringRound || (bool)GetGameRule("WarmupPeriod")!)
            {
                if (command.CallingContext == CommandCallingContext.Console) player.PrintToChat(Localizer["command.rollthedice.noactiveround"]);
                command.ReplyToCommand(Localizer["command.rollthedice.noactiveround"]);
                return;
            }
            if (player.PlayerPawn == null || !player.PlayerPawn.IsValid || player.PlayerPawn.Value == null || playerPawn.LifeState != (byte)LifeState_t.LIFE_ALIVE)
            {
                if (command.CallingContext == CommandCallingContext.Console) player.PrintToChat(Localizer["command.rollthedice.notalive"]);
                command.ReplyToCommand(Localizer["command.rollthedice.notalive"]);
                return;
            };
            if (_playersThatRolledTheDice.Contains(player))
            {
                if (command.CallingContext == CommandCallingContext.Console) player.PrintToChat(Localizer["command.rollthedice.alreadyrolled"]);
                command.ReplyToCommand(Localizer["command.rollthedice.alreadyrolled"]);
                return;
            }
            // add player to list
            _playersThatRolledTheDice.Add(player);
            // get random dice
            var dice = GetRandomDice();
            if (dice == -1)
            {
                if (command.CallingContext == CommandCallingContext.Console) player.PrintToChat(Localizer["core.nodicesenabled"]);
                command.ReplyToCommand(Localizer["command.rollthedice.nodicesenabled"]);
                return;
            }
            // execute dice function
            var message = _dices[dice](player, playerPawn);
            SendGlobalChatMessage(message);
        }
    }
}
