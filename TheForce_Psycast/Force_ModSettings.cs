using RimWorld;
using System.Collections.Generic;
using System.Linq;
using TheForce_Psycast.Abilities.Jedi_Training;
using UnityEngine;
using VanillaPsycastsExpanded;
using Verse;
// ReSharper disable All

namespace TheForce_Psycast
{
    public class Force_ModSettings : ModSettings
    {
        public static float spinRate = 1f;
        public static float entropyGain = 1f;
        public static bool usePsycastStat = false;
        public static float offSetMultiplier = 1;
        public static int apprenticeCapacity = 1;
        public static float deflectionMultiplier = 1f;
        public static float insanityChance = 1f;
        public static bool IncreaseDarksideOnKill = false;
        public static bool LightsaberGlowEnabled = false;
        public static bool LightsaberRealGlow = false;
        public static bool LightsaberFakeGlow = false;
        public static bool DeflectionSpin = true;
        public static float glowRadius = 2.5f;
        public static bool customStance = false;
        public static bool projectileDeflectionSelector = false;
        public static List<string> deflectableProjectileNames = new List<string>();
        public static HashSet<int> deflectableProjectileHashes = new HashSet<int>();
        public static bool lightsaberParryEnabled = true;
        public static bool lightsaberCustomizationUndrafted = false;
        public static bool rankUpApprentice = false;
        public static bool rankUpMaster = false;
        public static int requiredGraduatedApprentices = 1;
        public static int requiredPsycastLevel = 10; // Default value
        private PsycastSettings psycastSettings;
        public static int maxLevelReference;
        public Force_ModSettings()
        {
            // Ensure PsycastSettings is loaded before accessing
            var psycastMod = LoadedModManager.GetMod<PsycastsMod>();
            if (psycastMod != null)
            {
                psycastSettings = psycastMod.GetSettings<PsycastSettings>();
                maxLevelReference = psycastSettings?.maxLevel ?? 30;  // Default to 30 if psycastSettings is null
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref spinRate, "spinRate", 9f);
            Scribe_Values.Look(ref entropyGain, "entropyGain", 20f);
            Scribe_Values.Look(ref offSetMultiplier, "offSetMultiplier", 3f);
            Scribe_Values.Look(ref deflectionMultiplier, "deflectionMultiplier", 1f);
            Scribe_Values.Look(ref usePsycastStat, "usePsycastStat");
            Scribe_Values.Look(ref apprenticeCapacity, "apprenticeCapacity", 1);
            Scribe_Values.Look(ref insanityChance, "insanityChance", 0.25f);
            Scribe_Values.Look(ref IncreaseDarksideOnKill, "increasedarksideonkill");
            Scribe_Values.Look(ref LightsaberGlowEnabled, "lightsaberglowenabled", false);
            Scribe_Values.Look(ref LightsaberRealGlow, "lightsaberrealglow", false);
            Scribe_Values.Look(ref LightsaberFakeGlow, "lightsaberfakeglow", false);
            Scribe_Values.Look(ref glowRadius, "glowRadius", 2.5f);
            Scribe_Values.Look(ref DeflectionSpin, "deflectionspin", false);
            Scribe_Values.Look(ref customStance, "customStance", false);
            Scribe_Values.Look(ref projectileDeflectionSelector, "projectileDeflectionSelector", false);
            Scribe_Values.Look(ref lightsaberParryEnabled, "lightsaberparryEnabled", false);
            Scribe_Values.Look(ref lightsaberCustomizationUndrafted, "lightsaberCustomizationUndrafted", false);
            Scribe_Values.Look(ref rankUpApprentice, "rankUpApprentice", false);
            Scribe_Values.Look(ref rankUpMaster, "rankUpMaster", false);
            Scribe_Values.Look(ref requiredGraduatedApprentices, "requiredGraduatedApprentices", 1);
            Scribe_Values.Look(ref requiredPsycastLevel, "requiredPsycastLevel", 10);
            Scribe_Collections.Look(ref deflectableProjectileHashes, "deflectableProjectileHashes", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                deflectableProjectileHashes = deflectableProjectileHashes ?? new HashSet<int>();

                foreach (int hash in deflectableProjectileHashes)
                {
                    ThingDef projectileDef = DefDatabase<ThingDef>.AllDefsListForReading.FirstOrDefault(d => d.shortHash == hash);
                    if (projectileDef != null)
                    {
                        deflectableProjectileHashes.Add(projectileDef.shortHash);
                    }
                }
            }
        }

        public static void AddDeflectableProjectile(ThingDef def)
        {
            if (!deflectableProjectileHashes.Contains(def.shortHash))
            {
                deflectableProjectileHashes.Add(def.shortHash);
            }
        }

