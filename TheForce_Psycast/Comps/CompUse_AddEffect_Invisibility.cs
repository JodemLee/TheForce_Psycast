using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TheForce_Psycast.Comps
{
    internal class CompUse_AddEffect_Invisibility : CompUseEffect
    {
        public override void DoEffect(Pawn usedBy)
        {

            base.DoEffect(usedBy);
            Hediff Invisibility = HediffMaker.MakeHediff(ForceDefOf.Magick_Invisibility, usedBy);
            usedBy.health.AddHediff(Invisibility);

        }
    }
}
