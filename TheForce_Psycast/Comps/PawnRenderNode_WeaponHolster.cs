using System.Collections.Generic;
using TheForce_Psycast.Lightsabers;
using UnityEngine;
using Verse;

namespace TheForce_Psycast.Comps
{
    internal class PawnRenderNode_WeaponHolster : PawnRenderNode
    {
        private ThingWithComps primaryEquipment;

        public PawnRenderNode_WeaponHolster(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
            : base(pawn, props, tree)
        {
            primaryEquipment = pawn.equipment?.Primary;
        }

        protected override IEnumerable<Graphic> GraphicsFor(Pawn pawn)
        {
            if (primaryEquipment?.def?.graphicData != null)
            {
                yield return primaryEquipment.Graphic;
            }
            else
            {
                foreach (var graphic in base.GraphicsFor(pawn))
                {
                    yield return graphic;
                }
            }
        }

        protected override string TexPathFor(Pawn pawn)
        {
            if (primaryEquipment?.def?.graphicData != null)
            {
                return primaryEquipment.def.graphicData.texPath;
            }
            return base.TexPathFor(pawn);
        }

        public override Color ColorFor(Pawn pawn)
        {
            if (primaryEquipment?.def?.graphicData != null)
            {
                return primaryEquipment.DrawColor;
            }
            return base.ColorFor(pawn);
        }
    }

    public class PawnRenderNodeProperties_WeaponHolster : PawnRenderNodeProperties
    {
        public PawnRenderNodeProperties_WeaponHolster()
        {

        }
    }

    public class PawnRenderNodeWorker_WeaponHolster : PawnRenderNodeWorker
    {
        public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
        {
            // Check if the pawn has primary equipment and call base checks
            if (parms.pawn.equipment?.Primary == null || parms.pawn.IsCarryingWeaponOpenly())
            {
                return false;
            }
            return base.CanDrawNow(node, parms);
        }

        protected override Graphic GetGraphic(PawnRenderNode node, PawnDrawParms parms)
        {
            if (parms.pawn.equipment?.Primary != null)
            {
                ThingWithComps primary = parms.pawn.equipment.Primary;
                var lightsaberhilt = primary.TryGetComp<Comp_LightsaberBlade>();
                Graphic primaryGraphic = primary.DefaultGraphic;

                if (primaryGraphic != null && lightsaberhilt != null)
                {
                    var hiltDef = lightsaberhilt.selectedhiltgraphic;
                    if (hiltDef.graphicData != null)
                    {
                        primaryGraphic = hiltDef.graphicData.Graphic.GetColoredVersion(
                            hiltDef.graphicData.Graphic.Shader,
                            lightsaberhilt.hiltColorOneOverrideColor,
                            lightsaberhilt.hiltColorTwoOverrideColor
                        );
                    }
                }
                    if (primaryGraphic != null)
                {
                    return primaryGraphic;
                }
            }
            return base.GetGraphic(node, parms);
        }
    }
}