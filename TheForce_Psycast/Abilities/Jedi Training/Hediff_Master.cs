using System.Collections.Generic;
using Verse;


namespace TheForce_Psycast.Abilities.Jedi_Training
{
    internal class Hediff_Master : HediffWithComps
    {
        public HashSet<Pawn> apprentices = new HashSet<Pawn>();
        public int apprenticeCapacity = Force_ModSettings.apprenticeCapacity;
        private Gizmo_Apprentice bandwidthGizmo;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref apprentices, "apprentices", LookMode.Reference);
            Scribe_Values.Look(ref apprenticeCapacity, "apprenticeCapacity", 1);
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            {
                Clearapprentices();
            }
        }

        private void Clearapprentices()
        {
            foreach (var apprentice in apprentices)
            {
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
                yield return bandwidthGizmo ?? (bandwidthGizmo = new Gizmo_Apprentice(this));
            }
        }
    }
}