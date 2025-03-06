using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using VanillaPsycastsExpanded;
using Verse;
using VFECore.Abilities;

namespace TheForce_Psycast.Abilities.Sith_Sorcery
{
    internal class SithRitual_DrainEssence : Ability_WriteCombatLog
    {
        private const float DrainAmount = 0.02f;  // Amount of energy drained per tick
        private const int DurationTicks = 600;  // Duration of the ability in ticks (10 seconds)

        // This keeps track of whether the drain should be applied periodically
        public int TicksLeft { get; private set; } = DurationTicks;
        public bool ShouldDrain { get; private set; } = false;
        private List<Pawn> enemyPawnsToDrain = new List<Pawn>();  // List to track which enemies to drain over time

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);

            // Play a visual effect on casting
            MoteMaker.MakeStaticMote(Caster.Position, Caster.Map, ThingDefOf.Mote_CastPsycast, 1.5f);

            // Ensure targets are valid
            if (targets == null || targets.Length == 0)
            {
                Log.Warning("No targets found for SithRitual_DrainEssence.");
                return;
            }

            // Set ShouldDrain to true to start the periodic drain
            

            // Loop through all the targeted enemies
            foreach (var target in targets)
            {
                if (target.Thing is Pawn enemy && enemy.Faction != this.pawn.Faction)
                {
                    // Create and spawn the transfer orb (visual effect between caster and target)
                    MoteBetween mote = (MoteBetween)ThingMaker.MakeThing(VPE_DefOf.VPE_SoulOrbTransfer);
                    mote.Attach(target.Thing, this.pawn);  // Attach to enemy and caster
                    mote.exactPosition = target.Thing.DrawPos;  // Position of the target
                    GenSpawn.Spawn(mote, target.Thing.Position, this.pawn.Map);

                    // Add the enemy pawn to the list for periodic draining
                    enemyPawnsToDrain.Add(enemy);
                    ShouldDrain = true;
                }
            }
        }

        private void ApplyDrainEffect(Pawn enemy)
        {
            // Check if the enemy has Rest and Food needs and apply drain
            if (enemy.needs?.rest != null)
            {
                enemy.needs.rest.CurLevel -= DrainAmount;
            }

            if (enemy.needs?.food != null)
            {
                enemy.needs.food.CurLevel -= DrainAmount;
            }

            // Increase the caster's Rest as a benefit
            if (this.pawn.needs?.rest != null)
            {
                this.pawn.needs.rest.CurLevel += DrainAmount / 2;
            }
        }

        // Periodically drain energy from the enemies in range
        public override void Tick()
        {
            base.Tick();

            if (ShouldDrain && TicksLeft > 0)
            {
                TicksLeft--;

                // Every 60 ticks (1 second), apply the drain to all enemy pawns
                if (TicksLeft % 60 == 0)
                {
                    foreach (var enemy in enemyPawnsToDrain)
                    {
                        ApplyDrainEffect(enemy);
                    }
                }

                // Once the duration is over, stop the drain
                if (TicksLeft == 0)
                {
                    ShouldDrain = false;
                    enemyPawnsToDrain.Clear();  // Clear the list of drained pawns
                }
            }
        }
    }
}
