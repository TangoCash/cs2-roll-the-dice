using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private readonly List<string> _precacheModels = new List<string>
        {
            "models/chicken/chicken.vmdl",
            "particles/burning_fx/env_fire_tiny.vpcf",
            "models/props/cs_office/plant01.vmdl",
        };

        private void OnServerPrecacheResources(ResourceManifest manifest)
        {
            foreach (var model in _precacheModels)
            {
                manifest.AddResource(model);
            }
        }
    }
}
