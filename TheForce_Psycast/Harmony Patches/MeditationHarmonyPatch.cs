using HarmonyLib;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Verse;

namespace TheForce_Psycast.Harmony_Patches
{
    public class Base : Mod
    {
        public static List<ThingDef> AllMeditationSpots;

        public Base(ModContentPack content) : base(content)
        {
            HarmonyPatches.harmonyPatch.Patch(
                AccessTools.Method(AccessTools.FirstInner(typeof(MeditationUtility),
                    type => type.Name.Contains("AllMeditationSpotCandidates") && typeof(IEnumerator).IsAssignableFrom(type)), "MoveNext"),
                transpiler: new HarmonyMethod(typeof(Base), nameof(Transpiler)));
            LongEventHandler.ExecuteWhenFinished(() =>
            {
                AllMeditationSpots = new List<ThingDef> { ThingDefOf.MeditationSpot };
                foreach (var def in DefDatabase<ThingDef>.AllDefs)
                    if (def.HasModExtension<MeditationBuilding_Alignment>())
                        AllMeditationSpots.Add(def);
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<ThingDef, IEnumerable<Thing>> AllOnMapOfPawnWithDef(Pawn pawn) => def => pawn.Map.listerBuildings.AllBuildingsColonistOfDef(def);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<Thing> AllMeditationSpotsForPawn(Pawn pawn) => AllMeditationSpots.SelectMany(AllOnMapOfPawnWithDef(pawn));

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var list = instructions.ToList();
            var info1 = AccessTools.Method(typeof(ListerBuildings), nameof(ListerBuildings.AllBuildingsColonistOfDef));
            var idx1 = list.FindIndex(ins => ins.Calls(info1)) - 3;
            list.RemoveRange(idx1, 4);
            list.Insert(idx1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Base), nameof(AllMeditationSpotsForPawn))));
            return list;
        }
    }

    [HarmonyPatch(typeof(Pawn_PsychicEntropyTracker))]
    [HarmonyPatch("GainPsyfocus")]
    public static class GainPsyfocus_Patch
    {
        public static void Postfix(Pawn_PsychicEntropyTracker __instance)
        {
            Pawn pawn = __instance.Pawn;
            if (pawn != null && pawn.health != null && pawn.Map != null && pawn.Map.listerBuildings != null)
            {
                Hediff hediff = null;
                Hediff hediff2 = null;
                foreach (var def in Base.AllMeditationSpots)
                {
                    var buildings = pawn.Map.listerBuildings.AllBuildingsColonistOfDef(def);
                    if (buildings != null && buildings.Any())
                    {
                        var alignment = def.GetModExtension<MeditationBuilding_Alignment>();
                        if (alignment != null)
                        {
                            hediff = pawn.health.hediffSet.GetFirstHediffOfDef(alignment.hedifftoIncrease);
                            if (hediff != null)
                            {
                                // Increase the severity of the hediff
                                hediff.Severity += MeditationUtility.PsyfocusGainPerTick(pawn);
                            }

                            hediff2 = pawn.health.hediffSet.GetFirstHediffOfDef(alignment.hedifftoDecrease);
                            if (hediff2 != null)
                            {
                                // Decrease the severity of the hediff
                                hediff2.Severity -= MeditationUtility.PsyfocusGainPerTick(pawn);
                            }
                        }
                    }
                }
            }
            else
            {
                return;
            }
        }
    }
}
