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
            if (isDead)
            {
                if (!ForceGhostUtility.IsForceGhost(pawn)) 
                {
                    if (Severity >= 1f)
                    {
                        yield return new Command_Action
                        {
                            defaultLabel = "Return as Ghost",
                            defaultDesc = "Return as a Force Ghost.",
                            icon = ContentFinder<Texture2D>.Get("Abilities/Lightside/ForceGhost", true),
                            action = () =>
                            {
                                ResurrectionUtility.TryResurrect(pawn);
                                pawn.health.AddHediff(ForceDefOf.Force_Ghost);
                                isDead = false;
                                Severity = 1;
                            }
                        };
                    }
                }
            }

            // Yield other gizmos from base class
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
}