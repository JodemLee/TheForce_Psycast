using RimWorld;
using UnityEngine;
using Verse;

namespace TheForce_Psycast
{
    public class Force_ModSettings : ModSettings
    {
        public static float spinRate = 1f;
        public static float entropyGain = 1f;
        public static bool usePsycastStat = false;
        public static float offSetMultiplier = 1;
        public static int  apprenticeCapacity = 1;
        public static float deflectionMultiplier = 1f;
        public static float insanityChance = 1f;
        public static bool IncreaseDarksideOnKill = false;
        public static bool LightsaberGlowEnabled = false;
        public static bool LightsaberRealGlow = false;
        public static bool LightsaberFakeGlow = false;
        public static bool DeflectionSpin = false;
        public static float glowRadius = 4.5f;
        public static bool customStance = false;

        public override void ExposeData()
        {
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
            Scribe_Values.Look(ref glowRadius, "glowRadius", 4.5f);
            Scribe_Values.Look(ref DeflectionSpin, "deflectionspin", false);
            Scribe_Values.Look(ref customStance, "customStance", false);

            base.ExposeData();
        }
    }

    public class TheForce_Mod : Mod
    {
        Force_ModSettings settings;

        public TheForce_Mod(ModContentPack content) : base(content)
        {
            settings = GetSettings<Force_ModSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            listingStandard.CheckboxLabeled("Force_DarksideOnKill".Translate(), ref Force_ModSettings.IncreaseDarksideOnKill, "Force_AlignmentGain".Translate());

            listingStandard.CheckboxLabeled("Force_UsePsycastStat".Translate(), ref Force_ModSettings.usePsycastStat, "Force_PsycastStat".Translate());
            if (!Force_ModSettings.usePsycastStat)
            {
                listingStandard.Label("Force_offSetMultiplier".Translate() + ":" + " " + Force_ModSettings.offSetMultiplier);
                Force_ModSettings.offSetMultiplier = listingStandard.Slider(Force_ModSettings.offSetMultiplier, 1f, 10);
            }

            listingStandard.Label("Force_Spin_Rate".Translate() + ":" + " " + Force_ModSettings.spinRate);
            Force_ModSettings.spinRate = listingStandard.Slider(Force_ModSettings.spinRate, 1f, 100);

            listingStandard.Label("Force_DeflectionChance".Translate() + ":" + " " + Force_ModSettings.deflectionMultiplier);
            Force_ModSettings.deflectionMultiplier = listingStandard.Slider(Force_ModSettings.deflectionMultiplier, 0.1f, 20);

            listingStandard.Label("Force_Entropy_Gain".Translate() + ":" + " " + Force_ModSettings.entropyGain);
            Force_ModSettings.entropyGain = listingStandard.Slider(Force_ModSettings.entropyGain, 1f, 20);

            listingStandard.Label("Force_ApprenticeCapacity".Translate() + ":" + " " + Force_ModSettings.apprenticeCapacity);
            Force_ModSettings.apprenticeCapacity = (int)listingStandard.Slider(Force_ModSettings.apprenticeCapacity, 1, 10);

            listingStandard.CheckboxLabeled("Force.LightsaberGlow".Translate(), ref Force_ModSettings.LightsaberGlowEnabled, "Force.LightsaberGlowDesc".Translate());

            if (Force_ModSettings.LightsaberGlowEnabled)
            {
                // Show and allow the real and fake glow checkboxes only when LightsaberGlowEnabled is true
                listingStandard.CheckboxLabeled("Force.LightsaberRealGlow".Translate(), ref Force_ModSettings.LightsaberRealGlow, "Force.LightsaberRealGlowDesc".Translate());
                listingStandard.CheckboxLabeled("Force.LightsaberFakeGlow".Translate(), ref Force_ModSettings.LightsaberFakeGlow, "Force.LightsaberFakeGlowDesc".Translate());
                listingStandard.Label("Force_GlowRadius".Translate() + ": " + Force_ModSettings.glowRadius);
                Force_ModSettings.glowRadius = listingStandard.Slider(Force_ModSettings.glowRadius, 1, 10);
            }
            else
            {
                // Automatically disable the real and fake glow settings when LightsaberGlowEnabled is false
                Force_ModSettings.LightsaberRealGlow = false;
                Force_ModSettings.LightsaberFakeGlow = false;
            }
            listingStandard.CheckboxLabeled("Force.DeflectionSpin".Translate(), ref Force_ModSettings.DeflectionSpin, "Force.DeflectionSpinDesc".Translate());

            listingStandard.CheckboxLabeled("Force.CustomStance".Translate(), ref Force_ModSettings.customStance, "Force.CustomStanceDesc".Translate());

            // Add a button to reset settings to default values
            if (listingStandard.ButtonText("Force_ResettoDefault".Translate()))
            {
                ResetToDefaultValues();
            }

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
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
            Force_ModSettings.glowRadius = 4.5f;
            Force_ModSettings.DeflectionSpin = true;
            Force_ModSettings.customStance = false;
            Force_ModSettings.IncreaseDarksideOnKill = false;
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