        public static void RemoveDeflectableProjectile(ThingDef def)
        {
            deflectableProjectileHashes.Remove(def.shortHash);
        }
    }

    public class TheForce_Mod : Mod
    {
        private enum Tab
        {
            General,
            Projectiles
        }

        private Tab currentTab = Tab.General;
        Force_ModSettings settings;
        Vector2 scrollPosition = Vector2.zero;

        public TheForce_Mod(ModContentPack content) : base(content)
        {
            settings = GetSettings<Force_ModSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            // Adjusted the y position of the tab buttons to raise them higher
            Rect tabRect = new Rect(inRect.x, inRect.y - 30f, inRect.width, 30);  // Raised by subtracting 10 from y
            Rect generalTabRect = tabRect.LeftHalf().ContractedBy(4f);
            Rect projectilesTabRect = tabRect.RightHalf().ContractedBy(4f);

            if (Widgets.ButtonText(generalTabRect, "General"))
            {
                currentTab = Tab.General;
            }
            if (Widgets.ButtonText(projectilesTabRect, "Projectiles"))
            {
                if (Force_ModSettings.projectileDeflectionSelector)
                {
                    currentTab = Tab.Projectiles;
                }
                else
                {
                    Messages.Message("Force.projectileDeflectionSelectorDisabled".Translate(), MessageTypeDefOf.RejectInput);
                }
            }

            // Space after tabs
            listingStandard.Gap(36f);

            switch (currentTab)
            {
                case Tab.General:
                    DrawGeneralSettings(inRect);
                    break;
                case Tab.Projectiles:
                    if(Force_ModSettings.projectileDeflectionSelector)
                    {
                        DrawProjectileSettings(inRect);
                        if (Force_ModSettings.deflectableProjectileNames == null)
                        {
                            Force_ModSettings.deflectableProjectileNames = new List<string>();
                        }

                        if (Force_ModSettings.deflectableProjectileHashes == null)
                        {
                            Force_ModSettings.deflectableProjectileHashes = new HashSet<int>();
                        }

                        // Sync hashes if not yet synced after load
                        if (Scribe.mode == LoadSaveMode.PostLoadInit)
                        {
                            Force_ModSettings.deflectableProjectileHashes.Clear();
                            foreach (var defName in Force_ModSettings.deflectableProjectileNames)
                            {
                                var projectileDef = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
                                if (projectileDef != null)
                                {
                                    Force_ModSettings.deflectableProjectileHashes.Add(projectileDef.shortHash);
                                }
                            }
                        }
                    }
                    break;
            }

            listingStandard.End();
        }

