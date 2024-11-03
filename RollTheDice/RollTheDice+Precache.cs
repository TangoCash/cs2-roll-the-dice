using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private readonly List<string> _precacheModels = new List<string>
        {
            "models/props/cs_office/file_cabinet1.vmdl"
        };

        private void OnServerPrecacheResources(ResourceManifest manifest)
        {
            foreach (var model in _precacheModels)
            {
                Console.WriteLine($"[RollTheDice] Precaching {model}");
                manifest.AddResource(model);
            }
        }
    }
}
