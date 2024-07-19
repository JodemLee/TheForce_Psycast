using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TheForce_Psycast.Lightsabers
{
    public class Comp_LightsaberBlade : ThingComp
    {
        private Graphic graphicInt;
        private Graphic lightsaberCore1GraphicInt;
        private Graphic lightsaberBlade2GraphicInt;
        private Graphic lightsaberCore2GraphicInt;

        public Color lightsaberBlade1OverrideColor;
        private Color lightsaberCore1OverrideColor;
        private Color lightsaberBlade2OverrideColor;
        private Color lightsaberCore2OverrideColor;

        private readonly MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            InitializeColors();
        }

        public CompProperties_LightsaberBlade Props => (CompProperties_LightsaberBlade)props;

        public Graphic Graphic => GetOrCreateGraphic(ref graphicInt, Props.graphicData, lightsaberBlade1OverrideColor);

        public Graphic LightsaberCore1Graphic => GetOrCreateGraphic(ref lightsaberCore1GraphicInt, Props.lightsaberCore1GraphicData, lightsaberCore1OverrideColor);

        public Graphic LightsaberBlade2Graphic => GetOrCreateGraphic(ref lightsaberBlade2GraphicInt, Props.lightsaberBlade2GraphicData, lightsaberBlade2OverrideColor);

        public Graphic LightsaberCore2Graphic => GetOrCreateGraphic(ref lightsaberCore2GraphicInt, Props.lightsaberCore2GraphicData, lightsaberCore2OverrideColor);

        private Graphic GetOrCreateGraphic(ref Graphic graphicField, GraphicData graphicData, Color color)
        {
            if (graphicField == null && graphicData != null)
            {
                var newColor = color == Color.white ? parent.DrawColor : color;
                graphicField = graphicData.Graphic.GetColoredVersion(parent.Graphic.Shader, newColor, newColor);
            }
            return graphicField;
        }

        private void InitializeColors()
        {
            lightsaberBlade1OverrideColor = GetRandomRGBColor();
            lightsaberCore1OverrideColor = GetBlackOrWhiteCore();
            lightsaberBlade2OverrideColor = lightsaberBlade1OverrideColor;
            lightsaberCore2OverrideColor = lightsaberCore1OverrideColor;
        }

        private Color GetRandomRGBColor()
        {
            return Props.colorable ? new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value) : Color.white;
        }

        private Color GetBlackOrWhiteCore()
        {
            return Props.colorable ? (UnityEngine.Random.value < 0.9f ? Color.white : Color.black) : Color.white;
        }

        public void SetColor(Color color) => UpdateColor(ref lightsaberBlade1OverrideColor, color, UpdateGraphicsAndMaterialPropertyBlock);

        public void SetLightsaberCore1Color(Color color) => UpdateColor(ref lightsaberCore1OverrideColor, color, () => UpdateGraphic(ref lightsaberCore1GraphicInt, Props.lightsaberCore1GraphicData, lightsaberCore1OverrideColor, "_Lightsaber_Core1Color"));

        public void SetLightsaberBlade2Color(Color color) => UpdateColor(ref lightsaberBlade2OverrideColor, color, () => UpdateGraphic(ref lightsaberBlade2GraphicInt, Props.lightsaberBlade2GraphicData, lightsaberBlade2OverrideColor, "_Lightsaber_Blade2Color"));

        public void SetLightsaberCore2Color(Color color) => UpdateColor(ref lightsaberCore2OverrideColor, color, () => UpdateGraphic(ref lightsaberCore2GraphicInt, Props.lightsaberCore2GraphicData, lightsaberCore2OverrideColor, "_Lightsaber_Core2Color"));

        private void UpdateColor(ref Color overrideColor, Color newColor, System.Action updateAction)
        {
            if (Props.colorable)
            {
                overrideColor = newColor;
                updateAction();
            }
        }

        private void UpdateGraphicsAndMaterialPropertyBlock()
        {
            if (Props.graphicData != null)
            {
                UpdateGraphic(ref graphicInt, Props.graphicData, lightsaberBlade1OverrideColor, "_Color");
                materialPropertyBlock.Clear();
                materialPropertyBlock.SetColor("_Color", lightsaberBlade1OverrideColor);
                materialPropertyBlock.SetColor("_ColorTwo", lightsaberBlade1OverrideColor);
            }
        }

        private void UpdateGraphic(ref Graphic graphicField, GraphicData graphicData, Color color, string colorProperty)
        {
            if (graphicData != null)
            {
                var newColor = color == Color.white ? parent.DrawColor : color;
                graphicField = graphicData.Graphic.GetColoredVersion(parent.Graphic.Shader, newColor, newColor);
                materialPropertyBlock.Clear();
                materialPropertyBlock.SetColor(colorProperty, newColor);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref lightsaberBlade1OverrideColor, nameof(lightsaberBlade1OverrideColor), Color.white);
            Scribe_Values.Look(ref lightsaberCore1OverrideColor, nameof(lightsaberCore1OverrideColor), Color.white);
            Scribe_Values.Look(ref lightsaberBlade2OverrideColor, nameof(lightsaberBlade2OverrideColor), Color.white);
            Scribe_Values.Look(ref lightsaberCore2OverrideColor, nameof(lightsaberCore2OverrideColor), Color.white);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            if (Props.colorable)
            {
                yield return new Command_Action
                {
                    action = OpenColorPicker,
                    defaultLabel = "Change Lightsaber Color",
                    defaultDesc = "Open the color picker to change the lightsaber blade colors.",
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/ChangeColor", true)
                };
            }
        }

        private void OpenColorPicker()
        {
            Find.WindowStack.Add(new Dialog_LightsaberColorPicker(this, SetColors));
        }

        private void SetColors(Color blade1Color, Color core1Color, Color blade2Color, Color core2Color)
        {
            SetColor(blade1Color);
            SetLightsaberCore1Color(core1Color);
            SetLightsaberBlade2Color(blade2Color);
            SetLightsaberCore2Color(core2Color);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            InitializeColors();
        }
    }

    public class CompProperties_LightsaberBlade : CompProperties
    {
        public GraphicData graphicData;
        public GraphicData lightsaberCore1GraphicData;
        public GraphicData lightsaberBlade2GraphicData;
        public GraphicData lightsaberCore2GraphicData;
        public AltitudeLayer altitudeLayer;
        public float Altitude => Altitudes.AltitudeFor(altitudeLayer);
        public bool colorable = true;

        public CompProperties_LightsaberBlade()
        {
            this.compClass = typeof(Comp_LightsaberBlade);
        }
    }
}
