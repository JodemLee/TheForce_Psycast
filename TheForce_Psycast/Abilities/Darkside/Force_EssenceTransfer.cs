using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheForce_Psycast.Hediffs;
using VanillaPsycastsExpanded;
using Verse;
using VFECore.Abilities;
using PsycastUtility = VanillaPsycastsExpanded.PsycastUtility;

namespace TheForce_Psycast.Abilities.Darkside
{
    internal class Force_EssenceTransfer : VFECore.Abilities.Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);

            // Get the target pawn
            Pawn targetPawn = targets[0].Thing as Pawn;
            if (targetPawn == null || targetPawn.Dead || !targetPawn.health.hediffSet.HasHediff(ForceDefOf.Force_MindShardHediff))
            {
                return;
            }
            TransferEssence(targetPawn, this.pawn);
            Hediff hediff = targetPawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_MindShardHediff);
            if (hediff != null)
            {
                // Remove the hediff
                targetPawn.health.RemoveHediff(hediff);
            }
            this.pawn.psychicEntropy.RemoveAllEntropy();
        }

        public static void TransferEssence(Pawn targetPawn, Pawn casterPawn)
        {
            bool isCasterGhost = IsGhost(casterPawn);
            bool isTargetGhost = IsGhost(targetPawn);

            if (isTargetGhost)
            {
                Find.LetterStack.ReceiveLetter(
                    "Essence Transfer Failed".Translate(),
                    "You cannot swap bodies with a ghost.".Translate(),
                    LetterDefOf.NegativeEvent, new List<Pawn> { casterPawn });
                return;
            }

            if (isCasterGhost)
            {
                HandleGhost(casterPawn);
            }

            SwapNames(targetPawn, casterPawn);
            SwapBackstories(targetPawn, casterPawn);
            SwapTraits(targetPawn, casterPawn);
            SwapSkills(targetPawn, casterPawn);
            SwapIdeo(targetPawn, casterPawn);
            SwapFaction(targetPawn, casterPawn);
            SwapAbilities(targetPawn, casterPawn);
            TransferPsylinkAndPsycasts(targetPawn, casterPawn);


            PawnComponentsUtility.AddAndRemoveDynamicComponents(targetPawn);
            PawnComponentsUtility.AddAndRemoveDynamicComponents(casterPawn);

            Find.LetterStack.ReceiveLetter(
                "Essence Transfer Complete".Translate(),
                "You have successfully swapped bodies.".Translate(casterPawn.Named("Caster"), targetPawn.Named("Target")),
                LetterDefOf.NeutralEvent, new List<Pawn> { casterPawn, targetPawn });
        }

        private static bool IsGhost(Pawn pawn)
        {
            return ForceGhostUtility.IsForceGhost(pawn);
        }

        private static void HandleGhost(Pawn ghostPawn)
        {
            var ghostHediff = ghostPawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Darkside) as HediffWithComps_DarksideGhost;
            if (ghostHediff != null)
            {
                ghostHediff.linkedObject = null;
                var SithGhostHediff = ghostPawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_SithGhost);
                ghostPawn.health.RemoveHediff(SithGhostHediff);
                ghostPawn.apparel.DropAll(ghostPawn.Position);
            }
            ghostPawn.Destroy(DestroyMode.Vanish);
        }

        private static void SwapNames(Pawn targetPawn, Pawn casterPawn)
        {
            string tempName = targetPawn.Name.ToStringFull;
            targetPawn.Name = casterPawn.Name;
            casterPawn.Name = new NameSingle(tempName);
        }

        private static void SwapBackstories(Pawn targetPawn, Pawn casterPawn)
        {
            var tempChildhood = targetPawn.story.Childhood;
            var tempAdulthood = targetPawn.story.Adulthood;
            targetPawn.story.Childhood = casterPawn.story.Childhood;
            targetPawn.story.Adulthood = casterPawn.story.Adulthood;
            casterPawn.story.Childhood = tempChildhood;
            casterPawn.story.Adulthood = tempAdulthood;
        }

        private static void SwapFaction(Pawn targetPawn, Pawn casterPawn)
        {
            if (targetPawn.Faction != casterPawn.Faction && casterPawn.Faction != null)
            {
                targetPawn.SetFaction(casterPawn.Faction, casterPawn);
            }
        }

        private static void SwapTraits(Pawn targetPawn, Pawn casterPawn)
        {
            var targetTraits = targetPawn.story.traits.allTraits.ToList();
            var casterTraits = casterPawn.story.traits.allTraits.ToList();

            // Clear existing traits and re-add swapped traits
            targetPawn.story.traits.allTraits.Clear();
            casterPawn.story.traits.allTraits.Clear();

            foreach (var trait in targetTraits)
            {
                if (trait.sourceGene is null)
                {
                    casterPawn.story.traits.GainTrait(new Trait(trait.def, trait.Degree));
                }
            }

            foreach (var trait in casterTraits)
            {
                if (trait.sourceGene is null)
                {
                    targetPawn.story.traits.GainTrait(new Trait(trait.def, trait.Degree));
                }
            }
        }

        private static void SwapSkills(Pawn targetPawn, Pawn casterPawn)
        {
            var tempSkills = new List<(SkillDef def, int level, Passion passion, float xpSinceLastLevel, float xpSinceMidnight)>();

            // Save the caster's skills temporarily
            foreach (var skill in casterPawn.skills.skills)
            {
                tempSkills.Add((
                    skill.def,
                    skill.Level,
                    skill.passion,
                    skill.xpSinceLastLevel,
                    skill.xpSinceMidnight));
            }

            // Transfer target's skills to caster
            foreach (var skill in targetPawn.skills.skills)
            {
                var casterSkill = casterPawn.skills.GetSkill(skill.def);
                casterSkill.Level = skill.Level;
                casterSkill.passion = skill.passion;
                casterSkill.xpSinceLastLevel = skill.xpSinceLastLevel;
                casterSkill.xpSinceMidnight = skill.xpSinceMidnight;

            }

            // Transfer caster's stored skills to target
            foreach (var tempSkill in tempSkills)
            {
                var targetSkill = targetPawn.skills.GetSkill(tempSkill.def);
                targetSkill.Level = tempSkill.level;
                targetSkill.passion = tempSkill.passion;
                targetSkill.xpSinceLastLevel = tempSkill.xpSinceLastLevel;
                targetSkill.xpSinceMidnight = tempSkill.xpSinceMidnight;

            }
        }



        private static void SwapIdeo(Pawn targetPawn, Pawn casterPawn)
        {
            if (targetPawn.ideo.Ideo != null && casterPawn.Ideo != targetPawn.ideo.Ideo)
            {
                targetPawn.ideo.SetIdeo(casterPawn.Ideo);
            }
        }

        private static void SwapAbilities(Pawn targetPawn, Pawn casterPawn)
        {
            var casterCompAbilities = casterPawn.GetComp<CompAbilities>();
            var targetCompAbilities = targetPawn.GetComp<CompAbilities>();

            foreach (var ability in casterCompAbilities.LearnedAbilities.ToList())
            {
                if (ability.def.GetModExtension<AbilityExtension_Psycast>() != null)
                {
                    casterCompAbilities.LearnedAbilities.Remove(ability);
                    targetCompAbilities.GiveAbility(ability.def);
                }
            }
        }

        private static void TransferPsylinkAndPsycasts(Pawn targetPawn, Pawn casterPawn)
        {
            var sourcePsylink = casterPawn.GetMainPsylinkSource();
            var sourcePsycasts = casterPawn.Psycasts();

            // Remove psylink and psycasts from caster
            if (sourcePsylink != null)
            {
                casterPawn.health.RemoveHediff(sourcePsylink);
            }
            if (sourcePsycasts != null)
            {
                casterPawn.health.RemoveHediff(sourcePsycasts);
            }


            // Transfer psylink and psycasts from caster to target
            if (sourcePsylink != null)
            {
                sourcePsylink.pawn = targetPawn;
                targetPawn.health.AddHediff(sourcePsylink);
                PsycastUtility.RecheckPaths(targetPawn);
            }

            if (sourcePsycasts != null)
            {
                sourcePsycasts.pawn = targetPawn;
                targetPawn.health.AddHediff(sourcePsycasts);
                PsycastUtilityExtensions.AutoUnlockPsycasterPaths(targetPawn);
            }
        }
    }

    public static class PsycastUtilityExtensions
    {
        public static void AutoUnlockPsycasterPaths(this Pawn pawn)
        {
            var psycasts = pawn.Psycasts();
            if (psycasts != null)
            {
                foreach (var path in psycasts.previousUnlockedPaths.ToList())
                {
                    psycasts.previousUnlockedPaths.Remove(path);
                    psycasts.unlockedPaths.Add(path);
                }

                foreach (var path in DefDatabase<PsycasterPathDef>.AllDefs)
                {
                    if (!psycasts.unlockedPaths.Contains(path))
                    {
                        psycasts.unlockedPaths.Add(path);
                    }
                }
            }
        }
    }
}
