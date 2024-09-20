using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using Verse;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;

namespace TheForce_Psycast.Abilities.Lightside
{
    internal class Force_Combustion : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            AbilityExtension_Explosion modExtension = def.GetModExtension<AbilityExtension_Explosion>();

            // Loop through the targets
            foreach (GlobalTargetInfo target in targets)
            {
                Pawn targetPawn = target.Thing as Pawn;

                // Check if the target is a pawn and if it has a weapon
                if (modExtension != null && targetPawn?.equipment?.Primary != null)
                {
                    ThingWithComps targetWeapon = targetPawn.equipment.Primary; // Get the target's currently carried weapon

                    // Calculate explosion radius based on target weapon's mass
                    float weaponMass = targetWeapon.GetStatValue(StatDefOf.Mass);
                    float explosionRadius = Mathf.Clamp(weaponMass, 1f, 4f); // Scale the explosion radius (adjust this range as necessary)

                    // Destroy the target's weapon
                    targetPawn.equipment.DestroyEquipment(targetWeapon);

                    // Apply explosion at the target's location
                    GenExplosion.DoExplosion(
                        targetPawn.Position, // Target's position
                        targetPawn.Map,
                        explosionRadius, // Scaled explosion radius
                        modExtension.explosionDamageDef,
                        (Thing)pawn, // Source of the explosion is the caster
                        modExtension.explosionDamageAmount*(int)weaponMass,
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
                        modExtension.casterImmune ? new List<Thing> {} : null,
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
    }
}
