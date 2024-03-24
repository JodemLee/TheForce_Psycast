using VanillaPsycastsExpanded;
using Verse;
using VFECore.Abilities;

namespace TheForce_Psycast
{
    public class Ability_ForceDefense : Ability, IChannelledPsycast
    {
        public bool IsActive => pawn.health.hediffSet.HasHediff(ForceDefOf.ForceDefense);

        public override Gizmo GetGizmo()
        {
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.ForceDefense);
            if (hediff != null)
                return new Command_Action
                {
                    defaultLabel = "VPE.CancelForceDefense".Translate(),
                    defaultDesc = "VPE.CancelSkipbarrierDesc".Translate(),
                    icon = def.icon,
                    action = delegate { pawn.health.RemoveHediff(hediff); },
                    Order = 10f + (def.requiredHediff?.hediffDef?.index ?? 0) + (def.requiredHediff?.minimumLevel ?? 0)
                };
            return base.GetGizmo();
        }
    }
}