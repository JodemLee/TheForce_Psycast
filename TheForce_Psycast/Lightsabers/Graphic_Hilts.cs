using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TheForce_Psycast.Lightsabers
{
    internal class Graphic_Hilts : Graphic
    {
        protected Material mat;
        private Comp_LightsaberBlade compLightsaberBlade;
        private bool initialized = false;
        public static readonly string MaskSuffix = "_m";
        private string lastSelectedHiltGraphicPath;

        public override Material MatSingle => MatSingleFor(null); // Default call with no thing (or Pawn)
        public override Material MatWest => MatSingle;
        public override Material MatSouth => MatSingle;
        public override Material MatEast => MatSingle;
        public override Material MatNorth => MatSingle;

        public override bool ShouldDrawRotated => data == null || data.drawRotated;

        public override void TryInsertIntoAtlas(TextureAtlasGroup groupKey)
        {
            Texture2D mask = null;
            if (mat.HasProperty(ShaderPropertyIDs.MaskTex))
            {
                mask = (Texture2D)mat.GetTexture(ShaderPropertyIDs.MaskTex);
            }
            GlobalTextureAtlasManager.TryInsertStatic(groupKey, (Texture2D)mat.mainTexture, mask);
        }

        public override void Init(GraphicRequest req)
        {
            data = req.graphicData;
            path = req.path;
            maskPath = req.maskPath;
            color = req.color;
            colorTwo = req.colorTwo;
            drawSize = req.drawSize;

            UpdateMaterial(req);
        }

        public void UpdateMaterial(GraphicRequest req)
        {
            MaterialRequest materialRequest = new MaterialRequest
            {
                mainTex = req.texture ?? ContentFinder<Texture2D>.Get(req.path),
                shader = req.shader,
                color = req.color,
                colorTwo = req.colorTwo,
                renderQueue = req.renderQueue,
                shaderParameters = req.shaderParameters
            };
            if (req.shader.SupportsMaskTex())
            {
                materialRequest.maskTex = ContentFinder<Texture2D>.Get(maskPath.NullOrEmpty() ? (path + MaskSuffix) : maskPath, reportFailure: false);
            }
            mat = MaterialPool.MatFrom(materialRequest);
        }

        public void LinkToComp(Comp_LightsaberBlade comp)
        {
            if (comp == null) return;

            compLightsaberBlade = comp;

            // Update the material if the selected hilt graphic has changed
            GraphicRequest request = new GraphicRequest(
                typeof(Graphic_Hilts),
                comp.selectedhiltgraphic.graphicData.texPath,
                comp.selectedhiltgraphic.graphicData.Graphic.Shader,
                comp.selectedhiltgraphic.graphicData.Graphic.drawSize,
                comp.hiltColorOneOverrideColor,
                comp.hiltColorTwoOverrideColor,
                comp.selectedhiltgraphic.graphicData.Graphic.data,
                0,
                null,
                comp.selectedhiltgraphic.graphicData.maskPath
            );

            // Force material to reset
            path = comp.selectedhiltgraphic.graphicData.texPath;
            mat = null;  // Invalidate the cached material
            UpdateMaterial(request);
        }

        public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
        {
            return GraphicDatabase.Get<Graphic_Hilts>(path, newShader, drawSize, newColor, newColorTwo, data);
        }

        public override Material MatSingleFor(Thing thing)
        {
            if (thing != null && thing.TryGetComp<Comp_LightsaberBlade>() is Comp_LightsaberBlade comp)
            {
                LinkToComp(comp);
            }
            return mat;
        }

        public override Material MatAt(Rot4 rot, Thing thing = null)
        {
            return MatSingleFor(thing);
        }

        public override string ToString()
        {
            return string.Concat("Single(path=", path, ", color=", color, ", colorTwo=", colorTwo, ")");
        }
    }
}
