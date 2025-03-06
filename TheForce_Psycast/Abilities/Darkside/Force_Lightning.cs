using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using VFECore;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;

namespace TheForce_Psycast.Abilities.Darkside
{

    public class Ability_ForceLightning : Ability_ShootProjectile
    {
        private float range => this.def.range;
        private float coneAngle => 30;
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);

            if (targets.Length > 0)
            {
                List<IntVec3> affectedCells = AffectedCells(targets[0].Cell);

                foreach (IntVec3 cell in affectedCells)
                {
                    List<Thing> thingsInCell = cell.GetThingList(pawn.Map);

                    foreach (Thing thing in thingsInCell)
                    {
                        if (thing is Pawn targetPawn && targetPawn != pawn) // Ignore the caster
                        {
                            ShootProjectile(targetPawn);
                            DrawLightningEffect(pawn.DrawPos, targetPawn.DrawPos);
                        }
                    }
                }
            }
        }

        public override void PreCast(GlobalTargetInfo[] target, ref bool startAbilityJobImmediately, Action startJobAction)
        {
            base.PreCast(target, ref startAbilityJobImmediately, startJobAction);

            Find.BattleLog.Add(new BattleLogEntry_VFEAbilityUsed(this.pawn, target[0].Thing, this.def, RulePackDefOf.Event_AbilityUsed));
        }

        protected override Projectile ShootProjectile(GlobalTargetInfo target)
        {
            var projectile = base.ShootProjectile(target) as ForceLightningProjectile;
            projectile.ability = this;
            return projectile;
        }

        private List<IntVec3> AffectedCells(IntVec3 targetCell)
        {
            List<IntVec3> cells = new List<IntVec3>();
            Vector3 startVector = pawn.Position.ToVector3Shifted().Yto0();
            Vector3 targetVector = targetCell.ToVector3Shifted().Yto0();
            float targetAngle = Vector3.SignedAngle(targetVector - startVector, Vector3.right, Vector3.up);
            int numCells = GenRadial.NumCellsInRadius(range);

            for (int i = 0; i < numCells; i++)
            {
                IntVec3 cell = pawn.Position + GenRadial.RadialPattern[i];
                if (Mathf.Abs(Mathf.DeltaAngle(Vector3.SignedAngle(cell.ToVector3Shifted().Yto0() - startVector, Vector3.right, Vector3.up), targetAngle)) <= coneAngle)
                {
                    cells.Add(cell);
                }
            }

            return cells;
        }

        private void DrawLightningEffect(Vector3 startPos, Vector3 endPos)
        {
            // Create a line draw effect to simulate the lightning bolt
            FleckCreationData fleckData = FleckMaker.GetDataStatic(startPos, pawn.Map, FleckDefOf.LightningGlow);
            fleckData.rotation = (endPos - startPos).AngleFlat();
            fleckData.velocity = (endPos - startPos).normalized * 0.5f;
            fleckData.targetSize = (endPos - startPos).magnitude;

            pawn.Map.flecks.CreateFleck(fleckData);
        }

        public override void DrawHighlight(LocalTargetInfo target)
        {
            base.DrawHighlight(target);

            List<IntVec3> affectedCells = AffectedCells(target.Cell);
            GenDraw.DrawFieldEdges(affectedCells);
        }

    }

    public class ForceLightningProjectile : ExpandableProjectile
    {
        public Ability ability;
        public override void DoDamage(IntVec3 pos)
        {
            base.DoDamage(pos);
            {
                if (pos != this.launcher.Position && this.launcher.Map != null && GenGrid.InBounds(pos, this.launcher.Map))
                {
                    var list = this.launcher.Map.thingGrid.ThingsListAt(pos);
                    for (int num = list.Count - 1; num >= 0; num--)
                    {
                        if (IsDamagable(list[num]))
                        {
                            this.customImpact = true;
                            base.Impact(list[num]);
                            this.customImpact = false;
                            if (list[num] is Pawn pawn)
                            {
                                var severityImpact = (0.5f / pawn.Position.DistanceTo(launcher.Position));
                                if (ability.def.goodwillImpact != 0)
                                {
                                    ability.ApplyGoodwillImpact(pawn);
                                }
                            }
                        }
                    }
                }
            }
        }
  
        

        public override bool IsDamagable(Thing t)
        {
            return t is Pawn && base.IsDamagable(t) || t.def == ThingDefOf.Fire;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref ability, "ability");
        }
    }
}


