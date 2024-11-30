using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TheForce_Psycast.Lightsabers
{
    [StaticConstructorOnStartup]
    public class Hediff_LightsaberDeflection : HediffWithComps
    {

        private int lastInterceptTicks = -999999;
        private float lastInterceptAngle;
        private bool drawInterceptCone;
        public float entropyGain { get; set; }
        public float deflectionMultiplier { get; set; }

        public virtual void DeflectProjectile(Projectile projectile)
        {
            Effecter effecter = new Effecter(EffecterDefOf.Interceptor_BlockedProjectile);
            effecter.Trigger(new TargetInfo(projectile.Position, pawn.Map), TargetInfo.Invalid);
            effecter.Cleanup();
            lastInterceptAngle = projectile.ExactPosition.AngleToFlat(pawn.TrueCenter());
            lastInterceptTicks = Find.TickManager.TicksGame;
            drawInterceptCone = true;


            var lightsaber = pawn.equipment.Primary.TryGetComp<Comp_LightsaberBlade>();
            if (lightsaber != null)
            {
                lightsaber.lastInterceptAngle = lastInterceptAngle;
                float projectileSpeed = projectile.def.projectile.speed;
                int lightsaberSkill = pawn.skills.GetSkill(SkillDefOf.Melee).Level;


                if (!Force_ModSettings.DeflectionSpin)
                {
                    lightsaber.AnimationDeflectionTicks = 10;
                }
                else 
                {
                    float angleDifference = Mathf.Abs(projectile.ExactPosition.AngleToFlat(pawn.TrueCenter()) - lastInterceptAngle);
                    if (pawn.IsCarryingWeaponOpenly())
                    {

                        lightsaber.AnimationDeflectionTicks = (int)Mathf.Clamp(
                        (5000f / (projectileSpeed * (angleDifference + 1))) * Mathf.Lerp(1.2f, 0.8f, lightsaberSkill / 20f) + Random.Range(-100, 100),
                        300,
                        1200);

                        float deflectionBladeLength = lightsaber.MaxBladeLength;  // or a specific deflection length
                        lightsaber.targetScaleForCore1AndBlade1 = new Vector3(deflectionBladeLength, 1f, deflectionBladeLength);
                        lightsaber.targetScaleForCore2AndBlade2 = new Vector3(deflectionBladeLength, 1f, deflectionBladeLength);
                    }
                    else
                    {
                        lightsaber.AnimationDeflectionTicks = (int)Mathf.Clamp(
                        ((projectileSpeed * (angleDifference + 1))) * Mathf.Lerp(1.2f, 0.8f, lightsaberSkill / 20f) + Random.Range(-100, 100),
                        300,
                        1200
                    );

                        float deflectionBladeLength = lightsaber.MaxBladeLength;  // or a specific deflection length
                        lightsaber.targetScaleForCore1AndBlade1 = new Vector3(deflectionBladeLength, 1f, deflectionBladeLength);
                        lightsaber.targetScaleForCore2AndBlade2 = new Vector3(deflectionBladeLength, 1f, deflectionBladeLength);
                    }


                    float deflectionDistance = 0.3f;  // Example distance, adjust as necessary for the effect
                    Vector3 deflectionDirection = Quaternion.Euler(0f, lastInterceptAngle, 0f) * Vector3.forward;
                    Vector3 deflectionLocation = pawn.TrueCenter() + deflectionDirection * deflectionDistance;


                    FleckDef fleckDef = lightsaber.Props.Fleck;  // You can use any fleck you want here
                    if (fleckDef != null)
                    {
                        FleckCreationData fleckData = FleckMaker.GetDataStatic(deflectionLocation, pawn.Map, fleckDef);
                        fleckData.rotation = lastInterceptAngle;  // Align the fleck rotation with the intercept angle
                        pawn.Map.flecks.CreateFleck(fleckData);
                    }
                }
                
            }

            projectile.Launch(pawn, projectile.Launcher, projectile.Launcher, ProjectileHitFlags.All, false, projectile);
            AddEntropy(projectile);
        }


        public void AddEntropy(Projectile projectile)
        {
            // Calculate entropy gain based on projectile speed
            float EntropyGain = CalculateEntropyGain(projectile);

            // Add entropy to the pawn's psychicEntropy hediff
            this.pawn.psychicEntropy.TryAddEntropy(EntropyGain, overLimit: false);
        }

        public float CalculateEntropyGain(Projectile projectile)
        {
            entropyGain = Force_ModSettings.entropyGain;

            //entropy gain is proportional to the projectile's speed
            float EntropyGain = projectile.def.projectile.speed / entropyGain;
            return EntropyGain;
        }

        public override bool TryMergeWith(Hediff other)
        {
            return false;
        }

        public  void LightsaberScorchMark(Pawn pawn)
        {
            float chanceToSpawnFleck = .15f; // 15% chance

            if (Rand.Value < chanceToSpawnFleck)
            {
                FleckDef fleckDef = ForceDefOf.Force_LightsaberScorch_Fleck;
                if (fleckDef != null)
                {

                    IEnumerable<IntVec3> adjacentCells = GenAdj.CellsAdjacent8Way(pawn).Where(cell => cell.InBounds(pawn.Map));
                    List<IntVec3> validCells = adjacentCells.Where(cell => cell.Walkable(pawn.Map)).ToList();

                    if (validCells.Count > 0)
                    {
                        IntVec3 randomCell = validCells.RandomElement();
                        float randomRotation = Rand.Range(0f, 360f);
                        FleckCreationData fleckData = FleckMaker.GetDataStatic(randomCell.ToVector3(), pawn.Map, fleckDef);
                        fleckData.rotation = randomRotation;
                        pawn.Map.flecks.CreateFleck(fleckData);
                    }
                }
            }
        }

        public virtual bool ShouldDeflectProjectile(Projectile projectile)
        {
            float deflectionMultiplier = Force_ModSettings.deflectionMultiplier;

            HashSet<int> deflectableProjectileHashes = Force_ModSettings.deflectableProjectileHashes;
            if (!deflectableProjectileHashes.Contains(projectile.def.shortHash) && Force_ModSettings.projectileDeflectionSelector)
            {
                Log.Message($"Projectile {projectile.def.label} (shortHash: {projectile.def.shortHash}) is not deflectable.");
                return false;
            }


            if (this.pawn.psychicEntropy.EntropyValue >= this.pawn.psychicEntropy.MaxEntropy)
            {
                return false;
            }

            // Calculate the potential entropy gain
            float potentialEntropyGain = CalculateEntropyGain(projectile);
            float newEntropyValue = this.pawn.psychicEntropy.EntropyValue + potentialEntropyGain;

            // Check if the new entropy value exceeds 90% of the maximum entropy
            if (newEntropyValue >= (this.pawn.psychicEntropy.MaxEntropy * 0.99f))
            {;
                return false;
            }

            float deflectionSkillChance = pawn.GetStatValue(ForceDefOf.Force_Lightsaber_Deflection);
            float deflection = deflectionSkillChance * deflectionMultiplier;
            float randomValue = Rand.Range(0f, 1.0f);

            if (deflection >= randomValue)
            {
                return true; // Deflect the projectile
            }
            else
            {
                return false; // Do not deflect the projectile
            }
        }


        public override IEnumerable<Gizmo> GetGizmos()
        {
            yield return new Gizmo_LightsaberStance(pawn, this, pawn.equipment.Primary.def); // Pass ThingDef here
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref lastInterceptTicks, "lastInterceptTicks");
            Scribe_Values.Look(ref lastInterceptAngle, "lastInterceptTicks");
            Scribe_Values.Look(ref drawInterceptCone, "drawInterceptCone");
        }
    }
}