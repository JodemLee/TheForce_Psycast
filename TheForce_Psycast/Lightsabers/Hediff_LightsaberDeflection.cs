using RimWorld;
using System.Collections.Generic;
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

                // Calculate the deflection angle difference to adjust animation timing
                float angleDifference = Mathf.Abs(projectile.ExactPosition.AngleToFlat(pawn.TrueCenter()) - lastInterceptAngle);

                // Base the animation duration on all factors: speed, skill, and deflection angle, with added randomness
                lightsaber.AnimationDeflectionTicks = (int)Mathf.Clamp(
                    (5000f / (projectileSpeed * (angleDifference + 1))) * Mathf.Lerp(1.2f, 0.8f, lightsaberSkill / 20f) + Random.Range(-100, 100),
                    300,
                    1200
                );
            }

            projectile.Launch(pawn, ((Vector3)TheForce_Psycast.NonPublicFields.Projectile_origin.GetValue(projectile)).ToIntVec3(), ((Vector3)TheForce_Psycast.NonPublicFields.Projectile_origin.GetValue(projectile)).ToIntVec3(), ProjectileHitFlags.All, true, projectile);
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

            // Example calculation: entropy gain is proportional to the projectile's speed
            float EntropyGain = projectile.def.projectile.speed / entropyGain; // Adjust this factor as needed
            return EntropyGain;
        }


        public virtual bool ShouldDeflectProjectile(Projectile projectile)
        {
            float deflectionMultiplier = Force_ModSettings.deflectionMultiplier;

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
                return false; // Prevent deflection if it would cause entropy to go over 90% of max
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
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

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