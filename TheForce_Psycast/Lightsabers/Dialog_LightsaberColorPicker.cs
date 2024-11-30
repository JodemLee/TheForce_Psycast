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
        private const float WindowWidth = 860f; // Increased window width
        private const float WindowHeight = 600f;
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
        private int hiltcolor1Red, hiltcolor1Green, hiltcolor1Blue, hiltcolor1Alpha;
        private int hiltcolor2Red, hiltcolor2Green, hiltcolor2Blue, hiltcolor2Alpha;
        private readonly Comp_LightsaberBlade lightsaberBlade;
        private readonly Action<Color, Color, Color, Color> confirmAction;
        private static Color lightsaberBlade1OverrideColor = Color.white;
        private static Color lightsaberCore1CurrentColor = Color.white;
        private static Color lightsaberBlade2CurrentColor = Color.white;
        private static Color lightsaberCore2CurrentColor = Color.white;
        private static Color hiltcolor1CurrentColor = Color.white;
        private static Color hiltcolor2CurrentColor = Color.white;
        private static Texture2D outerglowTexture;
        private static Texture2D innercoreTexture;
        private static Texture2D dualglow1texture;
        private static Texture2D dualcore1Texture;
        private static Texture2D dualglow2texture;
        private static Texture2D dualcore2Texture;

        private enum Tab
        {
            RGBSelector,
            Hilts
        }

        private Tab currentTab = Tab.RGBSelector;

        public Dialog_LightsaberColorPicker(Pawn pawn, Comp_LightsaberBlade lightsaberBlade, Action<Color, Color, Color, Color> confirmAction)
        {
            this.pawn = pawn;
            this.lightsaberBlade = lightsaberBlade;
            this.confirmAction = confirmAction;

            if (outerglowTexture == null)
            {
                outerglowTexture = ContentFinder<Texture2D>.Get("Things/Item/Lightsaber/SingleSaber/SaberOutline/White_Outline");
                innercoreTexture = ContentFinder<Texture2D>.Get("Things/Item/Lightsaber/SingleSaber/SaberCore/White_Core");
            }

            if (dualglow1texture == null)
            {
                dualglow1texture = ContentFinder<Texture2D>.Get("Things/Item/Lightsaber/DualSaber/DualSaberOutline/White_Outline1");
                dualcore1Texture = ContentFinder<Texture2D>.Get("Things/Item/Lightsaber/DualSaber/DualSaberCore/White_Core1");
                dualglow2texture = ContentFinder<Texture2D>.Get("Things/Item/Lightsaber/DualSaber/DualSaberOutline/White_Outline2");
                dualcore2Texture = ContentFinder<Texture2D>.Get("Things/Item/Lightsaber/DualSaber/DualSaberCore/White_Core2");
            }

            if (lightsaberBlade.Graphic != null)
            {
                SyncBlade1Color(lightsaberBlade.Graphic.color);
            }
            if (lightsaberBlade.LightsaberCore1Graphic != null)
            {
                SyncCore1Color(lightsaberBlade.LightsaberCore1Graphic.color);
            }
            if (lightsaberBlade.LightsaberBlade2Graphic != null)
            {
                SyncBlade2Color(lightsaberBlade.LightsaberBlade2Graphic.color);
            }
            if (lightsaberBlade.LightsaberCore2Graphic != null)
            {
                SyncCore2Color(lightsaberBlade.LightsaberCore2Graphic.color);
            }

            UpdateCurrentColor();
            this.closeOnClickedOutside = false;
            this.doCloseX = true;
            doCloseButton = true;
            draggable = true;
            absorbInputAroundWindow = false;
            preventCameraMotion = false;
        }

        public override Vector2 InitialSize => new Vector2(WindowWidth, CalculateWindowHeight());

        private float CalculateWindowHeight()
        {
            float height = 40f; // Padding

            height = 725f + ButtonHeight;

            return Mathf.Max(height, WindowHeight);
        }

        public override void DoWindowContents(Rect inRect)
        {
            // Draw the hilt first, as it should be visible on every tab
            DrawHilt(inRect);
            DrawTabs(inRect);

            switch (currentTab)
            {
                case Tab.RGBSelector:
                    DrawRGBSelector(inRect);  // Blade will be drawn only here
                    break;
                case Tab.Hilts:
                    DrawHiltSelector(inRect);
                    break;
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
                if (labelHeight > 30f)
                {
                    tabRect.height = labelHeight + 10f;
                }
                if (Widgets.ButtonText(tabRect, label, true, true, true))
                {
                    currentTab = tab;
                }
            }
        }

        private string GetTabLabel(Tab tab)
        {
            switch (tab)
            {
                case Tab.RGBSelector:
                    return "ForceTab_RGB".Translate(); // Custom label with space
                default:
                    return tab.ToString(); // Fallback to enum name
            }
        }

        private void DrawHilt(Rect inRect)
        {
            // Calculate center for alignment
            float centerX = inRect.x + inRect.width / 2;
            float centerY = inRect.y + inRect.height / 2;

            // Check if hilts are available
            if (lightsaberBlade == null || lightsaberBlade.Props.availableHiltGraphics == null || lightsaberBlade.Props.availableHiltGraphics.Count == 0)
            {
                Widgets.Label(new Rect(inRect.x, inRect.y + 50f, inRect.width, 30f), "No hilts available.");
                return;
            }

            // Display selected hilt label
            string currentHiltLabel = lightsaberBlade.selectedhiltgraphic?.label ?? "No hilt selected";
            Text.Font = GameFont.Medium;
            Vector2 labelSize = Text.CalcSize(currentHiltLabel);
            float labelX = inRect.x + (inRect.width - labelSize.x) / 2;
            Rect labelRect = new Rect(labelX, inRect.y + 60f, labelSize.x, labelSize.y);
            Widgets.Label(labelRect, currentHiltLabel);
            Text.Font = GameFont.Small;

            // Reset GUI color
            GUI.color = Color.white;

            // Calculate positions for the hilts
            float hiltX = centerX - 350f; // Center the hilt
            float hiltY = centerY - 290f;
            Rect hiltRect = new Rect(hiltX, hiltY, 550f, 550f);

            // Second hilt and blade logic (if LightsaberBlade2Graphic is available)
            if (lightsaberBlade.LightsaberBlade2Graphic != null)
            {
                float hiltX2 = hiltX + 75f; // Offset second hilt if required
                Rect hiltRect2 = new Rect(hiltX2, hiltY, 550f, 550f);

                // Adjust pivot point for second hilt
                Matrix4x4 originalMatrix2 = GUI.matrix;
                Vector2 pivotPoint2 = new Vector2(hiltRect2.x + hiltRect2.width / 2, hiltRect2.y + hiltRect2.height / 2);

                // Rotate around pivot to counteract the 45-degree rotation
                GUIUtility.RotateAroundPivot(-135f, pivotPoint2);

                // RGB Selector logic
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

                // Draw hilt texture
                var mat = lightsaberBlade.selectedhiltgraphic.hiltgraphic.Graphic.MatSingle;
                    GUI.color = Color.white*2;
                    MaterialRequest materialRequest = new MaterialRequest
                    {
                        mainTex = mat.mainTexture,
                        maskTex = mat.GetMaskTexture(),
                        shader = mat.shader,
                        color = lightsaberBlade.hiltColorOneOverrideColor,   // Color 1 with full opacity (ensure alpha is 1)
                        colorTwo = lightsaberBlade.hiltColorTwoOverrideColor // Color 2 with full opacity (ensure alpha is 1)
                    };
                    Material material = MaterialPool.MatFrom(materialRequest);
                    GenUI.DrawTextureWithMaterial(hiltRect2, lightsaberBlade.selectedhiltgraphic.hiltgraphic.Graphic.MatSingle.mainTexture, material);
                
                // Reset matrix after transformation
                GUI.matrix = originalMatrix2;
                GUI.color = Color.white;
            }
            else // First hilt logic (if no second hilt)
            {
                // Draw first hilt
                GUI.color = Color.white;

                if (lightsaberBlade.selectedhiltgraphic != null)
                {
                    Matrix4x4 originalMatrix = GUI.matrix;
                    Vector2 pivotPoint = new Vector2(hiltRect.x + hiltRect.width / 2, hiltRect.y + hiltRect.height / 2);

                    // Rotate around pivot to counteract the 45-degree rotation
                    GUIUtility.RotateAroundPivot(-135f, pivotPoint);

                    // RGB Selector logic
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

                    if (lightsaberBlade.selectedhiltgraphic != null && lightsaberBlade.selectedhiltgraphic.hiltgraphic != null && lightsaberBlade.selectedhiltgraphic.hiltgraphic.Graphic.MatSingle != null)
                    {
                        var mat = lightsaberBlade.selectedhiltgraphic.hiltgraphic.Graphic.MatSingle;

                        // Set the GUI color to white with full opacity (multiplied by 2 for increased brightness, if necessary)
                        GUI.color = Color.white * 2;

                        // Create the material request for custom colors
                        MaterialRequest materialRequest = new MaterialRequest
                        {
                            mainTex = mat.mainTexture,
                            maskTex = mat.GetMaskTexture(),
                            shader = mat.shader,
                            color = lightsaberBlade.hiltColorOneOverrideColor,   // Color 1 with full opacity
                            colorTwo = lightsaberBlade.hiltColorTwoOverrideColor // Color 2 with full opacity
                        };

                        // Generate the material using the material request
                        Material material = MaterialPool.MatFrom(materialRequest);

                        // Draw the texture with the modified material
                        GenUI.DrawTextureWithMaterial(hiltRect, mat.mainTexture, material);
                    }
                    else
                    {
                        // Fallback: draw the regular graphic if the selectedhiltgraphic is null
                        if (lightsaberBlade.selectedhiltgraphic == null)
                        {
                            // Draw the regular graphic without custom colors
                            GUI.DrawTexture(hiltRect, lightsaberBlade.parent.Graphic.MatSingle.mainTexture);
                        }
                    }

                    GUI.matrix = originalMatrix;  // Restore the original GUI matrix
                    GUI.color = Color.white;  // Reset GUI color to white
                }
            }
        }

        [TweakValue("AAA_RowPosition", -2000f, 2000f)]
        private static float RowOffset2 = 0f;

        private void DrawRGBSelector(Rect inRect)
        {
            float rightPanelX = inRect.xMax - 220f;
            float rowOffset = inRect.y + 40f;
            float rowOffset2nd = inRect.y + 385;
            Rect bladeLengthAdjusterRect = new Rect(inRect.x, inRect.y, inRect.width / 2, 150f);
            Rect soundAdjusterRect = new Rect(inRect.x, RowOffset2, inRect.width / 2, 150f); // Adjusted based on the second row offset
            DrawBladeLengthAdjuster(bladeLengthAdjusterRect);
            DrawSoundSelector(soundAdjusterRect, lightsaberBlade, rowOffset2nd);
            if (lightsaberBlade.Graphic != null)
            {
                DrawColorPicker(ref rowOffset, ref lightsaberBlade1OverrideColorRed, ref lightsaberBlade1OverrideColorGreen, ref lightsaberBlade1OverrideColorBlue, ref lightsaberBlade1OverrideColorAlpha, "Blade", rightPanelX);
            }

            if (lightsaberBlade.LightsaberCore1Graphic != null)
            {
                DrawColorPicker(ref rowOffset, ref lightsaberCore1Red, ref lightsaberCore1Green, ref lightsaberCore1Blue, ref lightsaberCore1Alpha, "Core 1", rightPanelX);
            }

            if (lightsaberBlade.LightsaberBlade2Graphic != null)
            {
                DrawColorPicker(ref rowOffset2nd, ref lightsaberBlade2Red, ref lightsaberBlade2Green, ref lightsaberBlade2Blue, ref lightsaberBlade2Alpha, "Blade 2", rightPanelX);
            }

            if (lightsaberBlade.LightsaberCore2Graphic != null)
            {
                DrawColorPicker(ref rowOffset2nd, ref lightsaberCore2Red, ref lightsaberCore2Green, ref lightsaberCore2Blue, ref lightsaberCore2Alpha, "Core 2", rightPanelX);
            }
            rowOffset += 30f;
            Rect presetRect = new Rect(inRect.x + 20f, 450f, inRect.width - 40f, 120f);
            DrawPresets(presetRect);

            UpdateCurrentColor();
        }


        private void DrawRandomizeButton(float xOffset, float rowOffset, ref int red, ref int green, ref int blue)
        {
            Rect buttonRect = new Rect(xOffset + LabelWidth + 10f, rowOffset, 100f, 22f);
            if (Widgets.ButtonText(buttonRect, "Randomize"))
            {
                RandomizeRGBValues(ref red, ref green, ref blue);
            }
        }

        private void RandomizeRGBValues(ref int red, ref int green, ref int blue)
        {
            red = UnityEngine.Random.Range(0, 256);
            green = UnityEngine.Random.Range(0, 256);
            blue = UnityEngine.Random.Range(0, 256);
        }

        private void DrawColorPicker(ref float rowOffset, ref int colorRed, ref int colorGreen, ref int colorBlue, ref int colorAlpha, string label, float rightPanelX)
        {
            Widgets.Label(new Rect(rightPanelX, rowOffset, 200f, 22f), label + ":");
            DrawRandomizeButton(rightPanelX, rowOffset, ref colorRed, ref colorGreen, ref colorBlue);
            rowOffset += 22f;

            // Update colors in real-time on slider change
            DrawSliderWithInput(ref rowOffset, ref colorRed, "Red", rightPanelX);
            DrawSliderWithInput(ref rowOffset, ref colorGreen, "Green", rightPanelX);
            DrawSliderWithInput(ref rowOffset, ref colorBlue, "Blue", rightPanelX);
            DrawSliderWithInput(ref rowOffset, ref colorAlpha, "Alpha", rightPanelX);

            // Set the lightsaber color immediately after changing any color slider
            UpdateLightsaberColor(label, colorRed, colorGreen, colorBlue, colorAlpha);
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

            // Update the specific lightsaber color based on the label
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
                    // Add cases for additional labels if needed
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
                { "Ideo", pawn?.Ideo?.Color ?? Color.white },
                { "Favorite", pawn?.story?.favoriteColor ?? Color.white },
                { "Crystal", selectedCrystal.color}
            };

            // Calculate button dimensions and spacing
            float buttonWidth = 75f;
            float buttonHeight = 30f;
            float buttonSpacing = 10f;

            // Determine the starting Y position based on the height of the window and the Y offset
            float startY = inRect.yMax - (buttonHeight + buttonSpacing) * (presets.Count / (inRect.width / (buttonWidth + buttonSpacing))) - buttonSpacing + 95f;
            float currentX = inRect.x; // Starting X position
            float currentY = startY; // Start at calculated Y position
            foreach (var preset in presets)
            {
                Rect buttonRect = new Rect(currentX, currentY, buttonWidth, buttonHeight);
                // Wrap to the next row if the button exceeds the available space
                if (currentX + buttonWidth > inRect.xMax)
                {
                    currentX = inRect.x;
                    currentY += buttonHeight + buttonSpacing;
                    buttonRect = new Rect(currentX, currentY, buttonWidth, buttonHeight);
                }

                // Draw the button
                if (Widgets.ButtonText(buttonRect, preset.Key))
                {
                    // Store the selected color in temporary variables
                    Color tempBladeColor = preset.Value;
                    // Update color when "OK" button is clicked
                    confirmAction(tempBladeColor, lightsaberCore1CurrentColor, tempBladeColor, lightsaberCore2CurrentColor);
                    // Update the current color
                    UpdateCurrentColor();
                    SyncToColor(tempBladeColor, lightsaberCore1CurrentColor, tempBladeColor, lightsaberCore2CurrentColor);
                }

                // Move to the next position
                currentX += buttonWidth + buttonSpacing;
            }
        }

        private void DrawBladeLengthAdjuster(Rect inRect)
        {
            if (lightsaberBlade == null)
                return;

            float rowOffset = 35f;
            rowOffset = DrawBladeLengthSlider("Force_VariableBlade1", ref lightsaberBlade.bladeLengthCore1AndBlade1, inRect, rowOffset);
            if (lightsaberBlade.LightsaberBlade2Graphic == null)
            {
                rowOffset += 35f;
            }  
            rowOffset = DrawVibrationSlider("Force_Vibration1", ref lightsaberBlade.vibrationrate, inRect, rowOffset);
            if (lightsaberBlade.LightsaberBlade2Graphic != null)
            {
                rowOffset = DrawBladeLengthSlider("Force_VariableBlade2", ref lightsaberBlade.bladeLengthCore2AndBlade2, inRect, rowOffset);
                rowOffset = DrawVibrationSlider("Force_Vibration2", ref lightsaberBlade.vibrationrate2, inRect, rowOffset);
            }
            if (Widgets.ButtonText(new Rect(0f, rowOffset, 100f, ButtonHeight), "Apply"))
            {
                ApplyBladeLength();
            }
        }

        private float DrawBladeLengthSlider(string labelKey, ref float bladeLength, Rect inRect, float rowOffset)
        {
            Widgets.Label(new Rect(0f, rowOffset, 200f, 22f), labelKey.Translate());
            bladeLength = Widgets.HorizontalSlider(
                new Rect(0f, rowOffset + 25f, inRect.width - 80f, 20f),
                bladeLength,
                lightsaberBlade.MinBladeLength,
                lightsaberBlade.MaxBladeLength);
            Widgets.Label(new Rect(inRect.width - 100f, rowOffset + 45f, 100f, 22f),
                          $"{bladeLength:F2} m");
            lightsaberBlade.SetBladeLengths(lightsaberBlade.bladeLengthCore1AndBlade1, lightsaberBlade.bladeLengthCore2AndBlade2);
            return rowOffset + 35f;
        }

        private float DrawVibrationSlider(string labelKey, ref float lensFactor, Rect inRect, float rowOffset)
        {
            // Draw the label for the vibration slider
            Widgets.Label(new Rect(0f, rowOffset, 200f, 22f), labelKey.Translate());

            // Draw the horizontal slider for lens factor (vibration intensity)
            lensFactor = Widgets.HorizontalSlider(
                new Rect(0f, rowOffset + 25f, inRect.width - 80f, 20f),
                lensFactor,
                0.00f,
                0.05f);

            Widgets.Label(new Rect(inRect.width - 100f, rowOffset + 45f, 100f, 22f),
                          $"{lensFactor:F2} Hz");

            lightsaberBlade.SetVibrationRate(
                lightsaberBlade.vibrationrate,
                lightsaberBlade.vibrationrate2
            );

            return rowOffset + 70f;
        }

        private void ApplyBladeLength()
        {
            // Set the blade lengths on the lightsaber object
            lightsaberBlade.SetBladeLengths(lightsaberBlade.bladeLengthCore1AndBlade1, lightsaberBlade.bladeLengthCore2AndBlade2);
            Close();
        }

        private Vector2 scrollPosition = Vector2.zero;

        public void DrawSoundSelector(Rect inRect, Comp_LightsaberBlade lightsaberBlade, float rowOffset)
        {
            if (lightsaberBlade.lightsaberSound != null && lightsaberBlade.lightsaberSound.Count > 0)
            {
                // Retrieve the sound options from the lightsaberBlade component
                List<SoundDef> soundOptions = lightsaberBlade.lightsaberSound;

                const float padding = 10f;
                const float labelHeight = 22f;
                const float optionHeight = 25f;

                // Draw a labeled section box for sound effects
                Widgets.DrawMenuSection(new Rect(inRect.x + padding, rowOffset - padding / 2, inRect.width, inRect.height));

                // Set the font for the section title
                Text.Font = GameFont.Small;
                Widgets.Label(new Rect(inRect.x + padding * 2, rowOffset, inRect.width, labelHeight), "Select Sound Effect");
                Text.Font = GameFont.Small;

                // Calculate sound options' total height
                float soundOptionsHeight = soundOptions.Count * optionHeight + padding;
                float scrollViewHeight = Math.Min(soundOptionsHeight, inRect.height - labelHeight - padding);

                // Create a scrollable view if sound options exceed the section height
                Rect scrollViewRect = new Rect(inRect.x + padding, rowOffset + labelHeight, inRect.width - padding, scrollViewHeight);
                Rect scrollContentRect = new Rect(0f, 0f, inRect.width - padding * 3, soundOptionsHeight);

                // Begin scroll view
                Widgets.BeginScrollView(scrollViewRect, ref scrollPosition, scrollContentRect);

                // Render each sound option as a radio button within the scroll view's content area
                for (int i = 0; i < soundOptions.Count; i++)
                {
                    Rect optionRect = new Rect(0f, i * optionHeight, scrollContentRect.width, optionHeight); // Position relative to the scroll view

                    // Create a radio button for each sound
                    bool isSelected = lightsaberBlade.selectedSoundIndex == i;
                    if (Widgets.RadioButtonLabeled(optionRect, soundOptions[i].label, isSelected))
                    {
                        // Update selected sound only if it changes
                        if (!isSelected)
                        {
                            lightsaberBlade.selectedSoundIndex = i;
                            soundOptions[i].PlayOneShot(lightsaberBlade.parent);

                            // Update the selected sound effect
                            lightsaberBlade.SetSoundEffect(soundOptions[i]);
                        }
                    }

                    // Optional: Add hover feedback
                    if (optionRect.Contains(Event.current.mousePosition))
                    {
                        Widgets.DrawHighlight(optionRect);
                    }
                }

                // End scroll view
                Widgets.EndScrollView();
            }
        }

        // Define tweakable values for positions, sizes, and offsets
        [TweakValue("Hilt_Selector", 100f, 600f)]
        private static float hiltGraphicSize = 150f; // Size of the hilt graphic

        [TweakValue("Hilt_Selector", -2000f, 2000f)]
        private static float hiltGraphicXOffset = 0f; // Horizontal offset for the hilt graphic

        [TweakValue("Hilt_Selector", -2000f, 2000f)]
        private static float hiltGraphicYOffsetTweak = 20f; // Vertical offset for the hilt graphic

        [TweakValue("Hilt_Selector", 10f, 100f)]
        private static float descriptionHeightTweak = 60f; // Height of the hilt description box

        [TweakValue("Hilt_Selector", 50f, 300f)]
        private static float buttonHeightTweak = 30f; // Height of buttons

        [TweakValue("Hilt_Selector", 100f, 300f)]
        private static float buttonWidthTweak = 120f; // Width of buttons

        [TweakValue("Hilt_Selector", 5f, 50f)]
        private static float buttonSpacingTweak = 10f; // Spacing between buttons

        [TweakValue("Hilt_Selector", -1000f, 1000f)]
        private static float buttonYOffsetTweak = 10f; // Offset for buttons' Y position relative to the description

        [TweakValue("Hilt_Selector", 50f, 150f)]
        private static float arrowButtonSizeTweak = 50f; // Size of the arrow buttons

        [TweakValue("Hilt_Selector", -500f, 500f)]
        private static float colorBoxSpacingTweak = 10f; // Spacing between color boxes and labels

        [TweakValue("Hilt_Selector", 50f, 200f)]
        private static float colorBoxSizeTweak = 60f; // Size of the color boxes

        private void DrawHiltSelector(Rect inRect)
        {
            // Easily tweakable sizes and positions
            float buttonWidth = buttonWidthTweak;
            float buttonHeight = buttonHeightTweak;
            float buttonSpacing = buttonSpacingTweak;
            float colorBoxSize = colorBoxSizeTweak;
            float colorBoxSpacing = colorBoxSpacingTweak;
            float arrowButtonSize = arrowButtonSizeTweak;
            float descriptionHeight = 150;
            float buttonYOffset = buttonYOffsetTweak;
            float descriptionYOffset = 100;

            // Default selected hilt if none exists
            int currentIndex = lightsaberBlade.Props.availableHiltGraphics.IndexOf(lightsaberBlade.selectedhiltgraphic);
            if (currentIndex == -1) currentIndex = 0;

            Text.Font = GameFont.Small;
            string hiltDescription = lightsaberBlade.selectedhiltgraphic.description;
            Vector2 descriptionScrollPosition = Vector2.zero;

            // Calculate the content height dynamically based on the description length
            // For example, assuming each line of text takes 20px height
            float contentHeight = Mathf.Max(hiltDescription.Length * 20f, 300f);  // Ensure content is large enough for scrolling

            // Create the scrollable area rect
            Rect descriptionRect = new Rect(inRect.x, inRect.y + descriptionYOffset, inRect.width, descriptionHeight);

            // Begin the scroll view
            Widgets.BeginScrollView(descriptionRect, ref scrollPosition, descriptionRect);

            // Create a rectangle for the content (description text)
            Rect textRect = new Rect(0, 0, inRect.width - 16f, contentHeight);

            // Draw the label inside the scrollable content area
            Widgets.Label(textRect, hiltDescription);

            // End the scroll view
            Widgets.EndScrollView();
            float buttonY = 375;
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
                        lightsaberBlade.parent.Notify_ColorChanged();
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }

            // Color editing buttons - Position on either side of the "Select Hilt" button
            Rect colorOneButtonRect = new Rect(floatMenuButtonRect.x - buttonWidth - buttonSpacing, buttonY, buttonWidth, buttonHeight);
            if (Widgets.ButtonText(colorOneButtonRect, "Edit Color 1", true, true, true))
            {
                Find.WindowStack.Add(new StuffColorSelectionWindow((Color selectedColor) =>
                {
                    lightsaberBlade.SetHiltColorOne(selectedColor);
                    lightsaberBlade.parent.Notify_ColorChanged();
                }));
            }

            Rect colorTwoButtonRect = new Rect(floatMenuButtonRect.x + floatMenuButtonRect.width + buttonSpacing, buttonY, buttonWidth, buttonHeight);
            if (Widgets.ButtonText(colorTwoButtonRect, "Edit Color 2", true, true, true))
            {
                Find.WindowStack.Add(new StuffColorSelectionWindow((Color selectedColor) =>
                {
                    lightsaberBlade.SetHiltColorTwo(selectedColor);
                    lightsaberBlade.parent.Notify_ColorChanged();
                }));
            }

            // Draw arrow buttons for previous and next hilt
            float arrowY = buttonY + buttonYOffset;
            Rect previousHiltButtonRect = new Rect(inRect.x + buttonSpacing, 300, arrowButtonSize, arrowButtonSize);
            Rect nextHiltButtonRect = new Rect(inRect.x + inRect.width - arrowButtonSize - buttonSpacing, 300, arrowButtonSize, arrowButtonSize);

            if (Widgets.ButtonText(previousHiltButtonRect, "<"))
            {
                if (currentIndex > 0)
                {
                    lightsaberBlade.selectedhiltgraphic = lightsaberBlade.Props.availableHiltGraphics[currentIndex - 1];
                    lightsaberBlade.parent.Notify_ColorChanged();
                }
            }

            if (Widgets.ButtonText(nextHiltButtonRect, ">"))
            {
                if (currentIndex < lightsaberBlade.Props.availableHiltGraphics.Count - 1)
                {
                    lightsaberBlade.selectedhiltgraphic = lightsaberBlade.Props.availableHiltGraphics[currentIndex + 1];
                    lightsaberBlade.parent.Notify_ColorChanged();
                }
            }

            float categoryButtonYOffset = buttonY + 50f; // Space out from the existing buttons

            List<HiltPartDef> allHiltParts = DefDatabase<HiltPartDef>.AllDefsListForReading;

            // Get all categories once
            HiltPartCategory[] categories = Enum.GetValues(typeof(HiltPartCategory)).Cast<HiltPartCategory>().ToArray();

            // Layout variables
            float currentX = inRect.x + (inRect.width - buttonWidth) / 2 - 260f;
            float buttonYPosition = buttonY + 60f;
            float labelYOffset = buttonHeight + 5f;
            float statYOffset = labelYOffset + 35f;

            foreach (HiltPartCategory category in categories)
            {
                // Create button for each category
                Rect categoryButtonRect = new Rect(currentX, buttonYPosition, buttonWidth, buttonHeight);
                if (Widgets.ButtonText(categoryButtonRect, category.ToString(), true, true, true))
                {
                    // Generate options for the selected category
                    List<FloatMenuOption> options = GenerateHiltPartOptionsForCategory(category, lightsaberBlade, allHiltParts);
                    if (options.Any())
                    {
                        Find.WindowStack.Add(new FloatMenu(options));
                    }
                    else
                    {
                        Log.Warning($"No hilt parts found for category: {category}");
                    }
                }

                // Display the label of the currently selected part
                HiltPartDef selectedPart = lightsaberBlade.GetSelectedHiltPart(category);
                if (selectedPart != null)
                {
                    // Display the label for the selected part
                    Text.Font = GameFont.Small;

                    // Calculate the height of the label text dynamically
                    float labelHeight = Text.CalcHeight(selectedPart.label, buttonWidth);
                    Widgets.Label(new Rect(currentX, buttonYPosition + labelYOffset, buttonWidth, labelHeight), selectedPart.label);

                    // Revert the font size back to small for other UI elements
                    Text.Font = GameFont.Small;

                    // Display the stat modifier and its value
                    if (selectedPart.equippedStatOffsets != null && selectedPart.equippedStatOffsets.Any())
                    {
                        // Generate stat info string
                        string statInfo = string.Join("\n", selectedPart.equippedStatOffsets.Select(mod => $"{mod.stat.label}: {mod.value:F2}"));

                        // Calculate the required height for the stat info
                        Text.Font = GameFont.Small;
                        float textHeight = Text.CalcHeight(statInfo, buttonWidth);

                        // Create a background box for the stat info
                        Rect statBoxRect = new Rect(currentX, buttonYPosition + statYOffset, buttonWidth, textHeight + 25f); // Add padding
                        Widgets.DrawBoxSolid(statBoxRect, new Color(0.2f, 0.2f, 0.2f, 0.8f)); // Dark background
                        Widgets.DrawBox(statBoxRect); // Border

                        // Draw the stat info text
                        Widgets.Label(new Rect(statBoxRect.x + 5f, statBoxRect.y + 5f, statBoxRect.width - 10f, statBoxRect.height - 10f), statInfo);

                        // Add tooltip for overflow
                        TooltipHandler.TipRegion(statBoxRect, statInfo);
                    }
                }

                // Move to the next button position (horizontally)
                currentX += buttonWidth + buttonSpacing;

                // Ensure enough vertical space between rows (optional, for better look)
                buttonYPosition += Math.Max(0, Text.CalcHeight("Sample", buttonWidth) - buttonHeight);
            }

            float colorBoxY = arrowY + arrowButtonSize + colorBoxSpacing; // Same Y position for both boxes
            float colorBoxWidth = 75f;
            float colorBoxHeight = 75f;
            float rgbLabelWidth = 100f; // Width for RGB value labels
            Rect colorOneBoxRect = new Rect(colorOneButtonRect.x - colorBoxWidth - 10f, colorOneButtonRect.y + (buttonHeight - colorBoxHeight) / 2, colorBoxWidth, colorBoxHeight);
            Widgets.Label(new Rect(colorOneBoxRect.x, colorOneBoxRect.y - 20f, colorBoxWidth, 20f), "Color One");
            GUI.color = lightsaberBlade.hiltColorOneOverrideColor;
            Widgets.DrawBoxSolid(colorOneBoxRect, GUI.color);
            Rect colorOneRgbRect = new Rect(colorOneBoxRect.x - 50f, colorOneBoxRect.y, rgbLabelWidth, colorBoxHeight);
            Widgets.Label(new Rect(colorOneRgbRect.x, colorOneRgbRect.y, rgbLabelWidth, 20f), $"R: {(int)(lightsaberBlade.hiltColorOneOverrideColor.r * 255)}");
            Widgets.Label(new Rect(colorOneRgbRect.x, colorOneRgbRect.y + 20f, rgbLabelWidth, 20f), $"G: {(int)(lightsaberBlade.hiltColorOneOverrideColor.g * 255)}");
            Widgets.Label(new Rect(colorOneRgbRect.x, colorOneRgbRect.y + 40f, rgbLabelWidth, 20f), $"B: {(int)(lightsaberBlade.hiltColorOneOverrideColor.b * 255)}");
            GUI.color = Color.white;
            Rect colorTwoBoxRect = new Rect(colorTwoButtonRect.x + colorTwoButtonRect.width + 10f, colorTwoButtonRect.y + (buttonHeight - colorBoxHeight) / 2, colorBoxWidth, colorBoxHeight);
            Widgets.Label(new Rect(colorTwoBoxRect.x, colorTwoBoxRect.y - 20f, colorBoxWidth, 20f), "Color Two");
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
                HiltPartDef part = localPart; // Local reference for lambda

                options.Add(new FloatMenuOption(localPart.label, () =>
                {
                    // Log the type of the required component for debugging
                    Log.Message($"Required component for {localPart.label}: {localPart.requiredComponent?.defName}");

                    // Find the closest required component for the selected part
                    Thing requiredComponent = GenClosest.ClosestThingReachable(
                        pawn.Position,
                        pawn.Map,
                        ThingRequest.ForDef(localPart.requiredComponent),
                        PathEndMode.Touch,
                        TraverseParms.For(pawn),
                        999f);

                    if (requiredComponent == null)
                    {
                        Messages.Message($"No {localPart.requiredComponent.label} found for the upgrade!", MessageTypeDefOf.RejectInput, false);
                        return;
                    }

                    // Find the previous part of the same category
                    HiltPartDef previousPart = lightsaberBlade.GetSelectedHiltPart(localPart.category);
                    var job = new Job_UpgradeLightsaber
                    {
                        def = ForceDefOf.Force_UpgradeLightsaber, // Ensure the def is assigned correctly
                        selectedhiltPartDef = localPart,
                        previoushiltPartDef = previousPart,
                        targetA = requiredComponent // Assuming targetA is used for the component
                    };
                    Log.Message($"Job created for {localPart.label} - {job.selectedhiltPartDef?.label}");

                    pawn.jobs.StartJob(job, JobCondition.InterruptForced, null, true);
                    if (!pawn.jobs.TryTakeOrderedJob(job))
                    {
                        Log.Warning("Failed to take job for upgrading lightsaber.");
                    }
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
            hiltcolor1CurrentColor = new Color((float)hiltcolor1Red / 255f, (float)hiltcolor1Green / 255f, (float)hiltcolor1Blue / 255f, (float)hiltcolor1Alpha / 255f);
            hiltcolor2CurrentColor = new Color((float)hiltcolor2Red / 255f, (float)hiltcolor2Green / 255f, (float)hiltcolor2Blue / 255f, (float)hiltcolor2Alpha / 255f);
        }
    }

    public class StuffColorSelectionWindow : Window
    {
        private Action<Color> onColorSelected;
        private Dictionary<StuffCategoryDef, List<ThingDef>> categorizedStuffDefs;
        private List<StuffCategoryDef> categories;
        private StuffCategoryDef selectedCategory;  // Current selected tab
        private string searchString = "";  // Search field

        public StuffColorSelectionWindow(Action<Color> onColorSelected)
        {
            this.onColorSelected = onColorSelected;

            // Group ThingDefs by their stuffProps categories
            categorizedStuffDefs = new Dictionary<StuffCategoryDef, List<ThingDef>>();
            List<ThingDef> allStuffedDefs = StuffColorUtility.GetAllStuffed();

            foreach (ThingDef def in allStuffedDefs)
            {
                if (def.stuffProps != null && def.stuffProps.categories != null)
                {
                    foreach (StuffCategoryDef category in def.stuffProps.categories)
                    {
                        if (!categorizedStuffDefs.ContainsKey(category))
                        {
                            categorizedStuffDefs[category] = new List<ThingDef>();
                        }
                        categorizedStuffDefs[category].Add(def);
                    }
                }
            }

            // Store the list of categories for tab display
            categories = categorizedStuffDefs.Keys.ToList();

            // Initialize with the first category as selected
            if (categories.Count > 0)
            {
                selectedCategory = categories[0];
            }

            // Set window properties
            this.closeOnClickedOutside = false;
            this.doCloseX = true;
            doCloseButton = false;
            draggable = true;
            absorbInputAroundWindow = false;
            preventCameraMotion = false;
        }

        public override Vector2 InitialSize => new Vector2(500f, 600f); // Adjust size if needed

        public override void DoWindowContents(Rect inRect)
        {
            // Set the height for the tabs
            float tabHeight = 40f;

            // Draw Tabs at the top
            Rect tabsRect = new Rect(0f, 0f, inRect.width, tabHeight);
            DrawTabs(tabsRect);

            // Draw the search bar just below the tabs
            float searchBarHeight = 30f;
            Rect searchRect = new Rect(0f, tabHeight + 10f, inRect.width - 20f, searchBarHeight);
            searchString = Widgets.TextField(searchRect, searchString);

            // Adjust the starting position for the content after the tabs and search bar
            float contentStartY = tabHeight + searchBarHeight + 20f; // Adds space for tabs + search bar + padding
            float bottomPadding = 40f;  // Reserve space for the close button at the bottom
            Rect scrollRect = new Rect(0f, contentStartY, inRect.width - 16f, GetScrollHeight());
            Rect viewRect = new Rect(inRect.x, inRect.y + contentStartY, inRect.width, inRect.height - contentStartY - bottomPadding); // Account for bottom padding

            // Begin the scroll view
            Widgets.BeginScrollView(viewRect, ref scrollPosition, scrollRect);

            float yPos = 90f;

            // Show the ThingDefs for the selected category
            if (selectedCategory != null)
            {
                var filteredDefs = categorizedStuffDefs[selectedCategory]
                    .Where(def => string.IsNullOrEmpty(searchString) || def.label.ToLower().Contains(searchString.ToLower()))
                    .ToList();

                if (filteredDefs.Count > 0)
                {
                    foreach (ThingDef def in filteredDefs)
                    {
                        // Get the color and graphic of the ThingDef
                        Color defColor = StuffColorUtility.GetStuffColor(def);
                        Material defMaterial = def.graphic?.MatSingleFor(null) ?? BaseContent.BadMat; // Use MatSingle with color

                        // Draw the graphic on the left side with the correct color
                        Rect graphicRect = new Rect(10f, yPos + 5f, 30f, 30f);
                        if (defMaterial != null && defMaterial != BaseContent.BadMat)
                        {
                            GUI.color = defColor; // Apply the color to the GUI
                            GUI.DrawTexture(graphicRect, defMaterial.mainTexture); // Draw the texture
                            GUI.color = Color.white; // Reset GUI color to default
                        }
                        else
                        {
                            Widgets.DrawTextureFitted(graphicRect, BaseContent.BadTex, 1f);
                        }

                        // Create a button for each ThingDef with the label and color
                        Rect buttonRect = new Rect(graphicRect.xMax + 10f, yPos, inRect.width - 120f, 30f);
                        if (Widgets.ButtonText(buttonRect, def.label, true, true, true))
                        {
                            // Invoke the color selection action when a button is clicked
                            onColorSelected(defColor);
                        }

                        // Draw a small color box beside the label
                        Rect colorRect = new Rect(buttonRect.xMax + 5f, yPos + 5f, 20f, 20f);
                        Widgets.DrawBoxSolid(colorRect, defColor);

                        yPos += 35f;
                    }

                    yPos += 10f; // Add some space between categories
                }
            }

            Widgets.EndScrollView();

            // Draw the close button at the bottom
            Rect closeButtonRect = new Rect(inRect.width - 110f, inRect.height - bottomPadding, 100f, 30f);
            if (Widgets.ButtonText(closeButtonRect, "Close"))
            {
                this.Close();
            }
        }

        private Vector2 scrollPosition = Vector2.zero;

        // Draw tabs for each category
        private void DrawTabs(Rect rect)
        {
            float tabWidth = rect.width / categories.Count;
            for (int i = 0; i < categories.Count; i++)
            {
                StuffCategoryDef category = categories[i];
                Rect tabRect = new Rect(i * tabWidth, rect.y, tabWidth, 40f);

                // Is this the currently selected category?
                bool isActiveTab = selectedCategory == category;

                // Draw tab label (without button text functionality)
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(tabRect, category.label);
                Text.Anchor = TextAnchor.UpperLeft; // Reset text anchor
                if (Widgets.ButtonText(tabRect, category.label))
                {
                    Log.Message($"Tab clicked: {category.label}");
                    selectedCategory = category;
                    scrollPosition = Vector2.zero;
                }

                // Highlight active tab
                if (isActiveTab)
                {
                    Widgets.DrawHighlight(tabRect);
                }
            }
        }

        private float GetScrollHeight()
        {
            float height = 0f;
            if (selectedCategory != null)
            {
                var filteredDefs = categorizedStuffDefs[selectedCategory]
                    .Where(def => string.IsNullOrEmpty(searchString) || def.label.ToLower().Contains(searchString.ToLower()))
                    .ToList();

                if (filteredDefs.Count > 0)
                {
                    height += filteredDefs.Count * 35f;
                    height += 10f;
                }
            }
            return height;
        }
    }
}