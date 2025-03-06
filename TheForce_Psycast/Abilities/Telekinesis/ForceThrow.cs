using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using TheForce_Psycast.Abilities;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TheForce_Psycast
{
    public class Ability_ForceThrow : Ability_WriteCombatLog
    {
        public float baseDamage = 1f;

        public override void WarmupToil(Toil toil)
        {
            base.WarmupToil(toil);
            toil.AddPreTickAction(delegate
            {
                if (this.pawn.jobs.curDriver.ticksLeftThisToil != 5) return;
                for (int i = 0; i < this.Comp.currentlyCastingTargets.Length; i += 2)
                    if (this.Comp.currentlyCastingTargets[i].Thing is { } t)
                    {
                        GlobalTargetInfo dest = this.Comp.currentlyCastingTargets[i + 1];
                    }
            });
        }

        private void CastProjectile(GlobalTargetInfo dest, Thing target)
        {
            float totalMass = target.GetStatValue(StatDefOf.Mass);
            if (target is ThingWithComps thingWithComps && thingWithComps.def.CountAsResource)
            {
                totalMass *= thingWithComps.stackCount;
            }
            float scaledDamage = baseDamage + (totalMass * 1f);

            ForceThrowProjectile projectile = (ForceThrowProjectile)GenSpawn.Spawn(ThingDef.Named("Force_ThrowItem"), target.Position, this.pawn.Map);
            projectile.targetThing = target; // Store targetThing in the projectile
            projectile.DamageAmount = (int)scaledDamage;
            projectile.destCell = dest.Cell;
            projectile.Launch(CasterPawn, dest.Cell, target, ProjectileHitFlags.IntendedTarget, false);
            target.DeSpawn();
        }


        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            // Check if the target is a building
            if (target.Thing is Building targetBuilding)
            {
                // Check if the building can be minified
                if (!targetBuilding.def.Minifiable)
                {
                    // Show message that the building can't be thrown (minified)
                    if (showMessages)
                    {
                        Messages.Message("This building cannot be minified and thrown.", MessageTypeDefOf.RejectInput, false);
                    }
                    return false;
                }
            }

            // Call the base method to perform any other validations
            return base.ValidateTarget(target, showMessages);
        }

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            for (int i = 0; i < targets.Length; i += 2)
            {
                // Log the target information for each iteration
                Log.Message($"Cast target {i}: {targets[i]}");

                if (targets[i].Thing is Pawn targetPawn)
                {
                    GlobalTargetInfo dest = targets[i + 1];

                    // Log target pawn and destination information
                    Log.Message($"Pawn target: {targetPawn}, Destination: {dest}");

                    // Check if the destination cell is different from the initial cell
                    if (dest.Cell != targets[i].Cell && dest.Map != null)
                    {
                        Log.Message($"Destination is different, spawning flyer...");

                        var map = Caster.Map;
                        var flyer = PawnFlyer.MakeFlyer(ForceDefOf.Force_ThrownPawnThrow, targetPawn, dest.Cell, null, null);
                        GenSpawn.Spawn(flyer, dest.Cell, map);

                        float distancePushed = (dest.Cell - targets[i].Cell).LengthHorizontal;
                        float scaledDamage = baseDamage + (distancePushed * 2f); // Adjust the multiplier as needed
                        Log.Message($"Distance pushed: {distancePushed}, Scaled damage: {scaledDamage}");

                        if (!dest.Cell.Walkable(dest.Map))
                        {
                            Log.Message($"Destination cell is not walkable. Applying damage...");
                            DamageInfo damageInfo = new DamageInfo(DamageDefOf.Blunt, scaledDamage, 0f, -1f, this.pawn, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
                            targets[i].Thing.TakeDamage(damageInfo);
                        }
                    }
                }
                else if (targets[i].Thing is Building targetBuilding)
                {
                    // Log building target info
                    Log.Message($"Building target: {targetBuilding}");

                    // If it's a building, uninstall and minify it
                    MinifiedThing minifiedBuilding = targetBuilding.Uninstall();

                    if (minifiedBuilding != null)
                    {
                        Log.Message($"Minified building: {minifiedBuilding}");
                        // Launch the minified building as a projectile
                        CastProjectile(targets[i + 1], minifiedBuilding);
                    }
                    else
                    {
                        Log.Message($"Failed to uninstall the building.");
                    }
                }
                else
                {
                    // Log other object types being cast
                    Log.Message($"Launching regular projectile for target: {targets[i]}");

                    // If it's not a pawn or building, launch a regular projectile
                    CastProjectile(targets[i + 1], targets[i].Thing);
                }
            }

            Log.Message("Cast completed.");
            base.Cast(targets);
        }

    }


    [StaticConstructorOnStartup]
    public class ForceThrowProjectile : Projectile
    {
        public Thing targetThing;
        private static readonly Material shadowMaterial = MaterialPool.MatFrom("Things/Skyfaller/SkyfallerShadowCircle", ShaderDatabase.Transparent);
        public int DamageAmount; // Add this field to hold the damage amount
        public IntVec3 destCell;

        public static Func<Projectile, float> ArcHeightFactor = (Func<Projectile, float>)
       Delegate.CreateDelegate(typeof(Func<Projectile, float>), null, AccessTools.Method(typeof(Projectile), "get_ArcHeightFactor"));

        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            // Calculate the current position of the projectile based on its travel fraction
            var position = Vector3.Lerp(origin, destCell.ToVector3(), DistanceCoveredFraction);

            // Check if the target thing is a corpse
            if (targetThing is Corpse corpse)
            {
                corpse.InnerPawn.Drawer.renderer.RenderPawnAt(drawLoc, Rot4.South, neverAimWeapon: true);
            }
            else
            {
                // Get the graphic of the target thing
                Graphic targetGraphic = targetThing?.Graphic;

                // Check if the target thing has a graphic
                if (targetGraphic != null)
                {
                    // Draw the target thing's graphic at the calculated position
                    Material targetMaterial = targetGraphic.MatSingle;
                    Vector2 graphicSize = targetGraphic.drawSize; // Get the original size of the graphic
                    Mesh graphicMesh = MeshPool.GridPlane(graphicSize);
                    UnityEngine.Graphics.DrawMesh(graphicMesh, drawLoc, Quaternion.identity, targetMaterial, 0);
                }
            }
        }

        protected override void Impact(Thing hitThing, bool blockedByShield)
        {
            if (hitThing != null)
            {
                // Apply damage to the hitThing
                hitThing.TakeDamage(new DamageInfo(DamageDefOf.Blunt, DamageAmount, 0f, -1f, this.launcher, null, null, DamageInfo.SourceCategory.ThingOrUnknown));
            }

            if (targetThing != null && this.Map != null)
            {
                // Use the projectile's Map instead of targetThing.Map for safety
                Map map = this.Map;

                // Check if destCell is valid; if not, default to projectile's current cell
                IntVec3 finalCell = (destCell.IsValid && destCell.Standable(map) && destCell.GetFirstThing(map, targetThing.def) == null)
                                    ? destCell
                                    : FindNearbyEmptyCell(map, this.Position);

                // Place targetThing at the final position and re-spawn it on the map
                GenPlace.TryPlaceThing(targetThing, finalCell, map, ThingPlaceMode.Near);

                // Handle stacking if it lands on an existing stackable item of the same type
                Thing occupyingThing = finalCell.GetFirstThing(map, targetThing.def);
                if (occupyingThing != null && occupyingThing.def == targetThing.def && occupyingThing.stackCount < occupyingThing.def.stackLimit)
                {
                    int spaceLeft = occupyingThing.def.stackLimit - occupyingThing.stackCount;
                    int transferAmount = Mathf.Min(spaceLeft, targetThing.stackCount);
                    occupyingThing.stackCount += transferAmount;
                    targetThing.stackCount -= transferAmount;

                    if (targetThing.stackCount <= 0)
                    {
                        targetThing.Destroy(DestroyMode.Vanish);
                    }
                }
            }

            base.Impact(hitThing, blockedByShield);
        }

        private IntVec3 FindNearbyEmptyCell(Map map, IntVec3 center)
        {
            foreach (IntVec3 cell in GenAdj.CellsAdjacent8Way(new TargetInfo(center, map)))
            {
                if (cell.InBounds(map) && cell.Standable(map) && cell.GetFirstThing(map, targetThing.def) == null)
                {
                    return cell;
                }
            }
            return center; // Default to center if no empty cell found
        }
    }
}

