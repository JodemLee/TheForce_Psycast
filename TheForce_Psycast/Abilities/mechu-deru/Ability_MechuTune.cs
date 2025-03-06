using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using VFECore.Abilities;

namespace TheForce_Psycast.Abilities.Mechu_deru
{
    public class Ability_MechuTune : Ability_WriteCombatLog
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);

            if (targets[0].Thing is Pawn targetPawn)
            {
                OverclockImplants(targetPawn);
            }
        }

        private void OverclockImplants(Pawn targetPawn)
        {
            var implantHediffs = targetPawn.health.hediffSet.hediffs
                .Where(hediff => IsImplantOrProsthetic(hediff))
                .ToList();

            foreach (var implantHediff in implantHediffs)
            {
                var bodyPart = implantHediff.Part;
                if (bodyPart != null)
                {
                    targetPawn.health.AddHediff(ForceDefOf.Force_MechuTuneOverclocking, bodyPart);
                }
            }
        }

        private bool IsImplantOrProsthetic(Hediff hediff)
        {
            var implantDef = hediff.def;
            return implantDef != null && implantDef.countsAsAddedPartOrImplant;
        }
    }

    public class Ability_MechuFlare : VFECore.Abilities.Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);

            if (targets[0].Thing is Pawn targetPawn)
            {
                WeakenImplants(targetPawn);
            }
        }

        private void WeakenImplants(Pawn targetPawn)
        {
            var implantHediffs = targetPawn.health.hediffSet.hediffs
                .Where(hediff => IsImplantOrProsthetic(hediff))
                .ToList();

            foreach (var implantHediff in implantHediffs)
            {
                var bodyPart = implantHediff.Part;
                if (bodyPart != null)
                {
                    targetPawn.health.AddHediff(ForceDefOf.Force_ImplantFlare, bodyPart);
                }
            }
        }

        private bool IsImplantOrProsthetic(Hediff hediff)
        {
            var implantDef = hediff.def;
            return implantDef != null && implantDef.countsAsAddedPartOrImplant;
        }
    }
}