        private void DrawGeneralSettings(Rect inRect)
        {
            // Begin ScrollView
            Rect scrollRect = new Rect(0, 0, inRect.width - 16f, 1000); // Height is a large enough value to contain the content.
            Widgets.BeginScrollView(inRect, ref scrollPosition, scrollRect);

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(scrollRect);

            listingStandard.CheckboxLabeled("Force_DarksideOnKill".Translate(), ref Force_ModSettings.IncreaseDarksideOnKill, "Force_AlignmentGain".Translate());
            listingStandard.CheckboxLabeled("Force_UsePsycastStat".Translate(), ref Force_ModSettings.usePsycastStat, "Force_PsycastStat".Translate());

            if (!Force_ModSettings.usePsycastStat)
            {
                listingStandard.Label("Force_offSetMultiplier".Translate() + ": " + Force_ModSettings.offSetMultiplier);
                Force_ModSettings.offSetMultiplier = listingStandard.Slider(Force_ModSettings.offSetMultiplier, 1f, 10f);
            }

            listingStandard.Label("Force_Spin_Rate".Translate() + ": " + Force_ModSettings.spinRate);
            Force_ModSettings.spinRate = listingStandard.Slider(Force_ModSettings.spinRate, 1f, 100f);

            listingStandard.Label("Force_DeflectionChance".Translate() + ": " + Force_ModSettings.deflectionMultiplier);
            Force_ModSettings.deflectionMultiplier = listingStandard.Slider(Force_ModSettings.deflectionMultiplier, 0.1f, 20f);

            listingStandard.Label("Force_Entropy_Gain".Translate() + ": " + Force_ModSettings.entropyGain);
            Force_ModSettings.entropyGain = listingStandard.Slider(Force_ModSettings.entropyGain, 1f, 20f);

            listingStandard.Label("Force_ApprenticeCapacity".Translate() + ": " + Force_ModSettings.apprenticeCapacity);
            Force_ModSettings.apprenticeCapacity = (int)listingStandard.Slider(Force_ModSettings.apprenticeCapacity, 1, 10);

            listingStandard.CheckboxLabeled("Force.LightsaberGlow".Translate(), ref Force_ModSettings.LightsaberGlowEnabled, "Force.LightsaberGlowDesc".Translate());

            if (Force_ModSettings.LightsaberGlowEnabled)
            {
                listingStandard.CheckboxLabeled("Force.LightsaberRealGlow".Translate(), ref Force_ModSettings.LightsaberRealGlow, "Force.LightsaberRealGlowDesc".Translate());
                listingStandard.CheckboxLabeled("Force.LightsaberFakeGlow".Translate(), ref Force_ModSettings.LightsaberFakeGlow, "Force.LightsaberFakeGlowDesc".Translate());
                listingStandard.Label("Force_GlowRadius".Translate() + ": " + Force_ModSettings.glowRadius);
                Force_ModSettings.glowRadius = listingStandard.Slider(Force_ModSettings.glowRadius, 1, 10);
            }
            else
            {
                Force_ModSettings.LightsaberRealGlow = false;
                Force_ModSettings.LightsaberFakeGlow = false;
            }

            listingStandard.CheckboxLabeled("Force.DeflectionSpin".Translate(), ref Force_ModSettings.DeflectionSpin, "Force.DeflectionSpinDesc".Translate());
            listingStandard.CheckboxLabeled("Force.CustomStance".Translate(), ref Force_ModSettings.customStance, "Force.CustomStanceDesc".Translate());
            listingStandard.CheckboxLabeled("Force.projectileDeflectionSelector".Translate(), ref Force_ModSettings.projectileDeflectionSelector, "Force.projectileDeflectionSelector".Translate());
            listingStandard.CheckboxLabeled("Force.lightsaberparry".Translate(), ref Force_ModSettings.lightsaberParryEnabled, "Force.lightsaberparryDesc".Translate());
            listingStandard.CheckboxLabeled("Force.lightsaberCustomizationUndrafted".Translate(), ref Force_ModSettings.lightsaberCustomizationUndrafted, "Force.lightsaberCustomizationUndrafted".Translate());
            listingStandard.CheckboxLabeled("Force.apprenticeRankup".Translate(), ref Force_ModSettings.rankUpApprentice, "Force.apprenticeRankupDesc".Translate());
            if (Force_ModSettings.rankUpApprentice)
            {
                listingStandard.CheckboxLabeled("Force.masterRankup".Translate(), ref Force_ModSettings.rankUpMaster, "Force.masterRankupDesc".Translate());
                if (Force_ModSettings.rankUpMaster)
                {
                    listingStandard.Label("Force_requiredApprentices".Translate() + ": " + Force_ModSettings.requiredGraduatedApprentices);
                    Force_ModSettings.requiredGraduatedApprentices = (int)listingStandard.Slider(Force_ModSettings.requiredGraduatedApprentices, 1, 10);
                    listingStandard.Label("Force_requiredPsycast".Translate() + ": " + Force_ModSettings.requiredPsycastLevel);
                    Force_ModSettings.requiredPsycastLevel = (int)listingStandard.Slider(Force_ModSettings.requiredPsycastLevel, 1, PsycastsMod.Settings.maxLevel);
                }   
            }
            listingStandard.Gap();
            if (listingStandard.ButtonText("Force_ResettoDefault".Translate()))
            {
                ResetToDefaultValues();
            }

            listingStandard.End();
            Widgets.EndScrollView();
        }

        private Dictionary<string, bool> modExpandedStates = new Dictionary<string, bool>();
        private void DrawProjectileSettings(Rect inRect)
        {
            // Get all projectiles
            List<ThingDef> projectiles = ProjectileUtility.GetAllProjectiles();

            // Group projectiles by mod name
            var projectileGroups = new Dictionary<string, List<ThingDef>>();
            foreach (var projectileDef in projectiles)
            {
                string modName = projectileDef.modContentPack?.Name ?? "Vanilla"; // Default to "Vanilla" if no mod

                if (!projectileGroups.ContainsKey(modName))
                {
                    projectileGroups[modName] = new List<ThingDef>();
                }
                projectileGroups[modName].Add(projectileDef);
            }

            // Button to enable all projectiles as deflectable
            Rect enableAllButtonRect = new Rect(inRect.x, inRect.y, 120f, 30f);
            Rect disableAllButtonRect = new Rect(inRect.x + 130f, inRect.y, 120f, 30f);

            if (Widgets.ButtonText(enableAllButtonRect, "Enable All"))
            {
                foreach (var group in projectileGroups)
                {
                    foreach (var projectileDef in group.Value)
                    {
                        if (!Force_ModSettings.deflectableProjectileHashes.Contains(projectileDef.shortHash))
                        {
                            Force_ModSettings.AddDeflectableProjectile(projectileDef);
                        }
                    }
                }
            }

            if (Widgets.ButtonText(disableAllButtonRect, "Disable All"))
            {
                foreach (var group in projectileGroups)
                {
                    foreach (var projectileDef in group.Value)
                    {
                        if (Force_ModSettings.deflectableProjectileHashes.Contains(projectileDef.shortHash))
                        {
                            Force_ModSettings.RemoveDeflectableProjectile(projectileDef);
                        }
                    }
                }
            }

            // Calculate the height of the scroll view
            float scrollViewHeight = CalculateScrollViewHeight(projectileGroups, inRect.width);
            Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, scrollViewHeight);

