using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TheForce_Psycast.Hediffs
{
    internal class Hediff_ForceForms : HediffWithComps
    {
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            // Create and yield the custom gizmo
            if (pawn.equipment.Primary == null)
            {
                yield return new Gizmo_LightsaberStance(pawn, this);
            }
            
        }

    }
}
