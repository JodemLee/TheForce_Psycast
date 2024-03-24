using AnimalBehaviours;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VanillaPsycastsExpanded;
using Verse;
using Verse.Sound;

namespace TheForce_Psycast.Hediffs
{
    public class ForceGhostChanceHediff : HediffWithComps
    {
        public override void Notify_PawnDied()
        {
            Force_Ghost_Component.Instance.listOfPawnsThatDied.Add(this.pawn);
            base.Notify_PawnDied();
        }
    }

    public class ForceGhostHediff : HediffWithComps
    {
        public override void Notify_PawnDied()
        {
            Force_Ghost_Component.Instance.listOfPawnsThatDied.Add(this.pawn);
            base.Notify_PawnDied();
        }
    }

    internal class HediffComp_ForceGhost : HediffComp
    {
        public HediffCompProperties_ForceGhost Props
        {
            get
            {
                return (HediffCompProperties_ForceGhost)this.props;
            }
        }

        private Pawn pawn;

        public override void Notify_PawnDied()
        {
            Map map = this.parent.pawn.Corpse.Map;
            if (map != null)
            {
                {
                    SoundDefOf.PsychicPulseGlobal.PlayOneShot(new TargetInfo(this.parent.pawn.Corpse.Position, this.parent.pawn.Corpse.Map, false));
                    FleckMaker.AttachedOverlay(this.parent.pawn, DefDatabase<FleckDef>.GetNamed("PsycastPsychicEffect"), Vector3.zero, 1f, -1f);

                    // Create and drop clones of all equipment and apparel
                    foreach (Apparel apparel in base.Pawn.apparel.WornApparel)
                    {
                        lock (apparel) ;
                        // Clone the apparel
                        Apparel clonedApparel = (Apparel)ThingMaker.MakeThing(apparel.def, apparel.Stuff);
                        clonedApparel.SetColor(apparel.DrawColor);
                        clonedApparel.HitPoints = apparel.HitPoints;

                        // Drop the cloned apparel on the ground
                        GenPlace.TryPlaceThing(clonedApparel, this.parent.pawn.Corpse.Position, this.parent.pawn.Corpse.Map, ThingPlaceMode.Near);
                    }
                }
            }
    }
    }


    public class HediffCompProperties_ForceGhost : HediffCompProperties
    {
        public HediffCompProperties_ForceGhost()
        {
            this.compClass = typeof(HediffComp_ForceGhost);
        }
    }
}
    
