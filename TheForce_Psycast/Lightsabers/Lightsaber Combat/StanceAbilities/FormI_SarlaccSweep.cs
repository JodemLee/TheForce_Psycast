using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TheForce_Psycast.Lightsabers.Lightsaber_Combat.StanceAbilities
{
    internal class FormI_SarlaccSweep : Lightsaber_AbilityBase
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);

            foreach (var targetInfo in targets)
            {
                var localTarget = (LocalTargetInfo)targetInfo;
                NotifyMeleeAttackOn(localTarget);

                if (LightsaberCombatUtility.CanParry(localTarget.Pawn, CasterPawn))
                {
                    IntVec3? overlapPoint = GetRandomCellBetween(pawn.Position, localTarget.Pawn.Position);
                    if (overlapPoint.HasValue)
                    {
                        LightsaberCombatUtility.TriggerWeaponRotationOnParry(pawn, localTarget.Pawn);

                        DamageInfo deflectInfo = new DamageInfo(DamageDefOf.Blunt, 0, 0, -1, pawn);
                        localTarget.Pawn.Drawer.Notify_DamageDeflected(deflectInfo);

                        Effecter effecter = new Effecter(ForceDefOf.Force_LClashOne);
                        effecter.Trigger(new TargetInfo(overlapPoint.Value, pawn.Map), TargetInfo.Invalid);
                        effecter.Cleanup();
                    }
                    continue;
                }
                List<IntVec3> affectedCells = AffectedCells(localTarget.Cell);
                foreach (IntVec3 cell in affectedCells)
                {
                    Pawn targetPawn = cell.GetFirstPawn(pawn.Map);
                    if (targetPawn != null && targetPawn != CasterPawn && targetPawn.Faction != CasterPawn.Faction)
                    {
                        AttackTarget(new LocalTargetInfo(targetPawn));
                    }
                }
            }
        }

        private List<IntVec3> AffectedCells(IntVec3 targetCell)
        {
            List<IntVec3> cells = new List<IntVec3>();
            Vector3 startPosition = pawn.Position.ToVector3Shifted();
            Vector3 targetPosition = targetCell.ToVector3Shifted();
            float targetAngle = Vector3.SignedAngle(targetPosition - startPosition, Vector3.right, Vector3.up);
            int numCells = GenRadial.NumCellsInRadius(this.def.range);
            for (int i = 0; i < numCells; i++)
            {
                IntVec3 cell = pawn.Position + GenRadial.RadialPattern[i];
                if (cell == pawn.Position)
                    continue;
                Vector3 cellVector = cell.ToVector3Shifted();
                float cellAngle = Vector3.SignedAngle(cellVector - startPosition, Vector3.right, Vector3.up);
                if (Mathf.Abs(Mathf.DeltaAngle(cellAngle, targetAngle)) <= 90f)
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
