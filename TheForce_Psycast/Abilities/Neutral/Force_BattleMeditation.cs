using RimWorld.Planet;
using System.Linq;
using VanillaPsycastsExpanded;
using Verse;
using VFECore.Abilities;

namespace TheForce_Psycast
{
    internal class Force_BattleMeditation : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            var groupLinkMaster = this.pawn.health.hediffSet.GetFirstHediffOfDef(VPE_DefOf.VPE_GroupLink) as Hediff_BattleMeditation;
            if (groupLinkMaster != null)
            {
                foreach (var linkedPawn in groupLinkMaster.linkedPawns)
                {
                    if (!targets.Any(x => x.Thing == linkedPawn))
                    {
                        base.Cast([linkedPawn]);
                    }
                }
            }
        }

        public override Hediff ApplyHediff(Pawn targetPawn, HediffDef hediffDef, BodyPartRecord bodyPart, int duration, float severity)
        {
            var hediff = base.ApplyHediff(targetPawn, hediffDef, bodyPart, duration, severity) as Hediff_BattleMeditation;
            hediff.LinkAllPawnsAround();
            return hediff;
        }
    }
}
