using System;
using System.Collections.Generic;
using TheForce_Psycast.Lightsabers;
using UnityEngine;
using VanillaPsycastsExpanded;
using Verse;
using VFECore.Abilities;
using AbilityDef = VFECore.Abilities.AbilityDef;

namespace TheForce_Psycast
{
    public class Comp_LightsaberStance : ThingComp
    {
        public CompProperties_LightsaberStance Props => (CompProperties_LightsaberStance)props;

        private AbilityDef ability;
        private bool alreadyHad;

        // Store the last hediff severity
        private float lastSeverity = 0f;

        // Constants for clarity
        private const float MinSeverity = 1f;
        private const float MaxSeverity = 7f;

        public AbilityDef Ability => ability;
        public bool Added => !alreadyHad;

        public List<float> savedStanceAngles = new List<float>();
        public List<Vector3> savedDrawOffsets = new List<Vector3>();
        private DefStanceAngles extension;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref alreadyHad, "alreadyHad", false);
            Scribe_Values.Look(ref lastSeverity, "lastSeverity", 0f);
            Scribe_Collections.Look(ref savedStanceAngles, "savedStanceAngles", LookMode.Value);
            Scribe_Collections.Look(ref savedDrawOffsets, "savedDrawOffsets", LookMode.Value);
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);

            var ability = ForceDefOf.Force_ThrowWeapon;
            var hediffDef = ForceDefOf.Lightsaber_Stance;

            if (pawn == null || ability == null)
                return;

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
            hediff.Severity = lastSeverity != 0f ? lastSeverity : Rand.Range(MinSeverity, MaxSeverity);
            pawn.health.AddHediff(hediff);

            // Apply stance rotation after adding the hediff
            ApplyStanceRotation(pawn);
        }

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
                lastSeverity = hediff.Severity; // Store the current severity
                pawn.health.RemoveHediff(hediff);
            }

            if (pawn.equipment.Primary?.GetComp<Comp_LightsaberStance>() is Comp_LightsaberStance compStance)
            {
                compStance.savedStanceAngles = new List<float>(savedStanceAngles);
                compStance.savedDrawOffsets = new List<Vector3>(savedDrawOffsets);
            }

            alreadyHad = false;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            // Ensure we apply stance after the pawn and equipment are initialized
            if (parent is Pawn pawn && pawn.equipment?.Primary != null)
            {
                ApplyStanceRotation(pawn);
            }
        }

        private void ApplyStanceRotation(Pawn pawn)
        {
            var lightsaberComp = pawn.equipment.Primary?.GetComp<Comp_LightsaberBlade>();
            if (lightsaberComp != null)
            {
                var (angle, offset) = GetStanceAngleAndOffset(pawn);

                lightsaberComp.UpdateRotationForStance(angle);
                lightsaberComp.UpdateDrawOffsetForStance(offset);
            }
        }

        private (float angle, Vector3 offset) GetStanceAngleAndOffset(Pawn pawn)
        {
            var hediffDef = ForceDefOf.Lightsaber_Stance; // Ensure this is the correct definition
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);

            if (hediff == null)
                return (0f, Vector3.zero);

            // Get the DefStanceAngles extension from the ThingDef
            var thingDef = pawn.equipment.Primary?.def; // Ensure the ThingDef is properly retrieved
            extension = thingDef.GetModExtension<DefStanceAngles>() ?? hediff.def.GetModExtension<DefStanceAngles>();


            // Get the StanceData for the current severity
            StanceData stanceData = extension?.GetStanceDataForSeverity(hediff.Severity);

            // Return both the angle and offset from the StanceData, defaulting to 0f and Vector3.zero if not found
            return (stanceData?.Angle ?? 0f, stanceData?.Offset ?? Vector3.zero);
        }

        public void ResetToDefault(List<float> defaultStanceAngles, List<Vector3> defaultDrawOffsets)
        {
            savedStanceAngles = new List<float>(defaultStanceAngles);
            savedDrawOffsets = new List<Vector3>(defaultDrawOffsets);
        }
    }

    public class CompProperties_LightsaberStance : CompProperties
    {
        public CompProperties_LightsaberStance()
        {
            this.compClass = typeof(Comp_LightsaberStance);
        }
    }
}