using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VanillaPsycastsExpanded;
using Verse;
using Verse.Sound;
using VFECore.Abilities;

namespace TheForce_Psycast.Abilities.Darkside
{
    internal class Force_Rage : Ability, IChannelledPsycast
    {
        public bool IsActive => pawn.health.hediffSet.HasHediff(ForceDefOf.Force_Rage);

        public override Gizmo GetGizmo()
        {
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_Rage);
            if (hediff != null)
                return new Command_Action
                {
                    defaultLabel = "VPE.CancelForceDefense".Translate(),
                    defaultDesc = "VPE.CancelSkipbarrierDesc".Translate(),
                    icon = def.icon,
                    action = delegate { pawn.health.RemoveHediff(hediff); },
                    Order = 10f + (def.requiredHediff?.hediffDef?.index ?? 0) + (def.requiredHediff?.minimumLevel ?? 0)
                };
            return base.GetGizmo();
        }
    }

    public class Hediff_ForceRage : HediffWithComps
    {
        private Sustainer sustainer;

        public override void PostTick()
        {
            base.PostTick();
            this.AddEntropy();
        }

        private void AddEntropy()
        {
            if (Find.TickManager.TicksGame % 10 == 0) this.pawn.psychicEntropy.TryAddEntropy(1f, overLimit: true);
            if (this.pawn.psychicEntropy.EntropyValue >= this.pawn.psychicEntropy.MaxEntropy) this.pawn.health.RemoveHediff(this);
        }
    }
}