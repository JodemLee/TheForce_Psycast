using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TheForce_Psycast.Lightsabers
{
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
            this.forcePause = false;
            this.draggable = true;
            this.doCloseButton = false;
            this.closeOnClickedOutside = true;
        }

        public override Vector2 InitialSize => new Vector2(500f, 600f);

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
            Rect scrollRect = new Rect(0f, contentStartY, inRect.width - 16f, GetScrollHeight());
            Rect viewRect = new Rect(inRect.x, inRect.y + contentStartY, inRect.width, inRect.height - contentStartY);
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

