using HarmonyLib;

namespace TheForce_Psycast.Holding
{
    using global::TheForce_Psycast.Holding.TheForce_Psycast.Holding;
    using RimWorld;
    using System.Collections.Generic;
    using System.Linq;
    using Verse;
    using Verse.AI.Group;

    namespace TheForce_Psycast.Holding
    {
        public class GameComponent_NemesisTracker : GameComponent, IThingHolder
        {
            public static GameComponent_NemesisTracker Instance;
            public ThingOwner innerContainer = null;
            public int tickCounter = 0;
            public int tickInterval = 60000; // 1 in-game day in ticks

            public const int thirtyDaysInTicks = 60000 * 30; // 30 in-game days in ticks

            public IThingHolder ParentHolder => null;

            public GameComponent_NemesisTracker()
            {
                this.innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
                Instance = this;
            }

            public GameComponent_NemesisTracker(Game game) : base()
            {
                this.innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
                Instance = this;
            }

            public override void ExposeData()
            {
                base.ExposeData();
                Scribe_Deep.Look(ref this.innerContainer, "innerContainer", new object[] { this });
                Scribe_Values.Look(ref tickInterval, "tickInterval");
                Scribe_Values.Look(ref tickCounter, "tickCounter");
            }

            public ThingOwner GetDirectlyHeldThings()
            {
                return this.innerContainer;
            }

            public void GetChildHolders(List<IThingHolder> outChildren)
            {
                ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, this.GetDirectlyHeldThings());
            }

            public bool Accepts(Thing thing)
            {
                return this.innerContainer.CanAcceptAnyOf(thing, false);
            }

            public bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
            {
                if (!this.Accepts(thing))
                {
                    return false;
                }

                bool result;
                if (thing.holdingOwner != null)
                {
                    thing.holdingOwner.Remove(thing);
                    this.innerContainer.TryAdd(thing, thing.stackCount, false);
                    result = true;
                }
                else
                {
                    result = this.innerContainer.TryAdd(thing, true);
                }

                if (result)
                {
                    Log.Message($"Added {thing.Label} to innerContainer.");
                }

                return result;
            }

            public override void GameComponentTick()
            {
                tickCounter++;
                if (tickCounter > tickInterval)
                {
                    tickCounter = 0;

                    // Resurrect one pawn per tick interval
                    foreach (Thing thing in innerContainer)
                    {
                        if (thing is Pawn pawn && pawn.Dead)
                        {
                            Log.Message($"Attempting to resurrect {pawn.Name} from innerContainer.");
                            ResurrectionUtility.TryResurrect(pawn);
                            if (!pawn.Dead)
                            {
                                // Heal non-permanent injuries
                                HealNonPermanentInjuries(pawn);

                                // Reset the pawn's Lord
                                pawn.GetLord()?.RemovePawn(pawn);
                                Log.Message($"Resurrected {pawn.Name}, healed injuries, and reset Lord.");
                            }
                            else
                            {
                                Log.Error($"Failed to resurrect {pawn.Name}.");
                            }
                            break; // Resurrect only one pawn per tick
                        }
                    }
                }
            }

            public static void HealNonPermanentInjuries(Pawn pawn)
            {
                if (pawn == null || pawn.health == null || pawn.health.hediffSet == null)
                {
                    Log.Error("Pawn or health components are null.");
                    return;
                }

                // Iterate through all hediffs and remove non-permanent injuries
                List<Hediff> hediffsToRemove = new List<Hediff>();
                foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                {
                    // Check if the hediff is a non-permanent injury
                    if (hediff is Hediff_Injury && !hediff.IsPermanent())
                    {
                        hediffsToRemove.Add(hediff);
                    }
                }

                // Remove the hediffs
                foreach (Hediff hediff in hediffsToRemove)
                {
                    pawn.health.RemoveHediff(hediff);
                    Log.Message($"Healed {hediff.Label} from {pawn.Name}.");
                }

                Log.Message($"Finished healing non-permanent injuries for {pawn.Name}.");
            }
        }

        // Data structure for nemesis pawns
        public class NemesisData : IExposable
        {
            public Pawn Killer;
            public NemesisRank Rank;
            public int DeathCount;
            public bool IsDead;