            // Begin scrollable view
            Rect outRect = new Rect(inRect.x, inRect.y + 36f, inRect.width, inRect.height - 36f); // Adjust for tab height
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            float y = 0f;
            foreach (var group in projectileGroups)
            {
                Rect labelRect = new Rect(30f, y, viewRect.width, 40f); // Space for mod header
                bool isExpanded = modExpandedStates.TryGetValue(group.Key, out bool expanded) && expanded;

                if (Widgets.ButtonText(new Rect(labelRect.x, labelRect.y, 20f, 40f), isExpanded ? "▼" : "▶"))
                {
                    modExpandedStates[group.Key] = !isExpanded;
                }

                Text.Font = GameFont.Medium;
                Widgets.Label(new Rect(labelRect.x + 25f, labelRect.y, labelRect.width - 25f, labelRect.height), group.Key);
                Text.Font = GameFont.Small;

                y += 40f; // Increase y for the next group

                if (isExpanded)
                {
                    foreach (var projectileDef in group.Value)
                    {
                        bool isDeflectable = Force_ModSettings.deflectableProjectileHashes.Contains(projectileDef.shortHash);
                        bool checkboxValue = isDeflectable;

                        // Create a rectangle for the projectile checkbox
                        Rect projectileRect = new Rect(10f, y, viewRect.width - 30f, 30f);
                        Widgets.CheckboxLabeled(projectileRect, projectileDef.label ?? projectileDef.defName, ref checkboxValue);

                        // Add or remove the projectile based on checkbox state
                        if (checkboxValue && !isDeflectable)
                        {
                            Force_ModSettings.AddDeflectableProjectile(projectileDef); // Adds the projectile to deflectable list
                        }
                        else if (!checkboxValue && isDeflectable)
                        {
                            Force_ModSettings.RemoveDeflectableProjectile(projectileDef); // Removes it from the deflectable list
                        }

                        y += 40f; // Increase y for the next projectile option
                    }
                }

                y += 10f; // Space between groups
            }

            Widgets.EndScrollView();
        }
        private float CalculateScrollViewHeight(Dictionary<string, List<ThingDef>> projectileGroups, float viewWidth)
        {
            float height = 0f;
            height += 40f * projectileGroups.Count; // Space for mod headers

            foreach (var group in projectileGroups)
            {
                if (modExpandedStates.TryGetValue(group.Key, out bool isExpanded) && isExpanded)
                {
                    height += 600f * group.Value.Count; // Space for projectiles if expanded
                }
            }

            return height;
        }

        // Method to reset settings to default values
        private void ResetToDefaultValues()
        {
            // Set default values for each setting
            Force_ModSettings.usePsycastStat = false;
            Force_ModSettings.offSetMultiplier = 3f;
            Force_ModSettings.spinRate = 9f; 
            Force_ModSettings.entropyGain = 10f;
            Force_ModSettings.deflectionMultiplier = 1f;
            Force_ModSettings.apprenticeCapacity = 1;
            Force_ModSettings.LightsaberGlowEnabled = false;
            Force_ModSettings.glowRadius = 2.5f;
            Force_ModSettings.DeflectionSpin = true;
            Force_ModSettings.customStance = false;
            Force_ModSettings.IncreaseDarksideOnKill = false;
            Force_ModSettings.lightsaberParryEnabled = true;
            Force_ModSettings.lightsaberCustomizationUndrafted = false;
            Force_ModSettings.rankUpMaster = false;
            Force_ModSettings.rankUpApprentice= false;
            Force_ModSettings.requiredGraduatedApprentices = 1;
            Force_ModSettings.requiredPsycastLevel = PsycastsMod.Settings.maxLevel / 2;
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            ApplySettings();
        }

        private void ApplySettings()
        {
            LightSaberProjectile projectile = new LightSaberProjectile(); // Instantiate a LightSaberProjectile object
            projectile.spinRate = Force_ModSettings.spinRate; // Set the spinRate property
        }


        public override string SettingsCategory()
        {
            return "StarWars_TheForce".Translate();
        }
    }
}