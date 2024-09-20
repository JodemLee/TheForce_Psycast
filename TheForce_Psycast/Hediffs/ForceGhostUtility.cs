using UnityEngine;
using Verse;

namespace TheForce_Psycast.Hediffs
{
    public static class ForceGhostUtility
    {
        public static Color? GetGhostColor(Pawn pawn)
        {
            if (pawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Ghost) != null)
            {
                return new Color(0.5f, 0.5f, 1f, 0.7f); // Light transparent blue for Force Ghost
            }
            if (pawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_SithGhost) != null)
            {
                return new Color(1f, 0f, 0f, 0.7f); // Light transparent red for Darkside Ghost
            }
            if (pawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Sithzombie) != null)
            {
                return new Color(0.8f, 0.8f, 0.8f, 1f);
            }
            return null;
        }

        public static bool IsForceGhost(Pawn pawn)
        {
            return GetGhostColor(pawn) != null;
        }
    }
}
