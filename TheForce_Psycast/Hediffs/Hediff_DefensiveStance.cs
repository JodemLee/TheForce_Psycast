using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;


namespace TheForce_Psycast
{
    [StaticConstructorOnStartup]
    public class Hediff_DefensiveStance : HediffWithComps
    {

        private int lastInterceptTicks = -999999;
        private float lastInterceptAngle;
        private bool drawInterceptCone;
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
            return Vector3.Distance(projectile.ExactPosition.Yto0(), pawn.DrawPos.Yto0()) <= 1 &&
                !GenRadial.RadialCellsAround(pawn.Position, 1, true).ToList().Contains(cell);
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


 