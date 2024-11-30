using HarmonyLib;
using System.Reflection;
using Verse;

namespace TheForce_Psycast
{
    [StaticConstructorOnStartup]
    public static class NonPublicFields
    {

        public static FieldInfo Projectile_ticksToImpact = AccessTools.Field(typeof(Projectile), "ticksToImpact");
        public static FieldInfo Projectile_origin = AccessTools.Field(typeof(Projectile), "origin");
        public static FieldInfo Projectile_destination = AccessTools.Field(typeof(Projectile), "destination");
        public static FieldInfo Projectile_usedTarget = AccessTools.Field(typeof(Projectile), "usedTarget");
        
    }

}
