using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;

namespace TheForce_Psycast.Abilities.Mechu_deru
{
    
    internal class Ability_MechOverride : Ability_WriteCombatLog
    {
        HediffDef mechControlHediff = ForceDefOf.Force_MechControl;

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);

            if (targets[0].Thing is Pawn mechanoid && mechanoid.RaceProps.IsMechanoid)
            {
                if (mechanoid.health.hediffSet.HasHediff(mechControlHediff))
                    return;

                // Apply the MechOverride hediff
                var hediff = HediffMaker.MakeHediff(mechControlHediff, mechanoid);
                mechanoid.health.AddHediff(hediff);
               
            }
        }

        public override void PostCast(params GlobalTargetInfo[] targets)
        {
            base.PostCast(targets);
            AssignMechToCaster(targets[0].Pawn, CasterPawn);
        }

        public static void AssignMechToCaster(Pawn mech, Pawn caster)
        {
            if (caster != null && MechanitorUtility.IsMechanitor(caster))
            {
                caster.relations.AddDirectRelation(PawnRelationDefOf.Overseer, mech);
                Messages.Message(
                    "MessageMechanitorAssignedToMech".Translate(caster.LabelShort, mech.LabelShort),
                    new LookTargets(new[] { caster, mech }),
                    MessageTypeDefOf.PositiveEvent
                );
            }
            else
            {
                return;
            }
        }

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = false)
        {
            return target.Thing is Pawn pawn && pawn.RaceProps.IsMechanoid && base.ValidateTarget(target, showMessages);
        }
    }
}
