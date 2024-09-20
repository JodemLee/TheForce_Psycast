using RimWorld;
using VanillaPsycastsExpanded;
using Verse;

namespace TheForce_Psycast.Abilities.Jedi_Training
{
    public class Hediff_Apprentice : HediffWithComps
    {
        public Pawn master;
        private int ticksSinceLastXPGain = 0;
        public int xpGainInterval;

        public override string Label => base.Label + ": " + master.LabelShort;

        public Hediff_Apprentice()
        {
            xpGainInterval = Rand.Range(120000, 300000);
        }

        public override void Tick()
        {
            base.Tick();
            ticksSinceLastXPGain++;
            if (ticksSinceLastXPGain >= xpGainInterval)
            {
                GainExperienceAndCheckLevels();
                ticksSinceLastXPGain = 0;
                ApplyRandomHediff();
            }
        }

        private void GainExperienceAndCheckLevels()
        {
            var target = pawn;
            var totalXP = master.GetPsylinkLevel() - target.GetPsylinkLevel();
            pawn.Psycasts().GainExperience(totalXP * 10f);
            if (target.GetPsylinkLevel() > master.GetPsylinkLevel())
                EndApprenticeship();
        }

        private void EndApprenticeship()
        {
            Log.Message("Apprenticeship ended: Apprentice surpassed master's psycast level.");
            pawn.health.RemoveHediff(this);
        }

        private void ApplyRandomHediff()
        {
            if (Rand.Chance(0.1f)) // Change chance as needed
                ApplySpecificHediff(master, pawn);
        }

        private void ApplySpecificHediff(Pawn target, Pawn master)
        {
            HediffDef hediffDef = ForceDefOf.ForceBond_MasterApprentice;
            Hediff hediff = HediffMaker.MakeHediff(hediffDef, target);
            target.health.AddHediff(hediff);
            master.health.AddHediff(hediff);
        }

        public override void Notify_PawnKilled()
        {
            base.Notify_PawnKilled();
            pawn.health.RemoveHediff(this);
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            if (master != null)
            {
                var hediff = master.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Master) as Hediff_Master;
                hediff?.apprentices.Remove(pawn);
            }

        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref master, "master");
        }
    }
}