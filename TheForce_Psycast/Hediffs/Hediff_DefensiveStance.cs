using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using VanillaPsycastsExpanded;
using Verse;
using VFECore;
using VFECore.Abilities;


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
            projectile.Launch(pawn, ((Vector3)TheForce_Psycast.NonPublicFields.Projectile_origin.GetValue(projectile)).ToIntVec3(), ((Vector3)TheForce_Psycast.NonPublicFields.Projectile_origin.GetValue(projectile)).ToIntVec3(), ProjectileHitFlags.All, true, projectile);

        }

        public virtual bool CanDestroyProjectile(Projectile projectile)
        {
            var cell = ((Vector3)TheForce_Psycast.NonPublicFields.Projectile_origin.GetValue(projectile)).Yto0().ToIntVec3();
            return Vector3.Distance(projectile.ExactPosition.Yto0(), pawn.DrawPos.Yto0()) <= OverlaySize &&
                !GenRadial.RadialCellsAround(pawn.Position, OverlaySize, true).ToList().Contains(cell);
        }

        public virtual bool ShouldDeflectProjectile(Projectile projectile)
        {
            // Adjust the multiplier based on the desired probability
            float chanceMultiplier = (pawn.GetStatValue(StatDefOf.PsychicSensitivity)* .1f); // Adjust this value based on the desired chance

            // Calculate the chance based on melee skill * Psychic Sensitivity
            float meleeSkillChance = pawn.skills.GetSkill(SkillDefOf.Melee).Level * (pawn.GetStatValue(StatDefOf.PsychicSensitivity));

            // Generate a random value between 0 and 1
            float randomValue = Rand.Value;

            // Check if the random value is less than the calculated chance
            return randomValue < meleeSkillChance;
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

