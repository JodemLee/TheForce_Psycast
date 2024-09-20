using Verse;
using VFECore.Abilities;

namespace TheForce_Psycast
{
    public class Comp_LightsaberBlasterStance : Comp_LightsaberStance
    {
        private AbilityDef additionalAbility;

        private bool additionalAbilityAlreadyHad;

        public new CompProperties_LightsaberStance Props => (CompProperties_LightsaberStance)props;

        private const float AdditionalMinSeverity = 1f;

        private const float AdditionalMaxSeverity = 7f;

        public AbilityDef AdditionalAbility => additionalAbility;

        public bool AdditionalAdded => !additionalAbilityAlreadyHad;

        public Comp_LightsaberBlasterStance()
        {
            additionalAbility = ForceDefOf.Force_BlasterShot_Stun;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref additionalAbilityAlreadyHad, "additionalAbilityAlreadyHad", defaultValue: false);
            Scribe_Defs.Look<AbilityDef>(ref additionalAbility, "additionalAbility");
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            if (pawn == null || additionalAbility == null)
            {
                return;
            }
            CompAbilities comp = ((ThingWithComps)pawn).GetComp<CompAbilities>();
            if (comp != null)
            {
                additionalAbilityAlreadyHad = comp.HasAbility(additionalAbility);
                if (!additionalAbilityAlreadyHad)
                {
                    comp.GiveAbility(additionalAbility);
                }
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            if (additionalAbility == null || additionalAbilityAlreadyHad)
            {
                return;
            }
            CompAbilities compAbilities = pawn.GetComp<CompAbilities>();
            if (compAbilities != null)
            {
                compAbilities.LearnedAbilities.RemoveAll((Ability ab) => ab.def == additionalAbility);
            }
            additionalAbilityAlreadyHad = false;
        }
    }

    public class CompProperties_LightsaberBlasterStance : CompProperties
    {
        public CompProperties_LightsaberBlasterStance()
        {
            this.compClass = typeof(Comp_LightsaberBlasterStance);
        }
    }
}

