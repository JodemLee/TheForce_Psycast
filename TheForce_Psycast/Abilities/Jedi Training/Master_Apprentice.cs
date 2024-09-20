using RimWorld;
using RimWorld.Planet;
using TheForce_Psycast.Abilities.Jedi_Training;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace TheForce_Psycast.Abilities
{
    internal class Master_Apprentice : Ability
    {
        private Hediff_Master masterHediff;

        public override void Init()
        {
            base.Init();
            masterHediff = pawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Master) as Hediff_Master;
            if (masterHediff == null)
            {
                CreateMasterHediff();
            }
        }

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            if (target.Pawn == null || !this.CasterPawn.HasPsylink)
            {
                return false;
            }

            // Check if the target is a psycaster
            if (!target.Pawn.HasPsylink)
            {
                if (showMessages)
                {
                    Messages.Message("Force.TargetIsNotPsycaster".Translate(), MessageTypeDefOf.CautionInput);
                }
                return false;
            }

            // Check if the psycaster is already an apprentice
            if (target.Pawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Apprentice) != null)
            {
                if (showMessages)
                {
                    Messages.Message("Force.TargetIsApprenticeAlready".Translate(), MessageTypeDefOf.CautionInput);
                }
                return false;
            }

            // Check if the psycaster's psylink level is lower than the caster's level
            if (target.Pawn.GetPsylinkLevel() >= this.CasterPawn.GetPsylinkLevel())
            {
                if (showMessages)
                {
                    Messages.Message("Force.TargetPsylinkLevelTooHigh".Translate(), MessageTypeDefOf.CautionInput);
                }
                return false;
            }

            return base.ValidateTarget(target, showMessages);
        }
        public override bool IsEnabledForPawn(out string reason)
        {
            if (masterHediff != null && masterHediff.apprentices.Count >= masterHediff.apprenticeCapacity)
            {
                reason = "Force.CannotHaveMoreApprentice".Translate();
                return false;
            }
            return base.IsEnabledForPawn(out reason);
        }

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);

            if (targets == null || targets.Length == 0 || targets[0].Thing == null)
            {
                return;
            }

            var target = targets[0].Thing as Pawn;

            if (masterHediff == null)
            {
                CreateMasterHediff();
            }

            var puppetHediff = HediffMaker.MakeHediff(ForceDefOf.Force_Apprentice, target, target.health.hediffSet.GetBrain()) as Hediff_Apprentice;
            puppetHediff.master = pawn;
            masterHediff.apprentices.Add(target);
            target.health.AddHediff(puppetHediff);

            target.Notify_DisabledWorkTypesChanged();
            PawnComponentsUtility.AddAndRemoveDynamicComponents(target);
        }

        private void CreateMasterHediff()
        {
            masterHediff = HediffMaker.MakeHediff(ForceDefOf.Force_Master, pawn, pawn.health.hediffSet.GetBrain()) as Hediff_Master;
            pawn.health.AddHediff(masterHediff);
        }

    }
}
