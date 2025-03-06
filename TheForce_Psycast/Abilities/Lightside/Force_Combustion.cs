using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using VFECore.Abilities;

namespace TheForce_Psycast.Abilities.Lightside
{
    internal class Force_Combustion : Ability_WriteCombatLog
    {
        // Field to store the selected item for the explosion
        private ThingWithComps selectedItem;

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            AbilityExtension_Explosion modExtension = def.GetModExtension<AbilityExtension_Explosion>();

            // Only proceed if an item has been selected
            if (selectedItem == null)
            {
                Log.Warning("No item selected for Force_Combustion.");
                return;
            }

            foreach (GlobalTargetInfo target in targets)
            {
                Pawn targetPawn = target.Thing as Pawn;
                if (modExtension != null && targetPawn != null)
                {
                    float itemMass = selectedItem.GetStatValue(StatDefOf.Mass);
                    float explosionRadius = Mathf.Clamp(itemMass, 1f, 4f);

                    // Destroy the selected item
                    if (selectedItem.def.IsApparel)
                    {
                        targetPawn.apparel.Remove(selectedItem as Apparel);
                        selectedItem.Destroy();
                    }
                    else if (selectedItem.def.IsWeapon)
                    {
                        targetPawn.equipment.DestroyEquipment(selectedItem);
                    }
                    else
                    {
                        Thing droppedItem;
                        if (targetPawn.inventory.innerContainer.TryDrop(selectedItem, ThingPlaceMode.Near, out droppedItem))
                        {
                            droppedItem.Destroy();
                        }
                    }

                    // Trigger the explosion
                    TriggerExplosion(targetPawn, modExtension, selectedItem, itemMass, explosionRadius);
                }
            }
        }

        public override void PreCast(GlobalTargetInfo[] targets, ref bool startAbilityJobImmediately, Action startJobAction)
        {
            base.PreCast(targets, ref startAbilityJobImmediately, startJobAction);
            selectedItem = null;
            GlobalTargetInfo target = targets[0];
            Pawn targetPawn = target.Thing as Pawn;

            if (targetPawn != null)
            {
                // Create a list to hold all FloatMenuOptions
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (ThingWithComps item in targetPawn.EquippedWornOrInventoryThings)
                {
                    float itemMass = item.GetStatValue(StatDefOf.Mass);
                    string label = $"{item.LabelCap} (Mass: {itemMass})";
                    options.Add(new FloatMenuOption(label, () => OnItemSelected(item, startJobAction)));
                }
                if (options.Count > 0)
                {
                    // Prevent the ability from starting immediately
                    startAbilityJobImmediately = false;
                    Find.WindowStack.Add(new FloatMenu(options));
                }
                else
                {
                    Log.Message("No equipment or items found on the target.");
                }
            }
        }

        private void OnItemSelected(ThingWithComps item, Action startJobAction)
        {
            selectedItem = item;
            // Start the ability job after selecting the item
            startJobAction();
        }

        private void TriggerExplosion(Pawn targetPawn, AbilityExtension_Explosion modExtension, ThingWithComps item, float itemMass, float explosionRadius)
        {
            GenExplosion.DoExplosion(
                targetPawn.Position, // Target's position
                targetPawn.Map,
                explosionRadius, // Scaled explosion radius
                modExtension.explosionDamageDef,
                (Thing)pawn, // Source of the explosion is the caster
                modExtension.explosionDamageAmount * (int)itemMass,
                modExtension.explosionArmorPenetration,
                modExtension.explosionSound,
                (ThingDef)null,
                (ThingDef)null,
                (Thing)null,
                modExtension.postExplosionSpawnThingDef,
                modExtension.postExplosionSpawnChance,
                modExtension.postExplosionSpawnThingCount,
                modExtension.postExplosionGasType,
                modExtension.applyDamageToExplosionCellsNeighbors,
                modExtension.preExplosionSpawnThingDef,
                modExtension.preExplosionSpawnChance,
                modExtension.preExplosionSpawnThingCount,
                modExtension.chanceToStartFire,
                modExtension.damageFalloff,
                modExtension.explosionDirection,
                modExtension.casterImmune ? new List<Thing> { } : null,
                (FloatRange?)null,
                true,
                1f,
                0f,
                true,
                (ThingDef)null,
                1f
            );
        }
    }
}