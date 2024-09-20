using System.Collections.Generic;
using TheForce_Psycast.Lightsabers;
using UnityEngine;
using Verse;
using Verse.Sound;
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

        public float spinRate { get; set; }

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
            Vector3 vector = drawLoc;

            if (def.projectile.shadowSize > 0f)
            {
                DrawShadow(drawLoc, num);
            }

            Quaternion rotation = ExactRotation;

            Graphic graphicToDraw = def.graphic;
            Comp_LightsaberBlade compLightsaberBlade = null;

            if (launcher is Pawn pawn)
            {
                if (pawn.equipment != null && pawn.equipment.Primary != null)
                {
                    ThingWithComps equippedWeapon = pawn.equipment.Primary;
                    graphicToDraw = equippedWeapon.Graphic;

                    compLightsaberBlade = equippedWeapon.TryGetComp<Comp_LightsaberBlade>();
                }
            }

            float spinRate = Force_ModSettings.spinRate;
            float rotationSpeed = 100f * spinRate;
            float rotationAngle = Time.time * rotationSpeed;
            rotation *= Quaternion.Euler(0f, rotationAngle, 0f);

            if (def.projectile.useGraphicClass)
            {
                Graphic.Draw(vector, Rotation, this, rotation.eulerAngles.y);
            }
            else
            {
                Graphics.DrawMesh(MeshPool.GridPlane(graphicToDraw.drawSize), vector, rotation, graphicToDraw.MatSingle, 0);
            }


            if (compLightsaberBlade != null)
            {
                DrawLightsaberGraphics(compLightsaberBlade, DrawPos,  rotation, rotationSpeed);
                compLightsaberBlade.IsThrowingWeapon = true;
            }

            

            Comps_PostDraw();
        }

        private void DrawLightsaberGraphics(Comp_LightsaberBlade compLightsaberBlade, Vector3 drawLoc, Quaternion rotation, float rotationspeed, float drawSize = 2.0f)
        {
            if (compLightsaberBlade.Graphic?.MatSingle == null)
                return;

            // Adjust y-axis for blade and core offsets
            float coreYOffset = -0.001f;
            float bladeYOffset = -0.002f;

            List<Material> materialsToDraw = new List<Material>();
            List<Vector3> drawLocations = new List<Vector3>();

            materialsToDraw.Add(compLightsaberBlade.Graphic.MatSingle);
            drawLocations.Add(drawLoc + new Vector3(0, bladeYOffset, 0));

            if (compLightsaberBlade.LightsaberCore1Graphic?.MatSingle != null)
            {
                materialsToDraw.Add(compLightsaberBlade.LightsaberCore1Graphic.MatSingle);
                drawLocations.Add(drawLoc + new Vector3(0, coreYOffset, 0));
            }

            if (compLightsaberBlade.LightsaberBlade2Graphic?.MatSingle != null)
            {
                materialsToDraw.Add(compLightsaberBlade.LightsaberBlade2Graphic.MatSingle);
                drawLocations.Add(drawLoc + new Vector3(0, bladeYOffset, 0));
            }

            if (compLightsaberBlade.LightsaberCore2Graphic?.MatSingle != null)
            {
                materialsToDraw.Add(compLightsaberBlade.LightsaberCore2Graphic.MatSingle);
                drawLocations.Add(drawLoc + new Vector3(0, coreYOffset, 0));
            }

            if (compLightsaberBlade.LightsaberGlowGraphic?.MatSingle != null)
            {
                materialsToDraw.Add(compLightsaberBlade.LightsaberGlowGraphic.MatSingle);
                drawLocations.Add(drawLoc + new Vector3(0, coreYOffset, 0));
            }

            for (int i = 0; i < materialsToDraw.Count; i++)
            {
                // Create a scaling matrix with the desired draw size
                Matrix4x4 matrix = Matrix4x4.TRS(drawLocations[i], rotation, new Vector3(drawSize, 1, drawSize));

                // Draw the mesh with scaling applied
                Graphics.DrawMesh(MeshPool.plane10, matrix, materialsToDraw[i], -2 + i);
            }
        }

        private void DrawShadow(Vector3 drawLoc, float height)
        {
            if (shadowMaterial != null)
            {
                float num = def.projectile.shadowSize * Mathf.Lerp(1f, 0.6f, height);
                Vector3 s = new Vector3(num, 1f, num);
                Vector3 vector = new Vector3(0f, -0.01f, 0f);
                Matrix4x4 matrix = Matrix4x4.TRS(drawLoc + vector, Quaternion.identity, s);
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
                mote.SetLauncher(cachedLauncher, cachedLauncher.equipment.Primary.Graphic);
                GenSpawn.Spawn(mote, moteSpawnPos, map);
                SoundDef soundDef = SoundDef.Named("Force_ForceThrow_Return");
                soundDef.PlayOneShot(new TargetInfo(moteSpawnPos, map));
                
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
                Comp_LightsaberBlade compLightsaberBlade = originalLauncher.equipment.Primary.TryGetComp<Comp_LightsaberBlade>();
                if(compLightsaberBlade != null)
                {
                    compLightsaberBlade.IsThrowingWeapon = false;
                    SoundDef soundDef = ForceDefOf.Force_Interact_Lightsaber;
                    SoundStarter.PlayOneShot(soundDef, this);
                }
                
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