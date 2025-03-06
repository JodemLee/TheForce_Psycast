using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using TheForce_Psycast.Lightsabers;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using Text = Verse.Text;

namespace TheForce_Psycast
{
    [StaticConstructorOnStartup]
    public class Dialog_LightsaberColorPicker : Window
    {
        private readonly Pawn pawn;
        private const int MinValue = 0;
        private const int MaxValue = 255;
        private const float WindowWidth = 900f;
        private const float WindowHeight = 450f;
        private const float LabelWidth = 40f;
        private const float SliderWidth = 180f;
        private const float InputFieldWidth = 60f;
        private const float ButtonWidth = 80f;
        private const float ButtonHeight = 30f;
        private const float ButtonMargin = 1f;
        private const float ButtonSpacing = 10f;

        private int lightsaberBlade1OverrideColorRed, lightsaberBlade1OverrideColorGreen, lightsaberBlade1OverrideColorBlue, lightsaberBlade1OverrideColorAlpha;
        private int lightsaberCore1Red, lightsaberCore1Green, lightsaberCore1Blue, lightsaberCore1Alpha;
        private int lightsaberBlade2Red, lightsaberBlade2Green, lightsaberBlade2Blue, lightsaberBlade2Alpha;
        private int lightsaberCore2Red, lightsaberCore2Green, lightsaberCore2Blue, lightsaberCore2Alpha;

        private readonly Comp_LightsaberBlade lightsaberBlade;
        private readonly Action<Color, Color, Color, Color> confirmAction;

        private static Color lightsaberBlade1OverrideColor = Color.white;
        private static Color lightsaberCore1CurrentColor = Color.white;
        private static Color lightsaberBlade2CurrentColor = Color.white;
        private static Color lightsaberCore2CurrentColor = Color.white;

        private static Texture2D outerglowTexture;
        private static Texture2D innercoreTexture;
        private static Texture2D dualglow1texture;
        private static Texture2D dualcore1Texture;
        private static Texture2D dualglow2texture;
        private static Texture2D dualcore2Texture;

        private enum Tab { RGBSelector, Hilts }
        private Tab currentTab = Tab.RGBSelector;

        public Dialog_LightsaberColorPicker(Pawn pawn, Comp_LightsaberBlade lightsaberBlade, Action<Color, Color, Color, Color> confirmAction)
        {
            this.pawn = pawn;
            this.lightsaberBlade = lightsaberBlade;
            this.confirmAction = confirmAction;

            if (outerglowTexture == null)
            {
                if (lightsaberBlade.Graphic != null) outerglowTexture = lightsaberBlade.Graphic.MatSingle.mainTexture as Texture2D;
                if (lightsaberBlade.LightsaberCore1Graphic != null) innercoreTexture = lightsaberBlade.LightsaberCore1Graphic.MatSingle.mainTexture as Texture2D;
            }

            if (dualglow1texture == null)
            {
                if (lightsaberBlade.LightsaberBlade2Graphic != null) dualglow1texture = lightsaberBlade.Graphic.MatSingle.mainTexture as Texture2D;
                if (lightsaberBlade.LightsaberCore2Graphic != null) dualcore1Texture = lightsaberBlade.LightsaberCore1Graphic.MatSingle.mainTexture as Texture2D;
                if (lightsaberBlade.LightsaberCore2Graphic != null) dualcore2Texture = lightsaberBlade.LightsaberCore2Graphic.MatSingle.mainTexture as Texture2D;
                if (lightsaberBlade.LightsaberBlade2Graphic != null) dualglow2texture = lightsaberBlade.LightsaberBlade2Graphic.MatSingle.mainTexture as Texture2D;
            }

            if (lightsaberBlade.Graphic != null) SyncBlade1Color(lightsaberBlade.Graphic.color);
            if (lightsaberBlade.LightsaberCore1Graphic != null) SyncCore1Color(lightsaberBlade.LightsaberCore1Graphic.color);
            if (lightsaberBlade.LightsaberBlade2Graphic != null) SyncBlade2Color(lightsaberBlade.LightsaberBlade2Graphic.color);
            if (lightsaberBlade.LightsaberCore2Graphic != null) SyncCore2Color(lightsaberBlade.LightsaberCore2Graphic.color);

            UpdateCurrentColor();
            closeOnClickedOutside = false;
            doCloseX = true;
            doCloseButton = true;
            draggable = true;
            absorbInputAroundWindow = false;
            preventCameraMotion = false;
        }

        public override Vector2 InitialSize => new Vector2(WindowWidth, CalculateWindowHeight());

        private float CalculateWindowHeight()
        {
            float height = 40f;
            height = 725f + ButtonHeight;
            return Mathf.Max(height, WindowHeight);
        }

