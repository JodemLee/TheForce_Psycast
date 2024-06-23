using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TheForce_Psycast.Lightsabers
{
    public class Comp_LightsaberBlade : ThingComp
    {
        private Graphic graphicInt;
        private Graphic lightsaberCore1GraphicInt;
        private Graphic lightsaberBlade2GraphicInt;
        private Graphic lightsaberCore2GraphicInt;

        private Color lightsaberBlade1OverrideColor;
        private Color lightsaberCore1OverrideColor;
        private Color lightsaberBlade2OverrideColor;
        private Color lightsaberCore2OverrideColor;

        private readonly MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();

        // New fields for rotation and position offsets
        public float rotationOffset = 0f;
        public Vector3 positionOffset = Vector3.zero;

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

        public void SetColor(Color color) => SetOverrideColor(ref lightsaberBlade1OverrideColor, color, UpdateGraphics);

        public void SetLightsaberCore1Color(Color color) => SetOverrideColor(ref lightsaberCore1OverrideColor, color, UpdateLightsaberCore1Graphic);

        public void SetLightsaberBlade2Color(Color color) => SetOverrideColor(ref lightsaberBlade2OverrideColor, color, UpdateLightsaberBlade2Graphic);

        public void SetLightsaberCore2Color(Color color) => SetOverrideColor(ref lightsaberCore2OverrideColor, color, UpdateLightsaberCore2Graphic);

        private void SetOverrideColor(ref Color overrideColor, Color newColor, System.Action updateAction)
        {
            if (Props.colorable)
            {
                overrideColor = newColor;
                updateAction();
            }
        }

        private void UpdateGraphics()
        {
            graphicInt = GetOrCreateGraphic(ref graphicInt, Props.graphicData, lightsaberBlade1OverrideColor);
        }

        private void UpdateLightsaberCore1Graphic()
        {
            lightsaberCore1GraphicInt = GetOrCreateGraphic(ref lightsaberCore1GraphicInt, Props.lightsaberCore1GraphicData, lightsaberCore1OverrideColor);
        }

        private void UpdateLightsaberBlade2Graphic()
        {
            lightsaberBlade2GraphicInt = GetOrCreateGraphic(ref lightsaberBlade2GraphicInt, Props.lightsaberBlade2GraphicData, lightsaberBlade2OverrideColor);
        }

        private void UpdateLightsaberCore2Graphic()
        {
            lightsaberCore2GraphicInt = GetOrCreateGraphic(ref lightsaberCore2GraphicInt, Props.lightsaberCore2GraphicData, lightsaberCore2OverrideColor);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref lightsaberBlade1OverrideColor, "lightsaberBlade1OverrideColor", Color.white);
            Scribe_Values.Look(ref lightsaberCore1OverrideColor, "lightsaberCore1OverrideColor", Color.white);
            Scribe_Values.Look(ref lightsaberBlade2OverrideColor, "lightsaberBlade2OverrideColor", Color.white);
            Scribe_Values.Look(ref lightsaberCore2OverrideColor, "lightsaberCore2OverrideColor", Color.white);
        }

        public void PostDraw(Vector3 drawLoc, Rot4 Rotation, float aimAngle)
        {
            float angle = aimAngle - 90f + rotationOffset;
            bool flip = false;

            if (aimAngle > 20f && aimAngle < 160f)
            {
                angle += parent.def.equippedAngleOffset;
            }
            else if (aimAngle > 200f && aimAngle < 340f)
            {
                flip = true;
                angle -= 180f + parent.def.equippedAngleOffset;
            }
            else
            {
                angle += parent.def.equippedAngleOffset;
            }

            angle %= 360f;

            // Offset drawLoc slightly below parent def's position
            float yOffset = -0.001f; // Adjust this value as needed
            drawLoc.y += yOffset;

            Vector3 scale = new Vector3(parent.def.graphicData.drawSize.x, 1f, parent.def.graphicData.drawSize.y);
            Matrix4x4 matrix = Matrix4x4.TRS(drawLoc, Quaternion.AngleAxis(angle, Vector3.up), scale);

            DrawMesh(graphicInt, matrix, flip, Props.Altitude);

            if (LightsaberCore1Graphic != null)
            {
                DrawMesh(LightsaberCore1Graphic, matrix, flip, Props.Altitude);
            }
            if (LightsaberBlade2Graphic != null)
            {
                DrawMesh(LightsaberBlade2Graphic, matrix, flip, Props.Altitude);
            }
            if (LightsaberCore2Graphic != null)
            {
                DrawMesh(LightsaberCore2Graphic, matrix, flip, Props.Altitude);
            }
        }



        private static void DrawMesh(Graphic graphic, Matrix4x4 matrix, bool flip, float altitude)
        {
            if (graphic != null && graphic != BaseContent.BadGraphic)
            {
                Material material = graphic.MatSingle;
                Graphics.DrawMesh(flip ? MeshPool.plane10Flip : MeshPool.plane10, matrix, material, (int)altitude);
            }
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

        private void SetColors(Color lightsaberBlade1OverrideColor, Color lightsaberCore1Color, Color lightsaberBlade2Color, Color lightsaberCore2Color)
        {
            SetColor(lightsaberBlade1OverrideColor);
            SetLightsaberCore1Color(lightsaberCore1Color);
            SetLightsaberBlade2Color(lightsaberBlade2Color);
            SetLightsaberCore2Color(lightsaberCore2Color);
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
        public bool colorable = true; // Default to true if not specified

        public CompProperties_LightsaberBlade()
        {
            this.compClass = typeof(Comp_LightsaberBlade);
        }
    }
}
