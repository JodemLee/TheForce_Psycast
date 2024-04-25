using System.Collections.Generic;
using VanillaPsycastsExpanded;
using Verse;
using VFECore.Abilities;
using AbilityDef = VFECore.Abilities.AbilityDef;

namespace TheForce_Psycast
{
    public class LightsaberPatch : ThingWithComps
    {
        private AbilityDef ability;
        private bool alreadyHad;
        private static Dictionary<Pawn, float> previousSeverities = new Dictionary<Pawn, float>();

        // Constants for clarity
        private const float MinSeverity = 1f;
        private const float MaxSeverity = 7f;

        public AbilityDef Ability => ability;
        public bool Added => !alreadyHad;

        // ExposeData method
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref alreadyHad, "alreadyHad", false);
            Scribe_Collections.Look(ref previousSeverities, "previousSeverities", LookMode.Reference, LookMode.Value);
        }

        // Notify_Equipped method
        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);

            var ability = ForceDefOf.Force_ThrowWeapon;
            var hediffDef = ForceDefOf.Lightsaber_Stance;

            if (pawn == null || ability == null)
                return;

            // Check if the pawn has psycast abilities
            var psycastLevel = pawn.Psycasts()?.level ?? 0;
            if (psycastLevel <= 0)
                return;

            var comp = pawn.GetComp<CompAbilities>();
            if (comp == null)
                return;

            alreadyHad = comp.HasAbility(ability);
            if (!alreadyHad)
                comp.GiveAbility(ability);

            var hediff = HediffMaker.MakeHediff(hediffDef, pawn);
            hediff.Severity = previousSeverities.ContainsKey(pawn) ? previousSeverities[pawn] : Rand.Range(MinSeverity, MaxSeverity);
            pawn.health.AddHediff(hediff);
        }

        // Notify_Unequipped method
        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);

            var ability = ForceDefOf.Force_ThrowWeapon;
            if (ability == null || alreadyHad)
                return;

            var compAbilities = pawn.GetComp<CompAbilities>();
            compAbilities?.LearnedAbilities.RemoveAll(ab => ab.def == ability);

            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Lightsaber_Stance);
            if (hediff != null)
            {
                previousSeverities[pawn] = hediff.Severity;
                pawn.health.RemoveHediff(hediff);
            }

            alreadyHad = false;
        }
    }
}