using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TheForce_Psycast.Comps
{
    internal class PawnRenderNode_ModularHeadgear : PawnRenderNode_Apparel
    {
        public new PawnRenderNodeProperties_ModularHead Props => (PawnRenderNodeProperties_ModularHead)props;


        public PawnRenderNode_ModularHeadgear(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
            : base(pawn, props, tree)
        {
        }



        protected override IEnumerable<Graphic> GraphicsFor(Pawn pawn)
        {
            foreach (var graphicPath in Props.modulararmorparts)
            {
                // Get the suffix based on pawn's current rotation
                string directionSuffix = pawn.Rotation.AsCompassDirection();
                // Construct the graphic path with the direction suffix
                var graphic = GraphicDatabase.Get<Graphic_Multi>(
                    graphicPath,
                    GetShader(),
                    Props.drawSize,
                    GetColor());

                yield return graphic;
            }
        }
        private Shader GetShader()
        {
            Shader shader = ShaderDatabase.Cutout;
            if (apparel.StyleDef?.graphicData.shaderType != null)
            {
                shader = apparel.StyleDef.graphicData.shaderType.Shader;
            }
            else if ((apparel.StyleDef is null && apparel.def.apparel.useWornGraphicMask) || (apparel.StyleDef is not null && apparel.StyleDef.UseWornGraphicMask))
            {
                shader = ShaderDatabase.CutoutComplex;
            }
            return shader;
        }
        private Color GetColor()
        {
            return apparel.DrawColor;
        }
    }

    public class PawnRenderNodeProperties_ModularHead : PawnRenderNodeProperties
    {
        public List<string> modulararmorparts = new List<string>();
        public PawnRenderNodeProperties_ModularHead()
        {

        }
    }

    public class PawnRenderNodeWorker_ModularHead : PawnRenderNodeWorker_Apparel_Head
    {
        protected override Graphic GetGraphic(PawnRenderNode node, PawnDrawParms parms)
        {
            Apparel wornApparel = node.apparel;
            var compModularArmor = wornApparel?.TryGetComp<CompModularArmor>();

            if (compModularArmor != null)
            {
                // Check if default graphic is selected
                if (compModularArmor.selectedModularArmorIndex == 0)
                {
                    // Return the default graphic, which should be the first one in the list
                    return node.Graphics[compModularArmor.selectedModularArmorIndex];
                }
                else
                {
                    // Ensure the index is within the bounds of the graphics list
                    if (compModularArmor.selectedModularArmorIndex >= 0 && compModularArmor.selectedModularArmorIndex < node.Graphics.Count)
                    {
                        // Return selected modular graphic
                        return node.Graphics[compModularArmor.selectedModularArmorIndex];
                    }
                }
            }
            return node.Graphic;
        }

        public override Vector3 OffsetFor(PawnRenderNode node, PawnDrawParms parms, out Vector3 pivot)
        {
            Vector3 result = base.OffsetFor(node, parms, out pivot) + parms.pawn.Drawer.renderer.BaseHeadOffsetAt(parms.facing);
            if (parms.pawn.story.headType.narrow && node?.Props.narrowCrownHorizontalOffset != 0f && parms.facing.IsHorizontal)
            {
                if (parms.facing == Rot4.East)
                {
                    result.x -= node.Props.narrowCrownHorizontalOffset;
                }
                else if (parms.facing == Rot4.West)
                {
                    result.x += node.Props.narrowCrownHorizontalOffset;
                }
                result.z -= node.Props.narrowCrownHorizontalOffset;
            }
            return result;
        }
    }
}
    

