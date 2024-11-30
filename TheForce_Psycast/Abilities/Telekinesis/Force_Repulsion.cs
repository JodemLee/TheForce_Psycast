using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace TheForce_Psycast
{
    public class Ability_ForceRepulsion : VFECore.Abilities.Ability
    {
        Force_ModSettings modSettings = new Force_ModSettings();
        public bool usePsycastStat = false;
        public int offsetMultiplier { get; set; }

        public Ability_ForceRepulsion()
        {
            modSettings = new Force_ModSettings(); // Instantiate Force_ModSettings
        }

        public int GetOffsetMultiplier()
        {

            if (Force_ModSettings.usePsycastStat == true)
            {
                offsetMultiplier = (int)(offsetMultiplier * pawn.GetStatValue(StatDefOf.PsychicSensitivity));
                return offsetMultiplier;
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

            foreach (GlobalTargetInfo target in targets)
            {
                // Calculate the offset between caster and target
                IntVec3 offset = target.Cell - this.pawn.Position;

                // Normalize the offset to ensure correct direction
                IntVec3 normalizedOffset = NormalizeIntVec3(offset);

                // Apply the offset multiplier
                normalizedOffset *= offsetMultiplier;

                // Calculate the initial pushback position
                IntVec3 initialPushBackPosition = target.Cell + normalizedOffset;
                IntVec3 pushBackPosition = initialPushBackPosition;

                Map map = Caster.Map;
                IntVec3 lastValidCell = target.Cell;

                // Check each cell along the line of sight to the initial pushback position
                foreach (IntVec3 cell in GenSight.PointsOnLineOfSight(this.pawn.Position, initialPushBackPosition))
                {
                    // If we hit an impassable cell, stop and set pushBackPosition to last valid cell
                    if (!cell.InBounds(map) || cell.GetRoofHolderOrImpassable(map) is Building)
                    {
                        pushBackPosition = lastValidCell;
                        break;
                    }
                    lastValidCell = cell; // Update last valid cell if no obstacle is found
                }

                // Check if the final pushBackPosition is valid, otherwise adjust it closer to the target
                if (!PositionUtils.CheckValidPosition(pushBackPosition, map))
                {
                    pushBackPosition = PositionUtils.FindValidPosition(target.Cell, normalizedOffset, map);
                }

                // Apply explosive force to the target
                if (target.Thing is Pawn pawn)
                {
                    var flyer = PawnFlyer.MakeFlyer(ForceDefOf.Force_ThrownPawnRepulse, pawn, pushBackPosition, null, null);
                    GenSpawn.Spawn(flyer, pushBackPosition, map);
                }
                else
                {
                    target.Thing.Position = pushBackPosition;
                }
            }

            base.Cast(targets);
        }


        public override float GetPowerForPawn() => def.power + Mathf.FloorToInt((pawn.GetStatValue(StatDefOf.PsychicSensitivity) * 2));

        private IntVec3 NormalizeIntVec3(IntVec3 vector)
        {
            float magnitude = Mathf.Sqrt(vector.x * vector.x + vector.z * vector.z);

            if (magnitude > 0)
            {
                float scaleFactor = 1f / magnitude;
                return new IntVec3(Mathf.RoundToInt(vector.x * scaleFactor), 0, Mathf.RoundToInt(vector.z * scaleFactor));
            }

            return vector;
        }
    }
}