        public override void DoWindowContents(Rect inRect)
        {
            DrawHilt(inRect);
            DrawTabs(inRect);

            switch (currentTab)
            {
                case Tab.RGBSelector: DrawRGBSelector(inRect); break;
                case Tab.Hilts: DrawHiltSelector(inRect); break;
            }
        }

        private void DrawTabs(Rect inRect)
        {
            float tabWidth = inRect.width / Enum.GetValues(typeof(Tab)).Length;

            foreach (Tab tab in Enum.GetValues(typeof(Tab)))
            {
                Rect tabRect = new Rect(inRect.x + (float)tab * tabWidth, inRect.y, tabWidth, 30f);
                string label = GetTabLabel(tab);
                float labelHeight = Text.CalcHeight(label, tabWidth - 10f);
                if (labelHeight > 30f) tabRect.height = labelHeight + 10f;
                if (Widgets.ButtonText(tabRect, label, true, true, true)) currentTab = tab;
            }
        }

        private string GetTabLabel(Tab tab)
        {
            switch (tab)
            {
                case Tab.RGBSelector: return "ForceTab_RGB".Translate();
                default: return tab.ToString();
            }
        }

        private void DrawHilt(Rect inRect)
        {
            float centerX = inRect.x + inRect.width / 2;
            float centerY = inRect.y + inRect.height / 2;

            if (lightsaberBlade == null || lightsaberBlade.Props.availableHiltGraphics == null || lightsaberBlade.Props.availableHiltGraphics.Count == 0)
            {
                Widgets.Label(new Rect(inRect.x, inRect.y + 50f, inRect.width, 30f), "No hilts available.");
                return;
            }

            string currentHiltLabel = lightsaberBlade.selectedhiltgraphic?.label ?? "No hilt selected";
            Text.Font = GameFont.Medium;
            Vector2 labelSize = Text.CalcSize(currentHiltLabel);
            float labelX = inRect.x + (inRect.width - labelSize.x) / 2;
            Rect labelRect = new Rect(labelX, inRect.y + 60f, labelSize.x, labelSize.y);
            Widgets.Label(labelRect, currentHiltLabel);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;

            float hiltX = centerX - 350f;
            float hiltY = centerY - 290f;
            Rect hiltRect = new Rect(hiltX, hiltY, 550f, 550f);

            if (lightsaberBlade.LightsaberBlade2Graphic != null)
            {
                float hiltX2 = hiltX + 75f;
                Rect hiltRect2 = new Rect(hiltX2, hiltY, 550f, 550f);
                Matrix4x4 originalMatrix2 = GUI.matrix;
                Vector2 pivotPoint2 = new Vector2(hiltRect2.x + hiltRect2.width / 2, hiltRect2.y + hiltRect2.height / 2);
                GUIUtility.RotateAroundPivot(-135f, pivotPoint2);
                if (currentTab == Tab.RGBSelector)
                {
                    GUI.color = lightsaberBlade1OverrideColor;
                    GUI.DrawTexture(hiltRect2, dualglow1texture, ScaleMode.StretchToFill);
                    GUI.color = lightsaberCore1CurrentColor;
                    GUI.DrawTexture(hiltRect2, dualcore1Texture, ScaleMode.StretchToFill);
                    GUI.color = lightsaberBlade2CurrentColor;
                    GUI.DrawTexture(hiltRect2, dualglow2texture, ScaleMode.StretchToFill);
                    GUI.color = lightsaberCore2CurrentColor;
                    GUI.DrawTexture(hiltRect2, dualcore2Texture, ScaleMode.StretchToFill);
                }

                var mat = lightsaberBlade.selectedhiltgraphic.graphicData.Graphic.MatSingle;
                GUI.color = Color.white * 2;
                MaterialRequest materialRequest = new MaterialRequest
                {
                    mainTex = mat.mainTexture,
                    maskTex = mat.GetMaskTexture(),
                    shader = mat.shader,
                    color = lightsaberBlade.hiltColorOneOverrideColor,
                    colorTwo = lightsaberBlade.hiltColorTwoOverrideColor
                };
                Material material = MaterialPool.MatFrom(materialRequest);
                GenUI.DrawTextureWithMaterial(hiltRect2, lightsaberBlade.selectedhiltgraphic.graphicData.Graphic.MatSingle.mainTexture, material);
                GUI.matrix = originalMatrix2;
                GUI.color = Color.white;
            }
            else
            {
                GUI.color = Color.white;

                if (lightsaberBlade.selectedhiltgraphic != null)
                {
                    Matrix4x4 originalMatrix = GUI.matrix;
                    Vector2 pivotPoint = new Vector2(hiltRect.x + hiltRect.width / 2, hiltRect.y + hiltRect.height / 2);
                    GUIUtility.RotateAroundPivot(-135f, pivotPoint);

                    if (currentTab == Tab.RGBSelector)
                    {
                        if (lightsaberBlade.Graphic != null)
                        {
                            GUI.color = lightsaberBlade1OverrideColor;
                            GUI.DrawTexture(hiltRect, outerglowTexture, ScaleMode.StretchToFill);
                            GUI.color = lightsaberCore1CurrentColor;
                            GUI.DrawTexture(hiltRect, innercoreTexture, ScaleMode.StretchToFill);
                        }
                    }

                    if (lightsaberBlade.selectedhiltgraphic != null && lightsaberBlade.selectedhiltgraphic.graphicData != null && lightsaberBlade.selectedhiltgraphic.graphicData.Graphic.MatSingle != null)
                    {
                        var mat = lightsaberBlade.selectedhiltgraphic.graphicData.Graphic.MatSingle;
                        GUI.color = Color.white * 2;
                        MaterialRequest materialRequest = new MaterialRequest
                        {
                            mainTex = mat.mainTexture,
                            maskTex = mat.GetMaskTexture(),
                            shader = mat.shader,
                            color = lightsaberBlade.hiltColorOneOverrideColor,
                            colorTwo = lightsaberBlade.hiltColorTwoOverrideColor
                        };
                        Material material = MaterialPool.MatFrom(materialRequest);
                        GenUI.DrawTextureWithMaterial(hiltRect, mat.mainTexture, material);
                    }
                    else
                    {
                        if (lightsaberBlade.selectedhiltgraphic == null) GUI.DrawTexture(hiltRect, lightsaberBlade.parent.Graphic.MatSingle.mainTexture);
                    }

                    GUI.matrix = originalMatrix;
                    GUI.color = Color.white;
                }
            }
        }

