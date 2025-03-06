using System.Collections.Generic;
using System.Linq;
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

        private Dictionary<AbilityDef, bool> alreadyHadAbilities = new Dictionary<AbilityDef, bool>();
        private Dictionary<HediffDef, float> lastSeverities = new Dictionary<HediffDef, float>();

        public List<float> savedStanceAngles = new List<float>();
        public List<Vector3> savedDrawOffsets = new List<Vector3>();
        private DefStanceAngles extension;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref alreadyHadAbilities, "alreadyHadAbilities", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref lastSeverities, "lastSeverities", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref savedStanceAngles, "savedStanceAngles", LookMode.Value);
            Scribe_Collections.Look(ref savedDrawOffsets, "savedDrawOffsets", LookMode.Value);
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            if (pawn == null) return;

            var compAbilities = pawn.GetComp<CompAbilities>();
            if (compAbilities == null) return;


            bool hasPsylink = pawn.Psycasts()?.level > 0;

            // Assign abilities
            AssignAbilities(pawn, compAbilities, hasPsylink);

            // Assign hediffs
            AssignHediffs(pawn, hasPsylink);

            // Apply Stance Rotation
            ApplyStanceRotation(pawn);
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            if (pawn == null) return;

            var compAbilities = pawn.GetComp<CompAbilities>();
            if (compAbilities != null)
            {
                // Remove abilities only if they were added by this comp
                foreach (var ability in Props.abilitiesRequiringPsylink.Concat(Props.abilitiesNotRequiringPsylink))
                {
                    if (alreadyHadAbilities.TryGetValue(ability, out bool had) && !had)
                    {
                        compAbilities.LearnedAbilities.RemoveAll(ab => ab.def == ability);
                    }
                }
            }

            // Remove hediffs associated with this lightsaber
            RemoveHediffs(pawn);

            // Preserve stance settings if another stance-enabled weapon is equipped
            if (pawn.equipment.Primary?.GetComp<Comp_LightsaberStance>() is Comp_LightsaberStance compStance)
            {
                compStance.savedStanceAngles = new List<float>(savedStanceAngles);
                compStance.savedDrawOffsets = new List<Vector3>(savedDrawOffsets);
            }

            alreadyHadAbilities.Clear();
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (parent is Pawn pawn && pawn.equipment?.Primary != null)
            {
                ApplyStanceRotation(pawn);
            }
        }

        private void AssignAbilities(Pawn pawn, CompAbilities compAbilities, bool hasPsylink)
        {
            // Ensure the properties are not null to prevent NullReferenceException
            if (Props?.abilitiesRequiringPsylink == null || Props.abilitiesNotRequiringPsylink == null)
            {
                Log.Error($"[Comp_LightsaberStance] abilities list is null for {pawn?.Name}");
                return;
            }

            if (compAbilities == null)
            {
                Log.Warning($"[Comp_LightsaberStance] {pawn?.Name} does not have CompAbilities.");
                return;
            }

            foreach (var ability in Props.abilitiesRequiringPsylink)
            {
                if (ability == null) continue;
                if (hasPsylink && !compAbilities.HasAbility(ability))
                {
                    alreadyHadAbilities[ability] = false;
                    compAbilities.GiveAbility(ability);
                }
                else
                {
                    alreadyHadAbilities[ability] = true;
                }
            }

            foreach (var ability in Props.abilitiesNotRequiringPsylink)
            {
                if (ability == null) continue;
                if (!compAbilities.HasAbility(ability))
                {
                    alreadyHadAbilities[ability] = false;
                    compAbilities.GiveAbility(ability);
                }
                else
                {
                    alreadyHadAbilities[ability] = true;
                }
            }
        }


        private void AssignHediffs(Pawn pawn, bool hasPsylink)
        {
            foreach (var hediffDef in hasPsylink ? Props.hediffsRequiringPsylink : Props.hediffsNotRequiringPsylink)
            {
                if (hediffDef == null) continue;

                var uniqueHediff = HediffMaker.MakeHediff(hediffDef, pawn);
                uniqueHediff.Severity = lastSeverities.ContainsKey(hediffDef)
                    ? lastSeverities[hediffDef]
                    : Rand.Range(1f, 7f);

                pawn.health.AddHediff(uniqueHediff);
            }
        }

        private void RemoveHediffs(Pawn pawn)
        {
            foreach (var hediffDef in Props.hediffsRequiringPsylink.Concat(Props.hediffsNotRequiringPsylink))
            {
                var hediffs = pawn.health.hediffSet.hediffs.Where(h => h.def == hediffDef).ToList();
                foreach (var hediff in hediffs)
                {
                    lastSeverities[hediff.def] = hediff.Severity;
                    pawn.health.RemoveHediff(hediff);
                }
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
            var hediffs = Props.hediffsRequiringPsylink.Concat(Props.hediffsNotRequiringPsylink)
                .Select(hediffDef => pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef))
                .Where(h => h != null)
                .ToList();

            if (!hediffs.Any()) return (0f, Vector3.zero);

            var thingDef = pawn.equipment.Primary?.def;
            extension = thingDef?.GetModExtension<DefStanceAngles>() ?? hediffs.First().def.GetModExtension<DefStanceAngles>();

            var maxSeverityHediff = hediffs.OrderByDescending(h => h.Severity).First();
            StanceData stanceData = extension?.GetStanceDataForSeverity(maxSeverityHediff.Severity);

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
        public List<AbilityDef> abilitiesRequiringPsylink;
        public List<AbilityDef> abilitiesNotRequiringPsylink;
        public List<HediffDef> hediffsRequiringPsylink;
        public List<HediffDef> hediffsNotRequiringPsylink;

        public CompProperties_LightsaberStance()
        {
            this.compClass = typeof(Comp_LightsaberStance);
            this.hediffsRequiringPsylink = new List<HediffDef>();
            this.hediffsNotRequiringPsylink = new List<HediffDef>();
            this.abilitiesRequiringPsylink = new List<AbilityDef>();
            this.abilitiesNotRequiringPsylink = new List<AbilityDef>();
        }
    }
}