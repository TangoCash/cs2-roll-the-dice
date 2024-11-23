using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace RollTheDice
{
    public partial class RollTheDice : BasePlugin
    {
        private Dictionary<string, string> DiceChickenLeader(CCSPlayerController player, CCSPlayerPawn playerPawn)
        {
            var amountChickens = 16;
            // spawn chickens
            for (int i = 0; i < amountChickens; i++)
            {
                CChicken? chicken = Utilities.CreateEntityByName<CChicken>("chicken");
                if (chicken != null)
                {
                    chicken.Teleport(player.Pawn.Value!.AbsOrigin!, player.Pawn.Value.AbsRotation!, player.Pawn.Value.AbsVelocity);
                    Vector offset = new Vector(
                        (float)(100 * Math.Cos(2 * Math.PI * i / amountChickens)),
                        (float)(100 * Math.Sin(2 * Math.PI * i / amountChickens)),
                        0
                    );
                    chicken.Teleport(player.Pawn.Value!.AbsOrigin! + offset, player.Pawn.Value.AbsRotation!, player.Pawn.Value.AbsVelocity);
                    chicken.DispatchSpawn();
                    // create fire particle effect
                    var particle = Utilities.CreateEntityByName<CParticleSystem>("info_particle_system")!;
                    particle.StartActive = true;
                    particle.EffectName = "particles/burning_fx/env_fire_tiny.vpcf";
                    particle.DispatchSpawn();
                    particle.Teleport(player.Pawn.Value!.AbsOrigin! + offset, player.Pawn.Value.AbsRotation!, player.Pawn.Value.AbsVelocity);
                    Server.RunOnTick(Server.TickCount + 1, () =>
                    {
                        particle.AcceptInput("SetParent", chicken, null, "!activator");
                        particle.AcceptInput("Start");
                    });
                    // remove fire after some seconds
                    int delay = _random.Next(3, 6);
                    AddTimer(delay, () =>
                    {
                        if (particle == null || !particle.IsValid) return;
                        particle.AcceptInput("Kill");
                        particle.Remove();
                    });
                }
            }
            return new Dictionary<string, string>
            {
                {"_translation", "DiceChickenLeader"},
                { "playerName", player.PlayerName }
            };
        }
    }
}
