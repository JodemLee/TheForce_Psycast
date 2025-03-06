using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace TheForce_Psycast.Abilities.Sith_Sorcery
{
    internal class Ability_CraftSithArtifact : Ability_WriteCombatLog
    {
        private AbilityExtension_CraftableArtifacts extension;

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);

            if (targets != null && targets.Length > 0 && targets[0].HasThing)
            {
                Thing targetThing = targets[0].Thing;

                if (ValidateTarget(targetThing, true))
                {
                    if (extension == null) extension = def.GetModExtension<AbilityExtension_CraftableArtifacts>();
                    Find.WindowStack.Add(new Dialog_ChooseArtifactToCraft(this, targetThing));
                }
            }
            else
            {
                Messages.Message("Force.InvalidTargetForCrafting".Translate(targets[0].Thing.LabelShort), MessageTypeDefOf.RejectInput, false);
            }
        }

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            // Check if the pawn has the weakness hediff
            if (pawn.health.hediffSet.HasHediff(ForceDefOf.Force_SithArtifactWeakness))
            {
                if (showMessages)
                {
                    Messages.Message("You are too weak to craft another artifact right now.", MessageTypeDefOf.RejectInput, false);
                }
                return false;
            }

            if (!base.ValidateTarget(target, showMessages)) return false;
            if (!target.HasThing) return false;

            if (extension == null)
            {
                extension = def.GetModExtension<AbilityExtension_CraftableArtifacts>();
            }

            if (extension == null || extension.targetArtifactMappings == null)
            {
                if (showMessages)
                {
                    Messages.Message("Force.InvalidTargetForCrafting".Translate(), MessageTypeDefOf.RejectInput, false);
                }
                return false;
            }

            string targetDefName = target.Thing.def.defName;
            var targetCategories = target.Thing.def.thingCategories.Select(c => c.defName).ToList();

            bool isValidTarget = extension.targetArtifactMappings
                .Any(mapping =>
                {
                    bool defMatches = mapping.targetDefNames != null && mapping.targetDefNames.Contains(targetDefName);
                    bool categoryMatches = mapping.targetCategories != null && mapping.targetCategories.Any(category => targetCategories.Contains(category));

                    return defMatches || categoryMatches;
                });

            if (!isValidTarget)
            {
                if (showMessages)
                {
                    Messages.Message("Force.InvalidTargetForCrafting".Translate(), MessageTypeDefOf.RejectInput, false);
                }
                return false;
            }

            return true;
        }

        public void ApplyWeaknessHediff(Pawn pawn, ThingDef artifactDef)
        {
            float artifactValue = artifactDef.BaseMarketValue;

            // Calculate severity of the hediff based on the artifact's value
            float hediffSeverity = Mathf.Clamp(artifactValue / 1000f, 0.1f, 1.0f);

            // Apply the hediff to the pawn
            Hediff weaknessHediff = HediffMaker.MakeHediff(ForceDefOf.Force_SithArtifactWeakness, pawn);
            weaknessHediff.Severity = hediffSeverity;
            pawn.health.AddHediff(weaknessHediff);
        }


        public List<ThingDef> GetCraftableArtifactsForTarget(Thing target)
        {
            if (extension == null)
            {
                extension = def.GetModExtension<AbilityExtension_CraftableArtifacts>();
            }

            if (extension == null || extension.targetArtifactMappings == null)
            {
                Log.Error("AbilityExtension_CraftableArtifacts or targetArtifactMappings is null.");
                return new List<ThingDef>();
            }

            // Get target def name and categories
            string targetDefName = target.def.defName;
            var targetCategories = target.def.thingCategories.Select(c => c.defName).ToList();

            // Try to find a matching TargetArtifactMapping
            var mapping = extension.targetArtifactMappings
                .FirstOrDefault(x =>
                    (x.targetDefNames != null && x.targetDefNames.Contains(targetDefName)) ||
                    (x.targetCategories != null && x.targetCategories.Any(category => targetCategories.Contains(category)))
                );

            if (mapping == null)
            {
                Log.Error($"No TargetArtifactMapping found for target: {targetDefName}");
                return new List<ThingDef>();
            }

            return mapping.craftableArtifacts ?? new List<ThingDef>();
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
            if (artifactOptions == null || artifactOptions.Count == 0)
            {
                listingStandard.Label("No artifacts available for this target.");
                listingStandard.End();
                return;
            }

            float scrollViewHeight = CalculateScrollViewHeight(artifactOptions, inRect.width);
            Rect viewRect = new Rect(0, 0, inRect.width - 16f, scrollViewHeight);

            Widgets.BeginScrollView(new Rect(inRect.x, inRect.y + 40f, inRect.width, inRect.height - 80f), ref scrollPosition, viewRect);

            float y = 0f;
            foreach (ThingDef artifactDef in artifactOptions)
            {
                float descHeight = Text.CalcHeight(artifactDef.description, inRect.width - 120f);
                Rect itemRect = new Rect(0f, y, viewRect.width, 130f + descHeight);
                DrawArtifactOption(itemRect, artifactDef);
                y += 130f + descHeight;
            }

            Widgets.EndScrollView();

            listingStandard.End();
        }

        private float CalculateScrollViewHeight(List<ThingDef> artifactOptions, float viewWidth)
        {
            float height = 0f;
            foreach (ThingDef artifactDef in artifactOptions)
            {
                float descHeight = Text.CalcHeight(artifactDef.description, viewWidth - 120f);
                height += 130f + descHeight;
            }
            return height;
        }

        private void DrawArtifactOption(Rect rect, ThingDef artifactDef)
        {
            Widgets.DrawBoxSolid(rect, new Color(0.1f, 0.1f, 0.1f, 0.3f));

            float iconSize = 100f;
            float margin = 10f;
            float labelHeight = 30f;
            float buttonWidth = 60f;
            float buttonHeight = 30f;

            Texture2D texture = artifactDef.uiIcon;
            Rect iconRect = new Rect(rect.x + margin, rect.y + margin, iconSize, iconSize);
            Widgets.DrawTextureFitted(iconRect, texture, 1f);

            Rect labelRect = new Rect(iconRect.xMax + margin, rect.y + margin, rect.width - iconRect.width - buttonWidth - 4 * margin, labelHeight);
            Widgets.Label(labelRect, artifactDef.LabelCap);

            Text.WordWrap = true;
            float descHeight = Text.CalcHeight(artifactDef.description, rect.width - iconRect.width - 3 * margin);
            Rect descRect = new Rect(iconRect.xMax + margin, labelRect.yMax + margin, rect.width - iconRect.width - 3 * margin, descHeight);
            Widgets.Label(descRect, artifactDef.description);

            Rect buttonRect = new Rect(rect.xMax - buttonWidth - margin, rect.yMax - buttonHeight - margin, buttonWidth, buttonHeight);
            if (Widgets.ButtonText(buttonRect, "Craft"))
            {
                CraftArtifact(artifactDef);
                Close();
            }
        }

        private void CraftArtifact(ThingDef artifactDef)
        {
            IntVec3 cell = targetThing.Position;

            // Determine the appropriate stuff
            ThingDef stuffDef = GetAppropriateStuffForArtifact();

            // Create the artifact with the determined stuff
            Thing artifact;
            if (artifactDef.MadeFromStuff && stuffDef != null)
            {
                artifact = ThingMaker.MakeThing(artifactDef, stuffDef);
            }
            else
            {
                artifact = ThingMaker.MakeThing(artifactDef);
            }

            GenSpawn.Spawn(artifact, cell, ability.pawn.Map);

            // Apply weakness hediff through the ability
            ability.ApplyWeaknessHediff(ability.pawn, artifactDef);

            // Scale the number of resources destroyed based on the artifact's value
            float artifactValue = artifactDef.BaseMarketValue;

            // Calculate how many resources to destroy (adjust this formula as needed)
            int resourcesToDestroy = Mathf.Clamp(Mathf.FloorToInt(artifactValue / 100f), 1, targetThing.stackCount);

            // Destroy or decrease the stack count of resources
            if (resourcesToDestroy < targetThing.stackCount)
            {
                targetThing.stackCount -= resourcesToDestroy;
            }
            else
            {
                targetThing.Destroy(DestroyMode.Vanish);
            }

        }

        private ThingDef GetAppropriateStuffForArtifact()
        {
            // Get the stuff from the targetThing if available
            if (targetThing.def.stuffProps != null)
            {
                return targetThing.def;
            }
            else
            {
                // Default to steel if no specific stuff is available
                return DefDatabase<ThingDef>.GetNamed("Steel");
            }
        }
    }

    public class AbilityExtension_CraftableArtifacts : DefModExtension
    {
        private Dictionary<string, List<ThingDef>> targetArtifactMapByDef;
        private Dictionary<string, List<ThingDef>> targetArtifactMapByCategory;

        public List<TargetArtifactMapping> targetArtifactMappings;

        public void InitializeMapping()
        {
            if (targetArtifactMappings == null)
            {
                return;
            }

            targetArtifactMapByDef = targetArtifactMappings
                .Where(mapping => mapping.targetDefNames != null)
                .ToDictionary(mapping => mapping.targetDefNames.FirstOrDefault(), mapping => mapping.craftableArtifacts);

            targetArtifactMapByCategory = targetArtifactMappings
                .Where(mapping => mapping.targetCategories != null)
                .SelectMany(mapping => mapping.targetCategories, (mapping, category) => new { category, artifacts = mapping.craftableArtifacts })
                .ToDictionary(x => x.category, x => x.artifacts);
        }

        public List<ThingDef> GetCraftableArtifactsForTarget(Thing target)
        {
            if (targetArtifactMapByDef == null || targetArtifactMapByCategory == null)
            {
                InitializeMapping();
            }

            var targetDefName = target.def.defName;
            var targetCategories = target.def.thingCategories.Select(c => c.defName);

            if (targetArtifactMapByDef != null && targetArtifactMapByDef.TryGetValue(targetDefName, out var artifactsByDef))
            {
                return artifactsByDef;
            }

            foreach (var category in targetCategories)
            {
                if (targetArtifactMapByCategory != null && targetArtifactMapByCategory.TryGetValue(category, out var artifactsByCategory))
                {
                    return artifactsByCategory;
                }
            }

            return new List<ThingDef>();
        }
    }

    public class TargetArtifactMapping
    {
        public List<string> targetDefNames; // Specific ThingDefs
        public List<string> targetCategories; // ThingCategories
        public List<ThingDef> craftableArtifacts;
    }
}
