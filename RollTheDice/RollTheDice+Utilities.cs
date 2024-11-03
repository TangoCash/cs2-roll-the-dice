using System.Runtime.InteropServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        public void SendGlobalChatMessage(string message, float delay = 0)
        {
            foreach (CCSPlayerController player in Utilities.GetPlayers())
            {
                if (player.IsBot) continue;
                AddTimer(delay, () => player.PrintToChat(message));
            }
        }

        public void SendGlobalCenterMessage(string message, float delay = 0, bool alert = false)
        {
            foreach (CCSPlayerController player in Utilities.GetPlayers())
            {
                if (player.IsBot) continue;
                if (alert) AddTimer(delay, () => player.PrintToCenterAlert(message));
                else AddTimer(delay, () => player.PrintToCenterHtml(message));
            }
        }

        private static string GetPlayerModel(CCSPlayerPawn playerPawn)
        {
            var signature = "\\x40\\x53\\x48\\x83\\xEC\\x20\\x48\\x8B\\x41\\x30\\x48\\x8B\\xD9\\x48\\x8B\\x48\\x08\\x48\\x8B\\x01\\x2A\\x2A\\x2A\\x48\\x85";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) signature = "\\x55\\x48\\x89\\xE5\\x53\\x48\\x89\\xFB\\x48\\x83\\xEC\\x08\\x48\\x8B\\x47\\x38";
            var getModel = new VirtualFunctionWithReturn<IntPtr, string>(signature);
            string model = getModel.Invoke(playerPawn.Handle);
            return model;
        }
    }
}