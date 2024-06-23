using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
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
        public static float dropChance = 1f;
        public static float deflectionMultiplier = 1f;
        public static float insanityChance = 1f;
        public static bool IncreaseDarksideOnKill = false;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref spinRate, "spinRate", 9f, true);
            Scribe_Values.Look(ref entropyGain, "entropyGain", 20f, true);
            Scribe_Values.Look(ref offSetMultiplier, "offSetMultiplier", 3f, true);
            Scribe_Values.Look(ref deflectionMultiplier, "deflectionMultiplier", 1f, true);
            Scribe_Values.Look(ref dropChance, "dropChance", 0.01f, true);
            Scribe_Values.Look(ref usePsycastStat, "usePsycastStat");
            Scribe_Values.Look(ref apprenticeCapacity, "apprenticeCapacity", 1, true);
            Scribe_Values.Look(ref insanityChance, "insanityChance", 0.25f, true);
            Scribe_Values.Look(ref IncreaseDarksideOnKill, "increasedarksideonkill");

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

            listingStandard.CheckboxLabeled("Force_DarksideOnKill".Translate(), ref Force_ModSettings.IncreaseDarksideOnKill, "Increase Darkside on Kill".Translate());

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

            listingStandard.Label("Force_DropChance".Translate() + ":" + " " + Force_ModSettings.dropChance);
            Force_ModSettings.dropChance = listingStandard.Slider(Force_ModSettings.dropChance, 0.01f, 1);

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
            Force_ModSettings.spinRate = 9f; // You can set your default value here
            Force_ModSettings.entropyGain = 10f;
            Force_ModSettings.dropChance = 0.01f;// You can set your default value here
            Force_ModSettings.deflectionMultiplier = 1f;
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