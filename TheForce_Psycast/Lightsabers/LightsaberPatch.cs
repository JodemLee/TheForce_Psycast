using RimWorld;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VanillaPsycastsExpanded;
using Verse;
using VFECore.Abilities;
using AbilityDef = VFECore.Abilities.AbilityDef;
using Ability = VanillaPsycastsExpanded;
using UnityEngine;
using TheForce_Psycast.Hediffs;

namespace TheForce_Psycast
{
    public class LightsaberPatch : ThingWithComps
    {
        private AbilityDef ability;
        private bool alreadyHad;
        public float equippedAngleOffset;
        private static Dictionary<Pawn, float> previousSeverities = new Dictionary<Pawn, float>();
        public AbilityDef Ability => ability;
        public bool Added => !alreadyHad;

        public void ExposeData(Pawn pawn)
        {
            base.ExposeData();
            Scribe_Values.Look(ref alreadyHad, "alreadyHad", false);
            Scribe_Collections.Look(ref previousSeverities, "previousSeverities", LookMode.Reference, LookMode.Value);

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                var hediff = pawn?.health?.hediffSet?.GetFirstHediffOfDef(ForceDefOf.Lightsaber_Stance);
                Scribe_Deep.Look(ref hediff, "lightsaberHediff");
            }
            else if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (Scribe.loader.curXmlParent["lightsaberHediff"] != null)
                {
                    Hediff hediff = null;
                    Scribe_Deep.Look(ref hediff, "lightsaberHediff");
                    if (hediff != null && pawn != null)
                    {
                        pawn.health.AddHediff(hediff);
                    }
                }
            }
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            var ability = ForceDefOf.Force_ThrowWeapon;
            var hediffDef = ForceDefOf.Lightsaber_Stance;
            base.Notify_Equipped(pawn);

            if (pawn.Psycasts() is null || pawn.Psycasts().level <= 0 || ability == null)
            {
                return;
            }

            var comp = pawn.GetComp<CompAbilities>();
            if (comp == null) return;

            alreadyHad = comp.HasAbility(ability);
            if (!alreadyHad) comp.GiveAbility(ability);

            var hediff = HediffMaker.MakeHediff(hediffDef, pawn);
            if (!previousSeverities.ContainsKey(pawn))
            {
                hediff = HediffMaker.MakeHediff(hediffDef, pawn);
                hediff.Severity = Rand.Range(1f, 7f);
                pawn.health.AddHediff(hediff);
            }
            else
            {
                hediff = HediffMaker.MakeHediff(hediffDef, pawn);
                hediff.Severity = previousSeverities[pawn]; // Use the stored severity
                pawn.health.AddHediff(hediff);
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            var ability = ForceDefOf.Force_ThrowWeapon;
            base.Notify_Unequipped(pawn);
            if (ability == null || alreadyHad) return;

            var compAbilities = pawn.GetComp<CompAbilities>();
            compAbilities?.LearnedAbilities.RemoveAll(ab => ab.def == ability);

            // Remove the hediff
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Lightsaber_Stance);
            if (hediff != null)
            {
                previousSeverities[pawn] = hediff.Severity; // Store the severity before removing
                pawn.health.RemoveHediff(hediff);
            }

            alreadyHad = false;
        }
    }
}