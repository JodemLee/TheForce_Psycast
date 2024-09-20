using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using TheForce_Psycast.Abilities.Sith_Sorcery;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace TheForce_Psycast
{
    internal class SithLightningStrike : Ability
    {

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            foreach (GlobalTargetInfo target in targets)
            {
                foreach (Thing thing in target.Cell.GetThingList(target.Map).ListFullCopy())
                    thing.TakeDamage(new DamageInfo(ForceDefOf.Force_Lightning, def.power, -1f, this.pawn.DrawPos.AngleToFlat(thing.DrawPos), this.pawn));
                GenExplosion.DoExplosion(target.Cell, target.Map, this.GetRadiusForPawn(), ForceDefOf.Force_Lightning, this.pawn);
                this.pawn.Map.weatherManager.eventHandler.AddEvent(new SithLightningEvent(this.pawn.Map, target.Cell, UnityEngine.Color.red));
            }
        }

    }

    internal class DarkLightningChain : Ability
    {
        private const int BaseDamage = 25;
        private const int BaseStunDuration = 60;
        private const int DamageIncreasePerChain = 15;
        private const int MaxChains = 3;
        private const float ChainRadius = 4.5f;
        private const int DelayTicks = 30;

        private int currentChain;
        private IntVec3 currentCell;
        private Map map;
        private int lastCastTick;
        private bool isCasting;
        private HashSet<Pawn> hitPawns; // Track pawns already hit

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);

            if (targets.Length == 0) return;

            GlobalTargetInfo initialTarget = targets[0];
            currentCell = initialTarget.Cell;
            map = initialTarget.Map;
            currentChain = 0;
            lastCastTick = GenTicks.TicksGame;
            isCasting = true;
            hitPawns = new HashSet<Pawn>(); // Initialize the hitPawns set

            // Start the chaining process
            ApplyLightningEffect();
        }

        public override void Tick()
        {
            base.Tick();

            if (!isCasting) return;

            if (GenTicks.TicksGame >= lastCastTick + DelayTicks)
            {
                ApplyLightningEffect();

                if (currentChain >= MaxChains)
                {
                    isCasting = false; // Stop casting if the max chains are reached
                }
                else
                {
                    FindNextTarget();
                    currentChain++;
                    lastCastTick = GenTicks.TicksGame; // Reset delay
                }
            }
        }

        private void ApplyLightningEffect()
        {
            // Apply damage and stun to all things in the current cell
            foreach (Thing thing in currentCell.GetThingList(map).ListFullCopy())
            {
                if (thing.Faction == this.pawn.Faction)
                    continue;

                int damage = BaseDamage + (DamageIncreasePerChain * currentChain); // Increase damage with each chain
                thing.TakeDamage(new DamageInfo(ForceDefOf.Force_Lightning, damage, -1f, this.pawn.DrawPos.AngleToFlat(thing.DrawPos), this.pawn));

                // Apply stun effect to pawns
                if (thing is Pawn pawn)
                {
                    hitPawns.Add(pawn); // Add pawn to the hit list
                    pawn.stances.stunner.StunFor(BaseStunDuration, this.pawn);
                }
            }

            // Create explosion and weather event
            GenExplosion.DoExplosion(currentCell, map, GetRadiusForPawn(), ForceDefOf.Force_Lightning, this.pawn);
            this.pawn.Map.weatherManager.eventHandler.AddEvent(new SithLightningEvent(this.pawn.Map,currentCell, UnityEngine.Color.red));
        }

        private void FindNextTarget()
        {
            List<Thing> potentialTargets = GenRadial.RadialDistinctThingsAround(currentCell, map, def.radius, true)
                .Where(t => (t is Pawn) && t.Faction != this.pawn.Faction).ToList();

            // Exclude already-hit pawns, but if no new targets are available, allow previously-hit ones
            List<Thing> newTargets = potentialTargets.Where(t => !(t is Pawn) || !hitPawns.Contains((Pawn)t)).ToList();

            if (newTargets.Count > 0)
            {
                Thing nextTarget = newTargets.RandomElement();
                currentCell = nextTarget.Position;
            }
            else if (potentialTargets.Count > 0)
            {
                Thing nextTarget = potentialTargets.RandomElement();
                currentCell = nextTarget.Position;
            }
        }
    }
}