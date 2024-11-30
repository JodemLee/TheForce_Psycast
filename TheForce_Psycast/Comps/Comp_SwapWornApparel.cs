using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using static TheForce_Psycast.Comps.PawnRenderNode_ModularHeadgear;

namespace TheForce_Psycast.Comps
{
    public class CompModularArmor : ThingComp
    {
        public int selectedModularArmorIndex = 0; // Start with the default
        public string selectedModularArmorPart;

        public CompProperties_ModularArmor Props => (CompProperties_ModularArmor)props;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref selectedModularArmorPart, "selectedModularArmorPart");
            Scribe_Values.Look(ref selectedModularArmorIndex, "selectedModularArmorIndex");
        }

        public List<string> GetModularArmorParts()
        {
            List<string> modularArmorParts = new List<string>();

            if (parent.def.apparel?.RenderNodeProperties != null)
            {
                foreach (var properties in parent.def.apparel.RenderNodeProperties)
                {
                    if (properties is PawnRenderNodeProperties_Modular renderNodeProps && renderNodeProps.modulararmorparts != null)
                    {
                        modularArmorParts.AddRange(renderNodeProps.modulararmorparts);
                    }
                    else if (properties is PawnRenderNodeProperties_ModularHead renderNodePropsHead && renderNodePropsHead.modulararmorparts != null)
                    {
                        modularArmorParts.AddRange(renderNodePropsHead.modulararmorparts);
                    }
                }
            }

            return modularArmorParts;
        }

        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            yield return new Command_Action
            {
                defaultLabel = "Select Armor Part",
                defaultDesc = "Choose an armor part from the modular options.",
                icon = ContentFinder<Texture2D>.Get("UI/Icons/Gizmo/LightsaberCustomization"), // Replace with a valid texture path
                action = () =>
                {
                    Find.WindowStack.Add(new Dialog_ModularArmorSelector(this));
                }
            };
        }
    }

    public class CompProperties_ModularArmor : CompProperties
    {
        public CompProperties_ModularArmor()
        {
            this.compClass = typeof(CompModularArmor);
        }
    }

    public class Dialog_ModularArmorSelector : Window
    {
        private CompModularArmor compModularArmor;
        private List<string> armorParts;

        // Variables for scrolling
        private Vector2 scrollPosition;
        private float scrollViewHeight;

        // Increase dialog size to allow for a bigger texture preview
        public override Vector2 InitialSize => new Vector2(600f, 500f);

        public Dialog_ModularArmorSelector(CompModularArmor comp)
        {
            this.compModularArmor = comp;
            this.armorParts = comp.GetModularArmorParts();
            doCloseButton = true;
            closeOnClickedOutside = true;

            // Initialize scroll position and height
            scrollPosition = Vector2.zero;
            scrollViewHeight = armorParts.Count * 45f; // Adjust based on number of items
        }

        public override void DoWindowContents(Rect inRect)
        {
            float lineHeight = Text.LineHeight;
            float buttonHeight = 40f;
            float textureSize = 250f;  // Set size for the large texture preview

            // Title
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width, lineHeight), "Select Armor Part");

            // Reset font size
            Text.Font = GameFont.Small;

            // Centered texture preview
            Rect texturePreviewRect = new Rect(
                (inRect.width - textureSize) / 2,  // Center horizontally
                (inRect.height - textureSize) / 2 - 40f,  // Center vertically with some padding for buttons
                textureSize, textureSize
            );

            if (!string.IsNullOrEmpty(compModularArmor.selectedModularArmorPart))
            {
                Texture2D previewTexture = ContentFinder<Texture2D>.Get(compModularArmor.selectedModularArmorPart, false);
                if (previewTexture != null)
                {
                    Widgets.DrawTextureFitted(texturePreviewRect, previewTexture, 1.0f);
                }
                else
                {
                    Widgets.Label(texturePreviewRect, "No Preview");
                }
            }
            else
            {
                Widgets.Label(texturePreviewRect, "No Armor Selected");
            }

            // Default armor part button (above the texture)
            Rect buttonRect = new Rect((inRect.width - textureSize) / 2, texturePreviewRect.yMin - buttonHeight - 10f, textureSize, buttonHeight);
            if (Widgets.ButtonText(buttonRect, "Default Armor"))
            {
                SelectArmorPart(0);
            }

            // Scrollable area for armor part buttons (below the texture)
            float scrollAreaHeight = inRect.height - texturePreviewRect.yMax - 30f;
            Rect scrollOutRect = new Rect(0f, texturePreviewRect.yMax + 10f, inRect.width, scrollAreaHeight);
            Rect scrollViewRect = new Rect(0f, 0f, inRect.width - 16f, scrollViewHeight);  // Width is slightly smaller to account for scrollbar

            Widgets.BeginScrollView(scrollOutRect, ref scrollPosition, scrollViewRect);

            for (int i = 0; i < armorParts.Count; i++)
            {
                string armorPartName = armorParts[i];

                // Split the string to get the last part (file name or final part)
                string[] parts = armorPartName.Split('/');
                string displayedName = parts[parts.Length - 1];  // Get the last part of the path

                buttonRect = new Rect(0f, i * (buttonHeight + 5f), inRect.width - 20f, buttonHeight);
                if (Widgets.ButtonText(buttonRect, displayedName))
                {
                    SelectArmorPart(i + 1);
                    compModularArmor.parent.Notify_ColorChanged();
                }
            }

            Widgets.EndScrollView();
        }

        private void SelectArmorPart(int index)
        {
            List<string> armorParts = compModularArmor.GetModularArmorParts();
            {
                if (index - 1 >= 0 && index - 1 < armorParts.Count)
                {
                    compModularArmor.selectedModularArmorPart = armorParts[index - 1];
                    compModularArmor.selectedModularArmorIndex = index - 1;
                }
            }
        }
    }
}