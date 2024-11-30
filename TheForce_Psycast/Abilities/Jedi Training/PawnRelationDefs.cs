using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TheForce_Psycast.Abilities.Jedi_Training
{
    internal class PawnRelationDefs
    {
    }
    public class PawnRelationWorker_Apprentice : PawnRelationWorker
    {
        private HediffDef masterHediffDef = ForceDefOf.Force_Master;

        public override bool InRelation(Pawn me, Pawn other)
        {
            // Check if 'me' is the master and 'other' is an apprentice
            var masterHediff = me.health.hediffSet.GetFirstHediffOfDef(masterHediffDef) as Hediff_Master;
            return masterHediff != null && masterHediff.apprentices.Contains(other);
        }

        public override void CreateRelation(Pawn generated, Pawn other, ref PawnGenerationRequest request)
        {
            // Make sure the master has the Hediff_Master hediff
            var masterHediff = generated.health.hediffSet.GetFirstHediffOfDef(masterHediffDef) as Hediff_Master;
            if (masterHediff != null && masterHediff.apprentices.Count < masterHediff.apprenticeCapacity)
            {
                // Add apprentice to master's list if not already present
                if (!masterHediff.apprentices.Contains(other))
                {
                    masterHediff.apprentices.Add(other);

                    // Ensure apprentice has Hediff_Apprentice pointing to the master
                    var apprenticeHediff = HediffMaker.MakeHediff(ForceDefOf.Force_Apprentice, other) as Hediff_Apprentice;
                    if (apprenticeHediff != null)
                    {
                        apprenticeHediff.master = generated;
                        other.health.AddHediff(apprenticeHediff);
                    }

                    // Add reciprocal relations
                    if (!generated.relations.DirectRelationExists(ForceDefOf.Force_MasterRelation, other))
                    {
                        generated.relations.AddDirectRelation(ForceDefOf.Force_MasterRelation, other);
                    }
                    if (!other.relations.DirectRelationExists(ForceDefOf.Force_ApprenticeRelation, generated))
                    {
                        other.relations.AddDirectRelation(ForceDefOf.Force_ApprenticeRelation, generated);
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("Cannot add apprentice - capacity reached or master hediff missing.");
            }

            base.CreateRelation(generated, other, ref request);
        }
    }

    public class PawnRelationWorker_Master : PawnRelationWorker
    {
        private HediffDef apprenticeHediffDef = ForceDefOf.Force_Apprentice;

        public override bool InRelation(Pawn me, Pawn other)
        {
            // Check if 'me' is the apprentice and 'other' is the master
            var apprenticeHediff = me.health.hediffSet.GetFirstHediffOfDef(apprenticeHediffDef) as Hediff_Apprentice;
            return apprenticeHediff != null && apprenticeHediff.master == other;
        }

        public override void CreateRelation(Pawn generated, Pawn other, ref PawnGenerationRequest request)
        {
            // Make sure the apprentice has the Hediff_Apprentice hediff
            var apprenticeHediff = generated.health.hediffSet.GetFirstHediffOfDef(apprenticeHediffDef) as Hediff_Apprentice;
            if (apprenticeHediff == null)
            {
                apprenticeHediff = HediffMaker.MakeHediff(ForceDefOf.Force_Apprentice, generated) as Hediff_Apprentice;
                apprenticeHediff.master = other;
                generated.health.AddHediff(apprenticeHediff);

                // Ensure the master has Hediff_Master tracking the apprentice
                var masterHediff = other.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Master) as Hediff_Master;
                masterHediff?.apprentices.Add(generated);

                // Add reciprocal relations
                if (!generated.relations.DirectRelationExists(ForceDefOf.Force_ApprenticeRelation, other))
                {
                    generated.relations.AddDirectRelation(ForceDefOf.Force_ApprenticeRelation, other);
                }
                if (!other.relations.DirectRelationExists(ForceDefOf.Force_MasterRelation, generated))
                {
                    other.relations.AddDirectRelation(ForceDefOf.Force_MasterRelation, generated);
                }
            }
            else
            {
                throw new InvalidOperationException("Apprentice already has a master or apprentice hediff missing.");
            }
        }
    }
}
   