using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TheForce_ItsSorcery
{
    public static class PositionUtils
    {
        // Checks if a position is valid (walkable and in bounds)
        public static bool CheckValidPosition(IntVec3 position, Map map)
        {
            return position.InBounds(map) && position.Standable(map);
        }

        // Finds a valid position close to the original position using the offset
        public static IntVec3 FindValidPosition(IntVec3 originalPosition, IntVec3 offset, Map map)
        {
            IntVec3 closestValidPosition = originalPosition;
            float closestDistanceSquared = float.MaxValue;

            for (int i = 1; i <= 3; i++)
            {
                IntVec3 newPosition = originalPosition + (offset * i);
                if (newPosition.InBounds(map) && newPosition.Standable(map))
                {
                    // Calculate distance to original position
                    float distanceSquared = newPosition.DistanceToSquared(originalPosition);
                    if (distanceSquared < closestDistanceSquared)
                    {
                        closestValidPosition = newPosition;
                        closestDistanceSquared = distanceSquared;
                    }
                }
            }
            return closestValidPosition;
        }
    }
}