        private void DrawRGBSelector(Rect inRect)
        {
            float rightPanelX = inRect.xMax - 220f;
            float rowOffset = inRect.y + 40f;
            float rowOffset2nd = inRect.y + 385;
            Rect bladeLengthAdjusterRect = new Rect(inRect.x, inRect.y, inRect.width / 2, 150f);
            Rect soundAdjusterRect = new Rect(inRect.x, 0f, inRect.width / 2, 150f);
            DrawBladeLengthAdjuster(bladeLengthAdjusterRect);
            DrawSoundSelector(soundAdjusterRect, lightsaberBlade, rowOffset2nd);

            if (lightsaberBlade.Graphic != null) DrawColorPicker(ref rowOffset, ref lightsaberBlade1OverrideColorRed, ref lightsaberBlade1OverrideColorGreen, ref lightsaberBlade1OverrideColorBlue, ref lightsaberBlade1OverrideColorAlpha, "Blade", rightPanelX);
            if (lightsaberBlade.LightsaberCore1Graphic != null) DrawColorPicker(ref rowOffset, ref lightsaberCore1Red, ref lightsaberCore1Green, ref lightsaberCore1Blue, ref lightsaberCore1Alpha, "Core 1", rightPanelX);
            if (lightsaberBlade.LightsaberBlade2Graphic != null) DrawColorPicker(ref rowOffset2nd, ref lightsaberBlade2Red, ref lightsaberBlade2Green, ref lightsaberBlade2Blue, ref lightsaberBlade2Alpha, "Blade 2", rightPanelX);
            if (lightsaberBlade.LightsaberCore2Graphic != null) DrawColorPicker(ref rowOffset2nd, ref lightsaberCore2Red, ref lightsaberCore2Green, ref lightsaberCore2Blue, ref lightsaberCore2Alpha, "Core 2", rightPanelX);

            rowOffset += 30f;
            Rect presetRect = new Rect(inRect.x + 20f, 450f, inRect.width - 40f, 120f);
            DrawPresets(presetRect);
            UpdateCurrentColor();
        }

        private void DrawRandomizeButton(float xOffset, float rowOffset, ref int red, ref int green, ref int blue)
        {
            Rect buttonRect = new Rect(xOffset + LabelWidth + 10f, rowOffset, 100f, 22f);
            if (Widgets.ButtonText(buttonRect, "Randomize")) RandomizeRGBValues(ref red, ref green, ref blue);
        }

        private void RandomizeRGBValues(ref int red, ref int green, ref int blue)
        {
            red = UnityEngine.Random.Range(0, 256);
            green = UnityEngine.Random.Range(0, 256);
            blue = UnityEngine.Random.Range(0, 256);
        }

