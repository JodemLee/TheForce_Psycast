using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public Color hiltColorOneOverrideColor;
        public Color hiltColorTwoOverrideColor;

        private CompGlower compGlower;
        private bool dirty;
        private Map map;
        private IntVec3 prevPosition;
        private float curRadius;
        private bool hasPlayedSound = false;
        public bool IsThrowingWeapon = false;
        private bool colorsInitialized = false;
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
        public bool IsAnimatingNow => animationDeflectionTicks > 0;
        public CompProperties_LightsaberBlade Props => (CompProperties_LightsaberBlade)props;
        public readonly MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        public readonly MaterialPropertyBlock hiltMaterialPropertyBlock = new MaterialPropertyBlock();
        private float stanceRotation;
        private Vector3 drawOffset;
        public float CurrentRotation => stanceRotation;
        public Vector3 CurrentDrawOffset => drawOffset;
        public float bladeLengthCore1AndBlade1;
        public float bladeLengthCore2AndBlade2;
        public float MinBladeLength
        {
            get
            {
                if (props is CompProperties_LightsaberBlade bladeProps)
                {
                    return bladeProps.minBladeLength;
                }
                return 1f; // Default or fallback value
            }
        }

        public float MaxBladeLength
        {
            get
            {
                if (props is CompProperties_LightsaberBlade bladeProps)
                {
                    return bladeProps.maxBladeLength;
                }
                return 2.2f; // Default or fallback value
            }
        }
        public List<SoundDef> lightsaberSound => Props.lightsaberSound ?? new List<SoundDef>();
        public List<HiltDef> availablehilts => Props.availableHiltGraphics ?? new List<HiltDef>();
        public List<HiltPartDef> selectedHiltParts = new List<HiltPartDef>();
        public SoundDef selectedSoundEffect;
        public int selectedSoundIndex = 0;
        private bool hiltGraphicInitialized = false;
        private HiltDef _selectedHiltGraphic;
        public HiltDef selectedhiltgraphic
        {
            get => _selectedHiltGraphic;
            set => _selectedHiltGraphic = value;
        }
        private float scaleTimer;

        public Vector3 currentScaleForCore1AndBlade1 = Vector3.zero;
        public Vector3 currentScaleForCore2AndBlade2 = Vector3.zero;
        public Vector3 targetScaleForCore1AndBlade1;
        public Vector3 targetScaleForCore2AndBlade2;

        private int tickCounter = 0;
        private const int TicksPerGlowerUpdate = 60;

        private static float maxVibrate = 1.005f;
        public float minVibrate = 0.999f;
        public float vibrationrate = 0.00f;
        public float vibrationrate2 = 0.00f;

        public Vector3 currentDrawOffset; // Current offset for smooth transition
        public Vector3 targetDrawOffset;

        private CompEquippable compEquippable;
        public CompEquippable GetEquippable
        {
            get
            {
                if (compEquippable == null)
                {
                    CacheComps();  // Ensure comps are cached if GetEquippable is accessed prematurely
                }
                return compEquippable;
            }
        }

        public Pawn GetPawn
        {
            get
            {
                return GetEquippable?.verbTracker?.PrimaryVerb?.CasterPawn;
            }
        }

        public Graphic Graphic => GetOrCreateGraphic(ref graphicInt, Props?.graphicData, lightsaberBlade1OverrideColor, Props?.graphicData?.Graphic?.Shader ?? null);
        public Graphic LightsaberCore1Graphic => GetOrCreateGraphic(ref lightsaberCore1GraphicInt, Props?.lightsaberCore1GraphicData, lightsaberCore1OverrideColor, Props?.lightsaberCore1GraphicData?.Graphic?.Shader ?? null);
        public Graphic LightsaberBlade2Graphic => GetOrCreateGraphic(ref lightsaberBlade2GraphicInt, Props?.lightsaberBlade2GraphicData, lightsaberBlade2OverrideColor, Props?.lightsaberBlade2GraphicData?.Graphic?.Shader ?? null);
        public Graphic LightsaberCore2Graphic => GetOrCreateGraphic(ref lightsaberCore2GraphicInt, Props?.lightsaberCore2GraphicData, lightsaberCore2OverrideColor, Props?.lightsaberCore2GraphicData?.Graphic?.Shader ?? null);
        public Graphic LightsaberGlowGraphic => GetOrCreateGraphic(ref lightsaberGlowGraphicInt, Props?.lightsaberGlowGraphic, lightsaberBlade1OverrideColor, Props?.lightsaberGlowGraphic?.Graphic?.Shader ?? null);
        private Graphic cachedHiltGraphic;
        private bool needsUpdate = true;

        public void UpdateHiltGraphic()
        {
            if (parent.Graphic is Graphic_Hilts graphicHilts)
            {
                graphicHilts.MatSingleFor(parent);
            }
            parent.Notify_ColorChanged();
        }

        private void CacheComps()
        {
            var comps = parent.AllComps;
            for (int i = 0; i < comps.Count; i++)
            {
                var comp = comps[i];
                if (comp is CompEquippable equippable)
                {
                    compEquippable = equippable;
                }
            }
        }

        public void UpdateRotationForStance(float angle)
        {
            if (isFlipped)
            {
                float standardAngle = (angle - 45) % 360;
                if (standardAngle < 0) standardAngle += 360;

                float mirroredStandardAngle = (180 - standardAngle) % 360;
                if (mirroredStandardAngle < 0) mirroredStandardAngle += 360;

                float mirroredAngle = (mirroredStandardAngle + 45) % 360;
                if (mirroredAngle < 0) mirroredAngle += 360;
                stanceRotation = mirroredAngle;
            }
            else
            {
                stanceRotation = angle;
            }
        }

        public void UpdateDrawOffsetForStance(Vector3 offset)
        {
            drawOffset = new Vector3(isFlipped ? -offset.x : offset.x, offset.y, offset.z);
            targetDrawOffset = drawOffset;
        }

        public Pawn Wearer
        {
            get
            {
                return parent?.ParentHolder?.ParentHolder as Pawn;
            }
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            InitializeGlower();
            InitializeHilts();
            bladeLengthCore1AndBlade1 = Rand.Range(Props.minBladeLength, Props.maxBladeLength);
            bladeLengthCore2AndBlade2 = Rand.Range(Props.minBladeLength, Props.maxBladeLength);
            if (lightsaberSound != null && lightsaberSound.Count > 0)
            {
                int randomIndex = Rand.Range(0, lightsaberSound.Count); // Generate a random index within the list
                selectedSoundEffect = lightsaberSound[randomIndex];     // Set a random sound effect from the list
            }
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
            if (!colorsInitialized)
            {
                InitializeColors();
            }
            InitializeHilts();
            InitializeGlower();
            dirty = true;
            map = parent.MapHeld;
            LightsaberGlowManager.Instance.compGlowerToTick.Add(this);
            targetScaleForCore1AndBlade1 = new Vector3(bladeLengthCore1AndBlade1, 1f, bladeLengthCore1AndBlade1);
            targetScaleForCore2AndBlade2 = new Vector3(bladeLengthCore2AndBlade2, 1f, bladeLengthCore2AndBlade2);
            if (parent.Map != null)
            {
                //LightsaberSoundManager.Instance.RegisterLightsaberComponent(this);
            }
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
            CacheComps();
        }

        private bool sustainerActive = false;
        private Sustainer currentSustainer;


        private const int UpdateInterval = 10; // Adjust based on acceptable delay
        public override void CompTick()
        {
            if (parent.MapHeld == null)
            {
                return;
            }

            // Use a hash interval to reduce frequency of updates
            if (this.parent.IsHashIntervalTick(UpdateInterval))
            {
                if (map == null)
                {
                    map = parent.MapHeld;
                }
                // Update glow logic
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
        }

        public bool ShouldGlow()
        {
            if (Force_ModSettings.LightsaberRealGlow)
            {
                if (Wearer != null && parent.MapHeld != null)
                {
                    IntVec3 positionHeld = Wearer.Position;
                    if (positionHeld.InBounds(parent.MapHeld) && Wearer.IsCarryingWeaponOpenly())
                    {
                        return true;
                    }
                }
            }
            RemoveGlower(parent.MapHeld);
            dirty = false;
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

        public Graphic HiltGraphic
        {
            get
            {
                if (needsUpdate)
                {
                    if (selectedhiltgraphic == null || selectedhiltgraphic.graphicData == null)
                    {
                        cachedHiltGraphic = DefaultHiltGraphic();
                    }
                    else
                    {
                        cachedHiltGraphic = GetOrCreateHilt(ref cachedHiltGraphic, selectedhiltgraphic.graphicData, hiltColorOneOverrideColor, hiltColorTwoOverrideColor);
                    }
                    needsUpdate = false;
                }
                return cachedHiltGraphic;
            }
        }
        public void UpdateHiltGraphicCache()
        {
            needsUpdate = true;
        }

        public Graphic GetOrCreateHilt(ref Graphic graphicField, GraphicData graphicData, Color color, Color colortwo)
        {
            if (graphicData == null)
            {
                return DefaultHiltGraphic();
            }

            if (graphicField == null)
            {
                var primaryColor = color == hiltColorOneOverrideColor ? parent.DrawColor : color;
                var secondaryColor = colortwo == hiltColorTwoOverrideColor ? parent.DrawColorTwo : colortwo; // Use secondary color
                graphicField = graphicData.Graphic.GetColoredVersion(
                    selectedhiltgraphic.graphicData.Graphic.Shader,
                    primaryColor,
                    secondaryColor
                );
            }
            return graphicField;
        }

        private Graphic DefaultHiltGraphic()
        {
            return GetOrCreateHilt(ref cachedHiltGraphic, selectedhiltgraphic.graphicData, hiltColorOneOverrideColor, hiltColorTwoOverrideColor);  // Fallback to a safe default graphic
        }

        public Graphic GetOrCreateGraphic(ref Graphic graphicField, GraphicData graphicData, Color color, Shader shaderOverride)
        {
            if (graphicField == null && graphicData != null)
            {
                var newColor = color == Color.white ? parent.DrawColor : color;
                var shader = graphicData.Graphic.Shader ?? shaderOverride;
                graphicField = graphicData.Graphic.GetColoredVersion(shader, newColor, newColor);
            }
            return graphicField;
        }

        private void InitializeHilts()
        {
            if (!hiltGraphicInitialized)
            {
                selectedHiltParts = DefaultHiltParts();
                var pawn = Wearer;
                if (pawn != null)
                {
                    var modExt = pawn.kindDef.GetModExtension<ModExtension_LightsaberColors>();
                    if (modExt != null)
                    {
                        SetColorsFromModExtension(modExt);
                    }
                }
                if (selectedhiltgraphic == null && Props.availableHiltGraphics != null && Props.availableHiltGraphics.Any())
                {
                    selectedhiltgraphic = Props.availableHiltGraphics.RandomElement();
                }

                UpdateHiltGraphicCache();
                hiltGraphicInitialized = true;
            }
        }

        private void InitializeColors()
        {
            if (colorsInitialized)
            {
                Log.Message("Colors already initialized.");
                return;
            }
            var pawn = Wearer;
            if (pawn != null)
            {
                var modExt = pawn.kindDef.GetModExtension<ModExtension_LightsaberColors>();
                if (modExt != null)
                {
                    SetColorsFromModExtension(modExt);
                    colorsInitialized = true;
                    return;
                }
            }
            isFlipped = Rand.Chance(0.5f);
            bladeLengthCore1AndBlade1 = Rand.Range(Props.minBladeLength, Props.maxBladeLength);
            bladeLengthCore2AndBlade2 = Rand.Range(Props.minBladeLength, Props.maxBladeLength);
            targetScaleForCore1AndBlade1 = new Vector3(bladeLengthCore1AndBlade1, 1f, bladeLengthCore1AndBlade1);
            targetScaleForCore2AndBlade2 = new Vector3(bladeLengthCore2AndBlade2, 1f, bladeLengthCore2AndBlade2);
            SetFallbackColors();
            parent.Notify_ColorChanged();
            colorsInitialized = true;  // Mark as initialized
        }

        private void SetColorsFromModExtension(ModExtension_LightsaberColors modExt)
        {
            // Select a random blade color, core color, and glow color from the lists
            lightsaberBlade1OverrideColor = modExt.bladeColors.RandomElement();
            lightsaberCore1OverrideColor = modExt.coreColors.RandomElement();
            lightsaberBlade2OverrideColor = lightsaberBlade1OverrideColor;
            lightsaberCore2OverrideColor = lightsaberCore1OverrideColor;
            lightsaberGlowOverrideColor = lightsaberBlade1OverrideColor;
            hiltColorOneOverrideColor = StuffColorUtility.GetRandomColorFromStuffCategories(modExt.validStuffCategoriesHiltColorOne);
            hiltColorTwoOverrideColor = StuffColorUtility.GetRandomColorFromStuffCategories(modExt.validStuffCategoriesHiltColorTwo);
            bladeLengthCore1AndBlade1 = Rand.Range(Props.minBladeLength, Props.maxBladeLength);
            bladeLengthCore2AndBlade2 = Rand.Range(Props.minBladeLength, Props.maxBladeLength);
            if (modExt.preferredHilts != null && Props.availableHiltGraphics != null)
            {
                var matchingHilts = modExt.preferredHilts
                    .Where(hilt => Props.availableHiltGraphics.Contains(hilt))
                    .ToList();

                if (matchingHilts.Any())
                {
                    selectedhiltgraphic = matchingHilts.RandomElement();
                }
            }
            if (selectedhiltgraphic == null && Props.availableHiltGraphics != null && Props.availableHiltGraphics.Any())
            {
                selectedhiltgraphic = Props.availableHiltGraphics.RandomElement();
            }
        }

        private List<HiltPartDef> DefaultHiltParts()
        {
            return Enum.GetValues(typeof(HiltPartCategory))
                       .Cast<HiltPartCategory>()
                       .Select(category => DefDatabase<HiltPartDef>.AllDefsListForReading
                                          .Where(def => def.category == category)
                                          .RandomElement())
                       .Where(part => part != null)
                       .ToList();
        }


        private void SetFallbackColors()
        {
            // Get the selected crystal part from the hilt
            HiltPartDef selectedCrystal = GetSelectedHiltPart(HiltPartCategory.Crystal);

            // If a crystal part is selected, use its color directly
            if (selectedCrystal != null)
            {
                // Set the colors based on the selected crystal's color field
                lightsaberBlade1OverrideColor = selectedCrystal.color;
                lightsaberCore1OverrideColor = GetBlackOrWhiteCore();  // Assuming this method doesn't need modification
                lightsaberBlade2OverrideColor = lightsaberBlade1OverrideColor;
                lightsaberCore2OverrideColor = lightsaberCore1OverrideColor;
                lightsaberGlowOverrideColor = lightsaberBlade1OverrideColor;
            }
            else
            {
                lightsaberBlade1OverrideColor = GetRandomRGBColor();
                lightsaberCore1OverrideColor = GetBlackOrWhiteCore();
                lightsaberBlade2OverrideColor = lightsaberBlade1OverrideColor;
                lightsaberCore2OverrideColor = lightsaberCore1OverrideColor;
                lightsaberGlowOverrideColor = lightsaberBlade1OverrideColor;
            }
            hiltColorOneOverrideColor = GetRandomRGBColor();
            hiltColorTwoOverrideColor = GetRandomRGBColor();
            bladeLengthCore1AndBlade1 = Rand.Range(Props.minBladeLength, Props.maxBladeLength);
            bladeLengthCore2AndBlade2 = Rand.Range(Props.minBladeLength, Props.maxBladeLength);
        }

        private Color GetRandomRGBColor()
        {
            return Props.colorable ? new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value) : Color.white;
        }

        private Color GetBlackOrWhiteCore()
        {
            return Props.colorable ? (UnityEngine.Random.value < 0.9f ? Color.white : Color.black) : Color.white;
        }

        public MaterialPropertyBlock blade1MaterialPropertyBlock = new MaterialPropertyBlock();
        public MaterialPropertyBlock core1MaterialPropertyBlock = new MaterialPropertyBlock();
        public MaterialPropertyBlock blade2MaterialPropertyBlock = new MaterialPropertyBlock();
        public MaterialPropertyBlock core2MaterialPropertyBlock = new MaterialPropertyBlock();
        public MaterialPropertyBlock glowMaterialPropertyBlock = new MaterialPropertyBlock();

        public void SetLightsaberBlade1Color(Color color) => UpdateColor(ref lightsaberBlade1OverrideColor, color, () => UpdateGraphic(ref graphicInt, Props.graphicData, lightsaberBlade1OverrideColor, "_Lightsaber_Blade1Color"));
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

        public void SetHiltColorOne(Color color)
        {
            UpdateColor(ref hiltColorOneOverrideColor, color, UpdateHiltGraphics);
        }

        public void SetHiltColorTwo(Color color)
        {
            UpdateColor(ref hiltColorTwoOverrideColor, color, UpdateHiltGraphics);
        }

        public void UpdateHiltGraphics()
        {
            hiltMaterialPropertyBlock.Clear();
            SetColorInMaterialPropertyBlock(hiltMaterialPropertyBlock, "_Color", hiltColorOneOverrideColor);
            SetColorInMaterialPropertyBlock(hiltMaterialPropertyBlock, "_ColorTwo", hiltColorTwoOverrideColor);
        }

        private void UpdateGraphic(ref Graphic graphicField, GraphicData graphicData, Color color, string colorProperty)
        {
            if (graphicData != null)
            {
                var newColor = color == Color.white ? parent.DrawColor : color;
                var shader = graphicData.Graphic.Shader ?? ShaderDatabase.TransparentPostLight;
                graphicField = graphicData.Graphic.GetColoredVersion(shader, newColor, newColor);
                materialPropertyBlock.Clear();
                materialPropertyBlock.SetColor(colorProperty, newColor);
            }
        }

        private void SetColorInMaterialPropertyBlock(MaterialPropertyBlock block, string colorProperty, Color color)
        {
            block.SetColor(colorProperty, color);
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
            Scribe_Values.Look(ref colorsInitialized, "colorsInitialized", false);
            Scribe_Values.Look(ref bladeLengthCore1AndBlade1, nameof(bladeLengthCore1AndBlade1), 1f);
            Scribe_Values.Look(ref bladeLengthCore2AndBlade2, nameof(bladeLengthCore2AndBlade2), 1f);
            Scribe_Values.Look(ref hiltColorOneOverrideColor, nameof(hiltColorOneOverrideColor), Color.white);
            Scribe_Values.Look(ref hiltColorTwoOverrideColor, nameof(hiltColorTwoOverrideColor), Color.white);
            Scribe_Defs.Look(ref _selectedHiltGraphic, nameof(selectedhiltgraphic));
            Scribe_Collections.Look(ref selectedHiltParts, "selectedHiltParts", LookMode.Def);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                CacheComps();
            }
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                SetBladeLengths(bladeLengthCore1AndBlade1, bladeLengthCore2AndBlade2);
            }
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if (Props.colorable)
            {
                yield return new FloatMenuOption(
                    "Force_LightsaberCustomization".Translate(),
                    () => OpenColorPicker(selPawn)
                );
            }
        }

        public IEnumerable<Gizmo> EquippedGizmos()
        {
            if (Props.colorable && Wearer != null && Wearer.Drafted)
            {
                var pawn = GetPawn;
                if (pawn != null)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "Force_LightsaberCustomization".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Icons/Gizmo/LightsaberCustomization", true),
                        action = () => OpenColorPicker(pawn),
                    };
                }
            }
            if (Props.colorable && Wearer != null && !Wearer.Drafted && Force_ModSettings.lightsaberCustomizationUndrafted)
            {
                var pawn = GetPawn;
                if (pawn != null)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "Force_LightsaberCustomization".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Icons/Gizmo/LightsaberCustomization", true),
                        action = () => OpenColorPicker(pawn),
                    };
                }
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Props.colorable)
            {
                foreach (var current in base.CompGetGizmosExtra())
                {
                    yield return current;
                }

                foreach (var current in EquippedGizmos())
                {
                    yield return current;
                }
            }
        }

        private void OpenColorPicker(Pawn selPawn)
        {
            Find.WindowStack.Add(new Dialog_LightsaberColorPicker(selPawn, this, SetColors));
        }

        public void SetSoundEffect(SoundDef soundDef)
        {
            if (soundDef != null)
            {
                selectedSoundEffect = soundDef;
            }
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            targetScaleForCore1AndBlade1 = new Vector3(bladeLengthCore1AndBlade1, 1f, bladeLengthCore1AndBlade1);
            targetScaleForCore2AndBlade2 = new Vector3(bladeLengthCore2AndBlade2, 1f, bladeLengthCore2AndBlade2);
            ResetToZero();

            if (lightsaberSound != null && lightsaberSound.Count > 0 && selectedSoundEffect != null)
            {
                selectedSoundEffect.PlayOneShot(pawn);
            }
            if (!colorsInitialized)
            {
                InitializeColors();
                InitializeHilts();
            }
            else
            {
                return;
            }
        }

        public override void Notify_UsedWeapon(Pawn pawn)
        {
            LightsaberScorchMark(pawn);
        }

        public void LightsaberScorchMark(Pawn pawn)
        {
            float chanceToSpawnFleck = .15f;

            if (Rand.Value < chanceToSpawnFleck)
            {
                FleckDef fleckDef = Props.Fleck;
                if (fleckDef != null)
                {

                    IEnumerable<IntVec3> adjacentCells = GenAdj.CellsAdjacent8Way(pawn).Where(cell => cell.InBounds(pawn.Map));
                    List<IntVec3> validCells = adjacentCells.Where(cell => cell.Walkable(pawn.Map)).ToList();

                    if (validCells.Count > 0)
                    {
                        IntVec3 randomCell = validCells.RandomElement();
                        float randomRotation = Rand.Range(0f, 360f);
                        FleckCreationData fleckData = FleckMaker.GetDataStatic(randomCell.ToVector3(), pawn.Map, fleckDef);
                        fleckData.rotation = randomRotation;
                        pawn.Map.flecks.CreateFleck(fleckData);
                    }
                }
            }
        }

        private void SetColors(Color blade1Color, Color core1Color, Color blade2Color, Color core2Color)
        {
            SetLightsaberBlade1Color(blade1Color);
            SetLightsaberCore1Color(core1Color);
            SetLightsaberBlade2Color(blade2Color);
            SetLightsaberCore2Color(core2Color);
            SetLightsaberGlowColor(blade1Color);
        }

        public void SetBladeLengths(float length1, float length2)
        {
            bladeLengthCore1AndBlade1 = length1;
            bladeLengthCore2AndBlade2 = length2;

            // Update the target scales based on the new lengths
            targetScaleForCore1AndBlade1 = new Vector3(length1, 1f, length1);
            targetScaleForCore2AndBlade2 = new Vector3(length2, 1f, length2);
        }

        public void SetVibrationRate(float newLensFactor1, float newLensFactor2)
        {
            vibrationrate = newLensFactor1;
            vibrationrate2 = newLensFactor2;
        }

        public void UpdateScalingAndOffset()
        {

            scaleTimer += 5f / TicksPerGlowerUpdate;
            float t = Mathf.Clamp(scaleTimer, 0f, 1f);
            if (IsAnimatingNow)
            {
                currentScaleForCore1AndBlade1 = Vector3.Lerp(targetScaleForCore1AndBlade1, targetScaleForCore1AndBlade1, t);
                currentScaleForCore2AndBlade2 = Vector3.Lerp(targetScaleForCore2AndBlade2, targetScaleForCore2AndBlade2, t);
            }
            else
            {
                currentScaleForCore1AndBlade1 = Vector3.Lerp(Vector3.zero, targetScaleForCore1AndBlade1, t);
                currentScaleForCore2AndBlade2 = Vector3.Lerp(Vector3.zero, targetScaleForCore2AndBlade2, t);
            }
            currentDrawOffset = Vector3.Lerp(Vector3.zero, targetDrawOffset, t);
        }
        public void ResetToZero()
        {
            try
            {
                scaleTimer = 0f;
                currentScaleForCore1AndBlade1 = Vector3.zero;
                currentScaleForCore2AndBlade2 = Vector3.zero;
                currentDrawOffset = Vector3.zero;
            }
            catch (Exception ex)
            {

            }
        }

        public bool isFlipped = false;

        public void SetFlipped(bool flipped)
        {
            isFlipped = flipped;
        }

        public List<HiltPartDef> GetCurrentHiltParts()
        {
            return selectedHiltParts ?? new List<HiltPartDef>();
        }

        public HiltPartDef GetSelectedHiltPart(HiltPartCategory category)
        {
            if (selectedHiltParts == null) return null;
            return selectedHiltParts.FirstOrDefault(part => part.category == category);
        }
        public void AddHiltPart(HiltPartDef hiltPart)
        {
            if (!selectedHiltParts.Contains(hiltPart))
            {
                selectedHiltParts.Add(hiltPart);
            }
        }
        public void RemoveHiltPart(HiltPartDef hiltPart)
        {
            if (selectedHiltParts.Contains(hiltPart))
            {
                selectedHiltParts.Remove(hiltPart);
            }
        }
    }

    public class CompProperties_LightsaberBlade : CompProperties
    {
        public GraphicData graphicData;
        public GraphicData lightsaberCore1GraphicData;
        public GraphicData lightsaberBlade2GraphicData;
        public GraphicData lightsaberCore2GraphicData;
        public GraphicData lightsaberGlowGraphic;
        public List<HiltDef> availableHiltGraphics;
        public FleckDef Fleck;
        public float defaultBladeLength = 1.5f;
        public float maxBladeLength = 2.2f;
        public float minBladeLength = 1f;
        public AltitudeLayer altitudeLayer;
        public float Altitude => Altitudes.AltitudeFor(altitudeLayer);
        public bool colorable = true;
        public float overlightRadius = 2f;
        public List<SoundDef> lightsaberSound;
        public CompProperties_LightsaberBlade()
        {
            this.compClass = typeof(Comp_LightsaberBlade);
            lightsaberSound = new List<SoundDef>();
            availableHiltGraphics = new List<HiltDef>();
        }
    }

    public class ModExtension_LightsaberColors : DefModExtension
    {
        public List<Color> bladeColors = new List<Color> { Color.white };
        public List<Color> coreColors = new List<Color> { Color.white };
        public List<StuffCategoryDef> validStuffCategoriesHiltColorOne = new List<StuffCategoryDef>();
        public List<StuffCategoryDef> validStuffCategoriesHiltColorTwo = new List<StuffCategoryDef>();
        public List<HiltDef> preferredHilts = new List<HiltDef>();
    }
}