using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TheForce_Psycast.Hediffs
{
    public class HediffWithComps_LightsideGhost : HediffWithComps
    {
        private bool isDead;
        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            base.Notify_PawnDied(dinfo, culprit);
            isDead = true;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (isDead && Severity >= 1f && !ForceGhostUtility.IsForceGhost(pawn))
            {
                yield return new Command_Action
                {
                    defaultLabel = "Force_ReturnAsGhost".Translate(),
                    defaultDesc = "Force_ReturnAsGhost".Translate(),
                    icon = ContentFinder<Texture2D>.Get("Abilities/Lightside/ForceGhost", true),
                    action = () =>
                    {
                        GhostResurrectionUtility.TryReturnAsGhost(pawn, ForceDefOf.Force_Ghost, 1f);
                        pawn.apparel.LockAll();
                    }
                };
            }

            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
        }
    }

    public class HediffComps_ForceGhost : HediffComp
    {
        public HediffCompProperties_ForceGhost Props => (HediffCompProperties_ForceGhost)props;

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            base.Notify_PawnDied(dinfo);
            if (ForceGhostUtility.IsForceGhost(parent.pawn))
            { ResurrectionUtility.TryResurrect(parent.pawn); }
            else
            {
                return;
            }
        }
    }

    public class HediffCompProperties_ForceGhost : HediffCompProperties
    {
        public HediffCompProperties_ForceGhost()
        {
            this.compClass = typeof(HediffComps_ForceGhost);
        }
    }

    public static class GhostResurrectionUtility
    {
        public static void TryReturnAsGhost(Pawn pawn, HediffDef ghostHediff, float severity = 1f)
        {
            ResurrectionUtility.TryResurrect(pawn);
            pawn.health.AddHediff(ghostHediff);
            Hediff ghost = pawn.health.hediffSet.GetFirstHediffOfDef(ghostHediff);
            if (ghost != null)
            {
                ghost.Severity = severity;
            }
        }
    }
}