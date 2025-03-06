using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace TheForce_Psycast.Abilities.Darkside
{
    internal class Force_Phantom : Ability_WriteCombatLog
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            foreach (var target in targets)
            {
                if (target.Pawn.ageTracker.Adult)
                {
                    Pawn copy = PawnCloningUtility.Duplicate(target.Pawn);
                    GenSpawn.Spawn(copy, target.Cell, CasterPawn.Map);
                    var hediff = HediffMaker.MakeHediff(ForceDefOf.Force_Phantom, copy);
                    copy.health.AddHediff(hediff);
                    var stance = copy.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Lightsaber_Stance);
                    if (stance != null)
                    {
                        copy.health.RemoveHediff(stance);
                    }

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
                        for (int h = 0; h < 24; h++)
                        {
                            pawn.timetable.SetAssignment(h, TimeAssignmentDefOf.Work);
                        }

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

                        foreach (WorkTypeDef work in workTypeDefs)
                        {
                            if (pawn.workSettings != null && pawn.workSettings.WorkIsActive(work))
                            {
                                int casterPriority = CasterPawn.workSettings.GetPriority(work);
                                if (casterPriority > 0)
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
                    if (parent.pawn.apparel != null)
                    {
                        List<Apparel> wornApparel = new List<Apparel>(parent.pawn.apparel.WornApparel);
                        foreach (Apparel apparel in wornApparel)
                        {
                            parent.pawn.apparel.Remove(apparel);
                            GenSpawn.Spawn(apparel, parent.pawn.Position, parent.pawn.Map);
                        }
                    }

                    Find.TickManager.slower.SignalForceNormalSpeedShort();
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
                else
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