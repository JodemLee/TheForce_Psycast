using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TheForce_Psycast.Lightsabers.Lightsaber_Combat
{
    internal static class LightsaberCombatUtility
    {
        private static HediffDef ParryHediffDefName = ForceDefOf.Lightsaber_Stance;

        public static bool CanParry(Pawn targetPawn, Pawn attacker)
        {
            if (!Force_ModSettings.lightsaberParryEnabled)
            {
                return false;
            }

            if (targetPawn == null || attacker == null ||
                targetPawn.skills == null || attacker.skills == null ||
                targetPawn.skills.GetSkill(SkillDefOf.Melee) == null ||
                attacker.skills.GetSkill(SkillDefOf.Melee) == null)
            {
                return false;
            }

            if (targetPawn.health.hediffSet.HasHediff(ParryHediffDefName))
            {
                Hediff targetStance = targetPawn.health.hediffSet.GetFirstHediffOfDef(ParryHediffDefName);
                Hediff attackerStance = attacker.health.hediffSet.GetFirstHediffOfDef(ParryHediffDefName);

                if (targetStance != null && attackerStance != null)
                {
                    float targetSeverity = targetStance.Severity;
                    float attackerSeverity = attackerStance.Severity;

                    DefStanceAngles stanceDef = targetPawn.equipment.Primary?.def.GetModExtension<DefStanceAngles>()
                                                ?? targetStance.def.GetModExtension<DefStanceAngles>();

                    if (stanceDef != null)
                    {
                        StanceData targetStanceData = stanceDef.GetStanceDataForSeverity(targetSeverity);
                        StanceData attackerStanceData = stanceDef.GetStanceDataForSeverity(attackerSeverity);

                        if (targetStanceData != null && attackerStanceData != null)
                        {
                            float parryBonus = CalculateParryBonus(targetPawn, attacker);
                            float adjustedTargetMeleeSkill = GetAdjustedMeleeSkill(targetPawn);
                            float adjustedAttackerMeleeSkill = GetAdjustedMeleeSkill(attacker);
                            int skillDifference = (int)(adjustedTargetMeleeSkill - adjustedAttackerMeleeSkill);
                            float parryChance = Math.Min(0.95f, Math.Max(0.05f, (0.05f * skillDifference) + parryBonus));

                            if (Rand.Value <= parryChance)
                            {
                                string stanceDescription = targetStanceData.StanceID;
                                string parryChanceMessage = $"Parry Chance: {Math.Round(parryChance * 100, 2)}%";
                                string fullMessage = stanceDescription + " " + parryChanceMessage;
                                MoteMaker.ThrowText(new Vector3((float)targetPawn.Position.x + 1f, targetPawn.Position.y, (float)targetPawn.Position.z + 1f), targetPawn.Map, fullMessage, Color.white);
                                return true;
                            }
                        }
                    }
                }
            }
            else
            {
                float adjustedTargetMeleeSkill = GetAdjustedMeleeSkill(targetPawn);
                float adjustedAttackerMeleeSkill = GetAdjustedMeleeSkill(attacker);
                int skillDifference = (int)(adjustedTargetMeleeSkill - adjustedAttackerMeleeSkill);
                float parryChance = Math.Min(0.95f, Math.Max(0.05f, 0.05f * skillDifference));

                if (Rand.Value <= parryChance)
                {
                    string parryChanceMessage = $"Parry Chance: {Math.Round(parryChance * 100, 2)}%";
                    MoteMaker.ThrowText(new Vector3((float)targetPawn.Position.x + 1f, targetPawn.Position.y, (float)targetPawn.Position.z + 1f), targetPawn.Map, parryChanceMessage, Color.white);
                    return true;
                }
            }

            return false;
        }




        private static float GetAdjustedMeleeSkill(Pawn pawn)
        {
            float manipulation = pawn.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation);
            int meleeSkill = pawn.skills.GetSkill(SkillDefOf.Melee).Level;
            return meleeSkill * manipulation * 600;
        }

        private static float CalculateParryBonus(Pawn targetPawn, Pawn attackerPawn)
        {
            if (targetPawn == null || attackerPawn == null)
                return 0f;

            Hediff targetStanceHediff = targetPawn.health.hediffSet.GetFirstHediffOfDef(ParryHediffDefName);
            Hediff attackerStanceHediff = attackerPawn.health.hediffSet.GetFirstHediffOfDef(ParryHediffDefName);

            DefStanceAngles targetStanceMod = targetPawn.equipment.Primary?.def.GetModExtension<DefStanceAngles>()
                                              ?? targetStanceHediff.def.GetModExtension<DefStanceAngles>();

            DefStanceAngles attackerStanceMod = attackerPawn.equipment.Primary?.def.GetModExtension<DefStanceAngles>()
                                               ?? attackerStanceHediff.def.GetModExtension<DefStanceAngles>();

            if (targetStanceMod == null || attackerStanceMod == null)
                return 0f;

            StanceData targetStanceData = targetStanceMod.GetStanceDataForSeverity(targetStanceHediff.Severity);
            StanceData attackerStanceData = attackerStanceMod.GetStanceDataForSeverity(attackerStanceHediff.Severity);

            if (targetStanceData == null || attackerStanceData == null)
                return 0f;

            float parryBonus = 0f;

            if (targetStanceData.WeakAgainst?.Contains(attackerStanceData.StanceID) == true)
                parryBonus -= 0.2f;

            return parryBonus;
        }

        public static void TriggerWeaponRotationOnParry(Pawn caster, Pawn target)
        {
            var casterBlade = caster.equipment?.Primary?.TryGetComp<Comp_LightsaberBlade>();
            var targetBlade = target.equipment?.Primary?.TryGetComp<Comp_LightsaberBlade>();

            if (casterBlade != null && targetBlade != null)
            {
                float casterSkill = caster.skills.GetSkill(SkillDefOf.Melee).Level;
                float targetSkill = target.skills.GetSkill(SkillDefOf.Melee).Level;
                float angleToTarget = (caster.Position.ToVector3() - target.Position.ToVector3()).AngleFlat();
                float angleDifference = Mathf.DeltaAngle(angleToTarget, casterBlade.CurrentRotation);

                float baseAnimationTicks = Mathf.Clamp(
                    500f / (angleDifference + 1),
                    300,
                    1200
                );

                float casterSkillAdjustment = Mathf.Lerp(1.0f, 0.8f, casterSkill / 20);
                float targetSkillAdjustment = Mathf.Lerp(1.0f, 0.8f, targetSkill / 20);
                float randomVariation = UnityEngine.Random.Range(-0.1f, 0.1f);
                float finalCasterAnimationTicks = baseAnimationTicks * casterSkillAdjustment * (1 + randomVariation);
                float finalTargetAnimationTicks = baseAnimationTicks * targetSkillAdjustment * (1 + randomVariation);

                casterBlade.AnimationDeflectionTicks += (int)finalCasterAnimationTicks;
                targetBlade.AnimationDeflectionTicks += (int)(finalTargetAnimationTicks + 100);
            }
        }


        internal static void DestroyLimb(Pawn CasterPawn, Pawn target, BodyPartRecord limb)
        {
            // Get the weapon equipped by the pawn.
            ThingWithComps weapon = CasterPawn.equipment?.Primary;
            Tool selectedTool = SelectWeightedTool(weapon.def.tools);
            int damageAmount = CalculateDamageToDestroyLimb(target, limb);
            ToolCapacityDef mainCapacity = selectedTool.capacities.FirstOrDefault();
            DamageDef damageType = mainCapacity?.Maneuvers.FirstOrDefault().verb.meleeDamageDef ?? DamageDefOf.Cut;
            var damageInfo = new DamageInfo(damageType, damageAmount, selectedTool.armorPenetration, -1, CasterPawn, limb, weapon.def);
            target.TakeDamage(damageInfo);
        }

        internal static int CalculateDamageToDestroyLimb(Pawn target, BodyPartRecord limb)
        {
            float partHealth = target.health.hediffSet.GetPartHealth(limb);
            return (int)Math.Ceiling(partHealth) * 2;
        }


        internal static Tool SelectWeightedTool(List<Tool> tools)
        {
            float totalWeight = tools.Sum(tool => tool.chanceFactor);
            float randomPoint = Rand.Value * totalWeight;

            float cumulativeWeight = 0f;
            foreach (var tool in tools)
            {
                cumulativeWeight += tool.chanceFactor;
                if (randomPoint <= cumulativeWeight)
                {
                    return tool;
                }
            }
            return tools.Last(); // Fallback in case of rounding errors
        }


    }
}

