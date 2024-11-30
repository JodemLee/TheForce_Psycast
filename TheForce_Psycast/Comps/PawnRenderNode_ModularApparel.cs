using GraphicCustomization;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TheForce_Psycast.Comps
{
    internal class PawnRenderNode_Modular : PawnRenderNode_Apparel
    {
        public new PawnRenderNodeProperties_Modular Props => (PawnRenderNodeProperties_Modular)props;


        public PawnRenderNode_Modular(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
            : base(pawn, props, tree)
        {
        }

        protected override IEnumerable<Graphic> GraphicsFor(Pawn pawn)
        {
            foreach (var graphicPath in Props.modulararmorparts)
            {
                // Generate the correct graphic based on the pawn's body type and armor part path
                var graphic = GraphicDatabase.Get<Graphic_Multi>(
                    graphicPath + "_" + pawn?.story.bodyType.defName,
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

    public class PawnRenderNodeProperties_Modular : PawnRenderNodeProperties
    {
        public List<string> modulararmorparts = new List<string>();
        public PawnRenderNodeProperties_Modular()
        {
            
        }
    }

    public class PawnRenderNodeWorker_Modular : PawnRenderNodeWorker_Apparel_Body
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

            // Return the default graphic if no modular armor is applied or if selected index is out of bounds
            return node.Graphic;  // This should be the default graphic defined in the node
        }
    }
}



