using TheForce_Psycast.Lightsabers.Lightsaber_Combat;
using Verse;

namespace TheForce_Psycast.Lightsabers
{
    internal class DamageWorker_LightsaberCut : DamageWorker_AddInjury
    {
        public override DamageResult Apply(DamageInfo dinfo, Thing victim)
        {
            DamageResult damageResult = new DamageResult();
            if (victim is Pawn targetPawn)
            {
                var lightsaberBlade = targetPawn.equipment?.Primary?.TryGetComp<Comp_LightsaberBlade>();
                if (dinfo.Instigator is Pawn attacker)
                {
                    if (lightsaberBlade != null && LightsaberCombatUtility.CanParry(targetPawn, attacker)) // Check if target can parry
                    {
                        LightsaberCombatUtility.TriggerWeaponRotationOnParry(targetPawn, attacker);
                        Effecter effecter = new Effecter(ForceDefOf.Force_LClashOne);
                        effecter.Trigger(new TargetInfo(targetPawn.Position, targetPawn.Map), TargetInfo.Invalid);
                        effecter.Cleanup();

                        return damageResult;
                    }
                    else
                    {
                        base.Apply(dinfo, victim); 
                        return damageResult;
                    }
                }
            }
            base.Apply(dinfo, victim);

            return damageResult;
        }

        //protected override BodyPartRecord ChooseHitPart(DamageInfo dinfo, Pawn pawn)
        //{
        //    List<BodyPartRecord> manipulationSources = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined)
        //        .Where(part => part.def.tags.Contains(BodyPartTagDefOf.ManipulationLimbSegment)).ToList();

        //    if (manipulationSources.Any())
        //    {
        //        return manipulationSources.RandomElement();
        //    }
        //    else
        //    {
        //        return pawn.health.hediffSet.GetRandomNotMissingPart(dinfo.Def, dinfo.Height, BodyPartDepth.Outside);
        //    }
        //}
    }
}
