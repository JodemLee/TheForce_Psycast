using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TheForce_Psycast.Abilities.Jedi_Training
{
    internal class Hediff_Master : HediffWithComps
    {
        public HashSet<Pawn> apprentices = new HashSet<Pawn>();
        public int apprenticeCapacity = Force_ModSettings.apprenticeCapacity;
        public int graduatedApprenticesCount = 0;
        private Gizmo_Apprentice apprenticeGizmo;
        private bool hasBackstoryChanged = false;

        public override string Label
        {
            get
            {
                // Start with the base label
                string label = base.Label + " of : ";

                // Check if there are apprentices
                if (apprentices != null && apprentices.Count > 0)
                {
                    // Concatenate each apprentice's short label, separated by commas
                    label += string.Join(", ", apprentices.Select(apprentice => apprentice.LabelShort));
                }
                else
                {
                    // Fallback if there are no apprentices
                    label += "No apprentices";
                }

                return label;
            }
        }

        public float GetDarksideAlignment()
        {
            return this.pawn.GetStatValueForPawn(ForceDefOf.Force_Darkside_Attunement, this.pawn, true);
        }

        public float GetLightsideAlignment()
        {
            return this.pawn.GetStatValueForPawn(ForceDefOf.Force_Lightside_Attunement, this.pawn, true);
        }

        private Gizmo_Apprentice ApprenticeGizmo
        {
            get { return apprenticeGizmo ??= new Gizmo_Apprentice(this); }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref apprentices, "apprentices", LookMode.Reference);
            Scribe_Values.Look(ref apprenticeCapacity, "apprenticeCapacity", Force_ModSettings.apprenticeCapacity);
            Scribe_Values.Look(ref hasBackstoryChanged, "hasBackstoryChanged", false);
        }

        public void CheckAndPromoteMasterBackstory()
        {
            if (graduatedApprenticesCount >= Force_ModSettings.requiredGraduatedApprentices &&
                pawn.GetPsylinkLevel() >= Force_ModSettings.requiredPsycastLevel && Force_ModSettings.rankUpMaster)
            {
                if (!hasBackstoryChanged)
                {
                    if (GetDarksideAlignment() > GetLightsideAlignment())
                    {
                        pawn.story.Adulthood = ForceDefOf.Force_SithMaster;
                    }
                    else
                    {
                        pawn.story.Adulthood = ForceDefOf.Force_JediMaster;
                    }

                    hasBackstoryChanged = true;
                    Messages.Message("Force_MasterPromotion".Translate(pawn.Name.ToStringShort, pawn.story.Adulthood.title), MessageTypeDefOf.PositiveEvent);
                }
            }
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            ClearApprentices();
        }

        public void ClearApprentices()
        {
            if (apprentices == null) return;

            foreach (var apprentice in apprentices.ToList())
            {
                if (apprentice?.health?.hediffSet == null) continue;

                var hediff = apprentice.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Apprentice);
                if (hediff != null)
                {
                    apprentice.health.RemoveHediff(hediff);
                }
            }
            apprentices.Clear();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (this.pawn.IsColonistPlayerControlled)
            {
                apprenticeGizmo ??= new Gizmo_Apprentice(this);
                yield return apprenticeGizmo;
            }
        }

        public void ChangeApprenticeCapacitySetting(int newCapacity)
        {
            Force_ModSettings.apprenticeCapacity = newCapacity; // Update the setting
            foreach (var hediff in Find.CurrentMap.mapPawns.AllPawns.Where(p => p.health.hediffSet.HasHediff(ForceDefOf.Force_Master)))
            {
                var masterHediff = hediff.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Master) as Hediff_Master;
                masterHediff?.UpdateApprenticeCapacity(newCapacity); // Update each relevant Hediff_Master
            }
        }

        // Method to call when apprentice capacity changes
        public void UpdateApprenticeCapacity(int newCapacity)
        {
            apprenticeCapacity = newCapacity; // Update the capacity
            apprenticeGizmo = null; // Invalidate the gizmo to force a refresh next time
        }
    }

    public static class HediffUtility
    {
        public static void ReinitializeHediff(Pawn pawn, HediffDef hediffDef)
        {
            // Remove the existing hediff
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
            if (hediff != null)
            {
                pawn.health.RemoveHediff(hediff);
            }

            // Add a new instance of the hediff
            pawn.health.AddHediff(hediffDef);
        }
    }

}
