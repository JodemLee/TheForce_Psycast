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
}