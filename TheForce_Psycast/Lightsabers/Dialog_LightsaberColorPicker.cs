using System;
using System.Collections.Generic;
using TheForce_Psycast.Lightsabers;
using UnityEngine;
using Verse;

namespace TheForce_Psycast
{
    [StaticConstructorOnStartup]
    public class Dialog_LightsaberColorPicker : Window
    {
        private const int MinValue = 0;
        private const int MaxValue = 255;

        private const float WindowWidth = 460f;
        private const float WindowHeight = 500f;

        private const float LabelWidth = 40f;
        private const float SliderWidth = 180f;
        private const float InputFieldWidth = 60f;

        private const float ButtonWidth = 80f;
        private const float ButtonHeight = 30f;

        private const float ButtonRowOffset = 450f;
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

        private enum Tab
        {
            RGBSelector,
            Presets
        }

        private Tab currentTab = Tab.RGBSelector;

        public Dialog_LightsaberColorPicker(Comp_LightsaberBlade lightsaberBlade, Action<Color, Color, Color, Color> confirmAction)
        {
            this.lightsaberBlade = lightsaberBlade;
            this.confirmAction = confirmAction;

            if (outerglowTexture == null)
            {
                outerglowTexture = ContentFinder<Texture2D>.Get("Things/Item/Resource/CrystalColorPicker/OuterGlow");
                innercoreTexture = ContentFinder<Texture2D>.Get("Things/Item/Resource/CrystalColorPicker/InnerCore");
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
        }



        public override Vector2 InitialSize => new Vector2(WindowWidth, CalculateWindowHeight());

        private float CalculateWindowHeight()
        {
            // Base height
            float height = 40f; // Padding

            // Add height for each color picker if its graphic is not null
            if (lightsaberBlade.Graphic != null)
                height += 25f * 5; // 5 rows of sliders for blade 1

            if (lightsaberBlade.LightsaberCore1Graphic != null)
                height += 25f * 5; // 5 rows of sliders for core 1

            if (lightsaberBlade.LightsaberBlade2Graphic != null)
                height += 25f * 5; // 5 rows of sliders for blade 2

            if (lightsaberBlade.LightsaberCore2Graphic != null)
                height += 25f * 5; // 5 rows of sliders for core 2

            // Add height for color swatches
            height += 20f * (lightsaberBlade.Graphic != null ? 1 : 0); // Blade 1 swatch
            height += 20f * (lightsaberBlade.LightsaberCore1Graphic != null ? 1 : 0); // Core 1 swatch
            height += 20f * (lightsaberBlade.LightsaberBlade2Graphic != null ? 1 : 0); // Blade 2 swatch
            height += 20f * (lightsaberBlade.LightsaberCore2Graphic != null ? 1 : 0); // Core 2 swatch

            // Add height for buttons
            height += ButtonHeight + 50f; // Button row and padding

            return Mathf.Max(height, WindowHeight);
        }

        public override void DoWindowContents(Rect inRect)
        {
            DrawTabs(inRect);

            switch (currentTab)
            {
                case Tab.RGBSelector:
                    DrawRGBSelector(inRect);
                    break;
                case Tab.Presets:
                    DrawPresets(inRect);
                    break;
            }
        }

        private void DrawTabs(Rect inRect)
        {
            float tabWidth = inRect.width / Enum.GetValues(typeof(Tab)).Length;

            foreach (Tab tab in Enum.GetValues(typeof(Tab)))
            {
                Rect tabRect = new Rect(inRect.x + (float)tab * tabWidth, inRect.y, tabWidth, 30f);
                if (Widgets.ButtonText(tabRect, tab.ToString()))
                {
                    currentTab = tab;
                }
            }
        }

        private void DrawRGBSelector(Rect inRect)
        {
            float rowOffset = 40f;

            // Blade 1 color picker
            if (lightsaberBlade.Graphic != null)
            {
                Widgets.Label(new Rect(0f, rowOffset, 200f, 22f), "Blade:");
                DrawRandomizeButton(rowOffset, () =>
                {
                    RandomizeRGBValues(ref lightsaberBlade1OverrideColorRed, ref lightsaberBlade1OverrideColorGreen, ref lightsaberBlade1OverrideColorBlue);
                });
                rowOffset += 22f;
                DrawSliderWithInput(ref rowOffset, ref lightsaberBlade1OverrideColorRed, "Red");
                DrawSliderWithInput(ref rowOffset, ref lightsaberBlade1OverrideColorGreen, "Green");
                DrawSliderWithInput(ref rowOffset, ref lightsaberBlade1OverrideColorBlue, "Blue");
                DrawSliderWithInput(ref rowOffset, ref lightsaberBlade1OverrideColorAlpha, "Alpha");
            }

            // Core 1 color picker
            if (lightsaberBlade.LightsaberCore1Graphic != null)
            {
                rowOffset += 10f; // Add some space between Blade and Core
                Widgets.Label(new Rect(0f, rowOffset, 200f, 22f), "Core 1:");
                DrawRandomizeButton(rowOffset, () =>
                {
                    RandomizeRGBValues(ref lightsaberCore1Red, ref lightsaberCore1Green, ref lightsaberCore1Blue);
                });
                rowOffset += 22f;
                DrawSliderWithInput(ref rowOffset, ref lightsaberCore1Red, "Red");
                DrawSliderWithInput(ref rowOffset, ref lightsaberCore1Green, "Green");
                DrawSliderWithInput(ref rowOffset, ref lightsaberCore1Blue, "Blue");
                DrawSliderWithInput(ref rowOffset, ref lightsaberCore1Alpha, "Alpha");
            }

            // Blade 2 color picker
            if (lightsaberBlade.LightsaberBlade2Graphic != null)
            {
                rowOffset += 10f; // Add some space between Core 1 and Blade 2
                Widgets.Label(new Rect(0f, rowOffset, 200f, 22f), "Blade 2:");
                DrawRandomizeButton(rowOffset, () =>
                {
                    RandomizeRGBValues(ref lightsaberBlade2Red, ref lightsaberBlade2Green, ref lightsaberBlade2Blue);
                });
                rowOffset += 22f;
                DrawSliderWithInput(ref rowOffset, ref lightsaberBlade2Red, "Red");
                DrawSliderWithInput(ref rowOffset, ref lightsaberBlade2Green, "Green");
                DrawSliderWithInput(ref rowOffset, ref lightsaberBlade2Blue, "Blue");
                DrawSliderWithInput(ref rowOffset, ref lightsaberBlade2Alpha, "Alpha");
            }

            // Core 2 color picker
            if (lightsaberBlade.LightsaberCore2Graphic != null)
            {
                rowOffset += 10f; // Add some space between Blade 2 and Core 2
                Widgets.Label(new Rect(0f, rowOffset, 200f, 22f), "Core 2:");
                DrawRandomizeButton(rowOffset, () =>
                {
                    RandomizeRGBValues(ref lightsaberCore2Red, ref lightsaberCore2Green, ref lightsaberCore2Blue);
                });
                rowOffset += 22f;
                DrawSliderWithInput(ref rowOffset, ref lightsaberCore2Red, "Red");
                DrawSliderWithInput(ref rowOffset, ref lightsaberCore2Green, "Green");
                DrawSliderWithInput(ref rowOffset, ref lightsaberCore2Blue, "Blue");
                DrawSliderWithInput(ref rowOffset, ref lightsaberCore2Alpha, "Alpha");
            }

            UpdateCurrentColor();

            DrawColorSwatches();

            float buttonRowOffset = inRect.height - ButtonHeight - ButtonMargin;

            DrawButtons(buttonRowOffset);
        }

        private void DrawRandomizeButton(float rowOffset, Action randomizeAction)
        {
            Rect buttonRect = new Rect(LabelWidth + 20f, rowOffset, 100f, 22f);
            if (Widgets.ButtonText(buttonRect, "Randomize"))
            {
                randomizeAction?.Invoke();
            }
        }

        private void RandomizeRGBValues(ref int red, ref int green, ref int blue)
        {
            red = UnityEngine.Random.Range(0, 256);
            green = UnityEngine.Random.Range(0, 256);
            blue = UnityEngine.Random.Range(0, 256);
        }

        private void DrawSliderWithInput(ref float rowOffset, ref int value, string label)
        {
            Rect labelRect = new Rect(0f, rowOffset, LabelWidth, 22f);
            Rect sliderRect = new Rect(LabelWidth, rowOffset, SliderWidth, 22f);
            Rect inputFieldRect = new Rect(LabelWidth + SliderWidth + 10f, rowOffset, InputFieldWidth, 22f);

            Text.Font = GameFont.Tiny;
            Widgets.Label(labelRect, label);
            Text.Font = GameFont.Small;

            value = Mathf.RoundToInt(Widgets.HorizontalSlider(sliderRect, value, MinValue, MaxValue));
            string buffer = value.ToString();
            Widgets.TextFieldNumeric(inputFieldRect, ref value, ref buffer, MinValue, MaxValue);

            rowOffset += 22f;
        }

        private void DrawPresets(Rect inRect)
        {
            // Define the presets and their corresponding colors
            Dictionary<string, Color> presets = new Dictionary<string, Color>
                {
                    { "Blue", new Color(0, 0, 1) },
                    { "Red", new Color(1, 0, 0) },
                    { "Green", new Color(0, 1, 0) },
                    { "Yellow", new Color(1, 1, 0) },
                    { "Purple", new Color(0.5f, 0, 0.5f) },
                    { "Orange", new Color (1 , .64f, 0)  },
                    { "White", new Color(1, 1, 1 ) }
                    // Add more presets as needed
                };

            // Calculate button dimensions and positioning
            float buttonWidth = 100f;
            float buttonHeight = 30f;
            float buttonSpacing = 10f;
            float startX = inRect.x + 20f; // Starting X position
            float startY = inRect.y + 50f; // Starting Y position
            float currentX = startX;
            float currentY = startY;

            // Iterate over presets and draw buttons
            foreach (var preset in presets)
            {
                Rect buttonRect = new Rect(currentX, currentY, buttonWidth, buttonHeight);
                // Wrap to the next row if the button exceeds the available space
                if (currentX + buttonWidth > inRect.xMax - 20f)
                {
                    currentX = startX;
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

                float buttonRowOffset = inRect.height - ButtonHeight - ButtonMargin;

                DrawExitButtons(buttonRowOffset);
            }
        }


        private float SwatchWidth => 155f;
        private float SwatchHeight => 155f;

        private void DrawColorSwatches()
        {
            Rect primarySwatchRect = new Rect(300f, 100f, SwatchWidth, SwatchHeight);
            if (lightsaberBlade.Graphic != null)
            {
                GUI.color = new Color(1, 1, 1, 1);
                GUI.DrawTexture(primarySwatchRect, outerglowTexture);
                GUI.color = lightsaberBlade1OverrideColor;
                GUI.DrawTexture(primarySwatchRect, outerglowTexture);
            }

            Rect lightsaberCore1SwatchRect = new Rect(300f, 100f, SwatchWidth, SwatchHeight);
            if (lightsaberBlade.LightsaberCore1Graphic != null)
            {
                GUI.color = new Color(1, 1, 1, 1);
                GUI.DrawTexture(lightsaberCore1SwatchRect, innercoreTexture);
                GUI.color = lightsaberCore1CurrentColor;
                GUI.DrawTexture(lightsaberCore1SwatchRect, innercoreTexture);
            }

            Rect lightsaberBlade2SwatchRect = new Rect(300f, 300f, SwatchWidth, SwatchHeight);
            if (lightsaberBlade.LightsaberBlade2Graphic != null)
            {
                GUI.color = new Color(1, 1, 1, 1);
                GUI.DrawTexture(lightsaberBlade2SwatchRect, outerglowTexture);
                GUI.color = lightsaberBlade2CurrentColor;
                GUI.DrawTexture(lightsaberBlade2SwatchRect, outerglowTexture);
            }

            Rect lightsaberCore2SwatchRect = new Rect(300f, 300f, SwatchWidth, SwatchHeight);
            if (lightsaberBlade.LightsaberCore2Graphic != null)
            {
                GUI.color = new Color(1, 1, 1, 1);
                GUI.DrawTexture(lightsaberCore2SwatchRect, innercoreTexture);
                GUI.color = lightsaberCore2CurrentColor;
                GUI.DrawTexture(lightsaberCore2SwatchRect, innercoreTexture);
            }

            GUI.color = Color.white;
        }

        private void DrawExitButtons(float buttonRowOffset)
        {
            if (Widgets.ButtonText(new Rect(ButtonMargin, buttonRowOffset, ButtonWidth, ButtonHeight), "Exit"))
            {
                Close();
            }
        }

        private void DrawButtons(float buttonRowOffset)
        {
            if (Widgets.ButtonText(new Rect(ButtonMargin, buttonRowOffset, ButtonWidth, ButtonHeight), "Cancel"))
            {
                Close();
            }

            if (Widgets.ButtonText(new Rect(ButtonWidth + ButtonMargin * 2 + ButtonSpacing, buttonRowOffset, ButtonWidth, ButtonHeight), "OK"))
            {
                confirmAction(lightsaberBlade1OverrideColor, lightsaberCore1CurrentColor, lightsaberBlade2CurrentColor, lightsaberCore2CurrentColor);
                lightsaberBlade.SetColor(lightsaberBlade1OverrideColor);
                lightsaberBlade.SetLightsaberCore1Color(lightsaberCore1CurrentColor);
                lightsaberBlade.SetLightsaberBlade2Color(lightsaberBlade2CurrentColor);
                lightsaberBlade.SetLightsaberCore2Color(lightsaberCore2CurrentColor);
                Close();
            }
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