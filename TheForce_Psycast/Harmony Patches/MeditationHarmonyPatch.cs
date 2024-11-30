using HarmonyLib;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using Verse.AI;

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

    [HarmonyPatch(typeof(JobDriver_Meditate), "MeditationTick")]
    public static class Patch_JobDriver_Meditate_MeditationTick
    {
        // Prefix for starting the animation during meditation
        [HarmonyPrefix]
        public static void Prefix(JobDriver_Meditate __instance)
        {
            Pawn pawn = __instance.pawn;


            if (ModsConfig.RoyaltyActive && pawn.HasPsylink)
            {
                // Start the meditation animation if it's not already playing
                AnimationDef meditationAnimation = DefDatabase<AnimationDef>.GetNamed("Force_FloatingMeditation", false);

                if (meditationAnimation != null)
                {
                    if (pawn.Drawer.renderer.CurAnimation != meditationAnimation)
                    {
                        pawn.Drawer.renderer.SetAnimation(meditationAnimation);
                    }
                }
                if (pawn.IsHashIntervalTick(100))
                {
                    IEnumerable<IntVec3> adjacentCells = GenAdj.CellsAdjacent8Way(pawn).Where(cell => cell.InBounds(pawn.Map));
                    List<IntVec3> validCells = adjacentCells.Where(cell => cell.Walkable(pawn.Map)).ToList();
                    IntVec3 randomCell = validCells.RandomElement();
                    float randomRotation = Rand.Range(0f, 360f);
                    float randomSize = Rand.Range(1f, 2.5f);

                    FleckCreationData fleckData = FleckMaker.GetDataStatic(randomCell.ToVector3(), pawn.Map, ForceDefOf.Force_FleckStone, randomSize); ;
                    fleckData.rotation = randomRotation;
                    pawn.Map.flecks.CreateFleck(fleckData);
                }
            }
        }
    }

    // A separate patch to stop the animation when the job ends
    [HarmonyPatch(typeof(JobDriver), "Cleanup")]
    public static class Patch_JobDriver_Notify_JobEnded
    {
        // Postfix for stopping the animation when the meditation job ends
        [HarmonyPostfix]
        public static bool Prefix(JobDriver __instance)
        {
            // Check if the job is a meditation job
            if (__instance is JobDriver_Meditate)
            {
                Pawn pawn = __instance.pawn;

                if (ModsConfig.RoyaltyActive && pawn.HasPsylink)
                {
                    // Stop the meditation animation when the job ends
                    AnimationDef meditationAnimation = DefDatabase<AnimationDef>.GetNamed("Force_FloatingMeditation", false);

                    if (meditationAnimation != null && pawn.Drawer.renderer.CurAnimation == meditationAnimation)
                    {
                        // Stop the animation
                        pawn.Drawer.renderer.SetAnimation(null);
                    }
                }
            }
            return true;
        }
    }
}