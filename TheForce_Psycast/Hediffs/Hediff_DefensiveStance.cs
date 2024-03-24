using HarmonyLib;
using TheForce_Psycast;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using VanillaPsycastsExpanded;
using Verse;
using Verse.Sound;
using VFECore;
using VFECore.Abilities;
using static HarmonyLib.Code;
using VFECore.Shields;


namespace TheForce_Psycast
{
    [StaticConstructorOnStartup]
    public class Hediff_DefensiveStance : Hediff_Overlay
    {

        private int lastInterceptTicks = -999999;
        private float lastInterceptAngle;
        private bool drawInterceptCone;
        public override string OverlayPath => "Other/ForceField";

        public virtual Color OverlayColor => Color.white;
        public override void Tick()
        {
            base.Tick();
            if (pawn.Map != null)
            {
                foreach (var thing in GenRadial.RadialDistinctThingsAround(pawn.Position, pawn.Map, 1, true))
                {
                    if (thing is Projectile projectile)
                    {
                        if (CanDestroyProjectile(projectile))
                        {

                            DeflectProjectile(projectile);
                        }
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

        }

        public virtual bool CanDestroyProjectile(Projectile projectile)
        {
            var cell = ((Vector3)TheForce_Psycast.NonPublicFields.Projectile_origin.GetValue(projectile)).Yto0().ToIntVec3();
            return Vector3.Distance(projectile.ExactPosition.Yto0(), pawn.DrawPos.Yto0()) <= OverlaySize &&
                !GenRadial.RadialCellsAround(pawn.Position, OverlaySize, true).ToList().Contains(cell);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref lastInterceptTicks, "lastInterceptTicks");
            Scribe_Values.Look(ref lastInterceptAngle, "lastInterceptTicks");
            Scribe_Values.Look(ref drawInterceptCone, "drawInterceptCone");
        }
    }

    [StaticConstructorOnStartup]
    public class Hediff_LightsaberDeflection : Hediff_Overlay
    {

        private int lastInterceptTicks = -999999;
        private float lastInterceptAngle;
        private bool drawInterceptCone;

        public override string OverlayPath => "Other/ForceField";
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
            float entropyGain = CalculateEntropyGain(projectile);

            // Add entropy to the pawn's psychicEntropy hediff
            this.pawn.psychicEntropy.TryAddEntropy(entropyGain, overLimit: true);
        }

        private float CalculateEntropyGain(Projectile projectile)
        {
            // Example calculation: entropy gain is proportional to the projectile's speed
            float speedFactor = projectile.def.projectile.speed / 10f; // Adjust this factor as needed
            return speedFactor;
        }


        public virtual bool ShouldDeflectProjectile(Projectile projectile)
        {
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

            float deflectionSkillChance = pawn.GetStatValue(ForceDefOf.Force_Lightsaber_Deflection);
            float randomValue = Rand.Range(0f, 1.0f);

            if (deflectionSkillChance >= randomValue)
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

    public class MoteLightSaberSpin : Mote
    {
        private Pawn launcher;
        private Graphic originalWeaponGraphic;
        private float rotationAngle = 0f;
        private float rotationPerTick = Rand.Range(1.5f, 3f); // Rotation per tick in degrees
        public Vector3 deflectionPosition;

        public void SetLauncher(Pawn pawn)
        {
            launcher = pawn;
            if (launcher != null && launcher.equipment?.Primary != null)
            {
                ThingWithComps equippedWeapon = launcher.equipment.Primary;
                if (equippedWeapon.Graphic != null)
                {
                    originalWeaponGraphic = equippedWeapon.Graphic;
                    this.instanceColor = equippedWeapon.Graphic.color;
                }
            }
        }

        public override Graphic Graphic
        {
            get
            {
                return originalWeaponGraphic ?? base.Graphic;
            }
        }

        protected override void TimeInterval(float deltaTime)
        {
            base.TimeInterval(deltaTime);

            // Check if there is a launcher
            if (launcher != null)
            {
                // Update the position to match the deflection position
                exactPosition = deflectionPosition;

                // Update the rotation angle based on the rotation speed and elapsed time
                rotationAngle += rotationPerTick;
            }
        }

        public void DrawAt()
        {
            if (originalWeaponGraphic != null)
            {
                originalWeaponGraphic.Draw(DrawPos, Rotation, this, rotationAngle);
            }
        }
    }
}


 