        private void DrawColorPicker(ref float rowOffset, ref int colorRed, ref int colorGreen, ref int colorBlue, ref int colorAlpha, string label, float rightPanelX)
        {
            DrawLabel(rightPanelX, rowOffset, label);
            DrawRandomizeButton(rightPanelX, rowOffset, ref colorRed, ref colorGreen, ref colorBlue);
            rowOffset += 22f;

            DrawSliderWithInput(ref rowOffset, ref colorRed, "Red", rightPanelX);
            DrawSliderWithInput(ref rowOffset, ref colorGreen, "Green", rightPanelX);
            DrawSliderWithInput(ref rowOffset, ref colorBlue, "Blue", rightPanelX);
            DrawSliderWithInput(ref rowOffset, ref colorAlpha, "Alpha", rightPanelX);

            UpdateLightsaberColor(label, colorRed, colorGreen, colorBlue, colorAlpha);
        }

        private void DrawLabel(float x, float y, string label)
        {
            Widgets.Label(new Rect(x, y, 200f, 22f), label + ":");
        }

        private void DrawSliderWithInput(ref float rowOffset, ref int value, string label, float xOffset)
        {
            Rect labelRect = new Rect(xOffset, rowOffset, LabelWidth, 22f);
            Rect sliderRect = new Rect(xOffset + LabelWidth, rowOffset, SliderWidth, 22f);
            Rect inputFieldRect = new Rect(xOffset + LabelWidth - 120f, rowOffset, InputFieldWidth, 22f);

            Text.Font = GameFont.Tiny;
            Widgets.Label(labelRect, label);
            Text.Font = GameFont.Small;

            value = Mathf.RoundToInt(Widgets.HorizontalSlider(sliderRect, value, MinValue, MaxValue));
            string buffer = value.ToString();
            Widgets.TextFieldNumeric(inputFieldRect, ref value, ref buffer, MinValue, MaxValue);

            rowOffset += 22f;
        }

        private void UpdateLightsaberColor(string label, int red, int green, int blue, int alpha)
        {
            Color color = new Color(red / 255f, green / 255f, blue / 255f, alpha / 255f);

            switch (label)
            {
                case "Blade":
                    lightsaberBlade.SetLightsaberBlade1Color(color);
                    lightsaberBlade.SetLightsaberGlowColor(color);
                    break;
                case "Core 1":
                    lightsaberBlade.SetLightsaberCore1Color(color);
                    break;
                case "Blade 2":
                    lightsaberBlade.SetLightsaberBlade2Color(color);
                    break;
                case "Core 2":
                    lightsaberBlade.SetLightsaberCore2Color(color);
                    break;
            }
        }

        private void DrawPresets(Rect inRect)
        {
            HiltPartDef selectedCrystal = lightsaberBlade.GetSelectedHiltPart(HiltPartCategory.Crystal);

            Dictionary<string, Color> presets = new Dictionary<string, Color>
            {
                { "Blue", new Color(0, 0, 1) },
                { "Red", new Color(1, 0, 0) },
                { "Green", new Color(0, 1, 0) },
                { "Yellow", new Color(1, 1, 0) },
                { "Purple", new Color(0.5f, 0, 0.5f) },
                { "Orange", new Color(1, .64f, 0) },
                { "White", new Color(1, 1, 1) },
                { "Crystal", selectedCrystal.color}
            };

            if (ModsConfig.IdeologyActive)
            {
                presets.Add("Ideo", pawn?.Ideo?.Color ?? Color.white);
                presets.Add("Favorite", pawn?.story?.favoriteColor ?? Color.white);
            }

            float buttonWidth = 75f;
            float buttonHeight = 30f;
            float buttonSpacing = 10f;
            float startY = inRect.yMax - (buttonHeight + buttonSpacing) * (presets.Count / (inRect.width / (buttonWidth + buttonSpacing))) - buttonSpacing + 95f;
            float currentX = inRect.x;
            float currentY = startY;

            foreach (var preset in presets)
            {
                Rect buttonRect = new Rect(currentX, currentY, buttonWidth, buttonHeight);
                if (currentX + buttonWidth > inRect.xMax)
                {
                    currentX = inRect.x;
                    currentY += buttonHeight + buttonSpacing;
                    buttonRect = new Rect(currentX, currentY, buttonWidth, buttonHeight);
                }

                if (Widgets.ButtonText(buttonRect, preset.Key))
                {
                    Color tempBladeColor = preset.Value;
                    confirmAction(tempBladeColor, lightsaberCore1CurrentColor, tempBladeColor, lightsaberCore2CurrentColor);
                    UpdateCurrentColor();
                    SyncToColor(tempBladeColor, lightsaberCore1CurrentColor, tempBladeColor, lightsaberCore2CurrentColor);
                }

                currentX += buttonWidth + buttonSpacing;
            }
        }

