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
        private Graphic lightsaberGlowGraphicInt;

        public Color lightsaberBlade1OverrideColor;
        public Color lightsaberCore1OverrideColor;
        public Color lightsaberBlade2OverrideColor;
        public Color lightsaberCore2OverrideColor;
        public Color lightsaberGlowOverrideColor;

        private CompGlower compGlower;
        private bool dirty;
        private Map map;
        private IntVec3 prevPosition;
        private float curRadius;

        private bool hasPlayedSound = false;

        public bool IsThrowingWeapon = false;


        public float lastInterceptAngle;
        public float LastInterceptAngle
        {
            get => lastInterceptAngle;
            set => lastInterceptAngle = value;
        }


        private int animationDeflectionTicks;
        public int AnimationDeflectionTicks
        {
            set => animationDeflectionTicks = value;
            get => animationDeflectionTicks;
        }
        public bool IsAnimatingNow => animationDeflectionTicks >= 0;

        public CompProperties_LightsaberBlade Props => (CompProperties_LightsaberBlade)props;

        private readonly MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();

        private float stanceRotation;
        private Vector3 drawOffset;

        public float CurrentRotation => stanceRotation;
        public Vector3 CurrentDrawOffset => drawOffset;

        public void UpdateRotationForStance(float angle)
        {
            stanceRotation = angle;
        }

        public void UpdateDrawOffsetForStance(Vector3 offset)
        {
            drawOffset = offset;
        }

        public Pawn Wearer
        {
            get
            {
                if (parent.ParentHolder is Pawn_EquipmentTracker { pawn: var pawn })
                {
                    return pawn;
                }
                if (parent.ParentHolder is Pawn_ApparelTracker { pawn: var pawn2 })
                {
                    return pawn2;
                }
                if (parent.ParentHolder is Pawn_InventoryTracker { pawn: var pawn3 })
                {
                    return pawn3;
                }
                if (!(parent.ParentHolder is Pawn_CarryTracker { pawn: var pawn4 }))
                {
                    return null;
                }
                return pawn4;
            }
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            InitializeColors();
            InitializeGlower();
        }

        private void InitializeGlower()
        {
            dirty = true;
            map = parent.MapHeld;
            if (map != null)
            {
                LightsaberGlowManager.Instance.compGlowerToTick.Add(this);
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            InitializeGlower();
            dirty = true;
            map = parent.MapHeld;
            LightsaberGlowManager.Instance.compGlowerToTick.Add(this);
        }

        public override void PostDeSpawn(Map map)
        {
            RemoveGlower(map);
            base.PostDeSpawn(map);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            if (LightsaberGlowManager.Instance.compGlowerToTick.Contains(this))
            {
                LightsaberGlowManager.Instance.compGlowerToTick.Remove(this);
            }
            RemoveGlower(previousMap);
            base.PostDestroy(mode, previousMap);
        }

        public override void PostPostMake()
        {
            base.PostPostMake();
            LightsaberGlowManager.Instance.compGlowerToTick.Add(this);
        }

        private bool isDeferredInitializationPending = true;

        public void Tick()
        {
            if (parent.MapHeld == null)
            {
                return;
            }
            var pawn = Wearer;
            if (pawn != null)
            {
                if (isDeferredInitializationPending)
                {
                    InitializeColors();
                    isDeferredInitializationPending = false;
                    return;
                }
            }

            if (map == null)
            {
                map = parent.MapHeld;
            }
            if (prevPosition != parent.PositionHeld)
            {
                prevPosition = parent.PositionHeld;
                dirty = true;
            }
            bool flag = ShouldGlow();
            if ((compGlower == null && flag) || !flag)
            {
                dirty = true;
            }
            else if (compGlower != null && curRadius != GetRadius())
            {
                dirty = true;
            }
            if (dirty)
            {
                UpdateGlower();
                dirty = false;
            }
        }



        private bool ShouldGlow()
        {
            if (Force_ModSettings.LightsaberRealGlow)
            {
                bool result = false;
                IntVec3 positionHeld = parent.PositionHeld;
                if (positionHeld.InBounds(parent.MapHeld))
                {
                    if (Wearer != null && Wearer.IsCarryingWeaponOpenly() && Wearer.equipment?.Primary == parent)
                    {
                        result = true;
                    }
                }
                return result;
            }
            return false;
        }

        private void UpdateGlower()
        {
            IntVec3 position = GetPosition();
            Map mapHeld = parent.MapHeld;
            if (mapHeld != null && position.IsValid)
            {
                RemoveGlower(parent.MapHeld);
                if (ShouldGlow())
                {
                    compGlower = new CompGlower();
                    ThingWithComps thingWithComps = GetParent();
                    ColorInt glowColor = new ColorInt(lightsaberBlade1OverrideColor);
                    curRadius = GetRadius();
                    float glowRadius = Force_ModSettings.glowRadius;
                    compGlower.Initialize(new CompProperties_Glower
                    {
                        glowColor = glowColor,
                        glowRadius = glowRadius,
                        overlightRadius = Props.overlightRadius
                    });

                    compGlower.parent = thingWithComps;
                    map.mapDrawer.MapMeshDirty(position, MapMeshFlagDefOf.Things);
                    map.glowGrid.RegisterGlower(compGlower);
                }
            }
        }

        private IntVec3 GetPosition()
        {
            if (Wearer != null)
            {
                return Wearer.Position;
            }
            return parent.PositionHeld;
        }

        private ThingWithComps GetParent()
        {
            if (Wearer != null)
            {
                return Wearer;
            }
            return parent;
        }


        private void RemoveGlower(Map prevMap)
        {
            if (prevMap != null && compGlower != null)
            {
                try
                {
                    prevMap.glowGrid.DeRegisterGlower(compGlower);
                }
                catch { }
                compGlower = null;
            }
        }

        private float GetRadius()
        {
            float num = 0f;
            if (num == 0f)
            {
                return Force_ModSettings.glowRadius;
            }
            return num;
        }

        public Graphic Graphic => GetOrCreateGraphic(ref graphicInt, Props.graphicData, lightsaberBlade1OverrideColor);

        public Graphic LightsaberCore1Graphic => GetOrCreateGraphic(ref lightsaberCore1GraphicInt, Props.lightsaberCore1GraphicData, lightsaberCore1OverrideColor);

        public Graphic LightsaberBlade2Graphic => GetOrCreateGraphic(ref lightsaberBlade2GraphicInt, Props.lightsaberBlade2GraphicData, lightsaberBlade2OverrideColor);

        public Graphic LightsaberCore2Graphic => GetOrCreateGraphic(ref lightsaberCore2GraphicInt, Props.lightsaberCore2GraphicData, lightsaberCore2OverrideColor);

        public Graphic LightsaberGlowGraphic => GetOrCreateGraphic(ref lightsaberGlowGraphicInt, Props.lightsaberGlowGraphic, lightsaberBlade1OverrideColor);

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
            var pawn = Wearer;
            if (pawn != null)
            {

                var modExt = pawn.kindDef.GetModExtension<ModExtension_LightsaberColors>();
                if (modExt != null)
                {

                    // Select a random blade color, core color, and glow color from the lists
                    lightsaberBlade1OverrideColor = modExt.bladeColors.RandomElement();
                    lightsaberCore1OverrideColor = modExt.coreColors.RandomElement();
                    lightsaberBlade2OverrideColor = lightsaberBlade1OverrideColor;
                    lightsaberCore2OverrideColor = lightsaberCore1OverrideColor;
                    lightsaberGlowOverrideColor = lightsaberBlade1OverrideColor;
                    return;
                }
            }
            // Fallback to random colors if no ModExtension is present
            lightsaberBlade1OverrideColor = GetRandomRGBColor();
            lightsaberCore1OverrideColor = GetBlackOrWhiteCore();
            lightsaberBlade2OverrideColor = lightsaberBlade1OverrideColor;
            lightsaberCore2OverrideColor = lightsaberCore1OverrideColor;
            lightsaberGlowOverrideColor = lightsaberBlade1OverrideColor;
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

        public void SetLightsaberGlowColor(Color color) => UpdateColor(ref lightsaberGlowOverrideColor, color, () => UpdateGraphic(ref lightsaberGlowGraphicInt, Props.lightsaberGlowGraphic, lightsaberGlowOverrideColor, "_Lightsaber_GlowColor"));

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
            Scribe_Values.Look(ref lightsaberGlowOverrideColor, nameof(lightsaberGlowOverrideColor), Color.white);
            LightsaberGlowManager.Instance.compGlowerToTick.Add(this);
            Scribe_Values.Look(ref stanceRotation, "stanceRotation", 0f);
            Scribe_Values.Look(ref drawOffset, "drawOffset", Vector3.zero);

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
            SetLightsaberGlowColor(blade1Color);
        }
    }

    public class CompProperties_LightsaberBlade : CompProperties
    {
        public GraphicData graphicData;
        public GraphicData lightsaberCore1GraphicData;
        public GraphicData lightsaberBlade2GraphicData;
        public GraphicData lightsaberCore2GraphicData;
        public GraphicData lightsaberGlowGraphic;
        public AltitudeLayer altitudeLayer;
        public float Altitude => Altitudes.AltitudeFor(altitudeLayer);
        public bool colorable = true;
        public float overlightRadius = 2f;  // Example overlight radius

        public CompProperties_LightsaberBlade()
        {
            this.compClass = typeof(Comp_LightsaberBlade);
        }
    }

    public class ModExtension_LightsaberColors : DefModExtension
    {
        public List<Color> bladeColors = new List<Color> { Color.white };
        public List<Color> coreColors = new List<Color> { Color.white };
    }
}