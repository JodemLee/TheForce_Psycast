using RimWorld;
using System.Collections.Generic;
using TheForce_Psycast.Abilities.NightSisterMagick;
using UnityEngine;
using Verse;

namespace TheForce_Psycast.NightSisterMagick
{
    internal class Force_GeneMagick : Gene_Resource, IGeneResourceDrain
    {
        public bool IchorAllowed = true;

        public Gene_Resource Resource => this;

        public Pawn Pawn => pawn;

        public bool CanOffset
        {
            get
            {
                if (Active)
                {
                    return !pawn.Deathresting;
                }
                return false;
            }
        }

        public string DisplayLabel => Label + " (" + "Gene".Translate() + ")";

        public float ResourceLossPerDay => def.resourceLossPerDay;

        public override float InitialResourceMax => 1f;

        public override float MinLevelForAlert => 0.15f;

        public override float MaxLevelOffset => 0.1f;

        protected override Color BarColor => new ColorInt(3, 138, 3).ToColor;

        protected override Color BarHighlightColor => new ColorInt(42, 145, 42).ToColor;

        public override void PostAdd()
        {
            if (ModLister.CheckBiotech("Ichor"))
            {
                base.PostAdd();
                Reset();
            }
        }

        public override void Notify_IngestedThing(Thing thing, int numTaken)
        {
            if (thing.def.IsMeat)
            {
                IngestibleProperties ingestible = thing.def.ingestible;
                if (ingestible != null && ingestible.sourceDef?.race?.Humanlike == true)
                {
                    OffsetIchor(pawn, 0.0375f * thing.GetStatValue(StatDefOf.Nutrition) * (float)numTaken);
                }
            }
        }

        public static void OffsetIchor(Pawn pawn, float offset, bool applyStatFactor = true)
        {
            if (!ModsConfig.BiotechActive)
            {
                return;
            }

            if (offset > 0f && applyStatFactor)
            {
                offset *= pawn.GetStatValue(StatDefOf.HemogenGainFactor);
            }

            Force_GeneMagick gene_Ichor = pawn.genes?.GetFirstGeneOfType<Force_GeneMagick>();
            if (gene_Ichor != null)
            {
                gene_Ichor.Value += offset;
            }
        }

        public override void Tick()
        {
            base.Tick();
            IchorResourceDrainUtility.TickResourceDrain(this);
        }

        public override void SetTargetValuePct(float val)
        {
            targetValue = Mathf.Clamp(val * Max, 0f, Max - MaxLevelOffset);
        }

        public bool ShouldAbsorbIchorNow()
        {
            return Value < targetValue;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            foreach (Gizmo resourceDrainGizmo in IchorResourceDrainUtility.GetResourceDrainGizmos(this))
            {
                yield return resourceDrainGizmo;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref IchorAllowed, "IchorAllowed", defaultValue: true);
        }
    }
}
