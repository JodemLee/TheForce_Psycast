using RimWorld;
using System.Collections.Generic;
using System.Linq;
using VFECore.Abilities;
using Verse;
using Ability = VFECore.Abilities.Ability;
using RimWorld.Planet;

namespace TheForce_Psycast.Abilities.Darkside
{
    internal class Force_Phantom : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            foreach (var target in targets)
            {
                Pawn copy = Find.PawnDuplicator.Duplicate(CasterPawn);
                GenSpawn.Spawn(copy, target.Cell, CasterPawn.Map);

                // Fix CS1061 by ensuring 'copy' is of type Pawn before accessing workSettings
                if (copy is Pawn pawn)
                {
                    for (int h = 0; h < 24; h++)
                    {
                        pawn.timetable.SetAssignment(h, TimeAssignmentDefOf.Work);
                    }

                    List<WorkTypeDef> workTypeDefs = new List<WorkTypeDef>
                {
                    WorkTypeDefOf.Childcare,
                    WorkTypeDefOf.Handling,
                    WorkTypeDefOf.Doctor,
                    WorkTypeDefOf.Construction,
                    WorkTypeDefOf.Growing,
                    WorkTypeDefOf.Mining,
                    WorkTypeDefOf.Cleaning,
                    WorkTypeDefOf.Crafting,
                    WorkTypeDefOf.DarkStudy,
                    WorkTypeDefOf.Firefighter,
                    WorkTypeDefOf.Handling,
                    WorkTypeDefOf.Hauling,
                    WorkTypeDefOf.Hunting,
                    WorkTypeDefOf.PlantCutting,
                    WorkTypeDefOf.Research,
                    WorkTypeDefOf.Smithing,
                    WorkTypeDefOf.Warden
                };

                    foreach (WorkTypeDef work in workTypeDefs)
                    {
                        pawn.workSettings.SetPriority(work, CasterPawn.workSettings.GetPriority(work));
                    }
                }
            }
        }
    }
}
