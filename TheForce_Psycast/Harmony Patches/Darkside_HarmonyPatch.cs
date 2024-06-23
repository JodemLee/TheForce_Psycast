//using HarmonyLib;
//using RimWorld;
//using System;
//using System.Collections.Generic;
//using Verse;
//using VFECore.Abilities;

//namespace TheForce_Psycast.Harmony_Patches
//{
//    [HarmonyPatch(typeof(Pawn), "Kill")]
//    public class Darkside_Patch
//    {
//        public static bool IncreaseDarksideOnKill { get; set; }

//        private static void Postfix(Pawn __instance, DamageInfo? dinfo)
//        {
//            IncreaseDarksideOnKill = Force_ModSettings.IncreaseDarksideOnKill;
//            if (IncreaseDarksideOnKill)
//            {
//                if (__instance.Dead)
//                {
//                    if (dinfo.HasValue && dinfo.Value.Instigator is Pawn attacker)
//                    {
//                        // Check if attacker is hostile to __instance
//                        if (__instance.HostileTo(attacker))
//                        {
//                            var hediff = attacker.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Darkside);
//                            if (hediff != null)
//                            {
//                                hediff.Severity += 0.01f; // Increase the severity by 0.1
//                            }
//                        }
//                    }
//                }
//            }
//        }
//    }
//}
        