        private void DrawBladeLengthAdjuster(Rect inRect)
        {
            if (lightsaberBlade == null) return;

            float rowOffset = 35f;
            rowOffset = DrawBladeLengthSlider("Force_VariableBlade1", ref lightsaberBlade.bladeLengthCore1AndBlade1, inRect, rowOffset);
            if (lightsaberBlade.LightsaberBlade2Graphic == null) rowOffset += 35f;
            rowOffset = DrawVibrationSlider("Force_Vibration1", ref lightsaberBlade.vibrationrate, inRect, rowOffset);
            if (lightsaberBlade.LightsaberBlade2Graphic != null)
            {
                rowOffset = DrawBladeLengthSlider("Force_VariableBlade2", ref lightsaberBlade.bladeLengthCore2AndBlade2, inRect, rowOffset);
                rowOffset = DrawVibrationSlider("Force_Vibration2", ref lightsaberBlade.vibrationrate2, inRect, rowOffset);
            }
            if (Widgets.ButtonText(new Rect(0f, rowOffset, 100f, ButtonHeight), "Apply")) ApplyBladeLength();
        }

        private float DrawBladeLengthSlider(string labelKey, ref float bladeLength, Rect inRect, float rowOffset)
        {
            Widgets.Label(new Rect(0f, rowOffset, 200f, 22f), labelKey.Translate());
            bladeLength = Widgets.HorizontalSlider(new Rect(0f, rowOffset + 25f, inRect.width - 80f, 20f), bladeLength, lightsaberBlade.MinBladeLength, lightsaberBlade.MaxBladeLength);
            Widgets.Label(new Rect(inRect.width - 100f, rowOffset + 45f, 100f, 22f), $"{bladeLength:F2} m");
            lightsaberBlade.SetBladeLengths(lightsaberBlade.bladeLengthCore1AndBlade1, lightsaberBlade.bladeLengthCore2AndBlade2);
            return rowOffset + 35f;
        }

        private float DrawVibrationSlider(string labelKey, ref float lensFactor, Rect inRect, float rowOffset)
        {
            Widgets.Label(new Rect(0f, rowOffset, 200f, 22f), labelKey.Translate());
            lensFactor = Widgets.HorizontalSlider(new Rect(0f, rowOffset + 25f, inRect.width - 80f, 20f), lensFactor, 0.00f, 0.05f);
            Widgets.Label(new Rect(inRect.width - 100f, rowOffset + 45f, 100f, 22f), $"{lensFactor:F2} Hz");
            lightsaberBlade.SetVibrationRate(lightsaberBlade.vibrationrate, lightsaberBlade.vibrationrate2);
            return rowOffset + 70f;
        }

        private void ApplyBladeLength()
        {
            lightsaberBlade.SetBladeLengths(lightsaberBlade.bladeLengthCore1AndBlade1, lightsaberBlade.bladeLengthCore2AndBlade2);
            Close();
        }

        private Vector2 scrollPosition = Vector2.zero;

        public void DrawSoundSelector(Rect inRect, Comp_LightsaberBlade lightsaberBlade, float rowOffset)
        {
            if (lightsaberBlade.lightsaberSound != null && lightsaberBlade.lightsaberSound.Count > 0)
            {
                List<SoundDef> soundOptions = lightsaberBlade.lightsaberSound;

                const float padding = 10f;
                const float labelHeight = 22f;
                const float optionHeight = 25f;

                Widgets.DrawMenuSection(new Rect(inRect.x + padding, rowOffset - padding / 2, inRect.width, inRect.height));
                Text.Font = GameFont.Small;
                Widgets.Label(new Rect(inRect.x + padding * 2, rowOffset, inRect.width, labelHeight), "Select Sound Effect");
                Text.Font = GameFont.Small;

                float soundOptionsHeight = soundOptions.Count * optionHeight + padding;
                float scrollViewHeight = Math.Min(soundOptionsHeight, inRect.height - labelHeight - padding);

                Rect scrollViewRect = new Rect(inRect.x + padding, rowOffset + labelHeight, inRect.width - padding, scrollViewHeight);
                Rect scrollContentRect = new Rect(0f, 0f, inRect.width - padding * 3, soundOptionsHeight);

                Widgets.BeginScrollView(scrollViewRect, ref scrollPosition, scrollContentRect);

                for (int i = 0; i < soundOptions.Count; i++)
                {
                    Rect optionRect = new Rect(0f, i * optionHeight, scrollContentRect.width, optionHeight);
                    bool isSelected = lightsaberBlade.selectedSoundIndex == i;
                    if (Widgets.RadioButtonLabeled(optionRect, soundOptions[i].label, isSelected))
                    {
                        if (!isSelected)
                        {
                            lightsaberBlade.selectedSoundIndex = i;
                            soundOptions[i].PlayOneShot(lightsaberBlade.parent);
                            lightsaberBlade.SetSoundEffect(soundOptions[i]);
                        }
                    }

                    if (optionRect.Contains(Event.current.mousePosition)) Widgets.DrawHighlight(optionRect);
                }

                Widgets.EndScrollView();
            }
        }

