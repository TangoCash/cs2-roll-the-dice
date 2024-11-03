using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin, IPluginConfig<PluginConfig>
    {
        [ConsoleCommand("rollthedice", "Roll the Dice")]
        [ConsoleCommand("rtd", "Roll the Dice")]
        [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
        public void CommandRollTheDice(CCSPlayerController? player, CommandInfo command)
        {
            CCSPlayerPawn playerPawn = player!.PlayerPawn.Value!;
            if (!player.PlayerPawn.IsValid || player.PlayerPawn.Value == null || playerPawn.LifeState != (byte)LifeState_t.LIFE_ALIVE)
            {
                command.ReplyToCommand("[RollTheDice] You need to be alive to roll the dice (no bot)!");
                return;
            };
            if (_playersThatRolledTheDice.Contains(player))
            {
                command.ReplyToCommand("[RollTheDice] You already rolled the dice this round!");
                return;
            }
            // add player to list
            //_playersThatRolledTheDice.Add(player);
            // get random dice
            var dice = GetRandomDice();
            // execute dice function
            var message = _dices[dice](player, playerPawn);
            SendGlobalChatMessage($"[RollTheDice] {message}");
        }
    }
}
