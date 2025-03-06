using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VanillaPsycastsExpanded;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace TheForce_Psycast.Abilities.Lightside
{
    public class Ability_ForceMeld : Ability_WriteCombatLog
    {
        public override Hediff ApplyHediff(Pawn targetPawn, HediffDef hediffDef, BodyPartRecord bodyPart, int duration, float severity)
        {
            var hediff = base.ApplyHediff(targetPawn, hediffDef, bodyPart, duration, severity) as Hediff_Force_ForceMeld;
            hediff.LinkAllPawnsAround();
            return hediff;
        }
    }

    public class Hediff_Force_ForceMeld : Hediff_Overlay
    {
        public override string OverlayPath => "Other/ForceField";
        public virtual Color OverlayColor => GetColorForPawn();

        private Color GetColorForPawn()
        {
            if (pawn?.story?.favoriteColor.HasValue ?? false)
            {
                return (Color)pawn.story.favoriteColor;
            }
            else
            {
                // Default color when pawn doesn't have a favorite color (or any other condition)
                return new Color(Color.blue.r, Color.blue.g, Color.blue.b, 0.5f); // Example: White with 50% opacity
            }
        }
        public override float OverlaySize => this.ability.GetRadiusForPawn();

        public List<Pawn> linkedPawns = new List<Pawn>();

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            LinkAllPawnsAround();
        }

        public void LinkAllPawnsAround()
        {
            foreach (var pawnToLink in GenRadial.RadialDistinctThingsAround(pawn.Position, pawn.Map, this.ability.GetRadiusForPawn(), true)
                .OfType<Pawn>().Where(x => x.RaceProps.Humanlike && x != pawn))
            {
                if (!linkedPawns.Contains(pawnToLink))
                {
                    linkedPawns.Add(pawnToLink);
                }
            }

            // Adjust the severity based on the number of linked pawns
            AdjustSeverity();
        }

        private void AdjustSeverity()
        {
            // Increase severity by 0.01 for each linked pawn
            this.Severity = 0f + (0.1f * linkedPawns.Count);
        }

        private void UnlinkAll()
        {
            linkedPawns.Clear();
            AdjustSeverity();
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            UnlinkAll();
        }

        public override void Tick()
        {
            base.Tick();
            for (var i = linkedPawns.Count - 1; i >= 0; i--)
            {
                var linkedPawn = linkedPawns[i];
                if (linkedPawn.Map != this.pawn.Map || linkedPawn.Position.DistanceTo(pawn.Position) > this.ability.GetRadiusForPawn())
                {
                    linkedPawns.RemoveAt(i);
                }
            }
            AdjustSeverity(); // Adjust severity after removing linked pawns

            if (!linkedPawns.Any())
            {
                this.pawn.health.RemoveHediff(this);
            }
        }

        public override void Draw()
        {
            Vector3 pos = pawn.DrawPos;
            pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            Color value = OverlayColor;
            MatPropertyBlock.SetColor(ShaderPropertyIDs.Color, value);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(pos, Quaternion.identity, new Vector3(OverlaySize * 2f * 1.16015625f, 1f, OverlaySize * 2f * 1.16015625f));
            UnityEngine.Graphics.DrawMesh(MeshPool.plane10, matrix, OverlayMat, 0, null, 0, MatPropertyBlock);
            foreach (var linked in linkedPawns)
            {
                GenDraw.DrawLineBetween(linked.DrawPos, this.pawn.DrawPos, SimpleColor.Yellow);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref linkedPawns, "linkedPawns", LookMode.Reference);
        }
    }
}
