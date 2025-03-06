using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheForce_Psycast.Abilities;
using VanillaPsycastsExpanded;
using Verse;
using Verse.Sound;
using VFECore.Abilities;

namespace TheForce_Psycast.Abilities
{
    internal class Ability_ChanneledPsycast : Ability_WriteCombatLog, IChannelledPsycast
    {
        public virtual bool IsActive => pawn.health.hediffSet.HasHediff(GetHediffDef());

        public virtual HediffDef GetHediffDef()
        {
            var hediffExtension = def.GetModExtension<AbilityExtension_Hediff>();
            if (hediffExtension != null)
            {
                return hediffExtension.hediff;
            }
            Log.Error($"Ability {def.defName} is missing the AbilityExtension_Hediff extension.");
            return null;
        }

        public override Gizmo GetGizmo()
        {
            var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(GetHediffDef());
            if (hediff != null)
                return new Command_Action
                {
                    defaultLabel = "Force.CancelAbilityLabel".Translate(def.LabelCap),
                    defaultDesc = "Force.CancelAbilityDesc".Translate(def.LabelCap, hediff.Label),
                    icon = def.icon,
                    action = delegate {pawn.health.RemoveHediff(hediff);},
                    Order = 10f + (def.requiredHediff?.hediffDef?.index ?? 0) + (def.requiredHediff?.minimumLevel ?? 0)
                };
            return base.GetGizmo();
        }
    }
}
