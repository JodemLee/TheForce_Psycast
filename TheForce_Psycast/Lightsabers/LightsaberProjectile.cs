using UnityEngine;
using Verse;
using VFECore.Abilities;

namespace TheForce_Psycast
{
    [StaticConstructorOnStartup]
    public class LightSaberProjectile : AbilityProjectile
    {
        private float rotationAngle = 0f;
        public int ticksPerFrame = 8;

        public int TicksToImpact => ticksToImpact;
        Projectile projectile;
        public Graphic originalWeaponGraphic;
        private Pawn cachedLauncher;
        private static readonly Material shadowMaterial = MaterialPool.MatFrom("Things/Skyfaller/SkyfallerShadowCircle", ShaderDatabase.Transparent);
        private float ArcHeightFactor
        {
            get
            {
                float num = def.projectile.arcHeightFactor;
                float num2 = (destination - origin).MagnitudeHorizontalSquared();
                if (num * num > num2 * 0.2f * 0.2f)
                {
                    num = Mathf.Sqrt(num2) * 0.2f;
                }

                return num;
            }
        }

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            float num = ArcHeightFactor * GenMath.InverseParabola(DistanceCoveredFractionArc);
            Vector3 vector = drawLoc + new Vector3(0f, 0f, 1f) * num;

            if (def.projectile.shadowSize > 0f)
            {
                DrawShadow(drawLoc, num);
            }

            Quaternion rotation = ExactRotation;

            Graphic graphicToDraw;

            // Check if the pawn has an equipped weapon
            if (launcher is Pawn pawn && pawn.equipment?.Primary != null)
            {
                ThingWithComps equippedWeapon = pawn.equipment?.Primary;
                // Get the graphic of the equipped weapon
                graphicToDraw = equippedWeapon.Graphic;
            }
            else
            {
                graphicToDraw = def.graphic;
            }

            float rotationSpeed = 720f; // Adjust the rotation speed as needed
            float rotationAngle = Time.time * rotationSpeed;

            if (def.projectile.useGraphicClass)
            {
                // Apply continuous rotation
                rotation *= Quaternion.Euler(0f, rotationAngle, 0f);
                Graphic.Draw(vector, Rotation, this, rotation.eulerAngles.y);
            }
            else
            {
                // Apply continuous rotation
                rotation *= Quaternion.Euler(0f, rotationAngle, 0f);
                Graphics.DrawMesh(MeshPool.GridPlane(graphicToDraw.drawSize), vector, rotation, graphicToDraw.MatSingle, 0);
            }

            Comps_PostDraw();
        }

        private void DrawShadow(Vector3 drawLoc, float height)
        {
            if (!(shadowMaterial == null))
            {
                float num = def.projectile.shadowSize * Mathf.Lerp(1f, 0.6f, height);
                Vector3 s = new Vector3(num, 1f, num);
                Vector3 vector = new Vector3(0f, -0.01f, 0f);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(drawLoc + vector, Quaternion.identity, s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, shadowMaterial, 0);
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
                ThingWithComps equippedWeapon = pawn.equipment?.Primary;
                if (equippedWeapon.Graphic != null)
                {
                    originalLauncher = launcher;
                    originalWeaponGraphic = equippedWeapon.Graphic;
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
