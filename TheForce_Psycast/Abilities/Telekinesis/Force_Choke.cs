using RimWorld;
using RimWorld.Planet;
using System.Linq;
using TheForce_Psycast.Abilities;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace TheForce_Psycast
{
    internal class Force_Choke : Ability_WriteCombatLog
    {
        public override float GetPowerForPawn() => def.power + Mathf.FloorToInt((pawn.GetStatValue(StatDefOf.PsychicSensitivity) - 1) * 4);
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);

            foreach (var target in targets)
            {
                Pawn targetPawn = target.Thing as Pawn;

                if (targetPawn != null)
                {
                    var neckParts = targetPawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined)
                .Where(part => part.def.tags.Contains(BodyPartTagDefOf.BreathingPathway)).ToList();

                    if (neckParts != null && neckParts.Count > 0)
                    {
                        float damageAmount = GetPowerForPawn();
                        damageAmount = pawn.GetStatValue(StatDefOf.PsychicSensitivity) * damageAmount;
                        foreach (var neckPart in neckParts)
                        {
                            DamageInfo damageInfo = new DamageInfo(DamageDefOf.Blunt, damageAmount, 0, -1, null, neckPart);
                            targetPawn.TakeDamage(damageInfo);
                            var flyer = PawnFlyer.MakeFlyer(ForceDefOf.Force_ChokedPawn, targetPawn, targetPawn.Position, null, null);
                            GenSpawn.Spawn(flyer, targetPawn.Position, pawn.Map);
                        }
                    }
                }
            }
        }
    }
}
