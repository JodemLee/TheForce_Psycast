using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using UnityEngine;
using VanillaPsycastsExpanded;
using Verse;
using VFECore.Abilities;
using AbilityDef = VFECore.Abilities.AbilityDef;

namespace TheForce_Psycast
{
    public class LightSaberProjectile : AbilityProjectile
    {
        public DamageDef damageDef;
        protected virtual int GetDamageAmount => this.def.projectile.GetDamageAmount(1f);
        private float rotationAngle = 0f;
        public int ticksPerFrame = 8;
        public const float RotationSpeed = 1f;
        public int TicksToImpact => ticksToImpact;
        Projectile projectile;

        public override void Draw()
        {
            base.Draw();

            if (launcher is Pawn pawn)
            {
                Vector3 launcherPos = launcher.Position.ToVector3Shifted();
                Vector3 destinationPos = destination.ToIntVec3().ToVector3Shifted();
                float distance = Vector3.Distance(launcherPos, destinationPos);

                ThingWithComps equippedWeapon = pawn.equipment?.Primary;

                if (equippedWeapon != null)
                {

                    if (equippedWeapon != null && equippedWeapon.Graphic != null)
                    {
                        rotationAngle -= distance / pawn.GetStatValue(StatDefOf.PsychicSensitivity);
                        equippedWeapon.Graphic.Draw(DrawPos, Rotation, this, rotationAngle);
                    }
                }
            }
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            Map map = base.Map;
            base.Impact(hitThing, blockedByShield);
            DoImpact(hitThing, map);

            if (launcher is Pawn pawn)
            {

                IntVec3 moteSpawnPos = hitThing != null ? hitThing.Position : Position;
                MoteLightSaberReturn mote = (MoteLightSaberReturn)ThingMaker.MakeThing(ThingDef.Named("Mote_LightSaberReturn"));
                mote.exactPosition = moteSpawnPos.ToVector3Shifted();
                mote.rotationRate = 0f;
                mote.SetLauncher(pawn);
                GenSpawn.Spawn(mote, moteSpawnPos, map);

            }
        }
    }


    public class MoteLightSaberReturn : MoteThrown
    {
        private Pawn launcher;
        public int ticksPerFrame = 8;

        public void SetLauncher(Pawn pawn)
        {
            launcher = pawn;
            if (launcher != null && launcher.equipment?.Primary != null)
            {
                ThingWithComps equippedWeapon = launcher.equipment.Primary;
                if (equippedWeapon.Graphic != null)
                {
                    this.exactScale = equippedWeapon.Graphic.drawSize;
                    this.instanceColor = equippedWeapon.Graphic.color;
                }
            }
        }

        public override Graphic Graphic
        {
            get
            {
                return launcher?.equipment?.Primary?.Graphic;
            }
        }

        public static readonly Func<float, float> Sine = delegate (float x)
        {
            return Mathf.Sin(x * Mathf.PI);
        };

        protected override void TimeInterval(float deltaTime)
        {
            base.TimeInterval(deltaTime);

            if (launcher != null && !launcher.Destroyed)
            {
                Vector3 directionToLauncher = (launcher.Position.ToVector3Shifted() - this.exactPosition).normalized;

                // Add an arc effect
                float gravity = 9.8f; // Adjust this value to control the strength of the gravitational effect
                float verticalVelocity = ticksPerFrame * 1.1f;
                float horizontalVelocity = verticalVelocity * directionToLauncher.AngleFlat();

                // Apply gravitational effect
                verticalVelocity -= gravity * deltaTime;

                SetVelocity(horizontalVelocity, verticalVelocity);

                base.exactPosition += base.velocity * deltaTime;

                // Check if the mote has reached the target
                float distanceToLauncher = Vector3.Distance(this.exactPosition, launcher.Position.ToVector3Shifted());
                float despawnThreshold = 1f; // Adjust this value based on your needs

                if (distanceToLauncher <= despawnThreshold)
                {
                    // The mote has reached the target, destroy it
                    this.Destroy(DestroyMode.Vanish);
                }
            }
        }
    }
}
