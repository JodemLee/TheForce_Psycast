using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TheForce_Psycast.Abilities.Mechu_deru
{
    internal class Ability_Mechu_Risen_th : Ability_WriteCombatLog
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            
            if (targets[0].Thing is Corpse corpse)
            {
                var mech = corpse.InnerPawn;
                ResurrectionUtility.TryResurrect(mech);
                mech.health.RemoveAllHediffs();
                AssignMechToCaster(mech, CasterPawn);
            }
        }

        public static void AssignMechToCaster(Pawn mech, Pawn caster)
        {
            if (caster != null && MechanitorUtility.IsMechanitor(caster))
            {
                mech.SetFaction(Faction.OfPlayer);
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
            return target.Thing is Corpse pawn && pawn.InnerPawn.RaceProps.IsMechanoid && base.ValidateTarget(target, showMessages);
        }
    }
}