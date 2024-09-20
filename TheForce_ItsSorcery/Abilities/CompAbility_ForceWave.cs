using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TheForce_ItsSorcery.Abilities
{
    internal class CompAbility_ForceWave : CompAbilityEffect
    {
        public new CompProperties_ForceWave Props => (CompProperties_ForceWave)props;

        private Pawn Pawn => parent.pawn;
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            List<IntVec3> affectedCells = AffectedCells(target);

            foreach (IntVec3 cell in affectedCells)
            {
                Pawn pawn = cell.GetFirstPawn(parent.pawn.Map);
                if (pawn == null)
                {
                    continue;
                }

                // Get the stat value for scaling
                float statValue = Props.scalingStat != null ? pawn.GetStatValue(Props.scalingStat) : 1f;

                // If the stat value is below the threshold, do not apply the effect
                if (statValue <= Props.effectivenessThreshold)
                {
                    continue;
                }

                // Calculate knockback with an option for reverse scaling
                float knockback = Props.baseKnockback;
                if (Props.scaleInversely)
                {
                    knockback *= Props.scalingFactor / statValue;
                }
                else
                {
                    knockback *= Props.scalingFactor * statValue;
                }

                // Launch the pawn
                IntVec3 launchDirection = pawn.Position - parent.pawn.Position;
                IntVec3 destination = pawn.Position + launchDirection * (int)knockback;

                // Check if the destination is valid
                if (!PositionUtils.CheckValidPosition(destination, parent.pawn.Map))
                {
                    Log.Warning($"ForceWave: Destination {destination} is not valid.");
                    destination = PositionUtils.FindValidPosition(pawn.Position, launchDirection, parent.pawn.Map);
                }

                PawnFlyer pawnFlyer = PawnFlyer.MakeFlyer(ThingDefOf.PawnFlyer, pawn, destination, null, null);
                if (pawnFlyer != null)
                {
                    GenSpawn.Spawn(pawnFlyer, destination, parent.pawn.Map);
                    Log.Message($"ForceWave: PawnFlyer spawned at {destination}.");
                }
                else
                {
                    Log.Warning("ForceWave: Failed to create PawnFlyer.");
                }
            }
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            if (Pawn.Faction != null)
            {
                foreach (IntVec3 item in AffectedCells(target))
                {
                    List<Thing> thingList = item.GetThingList(Pawn.Map);
                    for (int i = 0; i < thingList.Count; i++)
                    {
                        if (thingList[i].Faction == Pawn.Faction)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private List<IntVec3> AffectedCells(LocalTargetInfo target)
        {
            List<IntVec3> tmpCells = new List<IntVec3>();
            Vector3 vector = parent.pawn.Position.ToVector3Shifted().Yto0();
            IntVec3 intVec = target.Cell.ClampInsideMap(parent.pawn.Map);
            if (parent.pawn.Position == intVec)
            {
                return tmpCells;
            }

            float lengthHorizontal = (intVec - parent.pawn.Position).LengthHorizontal;
            float num = (float)(intVec.x - parent.pawn.Position.x) / lengthHorizontal;
            float num2 = (float)(intVec.z - parent.pawn.Position.z) / lengthHorizontal;
            intVec.x = Mathf.RoundToInt((float)parent.pawn.Position.x + num * Props.range);
            intVec.z = Mathf.RoundToInt((float)parent.pawn.Position.z + num2 * Props.range);
            float targetAngle = Vector3.SignedAngle(intVec.ToVector3Shifted().Yto0() - vector, Vector3.right, Vector3.up);
            float halfWidth = Props.lineWidthEnd / 2f;
            float hypotenuse = Mathf.Sqrt(Mathf.Pow((intVec - parent.pawn.Position).LengthHorizontal, 2f) + Mathf.Pow(halfWidth, 2f));
            float angleWidth = 57.29578f * Mathf.Asin(halfWidth / hypotenuse);
            int numCells = GenRadial.NumCellsInRadius(Props.range);

            for (int i = 0; i < numCells; i++)
            {
                IntVec3 cell = parent.pawn.Position + GenRadial.RadialPattern[i];
                if (CanUseCell(cell) && Mathf.Abs(Mathf.DeltaAngle(Vector3.SignedAngle(cell.ToVector3Shifted().Yto0() - vector, Vector3.right, Vector3.up), targetAngle)) <= angleWidth)
                {
                    tmpCells.Add(cell);
                }
            }

            List<IntVec3> lineCells = GenSight.BresenhamCellsBetween(parent.pawn.Position, intVec);
            foreach (IntVec3 cell in lineCells)
            {
                if (!tmpCells.Contains(cell) && CanUseCell(cell))
                {
                    tmpCells.Add(cell);
                }
            }

            return tmpCells;
        }

        private bool CanUseCell(IntVec3 c)
        {
            if (!c.InBounds(parent.pawn.Map))
            {
                return false;
            }
            if (c == parent.pawn.Position)
            {
                return false;
            }
            if (!Props.canHitFilledCells && c.Filled(parent.pawn.Map))
            {
                return false;
            }
            if (!c.InHorDistOf(parent.pawn.Position, Props.range))
            {
                return false;
            }
            return parent.verb.TryFindShootLineFromTo(parent.pawn.Position, c, out _);
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            GenDraw.DrawFieldEdges(AffectedCells(target));
        }
    }

    public class CompProperties_ForceWave : CompProperties_AbilityEffect
    {
        public int baseKnockback = 5;
        public StatDef scalingStat; // The stat used for scaling
        public float scalingFactor = 1f;
        public bool scaleInversely = true; // Controls reverse scaling
        public float effectivenessThreshold = 0.2f; // Minimum stat value for the ability to have an effect
        public float range = 10f; // Range of the wave effect
        public float lineWidthEnd = 5f; // Width of the wave effect at the end
        public bool canHitFilledCells = false;

        public CompProperties_ForceWave()
        {
            compClass = typeof(CompAbility_ForceWave);
        }
    }
}
