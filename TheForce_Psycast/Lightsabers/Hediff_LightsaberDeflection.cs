using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VanillaPsycastsExpanded;
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

        public override void Tick()
        {
            base.Tick();
            if (pawn.Map != null)
            {
                var projectiles = GenRadial.RadialDistinctThingsAround(pawn.Position, pawn.Map, 1, true)
                    .OfType<Projectile>();

                if (!projectiles.Any())
                    return;

                foreach (var projectile in projectiles)
                {
                    if (ShouldDeflectProjectile(projectile))
                    {
                        DeflectProjectile(projectile);
                    }
                }
            }
        }
        protected virtual void DeflectProjectile(Projectile projectile)
        {
            Effecter effecter = new Effecter(EffecterDefOf.Interceptor_BlockedProjectile);
            effecter.Trigger(new TargetInfo(projectile.Position, pawn.Map), TargetInfo.Invalid);
            effecter.Cleanup();
            lastInterceptAngle = projectile.ExactPosition.AngleToFlat(pawn.TrueCenter());
            lastInterceptTicks = Find.TickManager.TicksGame;
            drawInterceptCone = true;
            projectile.Launch(pawn, ((Vector3)TheForce_Psycast.NonPublicFields.Projectile_origin.GetValue(projectile)).ToIntVec3(), ((Vector3)TheForce_Psycast.NonPublicFields.Projectile_origin.GetValue(projectile)).ToIntVec3(), ProjectileHitFlags.All, true, projectile);
            AddEntropy(projectile);
        }

        private void AddEntropy(Projectile projectile)
        {
            // Calculate entropy gain based on projectile speed
            float EntropyGain = CalculateEntropyGain(projectile);

            // Add entropy to the pawn's psychicEntropy hediff
            this.pawn.psychicEntropy.TryAddEntropy(EntropyGain, overLimit: false);
        }

        private float CalculateEntropyGain(Projectile projectile)
        {
            entropyGain = Force_ModSettings.entropyGain;

            // Example calculation: entropy gain is proportional to the projectile's speed
            float EntropyGain = projectile.def.projectile.speed / entropyGain; // Adjust this factor as needed
            return EntropyGain;
        }


        public virtual bool ShouldDeflectProjectile(Projectile projectile)
        {
            float deflectionMultiplier = Force_ModSettings.deflectionMultiplier;

            if (projectile.Launcher == null || projectile.Launcher.Faction == null || projectile.Launcher.Faction == pawn.Faction)
            {
                return false;
            }

            if (!pawn.Faction.HostileTo(projectile.Launcher.Faction)) // Check if the launcher's faction is hostile
            {
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

            // Create and yield the custom gizmo
            yield return new Gizmo_LightsaberStance(pawn, this);
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