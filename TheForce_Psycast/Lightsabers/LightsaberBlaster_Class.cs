using VanillaPsycastsExpanded;
using Verse;
using VFECore.Abilities;
using AbilityDef = VFECore.Abilities.AbilityDef;

namespace TheForce_Psycast
{
    public class EnhancedLightsaberPatch : LightsaberPatch
    {
        private AbilityDef additionalAbility;
        private bool additionalAbilityAlreadyHad;

        // Constants for clarity
        private const float AdditionalMinSeverity = 1f;
        private const float AdditionalMaxSeverity = 7f;

        public AbilityDef AdditionalAbility => additionalAbility;
        public bool AdditionalAdded => !additionalAbilityAlreadyHad;

        public EnhancedLightsaberPatch() => additionalAbility = ForceDefOf.Force_BlasterShot_Stun; // Replace with your desired ability

        // Override ExposeData method to include additionalAbilityAlreadyHad
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref additionalAbilityAlreadyHad, "additionalAbilityAlreadyHad", false);
            Scribe_Defs.Look(ref additionalAbility, "additionalAbility");
        }

        // Override Notify_Equipped method to add additional ability
        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);

            if (pawn == null || additionalAbility == null)
                return;

            var comp = pawn.GetComp<CompAbilities>();
            if (comp == null)
                return;

            additionalAbilityAlreadyHad = comp.HasAbility(additionalAbility);
            if (!additionalAbilityAlreadyHad)
                comp.GiveAbility(additionalAbility);

        }

        // Override Notify_Unequipped method to remove additional ability
        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);

            if (additionalAbility == null || additionalAbilityAlreadyHad)
                return;

            var compAbilities = pawn.GetComp<CompAbilities>();
            compAbilities?.LearnedAbilities.RemoveAll(ab => ab.def == additionalAbility);
            additionalAbilityAlreadyHad = false;
        }
    }
}
