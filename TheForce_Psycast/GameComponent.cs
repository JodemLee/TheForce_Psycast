using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Sound;

namespace TheForce_Psycast
{
    internal class Force_Ghost_Component : GameComponent, IThingHolder
    {

        public static Force_Ghost_Component Instance;
        public ThingOwner innerContainer = null;
        protected bool contentsKnown;
        public int tickCounter = 0;
        public int tickInterval = thirtyDaysInTicks;
        public int deadPawnsTickCounter = 0;
        public int deadPawnsTickInterval = 100;
        public List<Pawn> listOfPawnsThatDied = new List<Pawn>();

       // public const int thirtyDaysInTicks = 1800000;
       // public const int sixtyDaysInTicks = 3600000;

        // For testing purposes
        public const int thirtyDaysInTicks = 1800;
        public const int sixtyDaysInTicks = 3600;
        public IThingHolder ParentHolder => null;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look<ThingOwner>(ref this.innerContainer, "innerContainer", new object[] { this });
            Scribe_Values.Look<int>(ref tickInterval, "tickInterval");
            Scribe_Values.Look<int>(ref tickCounter, "tickCounter");
            Scribe_Values.Look<int>(ref deadPawnsTickCounter, "deadPawnsTickCounter");

            Scribe_Collections.Look(ref listOfPawnsThatDied, "listOfPawnsThatDied", LookMode.Reference);
        }
        public Force_Ghost_Component()
        {
            this.innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
            Instance = this;
        }
        public Force_Ghost_Component(Game game) : base()
        {
            this.innerContainer = new ThingOwner<Thing>(this, false, LookMode.Deep);
            Instance = this;
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
            bool flag;
            if (thing.holdingOwner != null)
            {
                thing.holdingOwner.Remove(thing);
                this.innerContainer.TryAdd(thing, thing.stackCount, false);
                flag = true;
            }
            else
            {
                flag = this.innerContainer.TryAdd(thing, true);
            }
            if (flag)
            {
                if (thing.Faction != null && thing.Faction.IsPlayer)
                {
                    this.contentsKnown = true;
                }
                return true;
            }
            return false;
        }

        
        public override void GameComponentTick()
        {
            tickCounter++;

            if ((tickCounter > tickInterval))
            {
                if (innerContainer?.Count > 0)
                {
                    Pawn pawn = innerContainer.RandomElement() as Pawn;
                    if (pawn != null)
                    {
                        Map map = Find.AnyPlayerHomeMap;
                        if (map != null)
                        {
                            TryFindEntryCell(map, out IntVec3 cell);
                            if (cell != null)
                            {
                                GenSpawn.Spawn(pawn, cell, map);
                                ChoiceLetter let = LetterMaker.MakeLetter("Force_Ghost".Translate(pawn.Name), "Force_GhostReturns".Translate(pawn.Name), LetterDefOf.PositiveEvent, pawn);
                                Find.LetterStack.ReceiveLetter(let);
                                SoundDefOf.Psycast_Skip_Exit.PlayOneShot(pawn);
                            }
                        }


                    }

                }
                tickInterval = new IntRange(thirtyDaysInTicks, sixtyDaysInTicks).RandomInRange;
                tickCounter = 0;
            }

            deadPawnsTickCounter++;

            if ((deadPawnsTickCounter > deadPawnsTickInterval))
            {
                if (listOfPawnsThatDied.Count > 0)
                {
                    List<Pawn> tempList = new List<Pawn>();

                    foreach (Pawn pawn in listOfPawnsThatDied)
                    {
                        if (pawn != null)
                        {
                            tempList.Add(pawn);
                        }

                    }

                    foreach (Pawn pawn in tempList)
                    {
                        if (pawn?.Corpse?.Map != null)
                        {

                            ResurrectionUtility.Resurrect(pawn);
                            Hediff yourHediff = HediffMaker.MakeHediff(ForceDefOf.ForceGhost, pawn);
                            pawn.health.AddHediff(yourHediff);

                            Hediff hediffToRemove = pawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.ForceGhostChance);
                            if (hediffToRemove != null)
                            {
                                pawn.health.RemoveHediff(hediffToRemove);
                            }

                            Trait yourTrait = new Trait(ForceDefOf.Force_ForceGhost);
                            pawn.story.traits.GainTrait(yourTrait);


                            listOfPawnsThatDied.Remove(pawn);
                            if (pawn.Faction != null && pawn.Faction.IsPlayer == true && !pawn.Dead)
                            {
                                TryAcceptThing(pawn);
                            }
                            else pawn.Destroy();
                        }



                    }


                }

                deadPawnsTickCounter = 0;
            }

        }

    
        private bool TryFindEntryCell(Map map, out IntVec3 cell)
        {
            return CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => map.reachability.CanReachColony(c) && !c.Fogged(map), map, CellFinder.EdgeRoadChance_Neutral, out cell);
        }
    }

}