        private void DrawHiltSelector(Rect inRect)
        {
            float buttonWidth = 120f;
            float buttonHeight = 30f;
            float buttonSpacing = 10f;
            float colorBoxSize = 60f;
            float colorBoxSpacing = 10f;
            float arrowButtonSize = 50f;
            float buttonYOffset = 10f;
            int currentIndex = lightsaberBlade.Props.availableHiltGraphics.IndexOf(lightsaberBlade.selectedhiltgraphic);
            if (currentIndex == -1) currentIndex = 0;
            Text.Font = GameFont.Small;
            float buttonY = 240;
            Rect floatMenuButtonRect = new Rect(inRect.x + (inRect.width - buttonWidth) / 2, buttonY, buttonWidth, buttonHeight);
            if (Widgets.ButtonText(floatMenuButtonRect, "Select Hilt", true, true, true))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (HiltDef hiltGraphic in lightsaberBlade.Props.availableHiltGraphics)
                {
                    HiltDef localHiltGraphic = hiltGraphic;
                    options.Add(new FloatMenuOption(localHiltGraphic.label, () =>
                    {
                        lightsaberBlade.selectedhiltgraphic = localHiltGraphic;
                        lightsaberBlade.UpdateHiltGraphic();
                        lightsaberBlade.parent.Notify_ColorChanged();
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }
            Rect colorOneButtonRect = new Rect(floatMenuButtonRect.x - buttonWidth - buttonSpacing, buttonY, buttonWidth, buttonHeight);
            if (Widgets.ButtonText(colorOneButtonRect, "Edit Color 1", true, true, true))
            {
                Find.WindowStack.Add(new StuffColorSelectionWindow((Color selectedColor) =>
                {
                    lightsaberBlade.SetHiltColorOne(selectedColor);
                    lightsaberBlade.UpdateHiltGraphic();
                    lightsaberBlade.parent.Notify_ColorChanged();
                }));
            }
            Rect colorTwoButtonRect = new Rect(floatMenuButtonRect.x + floatMenuButtonRect.width + buttonSpacing, buttonY, buttonWidth, buttonHeight);
            if (Widgets.ButtonText(colorTwoButtonRect, "Edit Color 2", true, true, true))
            {
                Find.WindowStack.Add(new StuffColorSelectionWindow((Color selectedColor) =>
                {
                    lightsaberBlade.SetHiltColorTwo(selectedColor);
                    lightsaberBlade.UpdateHiltGraphic();
                    lightsaberBlade.parent.Notify_ColorChanged();
                }));
            }
            float arrowY = buttonY + buttonYOffset;
            Rect previousHiltButtonRect = new Rect(inRect.x + buttonSpacing, 330, arrowButtonSize, arrowButtonSize);
            Rect nextHiltButtonRect = new Rect(inRect.x + inRect.width - arrowButtonSize - buttonSpacing, 330, arrowButtonSize, arrowButtonSize);
            if (Widgets.ButtonText(previousHiltButtonRect, "<"))
            {
                if (currentIndex > 0)
                {
                    lightsaberBlade.selectedhiltgraphic = lightsaberBlade.Props.availableHiltGraphics[currentIndex - 1];
                    lightsaberBlade.UpdateHiltGraphic();
                    lightsaberBlade.parent.Notify_ColorChanged();
                }
            }
            if (Widgets.ButtonText(nextHiltButtonRect, ">"))
            {
                if (currentIndex < lightsaberBlade.Props.availableHiltGraphics.Count - 1)
                {
                    lightsaberBlade.selectedhiltgraphic = lightsaberBlade.Props.availableHiltGraphics[currentIndex + 1];
                    lightsaberBlade.UpdateHiltGraphic();
                    lightsaberBlade.parent.Notify_ColorChanged();
                }
            }
            float categoryButtonYOffset = buttonY + 150f;
            List<HiltPartDef> allHiltParts = DefDatabase<HiltPartDef>.AllDefsListForReading;
            HiltPartCategory[] categories = Enum.GetValues(typeof(HiltPartCategory)).Cast<HiltPartCategory>().ToArray();
            float currentX = inRect.x + (inRect.width - buttonWidth) / 2 - 260f;
            float buttonYPosition = buttonY + 190f;
            float labelYOffset = buttonHeight + 10f;
            float statYOffset = labelYOffset + 40f;

            foreach (HiltPartCategory category in categories)
            {
                Rect categoryButtonRect = new Rect(currentX, buttonYPosition, buttonWidth, buttonHeight);
                if (Widgets.ButtonText(categoryButtonRect, category.ToString(), true, true, true))
                {
                    List<FloatMenuOption> options = GenerateHiltPartOptionsForCategory(category, lightsaberBlade, allHiltParts);
                    if (options.Any()) Find.WindowStack.Add(new FloatMenu(options));
                    else Log.Warning($"No hilt parts found for category: {category}");
                }

                HiltPartDef selectedPart = lightsaberBlade.GetSelectedHiltPart(category);
                if (selectedPart != null)
                {
                    Text.Font = GameFont.Small;
                    float labelHeight = Text.CalcHeight(selectedPart.label, buttonWidth);
                    Rect labelRect = new Rect(currentX, buttonYPosition + labelYOffset, buttonWidth, labelHeight);
                    Widgets.Label(labelRect, selectedPart.label);

                    if (selectedPart.equippedStatOffsets != null && selectedPart.equippedStatOffsets.Any())
                    {
                        string statInfo = string.Join("\n", selectedPart.equippedStatOffsets.Select(mod => $"{mod.stat.label}: {mod.value:F2}"));
                        float textHeight = Text.CalcHeight(statInfo, buttonWidth);
                        Rect statBoxRect = new Rect(currentX, buttonYPosition + statYOffset, buttonWidth, textHeight + 25f);
                        Widgets.DrawBoxSolid(statBoxRect, new Color(0.2f, 0.2f, 0.2f, 0.8f));
                        Widgets.DrawBox(statBoxRect);
                        Widgets.Label(new Rect(statBoxRect.x + 5f, statBoxRect.y + 5f, statBoxRect.width - 10f, statBoxRect.height - 10f), statInfo);
                        TooltipHandler.TipRegion(statBoxRect, statInfo);
                    }
                }

                currentX += buttonWidth + buttonSpacing;
                buttonYPosition += Math.Max(0, Text.CalcHeight("Sample", buttonWidth) - buttonHeight);
            }

            float colorBoxY = arrowY + arrowButtonSize + colorBoxSpacing;
            float colorBoxWidth = 55f;
            float colorBoxHeight = 55f;
            float rgbLabelWidth = 80f;

            Rect colorOneBoxRect = new Rect(colorOneButtonRect.x - colorBoxWidth - 10f, colorOneButtonRect.y + (buttonHeight - colorBoxHeight) / 2, colorBoxWidth, colorBoxHeight);
            Widgets.Label(new Rect(colorOneBoxRect.x, colorOneBoxRect.y - 25f, colorBoxWidth, 20f), "Color One");
            GUI.color = lightsaberBlade.hiltColorOneOverrideColor;
            Widgets.DrawBoxSolid(colorOneBoxRect, GUI.color);
            Rect colorOneRgbRect = new Rect(colorOneBoxRect.x - 50f, colorOneBoxRect.y, rgbLabelWidth, colorBoxHeight);
            Widgets.Label(new Rect(colorOneRgbRect.x, colorOneRgbRect.y, rgbLabelWidth, 20f), $"R: {(int)(lightsaberBlade.hiltColorOneOverrideColor.r * 255)}");
            Widgets.Label(new Rect(colorOneRgbRect.x, colorOneRgbRect.y + 20f, rgbLabelWidth, 20f), $"G: {(int)(lightsaberBlade.hiltColorOneOverrideColor.g * 255)}");
            Widgets.Label(new Rect(colorOneRgbRect.x, colorOneRgbRect.y + 40f, rgbLabelWidth, 20f), $"B: {(int)(lightsaberBlade.hiltColorOneOverrideColor.b * 255)}");
            GUI.color = Color.white;

            Rect colorTwoBoxRect = new Rect(colorTwoButtonRect.x + colorTwoButtonRect.width + 10f, colorTwoButtonRect.y + (buttonHeight - colorBoxHeight) / 2, colorBoxWidth, colorBoxHeight);
            Widgets.Label(new Rect(colorTwoBoxRect.x, colorTwoBoxRect.y - 25f, colorBoxWidth, 20f), "Color Two");
            GUI.color = lightsaberBlade.hiltColorTwoOverrideColor;
            Widgets.DrawBoxSolid(colorTwoBoxRect, GUI.color);
            Rect colorTwoRgbRect = new Rect(colorTwoBoxRect.x + colorBoxWidth + 5f, colorTwoBoxRect.y, rgbLabelWidth, colorBoxHeight);
            Widgets.Label(new Rect(colorTwoRgbRect.x, colorTwoRgbRect.y, rgbLabelWidth, 20f), $"R: {(int)(lightsaberBlade.hiltColorTwoOverrideColor.r * 255)}");
            Widgets.Label(new Rect(colorTwoRgbRect.x, colorTwoRgbRect.y + 20f, rgbLabelWidth, 20f), $"G: {(int)(lightsaberBlade.hiltColorTwoOverrideColor.g * 255)}");
            Widgets.Label(new Rect(colorTwoRgbRect.x, colorTwoRgbRect.y + 40f, rgbLabelWidth, 20f), $"B: {(int)(lightsaberBlade.hiltColorTwoOverrideColor.b * 255)}");
            GUI.color = Color.white;
        }

        private List<FloatMenuOption> GenerateHiltPartOptionsForCategory(HiltPartCategory category, Comp_LightsaberBlade lightsaberBlade, List<HiltPartDef> allHiltParts)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();

            foreach (HiltPartDef localPart in allHiltParts.Where(p => p.category == category))
            {
                HiltPartDef part = localPart;

                options.Add(new FloatMenuOption($"{localPart.label} (Requires: {localPart.requiredComponent.label})", () =>
                {
                    Thing requiredComponent = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(localPart.requiredComponent), PathEndMode.Touch, TraverseParms.For(pawn), 999f);

                    if (requiredComponent == null)
                    {
                        Messages.Message($"No {localPart.requiredComponent.label} found for the upgrade!", MessageTypeDefOf.RejectInput, false);
                        return;
                    }
                    HiltPartDef previousPart = lightsaberBlade.GetSelectedHiltPart(localPart.category);
                    var job = new Job_UpgradeLightsaber
                    {
                        def = ForceDefOf.Force_UpgradeLightsaber,
                        selectedhiltPartDef = localPart,
                        previoushiltPartDef = previousPart,
                        targetA = requiredComponent
                    };
                    pawn.jobs.StartJob(job, JobCondition.InterruptForced, null, true);
                    if (!pawn.jobs.TryTakeOrderedJob(job)) Log.Warning("Failed to take job for upgrading lightsaber.");
                }));
            }

            return options;
        }

        private void SyncToColor(Color lightsaberBlade1OverrideColor, Color lightsaberCore1Color, Color lightsaberBlade2Color, Color lightsaberCore2Color)
        {
            SyncBlade1Color(lightsaberBlade1OverrideColor);
            SyncCore1Color(lightsaberCore1Color);
            SyncBlade2Color(lightsaberBlade2Color);
            SyncCore2Color(lightsaberCore2Color);
        }

        private void SyncBlade1Color(Color color)
        {
            lightsaberBlade1OverrideColorRed = Mathf.RoundToInt(color.r * 255f);
            lightsaberBlade1OverrideColorGreen = Mathf.RoundToInt(color.g * 255f);
            lightsaberBlade1OverrideColorBlue = Mathf.RoundToInt(color.b * 255f);
            lightsaberBlade1OverrideColorAlpha = Mathf.RoundToInt(color.a * 255f);
        }

        private void SyncCore1Color(Color color)
        {
            lightsaberCore1Red = Mathf.RoundToInt(color.r * 255f);
            lightsaberCore1Green = Mathf.RoundToInt(color.g * 255f);
            lightsaberCore1Blue = Mathf.RoundToInt(color.b * 255f);
            lightsaberCore1Alpha = Mathf.RoundToInt(color.a * 255f);
        }

        private void SyncBlade2Color(Color color)
        {
            lightsaberBlade2Red = Mathf.RoundToInt(color.r * 255f);
            lightsaberBlade2Green = Mathf.RoundToInt(color.g * 255f);
            lightsaberBlade2Blue = Mathf.RoundToInt(color.b * 255f);
            lightsaberBlade2Alpha = Mathf.RoundToInt(color.a * 255f);
        }

        private void SyncCore2Color(Color color)
        {
            lightsaberCore2Red = Mathf.RoundToInt(color.r * 255f);
            lightsaberCore2Green = Mathf.RoundToInt(color.g * 255f);
            lightsaberCore2Blue = Mathf.RoundToInt(color.b * 255f);
            lightsaberCore2Alpha = Mathf.RoundToInt(color.a * 255f);
        }

        private void UpdateCurrentColor()
        {
            lightsaberBlade1OverrideColor = new Color((float)lightsaberBlade1OverrideColorRed / 255f, (float)lightsaberBlade1OverrideColorGreen / 255f, (float)lightsaberBlade1OverrideColorBlue / 255f, (float)lightsaberBlade1OverrideColorAlpha / 255f);
            lightsaberCore1CurrentColor = new Color((float)lightsaberCore1Red / 255f, (float)lightsaberCore1Green / 255f, (float)lightsaberCore1Blue / 255f, (float)lightsaberCore1Alpha / 255f);
            lightsaberBlade2CurrentColor = new Color((float)lightsaberBlade2Red / 255f, (float)lightsaberBlade2Green / 255f, (float)lightsaberBlade2Blue / 255f, (float)lightsaberBlade2Alpha / 255f);
            lightsaberCore2CurrentColor = new Color((float)lightsaberCore2Red / 255f, (float)lightsaberCore2Green / 255f, (float)lightsaberCore2Blue / 255f, (float)lightsaberCore2Alpha / 255f);
        }
    }
}