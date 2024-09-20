using RimWorld;
using System.Collections.Generic;
using Verse;

namespace TheForce_Psycast
{
    public  class  CompUse_AddEffectSithHealing : CompUseEffect
    {
        private List<Hediff> tmpHediffs = new List<Hediff>();
         Hediff hediff;
        public  CompProperties_BiosculpterPod_HealingCycle Props => (CompProperties_BiosculpterPod_HealingCycle)props;
        public override void DoEffect(Pawn usedBy)
        {

            base.DoEffect(usedBy);
            Hediff SithHealing = HediffMaker.MakeHediff(ForceDefOf.Force_SithHeal, usedBy);
            usedBy.health.AddHediff(SithHealing);

        }
    }
}
