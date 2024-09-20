using CombatExtended;
using HarmonyLib;
using System.Reflection;
using Verse;

namespace Force_CEPatch
{
    [StaticConstructorOnStartup]
    public static class NonPublicFields
    {

        public static FieldInfo Projectile_ticksToImpact = AccessTools.Field(typeof(ProjectileCE), "ticksToImpact");
        public static FieldInfo Projectile_origin = AccessTools.Field(typeof(ProjectileCE), "origin");
        public static FieldInfo Projectile_destination = AccessTools.Field(typeof(ProjectileCE), "destination");
        public static FieldInfo Projectile_usedTarget = AccessTools.Field(typeof(ProjectileCE), "usedTarget");
    }

}
