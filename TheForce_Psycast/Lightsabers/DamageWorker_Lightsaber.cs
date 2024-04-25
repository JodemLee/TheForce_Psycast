using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TheForce_Psycast.Lightsabers
{
    internal class DamageWorker_LightsaberCut: DamageWorker_AddInjury
    {
        protected override BodyPartRecord ChooseHitPart(DamageInfo dinfo, Pawn pawn)
        {
            // Check if there are any body parts responsible for manipulation
            List<BodyPartRecord> manipulationSources = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined)
                .Where(part => part.def.tags.Contains(BodyPartTagDefOf.ManipulationLimbSegment)).ToList();

            if (manipulationSources.Any())
            {
                // If there are manipulation sources, return a random one
                return manipulationSources.RandomElement();
            }
            else
            {
                // If there are no manipulation sources, return any other non-missing body part
                return pawn.health.hediffSet.GetRandomNotMissingPart(dinfo.Def, dinfo.Height, BodyPartDepth.Outside);
            }
        }
    }
}
