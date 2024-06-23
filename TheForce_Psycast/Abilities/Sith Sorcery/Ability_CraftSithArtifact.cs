using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace TheForce_Psycast.Abilities.Sith_Sorcery
{
    internal class Ability_CraftSithArtifact : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);

            if (targets != null && targets.Length > 0 && targets[0].HasThing)
            {
                Thing targetThing = targets[0].Thing;

                if (ValidateTarget(targetThing, true))
                {
                    Find.WindowStack.Add(new Dialog_ChooseArtifactToCraft(this, targetThing));
                }
                // If target validation fails, no need to show another message here because ValidateTarget already shows it
            }
            else
            {
                Messages.Message("Force.InvalidTargetForCrafting".Translate(), MessageTypeDefOf.RejectInput, false);
            }
        }

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            if (!base.ValidateTarget(target, showMessages)) return false;
            if (!target.HasThing) return false;

            AbilityExtension_CraftableArtifacts extension = def.GetModExtension<AbilityExtension_CraftableArtifacts>();
            if (extension == null || extension.targetArtifactMappings == null)
            {
                if (showMessages) Messages.Message("Force.InvalidTargetForCrafting".Translate(), MessageTypeDefOf.RejectInput, false);
                return false;
            }

            // Check if the target's defName is in the mappings
            string targetDefName = target.Thing.def.defName;
            TargetArtifactMapping mapping = extension.targetArtifactMappings.Find(x => x.targetDefName == targetDefName);

            if (mapping == null)
            {
                if (showMessages) Messages.Message("Force.InvalidTargetForCrafting".Translate(), MessageTypeDefOf.RejectInput, false);
                return false;
            }

            return true;
        }

        // Helper method to get the list of craftable artifacts for a specific target thing
        public List<ThingDef> GetCraftableArtifactsForTarget(Thing target)
        {
            AbilityExtension_CraftableArtifacts extension = def.GetModExtension<AbilityExtension_CraftableArtifacts>();
            if (extension != null && extension.targetArtifactMappings != null)
            {
                // Find the appropriate mapping based on target's defName
                TargetArtifactMapping mapping = extension.targetArtifactMappings.Find(x => x.targetDefName == target.def.defName);
                if (mapping != null)
                {
                    return mapping.craftableArtifacts;
                }
            }
            return new List<ThingDef>();
        }
    }


    internal class Dialog_ChooseArtifactToCraft : Window
    {
        private Ability_CraftSithArtifact ability;
        private Thing targetThing;
        private Vector2 scrollPosition = Vector2.zero;

        public Dialog_ChooseArtifactToCraft(Ability_CraftSithArtifact ability, Thing targetThing)
        {
            this.ability = ability;
            this.targetThing = targetThing;
            forcePause = true;
            doCloseX = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;
        }

        public override Vector2 InitialSize => new Vector2(600f, 400f);

        public override void DoWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            listingStandard.Label("Select artifact to craft:");

            List<ThingDef> artifactOptions = ability.GetCraftableArtifactsForTarget(targetThing);

            // Calculate the total height of the scroll view dynamically
            float scrollViewHeight = 0f;
            foreach (ThingDef artifactDef in artifactOptions)
            {
                float descHeight = Text.CalcHeight(artifactDef.description, inRect.width - 120f);
                scrollViewHeight += 130f + descHeight; // Reduced padding
            }
            Rect viewRect = new Rect(0, 0, inRect.width - 16f, scrollViewHeight);

            Widgets.BeginScrollView(new Rect(inRect.x, inRect.y + 40f, inRect.width, inRect.height - 80f), ref scrollPosition, viewRect);

            float y = 0f;
            foreach (ThingDef artifactDef in artifactOptions)
            {
                float descHeight = Text.CalcHeight(artifactDef.description, inRect.width - 120f);
                Rect itemRect = new Rect(0f, y, viewRect.width, 130f + descHeight); // Reduced item height
                DrawArtifactOption(itemRect, artifactDef);
                y += 130f + descHeight;
            }

            Widgets.EndScrollView();

            listingStandard.End();
        }

        private void DrawArtifactOption(Rect rect, ThingDef artifactDef)
        {
            // Draw a background for clarity
            Widgets.DrawBoxSolid(rect, new Color(0.1f, 0.1f, 0.1f, 0.3f));

            // Calculate margins and dimensions for elements
            float iconSize = 100f;
            float margin = 10f;
            float labelHeight = 30f;
            float buttonWidth = 60f;
            float buttonHeight = 30f;

            // Draw the item icon
            Texture2D texture = artifactDef.uiIcon;
            Rect iconRect = new Rect(rect.x + margin, rect.y + margin, iconSize, iconSize);
            Widgets.DrawTextureFitted(iconRect, texture, 1f);

            // Draw the item label
            Rect labelRect = new Rect(iconRect.xMax + margin, rect.y + margin, rect.width - iconRect.width - buttonWidth - 4 * margin, labelHeight);
            Widgets.Label(labelRect, artifactDef.LabelCap);

            // Calculate the height of the description text
            Text.WordWrap = true;
            float descHeight = Text.CalcHeight(artifactDef.description, rect.width - iconRect.width - 3 * margin);

            // Draw the item description with word wrap
            Rect descRect = new Rect(iconRect.xMax + margin, labelRect.yMax + margin, rect.width - iconRect.width - 3 * margin, descHeight);
            Widgets.Label(descRect, artifactDef.description);

            // Draw the Craft button below the description
            Rect buttonRect = new Rect(rect.xMax - buttonWidth - margin, rect.yMax - buttonHeight - margin, buttonWidth, buttonHeight);
            if (Widgets.ButtonText(buttonRect, "Craft"))
            {
                CraftArtifact(artifactDef);
                Close(); // Close the window after crafting
            }
        }

        private void CraftArtifact(ThingDef artifactDef)
        {
            IntVec3 cell = ability.pawn.Position; // Adjust position as needed
            Thing artifact = ThingMaker.MakeThing(artifactDef);
            GenSpawn.Spawn(artifact, cell, ability.pawn.Map);
        }
    }



    public class AbilityExtension_CraftableArtifacts : DefModExtension
    {
        public List<TargetArtifactMapping> targetArtifactMappings;

        public List<ThingDef> GetCraftableArtifactsForTarget(Thing target)
        {
            foreach (var mapping in targetArtifactMappings)
            {
                if (mapping.targetDefName == target.def.defName)
                {
                    return mapping.craftableArtifacts;
                }
            }
            return new List<ThingDef>();
        }
    }

    public class TargetArtifactMapping
    {
        public string targetDefName;
        public List<ThingDef> craftableArtifacts;
    }
}

