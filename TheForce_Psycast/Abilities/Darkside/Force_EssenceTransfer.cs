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
    internal class Force_EssenceTransfer : Ability_WriteCombatLog
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
            // Check if the pawn has any hediff with the Ghost comp
            var ghostHediff = ghostPawn.health.hediffSet.hediffs
                .FirstOrDefault(h => h.TryGetComp<HediffComp_Ghost>() != null);

            if (ghostHediff != null)
            {
                var ghostComp = ghostHediff.TryGetComp<HediffComp_Ghost>();
                if (!ghostComp.Props.isLightSide)
                {
                    ghostComp.LinkedObject = null;
                    ghostPawn.health.RemoveHediff(ghostHediff);
                    ghostPawn.apparel.DropAll(ghostPawn.Position);
                    ghostPawn.Destroy(DestroyMode.Vanish);
                }
                else
                {
                    ghostPawn.health.RemoveHediff(ghostHediff);
                    ghostPawn.apparel.DropAll(ghostPawn.Position);
                    ghostPawn.Destroy(DestroyMode.Vanish);
                }
            }

            // Handle Sith Zombie logic
            var zombieHediff = ghostPawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Sithzombie);
            if (zombieHediff != null)
            {
                ghostPawn.health.RemoveHediff(zombieHediff);
                return;
            }

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
            var targetTraits = new List<Trait>(targetPawn.story.traits.allTraits);
            var casterTraits = new List<Trait>(casterPawn.story.traits.allTraits);

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

            foreach (var skill in targetPawn.skills.skills)
            {
                var casterSkill = casterPawn.skills.GetSkill(skill.def);
                casterSkill.Level = skill.Level;
                casterSkill.passion = skill.passion;
                casterSkill.xpSinceLastLevel = skill.xpSinceLastLevel;
                casterSkill.xpSinceMidnight = skill.xpSinceMidnight;

            }

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
            int originalCasterPoints;
            int originalTargetPoints;

            casterPawn.ResetPsyPoints(out originalCasterPoints);

            targetPawn.ResetPsyPoints(out originalTargetPoints);

            var sourcePsylink = casterPawn.GetMainPsylinkSource();
            var sourcePsycasts = casterPawn.Psycasts();

            if (sourcePsylink != null)
            {
                casterPawn.health.RemoveHediff(sourcePsylink);
            }
            if (sourcePsycasts != null)
            {
                casterPawn.health.RemoveHediff(sourcePsycasts);
            }

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

            targetPawn.RestorePsyPoints(originalCasterPoints);
            casterPawn.RestorePsyPoints(originalTargetPoints);
        }
    }

    public static class PsycastUtilityExtensions
    {
        public static void AutoUnlockPsycasterPaths(this Pawn pawn)
        {
            var psycasts = pawn.Psycasts();
            if (psycasts != null)
            {
                psycasts.unlockedPaths.Clear();
                var compAbilities = pawn.GetComp<CompAbilities>();
                if (compAbilities != null)
                {
                    foreach (var ability in compAbilities.LearnedAbilities)
                    {
                        var psycastExtension = ability.def.GetModExtension<AbilityExtension_Psycast>();
                        if (psycastExtension != null && psycastExtension.path != null)
                        {
                            if (!psycasts.unlockedPaths.Contains(psycastExtension.path))
                            {
                                psycasts.unlockedPaths.Add(psycastExtension.path);
                            }
                        }
                    }
                }
                PsycastUtility.RecheckPaths(pawn);
            }
        }

        public static void ResetPsyPoints(this Pawn pawn, out int originalPoints)
        {
            var psycasts = pawn.Psycasts();
            if (psycasts != null)
            {
                originalPoints = psycasts.points;
                psycasts.points = 0;
                PsycastUtility.RecheckPaths(pawn);
            }
            else
            {
                originalPoints = 0;
            }
        }

        public static void RestorePsyPoints(this Pawn pawn, int originalPoints)
        {
            var psycasts = pawn.Psycasts();
            if (psycasts != null)
            {
                psycasts.points = originalPoints;
                PsycastUtility.RecheckPaths(pawn);
            }
        }
    }
}

