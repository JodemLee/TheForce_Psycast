using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TheForce_Psycast.Yuuzhan_Vong
{
    public class ThingClass_WeaponShift : ThingWithComps, IThingHolder
    {
        public ThingOwner<Thing> innerContainer;

        public ThingClass_WeaponShift()
        {
            innerContainer = new ThingOwner<Thing>(this, oneStackOnly: false);
        }

        public bool TryAcceptPawn(Pawn pawn)
        {
            if (innerContainer.CanAcceptAnyOf(pawn))
            {
                if (pawn.Spawned)
                {
                    pawn.DeSpawn();
                }
                return innerContainer.TryAdd(pawn);
            }
            return false;
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
        }

        public override void TickRare()
        {
            base.TickRare();
            innerContainer.ThingOwnerTickRare();
        }

        public override void Tick()
        {
            base.Tick();
            innerContainer.ThingOwnerTick();
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            Map map = Map;

            // Check if the Thing is being destroyed and handle pawn inside
            HandlePawnOnDestroy(mode);

            base.Destroy(mode);

            if (innerContainer.Count > 0 && (mode == DestroyMode.Deconstruct || mode == DestroyMode.KillFinalize))
            {
                innerContainer.TryDropAll(Position, map, ThingPlaceMode.Near);
            }
            innerContainer.ClearAndDestroyContents();
        }

        private void HandlePawnOnDestroy(DestroyMode mode)
        {
            if (innerContainer.Count > 0)
            {
                Thing animalThing = innerContainer[0];
                Pawn animal = animalThing as Pawn;

                if (animal != null)
                {
                    if (mode == DestroyMode.KillFinalize || mode == DestroyMode.Deconstruct)
                    {
                        animal.Kill(null); // Kill the pawn
                        GenSpawn.Spawn(animal.Corpse, Position, Map); // Spawn the corpse
                    }
                    else
                    {
                        innerContainer.Remove(animal);
                        GenSpawn.Spawn(animal, Position, Map);
                    }
                }
            }
        }

        public void RevertToAnimal()
        {
            if (innerContainer.Count > 0)
            {
                Thing animalThing = innerContainer[0];
                Pawn animal = animalThing as Pawn;

                if (animal != null)
                {
                    IntVec3 position = this.Position;
                    Map map = this.Map;

                    innerContainer.Remove(animal);
                    GenSpawn.Spawn(animal, position, map);
                    this.Destroy(DestroyMode.Vanish);

                    Messages.Message(animal.Label + " has been reverted back to an animal!", MessageTypeDefOf.PositiveEvent);
                }
                else
                {
                    Log.Error("No animal found in the inner container to revert.");
                }
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            if (innerContainer.Count > 0)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Revert to Animal",
                    defaultDesc = "Revert the weapon back to the animal.",
                    icon = ContentFinder<Texture2D>.Get("Path/To/Your/Icon", true),
                    action = RevertToAnimal
                };
            }
        }
    }
}
