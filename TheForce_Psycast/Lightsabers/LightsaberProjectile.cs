using RimWorld;
using UnityEngine;
using Verse;
using VFECore.Abilities;

namespace TheForce_Psycast
{
    public class LightSaberProjectile : AbilityProjectile
    {
        private float rotationAngle = 0f;
        public int ticksPerFrame = 8;
        public int TicksToImpact => ticksToImpact;
        Projectile projectile;

        public Graphic originalWeaponGraphic;
        private Pawn cachedLauncher;

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
                    if (originalWeaponGraphic == null)
                    {
                        originalWeaponGraphic = equippedWeapon.Graphic;

                        if (originalWeaponGraphic == null)
                        {
                            return;
                        }
                    }

                    if (originalWeaponGraphic != null)
                    {
                        rotationAngle -= 3f;
                        originalWeaponGraphic.Draw(DrawPos, Rotation, this, rotationAngle);
                    }

                }
            }
        }

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            Map map = base.Map;
            base.Impact(hitThing, blockedByShield);
            DoImpact(hitThing, map);
            if (launcher is Pawn cachedLauncher && cachedLauncher.equipment?.Primary != null)
            {
                IntVec3 moteSpawnPos = hitThing != null ? hitThing.Position : Position;
                MoteLightSaberReturn mote = (MoteLightSaberReturn)ThingMaker.MakeThing(ThingDef.Named("Mote_LightSaberReturn"));
                mote.exactPosition = moteSpawnPos.ToVector3Shifted();
                mote.rotationRate = 0f;
                mote.SetLauncher(cachedLauncher, originalWeaponGraphic);
                GenSpawn.Spawn(mote, moteSpawnPos, map);
            }

            else
            {
                Log.Warning("Attempted to launch LightSaberProjectile without a primary weapon equipped.");
            }
        }
    }


    public class MoteLightSaberReturn : MoteThrown
    {
        private Pawn launcher;
        private Pawn originalLauncher;
        private Graphic originalWeaponGraphic;
        public int ticksPerFrame = 8;

        public void SetLauncher(Pawn pawn, Graphic weaponGraphic)
        {
            launcher = pawn;
            if (launcher != null && launcher.equipment?.Primary != null)
            {
                ThingWithComps equippedWeapon = launcher.equipment.Primary;
                if (equippedWeapon.Graphic != null)
                {
                    originalLauncher = launcher;
                    originalWeaponGraphic = weaponGraphic;
                    this.instanceColor = equippedWeapon.Graphic.color;
                }
            }
        }

        protected override void TimeInterval(float deltaTime)
        {
            base.TimeInterval(deltaTime);

            if (originalLauncher == null || originalLauncher.Destroyed)
            {
                return;
            }

            if (originalWeaponGraphic == null)
            {
                Log.Warning("originalWeaponGraphic is null in MoteLightSaberReturn.TimeInterval");
                return;
            }

            Vector3 directionToLauncher = (originalLauncher.Position.ToVector3Shifted() - this.exactPosition).normalized;

            float verticalVelocity = ticksPerFrame * 1.1f;
            float horizontalVelocity = verticalVelocity * directionToLauncher.AngleFlat();

            SetVelocity(horizontalVelocity, verticalVelocity);
            base.exactPosition += base.velocity * deltaTime;

            if (CheckCollisionWithLauncher())
            {
                this.Destroy(DestroyMode.Vanish);
            }
        }

        public override Graphic Graphic
        {
            get
            {
                if (originalWeaponGraphic == null)
                {
                    return base.Graphic;
                }
                else
                {
                    return originalWeaponGraphic;
                }
            }
        }

        private bool CheckCollisionWithLauncher()
        {
            if (originalLauncher == null)
                return false;

            Vector2 moteSize = this.originalWeaponGraphic.drawSize;

            Rect moteRect = new Rect(this.exactPosition.x, this.exactPosition.z, moteSize.x, moteSize.y);
            Rect launcherRect = new Rect(originalLauncher.Position.x, originalLauncher.Position.z, originalLauncher.def.size.x, originalLauncher.def.size.z);

            if (moteRect.Overlaps(launcherRect))
            {
                return true;
            }

            return false;
        }
    }
}
