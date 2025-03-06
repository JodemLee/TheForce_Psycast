using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TheForce_Psycast.Lightsabers
{
    public class Hediff_LightsaberDeflection : HediffWithComps
    {
        // Constants
        private const int MinDeflectionTicks = 300;
        private const int MaxDeflectionTicks = 1200;
        private const float DefaultDeflectionDistance = 0.3f;
        private const float ScorchMarkChance = 0.15f;

        // Private fields
        private int _lastInterceptTicks = -999999;
        private float _lastInterceptAngle;
        private bool _drawInterceptCone;
        private Dictionary<Thing, Gizmo_LightsaberStance> _weaponStances = new Dictionary<Thing, Gizmo_LightsaberStance>();

        // Properties
        public float EntropyGain { get; set; }
        public float DeflectionMultiplier { get; set; }

        // Main deflection logic
        public virtual void DeflectProjectile(Projectile projectile)
        {
            if (projectile == null || pawn == null)
            {
                Log.Warning("Projectile or pawn is null in DeflectProjectile.");
                return;
            }

            try
            {
                TriggerDeflectionEffect(projectile);

                _lastInterceptAngle = projectile.ExactPosition.AngleToFlat(pawn.TrueCenter());
                _lastInterceptTicks = Find.TickManager.TicksGame;
                _drawInterceptCone = true;

                // Handle deflection for all equipped lightsabers
                foreach (var weapon in pawn.equipment.AllEquipmentListForReading)
                {
                    if (weapon.TryGetComp<Comp_LightsaberBlade>() is Comp_LightsaberBlade lightsaber)
                    {
                        HandleLightsaberDeflection(projectile, lightsaber);
                    }
                }

                RedirectProjectile(projectile);
                AddEntropy(projectile);
            }
            catch (Exception ex)
            {
                Log.Error($"Error in DeflectProjectile: {ex.Message}");
            }
        }

        // Trigger visual effect for deflection
        private void TriggerDeflectionEffect(Projectile projectile)
        {
            var effecter = new Effecter(EffecterDefOf.Interceptor_BlockedProjectile);
            try
            {
                effecter.Trigger(new TargetInfo(projectile.Position, projectile.Map, true), TargetInfo.Invalid);
            }
            finally
            {
                effecter.Cleanup();
            }
        }

        // Handle deflection logic for a specific lightsaber
        private void HandleLightsaberDeflection(Projectile projectile, Comp_LightsaberBlade lightsaber)
        {
            float projectileSpeed = projectile.def.projectile.speed;
            int meleeSkill = pawn.skills.GetSkill(SkillDefOf.Melee).Level;

            float angleDifference = Mathf.Abs(projectile.ExactPosition.AngleToFlat(pawn.TrueCenter()) - _lastInterceptAngle);
            lightsaber.AnimationDeflectionTicks = (int)Mathf.Clamp(
                CalculateDeflectionTicks(projectileSpeed, angleDifference, meleeSkill),
                MinDeflectionTicks,
                MaxDeflectionTicks);

            SetBladeScaleForDeflection(lightsaber);
            CreateDeflectionFleck(lightsaber, angleDifference);
        }

        // Calculate deflection ticks based on speed, angle, and skill
        private float CalculateDeflectionTicks(float speed, float angleDifference, int skillLevel)
        {
            float skillFactor = Mathf.Lerp(1.2f, 0.8f, skillLevel / 20f);
            return (5000f / (speed * (angleDifference + 1))) * skillFactor + Rand.Range(-100, 100);
        }

        // Adjust lightsaber blade scale during deflection
        private void SetBladeScaleForDeflection(Comp_LightsaberBlade lightsaber)
        {
            float bladeLength = lightsaber.MaxBladeLength;
            lightsaber.targetScaleForCore1AndBlade1 = new Vector3(bladeLength, 1f, bladeLength);
            lightsaber.targetScaleForCore2AndBlade2 = new Vector3(bladeLength, 1f, bladeLength);
        }

        // Create a visual fleck for deflection
        private void CreateDeflectionFleck(Comp_LightsaberBlade lightsaber, float angleDifference)
        {
            Vector3 deflectionDirection = Quaternion.Euler(0f, _lastInterceptAngle, 0f) * Vector3.forward;
            Vector3 deflectionLocation = pawn.TrueCenter() + deflectionDirection * DefaultDeflectionDistance;

            if (lightsaber.Props.Fleck is FleckDef fleckDef)
            {
                FleckCreationData fleckData = FleckMaker.GetDataStatic(deflectionLocation, pawn.Map, fleckDef);
                fleckData.rotation = _lastInterceptAngle;
                fleckData.scale = Rand.Range(0.8f, 1.2f);
                pawn.Map.flecks.CreateFleck(fleckData);
            }
        }

        // Redirect the projectile after deflection
        private void RedirectProjectile(Projectile projectile)
        {
            int meleeSkill = pawn.skills.GetSkill(SkillDefOf.Melee).Level;
            float basePrecisionFactor = Mathf.Lerp(50.0f, 0.1f, meleeSkill / 20f);
            float speedFactor = projectile.def.projectile.speed / 100f;
            float precisionFactor = basePrecisionFactor * speedFactor;

            Vector3 launcherPosition = projectile.Launcher.DrawPos;
            Vector3 projectileDirection = (projectile.ExactPosition - launcherPosition).normalized;
            float redirectionDistance = Mathf.Lerp(10f, 2f, meleeSkill / 20f);

            Vector3 randomOffset = new Vector3(
                Rand.Range(-precisionFactor, precisionFactor),
                0,
                Rand.Range(-precisionFactor, precisionFactor)
            );

            Vector3 targetPosition = launcherPosition + (projectileDirection * redirectionDistance) + randomOffset;

            if (!targetPosition.ToIntVec3().InBounds(pawn.Map))
            {
                targetPosition = launcherPosition + projectileDirection * redirectionDistance;
            }

            if (meleeSkill >= 20)
            {
                targetPosition = launcherPosition;
            }

            projectile.Launch(
                pawn,
                targetPosition.ToIntVec3(),
                projectile.Launcher,
                ProjectileHitFlags.All,
                false,
                projectile
            );

            float xpGained = CalculateMeleeXP(projectile, pawn, redirectionDistance);
            pawn.skills.Learn(SkillDefOf.Melee, xpGained);
        }

        // Calculate XP gained from deflection
        private float CalculateMeleeXP(Projectile projectile, Pawn pawn, float redirectionDistance)
        {
            float baseXP = 100f;
            float skillMultiplier = Mathf.Lerp(1.5f, 0.5f, pawn.skills.GetSkill(SkillDefOf.Melee).Level / 20f);
            return baseXP * skillMultiplier;
        }

        // Add entropy after deflection
        public void AddEntropy(Projectile projectile)
        {
            float entropyGain = CalculateEntropyGain(projectile);
            if (CanGainEntropy(entropyGain))
            {
                pawn.psychicEntropy.TryAddEntropy(entropyGain, overLimit: false);
            }
        }

        // Check if entropy can be added without exceeding the limit
        private bool CanGainEntropy(float entropyGain)
        {
            return pawn.psychicEntropy.EntropyValue + entropyGain < pawn.psychicEntropy.MaxEntropy * 0.99f;
        }

        // Calculate entropy gain based on projectile speed
        public float CalculateEntropyGain(Projectile projectile)
        {
            return projectile.def.projectile.speed / Force_ModSettings.entropyGain;
        }

        // Create scorch marks after deflection
        public void LightsaberScorchMark(Pawn pawn)
        {
            if (Rand.Value >= ScorchMarkChance) return;

            if (ForceDefOf.Force_LightsaberScorch_Fleck is FleckDef fleckDef)
            {
                var validCells = GenAdj.CellsAdjacent8Way(pawn).Where(cell => cell.InBounds(pawn.Map) && cell.Walkable(pawn.Map));

                if (validCells.TryRandomElement(out var scorchCell))
                {
                    FleckCreationData fleckData = FleckMaker.GetDataStatic(scorchCell.ToVector3(), pawn.Map, fleckDef);
                    fleckData.rotation = Rand.Range(0f, 360f);
                    pawn.Map.flecks.CreateFleck(fleckData);
                }
            }
        }

        // Check if a projectile should be deflected
        public virtual bool ShouldDeflectProjectile(Projectile projectile)
        {
            if (!IsProjectileDeflectable(projectile)) return false;
            if (!CanGainEntropy(CalculateEntropyGain(projectile))) return false;

            float deflectionSkillChance = pawn.GetStatValue(ForceDefOf.Force_Lightsaber_Deflection);
            return CalculateDeflectionChance(deflectionSkillChance);
        }

        // Check if a projectile is deflectable
        private bool IsProjectileDeflectable(Projectile projectile)
        {
            if (!Force_ModSettings.deflectableProjectileHashes.Contains(projectile.def.shortHash) && Force_ModSettings.projectileDeflectionSelector)
            {
                if (Prefs.DevMode)
                {
                    Log.Message($"Projectile {projectile.def.label} (shortHash: {projectile.def.shortHash}) is not deflectable.");
                }
                return false;
            }

            return true;
        }

        // Calculate deflection chance based on skill and multiplier
        private bool CalculateDeflectionChance(float deflectionSkillChance)
        {
            float deflection = deflectionSkillChance * Force_ModSettings.deflectionMultiplier;
            return deflection >= Rand.Range(0f, 1f);
        }

        // Provide Gizmos for lightsaber stances
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var weapon in pawn.equipment.AllEquipmentListForReading)
            {
                if (weapon.TryGetComp<Comp_LightsaberBlade>() != null)
                {
                    if (!_weaponStances.TryGetValue(weapon, out var stance))
                    {
                        stance = new Gizmo_LightsaberStance(pawn, this, weapon);
                        _weaponStances[weapon] = stance;
                    }
                    yield return stance;
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _lastInterceptTicks, "lastInterceptTicks");
            Scribe_Values.Look(ref _lastInterceptAngle, "lastInterceptAngle");
            Scribe_Values.Look(ref _drawInterceptCone, "drawInterceptCone");

            List<Thing> weapons = _weaponStances.Keys.ToList();
            List<List<StanceData>> stanceData = _weaponStances.Values.Select(gizmo => gizmo.stanceDataList).ToList();

            Scribe_Collections.Look(ref weapons, "weapons", LookMode.Reference);
            Scribe_Collections.Look(ref stanceData, "stanceData", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (weapons != null && stanceData != null && weapons.Count == stanceData.Count)
                {
                    _weaponStances = new Dictionary<Thing, Gizmo_LightsaberStance>();
                    for (int i = 0; i < weapons.Count; i++)
                    {
                        var gizmo = new Gizmo_LightsaberStance(pawn, this, weapons[i]);
                        gizmo.stanceDataList = stanceData[i];
                        _weaponStances[weapons[i]] = gizmo;
                    }
                }
            }
        }
    }
}