using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TheForce_Psycast.Yuuzhan_Vong
{
    internal class WeaponShift_ThingComp : ThingComp, IThingHolder
    {
        private ThingOwner<Thing> weaponContainer;

        public WeaponShift_ThingComp()
        {
            weaponContainer = new ThingOwner<Thing>(this, oneStackOnly: true);
        }

        public CompProperties_WeaponShift Props => (CompProperties_WeaponShift)this.props;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo g in base.CompGetGizmosExtra())
            {
                yield return g;
            }

            yield return new Command_Action
            {
                defaultLabel = "Transform into Weapon",
                defaultDesc = "Transform this animal into a weapon.",
                icon = ContentFinder<Texture2D>.Get("Path/To/Your/Icon", true),
                action = delegate
                {
                    TransformAnimal();
                }
            };
        }

        private void TransformAnimal()
        {
            Pawn animal = this.parent as Pawn;
            if (animal != null)
            {
                Messages.Message(animal.Label + " has been transformed into a weapon!", MessageTypeDefOf.PositiveEvent);
                TransformToWeapon(animal);
            }
        }

        private void TransformToWeapon(Pawn animal)
        {
            IntVec3 position = animal.Position;
            Map map = animal.Map;

            // Create the weapon
            ThingDef weaponDef = ThingDef.Named(Props.weaponDef);
            ThingClass_WeaponShift weapon = (ThingClass_WeaponShift)ThingMaker.MakeThing(weaponDef);

            // Spawn the weapon at the animal's position
            GenPlace.TryPlaceThing(weapon, position, map, ThingPlaceMode.Near);

            // Add the animal to the weapon's inner container
            if (weapon.TryAcceptPawn(animal))
            {
                Log.Message("Animal successfully stored in weapon's inner container.");
                if (!weaponContainer.TryAdd(weapon))
                {
                    Log.Error("Failed to store weapon in the ThingComp's inner container.");
                }
            }
            else
            {
                Log.Error("Failed to add animal to weapon's inner container.");
                // If adding the pawn fails, respawn the animal to prevent loss
                GenSpawn.Spawn(animal, position, map);
            }
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return weaponContainer;
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref weaponContainer, "weaponContainer", this);
        }
    }

    public class CompProperties_WeaponShift : CompProperties
    {
        public string weaponDef;

        public CompProperties_WeaponShift()
        {
            this.compClass = typeof(WeaponShift_ThingComp);
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (string error in base.ConfigErrors(parentDef))
            {
                yield return error;
            }

            if (string.IsNullOrEmpty(weaponDef))
            {
                yield return "CompProperties_WeaponShift requires a weaponDef.";
            }
        }
    }
}
