using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using Verse;
using VFECore.Abilities;
using static HarmonyLib.Code;
using static UnityEngine.GraphicsBuffer;
using Ability = VFECore.Abilities.Ability;

namespace TheForce_Psycast.Abilities.Lightside
{
    internal class Force_Ionize : Ability
    {
        public float coneAngle = 15f; // Angle of the cone

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            AbilityExtension_Explosion modExtension = def.GetModExtension<AbilityExtension_Explosion>();
            if (modExtension != null)
            {
                foreach (GlobalTargetInfo globalTargetInfo in targets)
                {
                    List<IntVec3> affectedCells = AffectedCells(globalTargetInfo.Cell);
                    foreach (IntVec3 cell in affectedCells)
                    {
                        GenExplosion.DoExplosion(cell, pawn.Map, 0f, modExtension.explosionDamageDef, (Thing)pawn, modExtension.explosionDamageAmount, modExtension.explosionArmorPenetration, modExtension.explosionSound, (ThingDef)null, (ThingDef)null, (Thing)null, modExtension.postExplosionSpawnThingDef, modExtension.postExplosionSpawnChance, modExtension.postExplosionSpawnThingCount, modExtension.postExplosionGasType, modExtension.applyDamageToExplosionCellsNeighbors, modExtension.preExplosionSpawnThingDef, modExtension.preExplosionSpawnChance, modExtension.preExplosionSpawnThingCount, modExtension.chanceToStartFire, modExtension.damageFalloff, modExtension.explosionDirection, modExtension.casterImmune ? new List<Thing> { pawn } : null, (FloatRange?)null, true, 1f, 0f, true, (ThingDef)null, 1f, overrideCells: AffectedCells(cell));                  
                    }
                }
            }
        }
        private List<IntVec3> AffectedCells(IntVec3 targetCell)
        {
            List<IntVec3> cells = new List<IntVec3>();
            Vector3 startVector = pawn.Position.ToVector3Shifted().Yto0();
            Vector3 targetVector = targetCell.ToVector3Shifted().Yto0();
            float targetAngle = Vector3.SignedAngle(targetVector - startVector, Vector3.right, Vector3.up);
            int numCells = GenRadial.NumCellsInRadius(def.range);
            for (int i = 0; i < numCells; i++)
            {
                IntVec3 cell = pawn.Position + GenRadial.RadialPattern[i];
                if (Mathf.Abs(Mathf.DeltaAngle(Vector3.SignedAngle(cell.ToVector3Shifted().Yto0() - startVector, Vector3.right, Vector3.up), targetAngle)) <= coneAngle)
                {
                    cells.Add(cell);
                }
            }
            return cells;
        }
        public override void DrawHighlight(LocalTargetInfo target)
        {
            base.DrawHighlight(target);

            List<IntVec3> affectedCells = AffectedCells(target.Cell);
            GenDraw.DrawFieldEdges(affectedCells);
        }
    }
}