            public void ExposeData()
            {
                Scribe_References.Look(ref Killer, "killer");
                Scribe_Values.Look(ref Rank, "rank");
                Scribe_Values.Look(ref DeathCount, "deathCount");
                Scribe_Values.Look(ref IsDead, "isDead");
            }
        }

        public enum NemesisRank { Grunt, Captain, Warlord }



        [HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
        public static class Patch_Pawn_Kill
        {
            public static bool Prefix(Pawn __instance, DamageInfo? dinfo)
            {
                GameComponent_NemesisTracker tracker = Current.Game.GetComponent<GameComponent_NemesisTracker>();
                if (tracker != null && __instance.RaceProps.Humanlike && __instance.Faction?.HostileTo(Faction.OfPlayer) == true)
                {
                    if (__instance.Spawned && !__instance.Dead)
                    {
                        __instance.DeSpawn();
                        tracker.TryAcceptThing(__instance);
                        Log.Message($"Despawned and added {__instance.Name} to innerContainer.");
                    }
                    return false; // Skip the original Kill method
                }
                return true; // Continue with the original Kill method
            }
        }

        [HarmonyPatch(typeof(RaidStrategyWorker), nameof(RaidStrategyWorker.SpawnThreats))]
        public static class Patch_RaidStrategyWorker_SpawnThreats
        {
            public static bool Prefix(RaidStrategyWorker __instance, IncidentParms parms, ref List<Pawn> __result)
            {
                GameComponent_NemesisTracker tracker = Current.Game.GetComponent<GameComponent_NemesisTracker>();
                if (tracker == null)
                {
                    Log.Error("NemesisTracker not found in the game.");
                    return true; // Continue with the original method
                }

                List<Pawn> raidPawns = new List<Pawn>();

                // Log the contents of the innerContainer
                Log.Message($"InnerContainer contents: {tracker.innerContainer.Count} items.");
                foreach (Thing thing in tracker.innerContainer.ToList()) // Use ToList() to avoid modifying the collection while iterating
                {
                    if (thing is Pawn pawn)
                    {
                        Log.Message($"Checking pawn {pawn.Name} for raid eligibility.");
                        Log.Message($"Faction: {pawn.Faction?.Name}, Hostile to player: {pawn.Faction?.HostileTo(Faction.OfPlayer)}");
                        Log.Message($"Lord: {pawn.GetLord() != null}, Dead: {pawn.Dead}");

                        if (pawn.Faction?.HostileTo(Faction.OfPlayer) == true && !pawn.Dead)
                        {

                            // Reset the pawn's Lord if it's part of a group
                            if (pawn.GetLord() != null)
                            {
                                Log.Message($"Resetting Lord for {pawn.Name}.");
                                pawn.GetLord()?.RemovePawn(pawn);
                            }

                            // Notify the player
                            NotifyNemesisInRaid(pawn);

                            Log.Message($"Adding {pawn.Name} to raid.");
                            raidPawns.Add(pawn);
                            tracker.innerContainer.Remove(pawn); // Remove the pawn from storage
                            Log.Message($"Added stored pawn {pawn.Name} to raid.");
                        }
                    }
                }

                // If no stored pawns were added, continue with the original method
                if (raidPawns.Count == 0)
                {
                    Log.Message("No stored nemesis pawns to resummon.");
                    return true; // Continue with the original method
                }

                // Add the custom raid pawns to the raid
                parms.raidArrivalMode.Worker.Arrive(raidPawns, parms);
                __result = raidPawns; // Set the result to the custom raid pawns
                Log.Message($"Spawned {raidPawns.Count} nemesis pawns in raid.");
                return false; // Skip the original method
            }


            public static void NotifyNemesisInRaid(Pawn pawn)
            {
                if (pawn == null)
                {
                    Log.Error("Pawn is null.");
                    return;
                }

                // Create the letter
                string label = "Nemesis Returns";
                string text = $"{pawn.Name} has returned to attack your colony!";
                LetterDef letterDef = LetterDefOf.ThreatBig; // Use a threatening letter type

                // Send the letter and move the camera
                Find.LetterStack.ReceiveLetter(label, text, letterDef, new LookTargets(pawn));
                CameraJumper.TryJump(pawn);

                Log.Message($"Sent raid alert for {pawn.Name}.");
            }
        }
    }
}



