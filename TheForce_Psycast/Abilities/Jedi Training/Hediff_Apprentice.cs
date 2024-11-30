using RimWorld;
using VanillaPsycastsExpanded;
using Verse;

namespace TheForce_Psycast.Abilities.Jedi_Training
{
    public class Hediff_Apprentice : HediffWithComps
    {
        public Pawn master;
        private int ticksSinceLastXPGain;
        private int xpGainInterval;
        private const int ApplyBondCooldown = 60000; // 1 in-game day
        private int ticksSinceLastBondAttempt;

        public override string Label => $"{base.Label} of: {master?.LabelShort ?? "Unknown Master"}";

        public Hediff_Apprentice()
        {
            xpGainInterval = Rand.Range(GenDate.TicksPerDay * 2, GenDate.TicksPerDay * 5);
        }

        public override void Tick()
        {
            base.Tick();
            ticksSinceLastXPGain += 1;
            ticksSinceLastBondAttempt += 1;

            if (ticksSinceLastXPGain >= xpGainInterval)
            {
                GainExperience();
                ticksSinceLastXPGain = 0;
            }

            if (ticksSinceLastBondAttempt >= ApplyBondCooldown)
            {
                TryApplyForceBond();
                ticksSinceLastBondAttempt = 0;
            }
        }

        private void GainExperience()
        {
            if (master == null || pawn == null) return;
            var masterHediff = master.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Master) as Hediff_Master;

            int levelDifference = master.GetPsylinkLevel() - pawn.GetPsylinkLevel();
            if (levelDifference > 0)
            {
                pawn.Psycasts().GainExperience(levelDifference * 10f);
            }

            if (pawn.GetPsylinkLevel() >= master.GetPsylinkLevel())
            {
                EndApprenticeship();
            }
        }

        private void TryApplyForceBond()
        {
            if (Rand.Chance(0.1f) && master != null && pawn != null)
            {
                if (!master.health.hediffSet.HasHediff(ForceDefOf.ForceBond_MasterApprentice))
                {
                    Hediff hediff = HediffMaker.MakeHediff(ForceDefOf.ForceBond_MasterApprentice, master);
                    master.health.AddHediff(hediff);
                    pawn.health.AddHediff(hediff);
                }
            }
        }

        private void EndApprenticeship()
        {
            var masterHediff = master.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Master) as Hediff_Master;

            Log.Message($"Apprenticeship ended: {pawn.LabelShort} has surpassed master {master?.LabelShort}'s level.");
            pawn?.health?.RemoveHediff(this);
            masterHediff.apprentices.Remove(pawn);
            masterHediff.graduatedApprenticesCount++;
            masterHediff.CheckAndPromoteMasterBackstory();
            pawn.relations.RemoveDirectRelation(ForceDefOf.Force_MasterRelation, master);
            master.relations.RemoveDirectRelation(ForceDefOf.Force_ApprenticeRelation, pawn);
            Find.WindowStack.Add(new Dialog_SelectBackstory(pawn));
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            if (master != null)
            {
                var masterHediff = master.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Master) as Hediff_Master;
                masterHediff?.apprentices.Remove(pawn);
            }
        }

        public override void Notify_KilledPawn(Pawn victim, DamageInfo? dinfo)
        {
            base.Notify_KilledPawn(victim, dinfo);

            // Check if the killed pawn is the master
            if (victim == master && pawn.GetStatValue(ForceDefOf.Force_Darkside_Attunement) > pawn.GetStatValue(ForceDefOf.Force_Lightside_Attunement))
            {
                EndApprenticeship();
                Find.WindowStack.Add(new Dialog_SelectBackstory(pawn));
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref master, "master");
            Scribe_Values.Look(ref xpGainInterval, "xpGainInterval", 180000);
            Scribe_Values.Look(ref ticksSinceLastXPGain, "ticksSinceLastXPGain", 0);
            Scribe_Values.Look(ref ticksSinceLastBondAttempt, "ticksSinceLastBondAttempt", 0);
        }
    }
}
