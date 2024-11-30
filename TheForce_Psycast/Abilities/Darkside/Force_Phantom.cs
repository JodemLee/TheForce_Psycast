    using RimWorld;
    using RimWorld.Planet;
    using System.Collections.Generic;
    using Verse;
    using Ability = VFECore.Abilities.Ability;

    namespace TheForce_Psycast.Abilities.Darkside
    {
        internal class Force_Phantom : Ability
        {
            public override void Cast(params GlobalTargetInfo[] targets)
            {
                foreach (var target in targets)
                {
                if (target.Pawn.ageTracker.Adult)
                {
                    Pawn copy = Find.PawnDuplicator.Duplicate(target.Pawn);
                    GenSpawn.Spawn(copy, target.Cell, CasterPawn.Map);
                    var hediff = HediffMaker.MakeHediff(ForceDefOf.Force_Phantom, copy);
                    copy.health.AddHediff(hediff);
                    var stance = copy.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Lightsaber_Stance);
                    if (stance != null)
                    {
                        copy.health.RemoveHediff(stance);
                    }

                    // Copy apparel
                    if (CasterPawn.apparel != null)
                    {
                        foreach (Apparel apparel in CasterPawn.apparel.WornApparel)
                        {
                            Apparel newApparel = (Apparel)ThingMaker.MakeThing(apparel.def, apparel.Stuff);
                            newApparel.HitPoints = apparel.HitPoints;
                            copy.apparel.Wear(newApparel);
                            copy.apparel.LockAll();
                        }
                    }

                    if (copy is Pawn pawn)
                    {
                        // Set the pawn's entire timetable to Work
                        for (int h = 0; h < 24; h++)
                        {
                            pawn.timetable.SetAssignment(h, TimeAssignmentDefOf.Work);
                        }

                        // List of WorkTypeDefs to copy
                        List<WorkTypeDef> workTypeDefs = new List<WorkTypeDef>
                        {
                            WorkTypeDefOf.Childcare,
                            WorkTypeDefOf.Handling,
                            WorkTypeDefOf.Doctor,
                            WorkTypeDefOf.Construction,
                            WorkTypeDefOf.Growing,
                            WorkTypeDefOf.Mining,
                            WorkTypeDefOf.Cleaning,
                            WorkTypeDefOf.Crafting,
                            WorkTypeDefOf.DarkStudy,
                            WorkTypeDefOf.Firefighter,
                            WorkTypeDefOf.Hauling,
                            WorkTypeDefOf.Hunting,
                            WorkTypeDefOf.PlantCutting,
                            WorkTypeDefOf.Research,
                            WorkTypeDefOf.Smithing,
                            WorkTypeDefOf.Warden
                        };

                        // Loop through each WorkTypeDef
                        foreach (WorkTypeDef work in workTypeDefs)
                        {
                            // Check if the pawn is capable of the work
                            if (pawn.workSettings != null && pawn.workSettings.WorkIsActive(work))
                            {
                                // Copy work priority if the CasterPawn can do the work and the pawn can as well
                                int casterPriority = CasterPawn.workSettings.GetPriority(work);
                                if (casterPriority > 0)  // Ensure valid priority (0 = cannot do the work)
                                {
                                    pawn.workSettings.SetPriority(work, casterPriority);
                                }
                            }
                        }
                    }
                }
            }
        }
                   }

            internal class HediffComp_Vanish : HediffComp
            {
                public HediffCompProperties_Vanish Props => (HediffCompProperties_Vanish)props;

                public override void CompPostPostRemoved()
                {
                    base.CompPostPostRemoved();
                    HandleVanish();
                }

                public override void Notify_PawnKilled()
                {
                    base.Notify_PawnKilled();
                    HandleVanish();
                }

                private void HandleVanish()
                {
                    if (parent.pawn != null && !parent.pawn.Destroyed)
                    {
                        if (Props.vanish)
                        {
                            // Schedule destruction on the next tick instead of immediately
                            Find.TickManager.slower.SignalForceNormalSpeedShort(); // Optional: forces a tick event
                            LongEventHandler.ExecuteWhenFinished(() =>
                            {
                                if (parent.pawn != null && !parent.pawn.Destroyed)
                                {
                                    parent.pawn.Destroy(DestroyMode.Vanish);
                                    if (Find.WorldPawns.Contains(parent.pawn))
                                    {
                                        Find.WorldPawns.RemovePawn(parent.pawn);
                                    }
                                }
                            });
                        }
                        if(!Props.vanish)
                {
                            parent.pawn.Kill(null);
                        }
                    }
                }
            }

            public class HediffCompProperties_Vanish : HediffCompProperties
            {
                public bool vanish;

                public HediffCompProperties_Vanish()
                {
                    compClass = typeof(HediffComp_Vanish);
                }
            }
        }
