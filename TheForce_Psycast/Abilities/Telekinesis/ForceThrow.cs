using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TheForce_Psycast
{
    public class Ability_ForceThrow : VFECore.Abilities.Ability
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
            // Scale damage based on target mass
            float scaledDamage = baseDamage + (totalMass * 1f); // Adjust the multiplier as needed

            // Spawn the projectile
            ForceThrowProjectile projectile = (ForceThrowProjectile)GenSpawn.Spawn(ThingDef.Named("Force_ThrowItem"), target.Position, this.pawn.Map);
            projectile.targetThing = target;
            projectile.DamageAmount = (int)scaledDamage; // Assign the scaled damage to DamageAmount
            projectile.destCell = dest.Cell;
            projectile.Launch(target, dest.Cell, target, ProjectileHitFlags.All, false);
        }

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            for (int i = 0; i < targets.Length; i += 2)
            {
                if (targets[i].Thing is Pawn)
                {
                    GlobalTargetInfo dest = targets[i + 1];

                    // Check if the destination cell is different from the initial cell
                    if (dest.Cell != targets[i].Cell && dest.Map != null)
                    {
                        var map = Caster.Map;
                        var flyer = PawnFlyer.MakeFlyer(ForceDefOf.Force_ThrownPawn, targets[i].Thing as Pawn, dest.Cell, null, null);
                        GenSpawn.Spawn(flyer, dest.Cell, map);
                        float distancePushed = (dest.Cell - targets[i].Cell).LengthHorizontal;
                        float scaledDamage = baseDamage + (distancePushed * 2f); // Adjust the multiplier as needed
                        if (!dest.Cell.Walkable(dest.Map))
                        {
                            DamageInfo damageInfo = new DamageInfo(DamageDefOf.Blunt, 5f, 0f, -1f, this.pawn, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
                            targets[i].Thing.TakeDamage(damageInfo);
                        }
                    }
                }
                else
                {
                    // If it's not a pawn, launch a projectile instead
                    CastProjectile(targets[i + 1], targets[i].Thing);
                }
            }

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
            // Calculate the position of the projectile based on its current position and destination
            var position = Vector3.Lerp(origin, destCell.ToVector3(), DistanceCoveredFraction);

            // Check if the target thing is a corpse
            if (targetThing is Corpse corpse)
            {
                corpse.InnerPawn.Drawer.renderer.RenderPawnAt(position, Rot4.South, neverAimWeapon: true);
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
                    UnityEngine.Graphics.DrawMesh(graphicMesh, position, Quaternion.identity, targetMaterial, 0);
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

            // Check if the targetThing is in the MortarShells category
            if (targetThing.def.thingCategories != null && targetThing.def.thingCategories.Contains(ThingCategoryDef.Named("MortarShells")))
            {
                // Add 30 damage to the original target
                targetThing.TakeDamage(new DamageInfo(DamageDefOf.Blunt, 30));
            }

            base.Impact(hitThing, blockedByShield);

            if (targetThing != null)
            {
                // Check if the destination cell is occupied
                Thing occupyingThing = destCell.GetFirstThing(targetThing.Map, targetThing.def);
                if (occupyingThing != null)
                {
                    if (occupyingThing.def == targetThing.def && occupyingThing.stackCount < occupyingThing.def.stackLimit)
                    {
                        // If it's the same item and stackable, transfer to the stack
                        int spaceLeft = occupyingThing.def.stackLimit - occupyingThing.stackCount;
                        int transferAmount = Mathf.Min(spaceLeft, targetThing.stackCount);
                        occupyingThing.stackCount += transferAmount;
                        targetThing.stackCount -= transferAmount;

                        // If targetThing's stack count reaches zero, destroy it to prevent spawning issues
                        if (targetThing.stackCount <= 0)
                        {
                            targetThing.Destroy(DestroyMode.Vanish);
                        }
                        else
                        {
                            // Find a nearby empty spot and place the remaining stack there
                            IntVec3 nearbyCell = FindNearbyEmptyCell(targetThing.Map, destCell);
                            targetThing.Position = nearbyCell;
                        }
                    }
                    else
                    {
                        // Find a nearby empty spot and place the item there
                        IntVec3 nearbyCell = FindNearbyEmptyCell(targetThing.Map, destCell);
                        targetThing.Position = nearbyCell;
                    }
                }
                else
                {
                    // If the destination cell is unoccupied, just move the item there without spawning
                    IntVec3 nearbyCell = FindNearbyEmptyCell(targetThing.Map, destCell);
                    targetThing.Position = nearbyCell;
                }
            }
        }

        private IntVec3 FindNearbyEmptyCell(Map map, IntVec3 center)
        {
            TargetInfo targetInfo = new TargetInfo(destCell, targetThing.Map);

            foreach (IntVec3 cell in GenAdj.CellsAdjacent8Way(targetInfo))
            {
                if (cell.InBounds(map) && cell.Standable(map) && cell.GetFirstThing(map, def) == null)
                {
                    return cell;
                }
            }
            // If no adjacent empty spot found, return the original cell (this should be rare)
            return center;
        }
    }
}
