using UnityEngine;
using Verse;

namespace TheForce_Psycast
{
    public class CompColorCrystal : ThingComp
    {
        public ColorInt CrystalColor;

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);

            // Initialize with the color from CompColorable if available
            var colorableComp = parent.GetComp<CompColorable>();
            if (colorableComp != null)
            {
                CrystalColor = new ColorInt(colorableComp.Color);
            }
            else
            {
                CrystalColor = new ColorInt(Color.white); // Default to white if no colorable component is found
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref CrystalColor, "CrystalColor", new ColorInt(Color.white));
        }
    }

    public class CompProperties_ColorCrystal : CompProperties
    {
        public CompProperties_ColorCrystal()
        {
            this.compClass = typeof(CompColorCrystal);
        }
    }
}