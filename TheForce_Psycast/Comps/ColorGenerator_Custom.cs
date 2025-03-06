using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using Verse;

namespace TheForce_Psycast.Comps
{
    public class ColorGenerator_Temperature : ColorGenerator
    {
        public override Color NewRandomizedColor()
        {
            if (Find.CurrentMap == null)
            {
                return Color.white;
            }
            float temperature = Find.CurrentMap.mapTemperature.OutdoorTemp;
            float minTemp = -20f;
            float maxTemp = 40f;
            float normalizedTemperature = Mathf.InverseLerp(minTemp, maxTemp, temperature);
            if (normalizedTemperature < 0.5f)
            {
                return Color.Lerp(Color.blue, Color.cyan, normalizedTemperature * 2); // Cold range
            }
            else
            {
                return Color.Lerp(Color.yellow, Color.red, (normalizedTemperature - 0.5f) * 2); // Hot range
            }
        }
    }

    public class ColorGenerator_Seasonal : ColorGenerator
    {
        public override Color NewRandomizedColor()
        {
            Season currentSeason = GenLocalDate.Season(Find.CurrentMap.Tile); // Get season for the current map location
            switch (currentSeason)
            {
                case Season.Winter:
                    return new Color(Rand.Range(0.7f, 1f), Rand.Range(0.7f, 1f), 1f); // Cool blues
                case Season.Summer:
                    return new Color(1f, Rand.Range(0.8f, 1f), Rand.Range(0.5f, 0.8f)); // Warm oranges
                case Season.Fall:
                    return new Color(1f, Rand.Range(0.6f, 0.8f), Rand.Range(0.3f, 0.5f)); // Autumnal colors
                case Season.Spring:
                    return new Color(Rand.Range(0.6f, 0.8f), 1f, Rand.Range(0.6f, 0.8f)); // Fresh greens
                default:
                    return Color.white;
            }
        }
    }

    public class ColorGenerator_Biome : ColorGenerator
    {
        private Dictionary<BiomeDef, Color> biomeColors = new Dictionary<BiomeDef, Color>();
        private Color defaultColor = Color.white;

        public override Color NewRandomizedColor()
        {
            if (Find.CurrentMap == null)
            {
                return defaultColor;
            }

            BiomeDef currentBiome = Find.CurrentMap.Biome;
            if (biomeColors.TryGetValue(currentBiome, out Color color))
            {
                return color;
            }

            return defaultColor;
        }

        public void LoadFromXmlCustom(XmlNode xmlRoot)
        {
            foreach (XmlNode childNode in xmlRoot.ChildNodes)
            {
                if (childNode.Name == "defaultColor")
                {
                    defaultColor = ParseHelper.FromString<Color>(childNode.InnerText);
                }
                else if (childNode.Name == "biomeColors")
                {
                    foreach (XmlNode biomeNode in childNode.ChildNodes)
                    {
                        var biomeDef = DefDatabase<BiomeDef>.GetNamed(biomeNode["key"].InnerText, true);
                        var color = ParseHelper.FromString<Color>(biomeNode["value"].InnerText);
                        biomeColors[biomeDef] = color;
                    }
                }
            }
        }
    }

    public class ColorGenerator_Holographic : ColorGenerator
    {
        private float minAlpha = 0.5f;
        private float maxAlpha = 0.8f;
        public List<Color> colors = new List<Color>();

        public override Color NewRandomizedColor()
        {
            Color color = colors.RandomElement();
            float alpha = Rand.Range(minAlpha, maxAlpha);
            return new Color(color.r, color.g, color.b, alpha);
        }
    }

    public class ColorGenerator_TimeOfDay : ColorGenerator
    {
        private Color dayColor = new Color(1f, 1f, 1f);
        private Color nightColor = new Color(0.2f, 0.2f, 0.5f);

        public override Color NewRandomizedColor()
        {
            if (Find.CurrentMap == null)
            {
                return dayColor;
            }
            int currentHour = GenLocalDate.HourInteger(Find.CurrentMap);
            float normalizedHour = Mathf.InverseLerp(0f, 12f, Mathf.Abs(currentHour % 24 - 12));
            Color currentColor = Color.Lerp(dayColor, nightColor, normalizedHour);
            return currentColor;
        }
    }
}

