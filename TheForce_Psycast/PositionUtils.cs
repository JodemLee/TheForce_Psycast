using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace TheForce_Psycast
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

        public static bool IsCarryingWeaponOpenly(this Pawn pawn)
        {
            if (pawn.carryTracker != null && pawn.carryTracker.CarriedThing != null)
            {
                return false;
            }
            if (pawn.Drafted)
            {
                return true;
            }
            if ((pawn.CurJob?.def?.alwaysShowWeapon).GetValueOrDefault())
            {
                return true;
            }
            if ((pawn.mindState?.duty?.def?.alwaysShowWeapon).GetValueOrDefault())
            {
                return true;
            }
            Lord lord = pawn.GetLord();
            if (lord != null && lord.LordJob != null && lord.LordJob.AlwaysShowWeapon)
            {
                return true;
            }
            return false;
        }

        public static string AsCompassDirection(this Rot4 rotation)
        {
            return rotation == Rot4.South ? "south" :
                   rotation == Rot4.North ? "north" :
                   rotation == Rot4.East ? "east" : "west";
        }
    }

    public static class ProjectileUtility
    {
        // Method to get all projectile ThingDefs from the DefDatabase
        public static List<ThingDef> GetAllProjectiles()
        {
            List<ThingDef> allProjectiles = new List<ThingDef>();

            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
            {
                // Check if this ThingDef has a projectile component
                if (def.projectile != null)
                {
                    allProjectiles.Add(def);
                }
            }

            return allProjectiles;
        }
    }

    public static class StuffColorUtility
    {
        private static System.Random random = new System.Random();
        public static List<ThingDef> GetAllStuffed()
        {
            List<ThingDef> allStuffedDefs = new List<ThingDef>();
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
            {
                if (def.stuffProps != null)
                {
                    allStuffedDefs.Add(def);
                }
            }

            return allStuffedDefs;
        }
        public static Color GetStuffColor(ThingDef thingDef)
        {
            if (thingDef.stuffProps != null && thingDef.stuffProps.color != null)
            {
                return thingDef.stuffProps.color;
            }
            if (thingDef.graphicData != null && thingDef.graphicData.color != null)
            {
                return thingDef.graphicData.color;
            }
            return Color.white;
        }

        public static Color GetRandomColorFromStuffCategories(List<StuffCategoryDef> categories)
        {
            List<ThingDef> allStuffedDefs = GetAllStuffed();
            Dictionary<StuffCategoryDef, List<ThingDef>> categorizedStuffDefs = new Dictionary<StuffCategoryDef, List<ThingDef>>();

            foreach (ThingDef def in allStuffedDefs)
            {
                if (def.stuffProps != null && def.stuffProps.categories != null)
                {
                    foreach (StuffCategoryDef category in def.stuffProps.categories)
                    {
                        if (category.label != "Metallic")
                        {
                            if (!categorizedStuffDefs.ContainsKey(category))
                            {
                                categorizedStuffDefs[category] = new List<ThingDef>();
                            }
                            categorizedStuffDefs[category].Add(def);
                        }
                    }
                }
            }

            List<ThingDef> matchingDefs = categorizedStuffDefs
                .Where(pair => categories.Contains(pair.Key))
                .SelectMany(pair => pair.Value)
                .ToList();

            List<Color> colors = matchingDefs
                .Select(def => StuffColorUtility.GetStuffColor(def))
                .Where(color => color != Color.white)
                .ToList();

            if (colors.Count > 0)
            {
                return colors[random.Next(colors.Count)];
            }
            else
            {
                return Color.white;
            }
        }
    }
}