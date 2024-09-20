using RimWorld;
using UnityEngine;
using Verse;

namespace TheForce_Psycast
{
    public class CompGlower_Options : CompGlower
    {
        private ColorInt defaultColorInt = new ColorInt(Color.white);

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            if (parent is ThingWithComps thingWithComps)
            {
                var colorableComp = thingWithComps.GetComp<CompColorable>();
                if (colorableComp != null)
                {
                    ApplyColor(colorableComp.Color);
                }
            }
        }

        private void ApplyColor(Color color)
        {
            defaultColorInt = new ColorInt(color);
            this.GlowColor = defaultColorInt;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref defaultColorInt, "defaultColorInt", new ColorInt(Color.white));
        }
    }

    public class CompProperties_GlowerOptions : CompProperties_Glower
    {
        public CompProperties_GlowerOptions()
        {
            this.compClass = typeof(CompGlower_Options);
        }
    }
}
