using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TheForce_Psycast.Abilities.Telekinesis
{
    public class Ability_ForceWave : Ability_WriteCombatLog
    {
        public float baseDamage = 1f;
        Force_ModSettings modSettings = new Force_ModSettings();
        public bool usePsycastStat = false;
        public int offsetMultiplier { get; set; }
        public float coneAngle = 30f; // Angle of the cone
        public float range = 5f; // Range of the cone

        public Ability_ForceWave()
        {
            modSettings = new Force_ModSettings(); // Instantiate Force_ModSettings
        }

        public int GetOffsetMultiplier()
        {
            if (Force_ModSettings.usePsycastStat)
            {
                offsetMultiplier = (int)(offsetMultiplier * pawn.GetStatValue(StatDefOf.PsychicSensitivity));
            }
            else
            {
                offsetMultiplier = (int)Force_ModSettings.offSetMultiplier;
            }
            return offsetMultiplier;
        }

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            int offsetMultiplier = GetOffsetMultiplier();
            var map = pawn.Map;

            if (map == null)
            {
                Log.Error("Ability_ForceWave.Cast: Map is null.");
                return;
            }

            foreach (var target in targets)
            {
                if (!target.IsValid)
                {
                    Log.Warning("Ability_ForceWave.Cast: Invalid target.");
                    continue;
                }

                // Calculate the affected cells in the cone area
                List<IntVec3> affectedCells = AffectedCells(target.Cell);

                foreach (IntVec3 cell in affectedCells)
                {
                    if (!cell.IsValid || !cell.InBounds(map))
                    {
                        continue;
                    }

                    Pawn targetPawn = cell.GetFirstPawn(map);
                    if (targetPawn == null)
                    {
                        continue;
                    }

                    // Calculate the offset between the caster and the target
                    IntVec3 offset = cell - this.pawn.Position;

                    if (offset.LengthHorizontal < 3)
                    {
                        offset = offset * 3;
                    }

                    offset *= offsetMultiplier;
                    IntVec3 initialPushBackPosition = cell + offset;
                    IntVec3 pushBackPosition = initialPushBackPosition;

                    IntVec3 lastValidCell = cell;

                    // Line of sight check for each cell from the caster to the initial pushback position
                    foreach (IntVec3 lineCell in GenSight.PointsOnLineOfSight(this.pawn.Position, initialPushBackPosition))
                    {
                        if (!lineCell.InBounds(map) || lineCell.GetRoofHolderOrImpassable(map) is Building)
                        {
                            pushBackPosition = lastValidCell;  // Set to the last unobstructed cell
                            break;
                        }
                        lastValidCell = lineCell;
                    }

                    // Check if the final pushBackPosition is valid; adjust if not
                    if (!PositionUtils.CheckValidPosition(pushBackPosition, map))
                    {
                        pushBackPosition = PositionUtils.FindValidPosition(cell, offset, map);
                    }

                    if (!pushBackPosition.InBounds(map))
                    {
                        Log.Warning("Ability_ForceWave.Cast: pushBackPosition is out of bounds.");
                        continue;
                    }

                    // Calculate damage based on distance pushed
                    float distancePushed = (pushBackPosition - cell).LengthHorizontal;
                    float scaledDamage = baseDamage + (distancePushed * 2f); // Adjust the multiplier as needed
                    DamageInfo damageInfo = new DamageInfo(DamageDefOf.Blunt, (int)scaledDamage, 0f, -1f, this.pawn, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
                    targetPawn.TakeDamage(damageInfo);

                    // Move or throw the target to the calculated position
                    if (ForceDefOf.Force_ThrownPawnWave == null || targetPawn == null || !pushBackPosition.IsValid)
                    {
                        Log.Error("Ability_ForceWave.Cast: Invalid parameters for MakeFlyer.");
                        continue;
                    }

                    var flyer = PawnFlyer.MakeFlyer(ForceDefOf.Force_ThrownPawnWave, targetPawn, pushBackPosition, null, null);
                    if (flyer == null)
                    {
                        Log.Error("Ability_ForceWave.Cast: Failed to create PawnFlyer.");
                        continue;
                    }

                    GenSpawn.Spawn(flyer, pushBackPosition, map);
                }
            }

            base.Cast(targets);
        }

        private List<IntVec3> AffectedCells(IntVec3 targetCell)
        {
            List<IntVec3> cells = new List<IntVec3>();

            // Cache start and target vectors to avoid recomputation
            Vector3 startPosition = pawn.Position.ToVector3Shifted();
            Vector3 targetPosition = targetCell.ToVector3Shifted();

            // Compute the target angle once (the angle to the target)
            float targetAngle = Vector3.SignedAngle(targetPosition - startPosition, Vector3.right, Vector3.up);
            int numCells = GenRadial.NumCellsInRadius(range);

            // Iterate through radial pattern to find affected cells
            for (int i = 0; i < numCells; i++)
            {
                IntVec3 cell = pawn.Position + GenRadial.RadialPattern[i];

                // Skip the caster's position
                if (cell == pawn.Position)
                    continue;

                // Calculate angle from the caster to the current cell
                Vector3 cellVector = cell.ToVector3Shifted();
                float cellAngle = Vector3.SignedAngle(cellVector - startPosition, Vector3.right, Vector3.up);

                // Check if the cell is within the cone angle, relative to the target angle
                if (Mathf.Abs(Mathf.DeltaAngle(cellAngle, targetAngle)) <= coneAngle)
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