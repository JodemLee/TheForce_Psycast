using RimWorld;
using System.Collections.Generic;
using TheForce_Psycast;
using TheForce_Psycast.Lightsabers;
using UnityEngine;
using Verse;

namespace ModularWeapon_Patch
{
    internal class Comp_WeaponStance : Comp_LightsaberStance
    {
        public new CompProperties_WeaponStance Props => (CompProperties_WeaponStance)props;
        private DefStanceAngles extension;

        private float stanceRotation;
        private Vector3 drawOffset;

        public float CurrentRotation => stanceRotation;
        public Vector3 CurrentDrawOffset => drawOffset;

        private int animationDeflectionTicks;
        public int AnimationDeflectionTicks
        {
            set => animationDeflectionTicks = value;
            get => animationDeflectionTicks;
        }
        public bool IsAnimatingNow => animationDeflectionTicks >= 0;

        public void UpdateRotationForStance(float angle)
        {
            stanceRotation = angle;
        }

        public void UpdateDrawOffsetForStance(Vector3 offset)
        {
            drawOffset = offset;
        }


        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            ApplyStanceRotation(pawn);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            // Ensure we apply stance after the pawn and equipment are initialized
            if (parent is Pawn pawn && pawn.equipment?.Primary != null)
            {
                ApplyStanceRotation(pawn);
            }
        }

        private void ApplyStanceRotation(Pawn pawn)
        {
            var lightsaberComp = pawn.equipment.Primary?.GetComp<Comp_WeaponStance>();
            if (lightsaberComp != null)
            {
                var (angle, offset) = GetStanceAngleAndOffset(pawn);

                lightsaberComp.UpdateRotationForStance(angle);
                lightsaberComp.UpdateDrawOffsetForStance(offset);
            }
        }

        private (float angle, Vector3 offset) GetStanceAngleAndOffset(Pawn pawn)
        {
            var hediffDef = ForceDefOf.Lightsaber_Stance; // Ensure this is the correct definition
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);

            if (hediff == null)
                return (0f, Vector3.zero);

            var thingDef = pawn.equipment.Primary?.def; // Ensure the ThingDef is properly retrieved
            extension = thingDef.GetModExtension<DefStanceAngles>() ?? hediff.def.GetModExtension<DefStanceAngles>();
            StanceData stanceData = extension?.GetStanceDataForSeverity(hediff.Severity);
            return (stanceData?.Angle ?? 0f, stanceData?.Offset ?? Vector3.zero);
        }
    }

    public class CompProperties_WeaponStance : CompProperties_LightsaberStance
    {
        public CompProperties_WeaponStance()
        {
            this.compClass = typeof(Comp_WeaponStance);
        }
    }
}

