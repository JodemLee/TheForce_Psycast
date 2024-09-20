using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TheForce_Psycast.Abilities.Telekinesis
{
    public class Ability_ForceWave : VFECore.Abilities.Ability
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

            foreach (var target in targets)
            {
                // Calculate the affected cells in the cone area
                List<IntVec3> affectedCells = AffectedCells(target.Cell);

                foreach (IntVec3 cell in affectedCells)
                {
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
                    IntVec3 pushBackPosition = cell + offset;

                    if (!PositionUtils.CheckValidPosition(pushBackPosition, map))
                    {
                        pushBackPosition = PositionUtils.FindValidPosition(cell, offset, map);

                        float distancePushed = (pushBackPosition - cell).LengthHorizontal;
                        float scaledDamage = baseDamage + (distancePushed * 2f); // Adjust the multiplier as needed

                        DamageInfo damageInfo = new DamageInfo(DamageDefOf.Blunt, (int)scaledDamage, 0f, -1f, this.pawn, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
                        targetPawn.TakeDamage(damageInfo);
                    }

                    if (targetPawn != null)
                    {
                        var flyer = PawnFlyer.MakeFlyer(ForceDefOf.Force_ThrownPawn, targetPawn, pushBackPosition, null, null);
                        GenSpawn.Spawn(flyer, pushBackPosition, map);
                    }
                    else
                    {
                        targetPawn.Position = pushBackPosition;
                    }
                }
            }

            base.Cast(targets);
        }

        private List<IntVec3> AffectedCells(IntVec3 targetCell)
        {
            List<IntVec3> cells = new List<IntVec3>();
            Vector3 startVector = pawn.Position.ToVector3Shifted().Yto0();
            Vector3 targetVector = targetCell.ToVector3Shifted().Yto0();
            float targetAngle = Vector3.SignedAngle(targetVector - startVector, Vector3.right, Vector3.up);
            int numCells = GenRadial.NumCellsInRadius(range);

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