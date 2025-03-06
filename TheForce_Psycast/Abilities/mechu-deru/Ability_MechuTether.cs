using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using VanillaPsycastsExpanded.Technomancer;

namespace TheForce_Psycast.Abilities.Mechu_deru
{
    internal class Ability_MechuTether : Ability_WriteCombatLog
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);

            foreach (var target in targets)
            {
                if (target.Thing == null) continue;

                var hediffComp = CasterPawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_MechuLinkImplant)?.TryGetComp<HediffComp_MechuDeru>();
                if (hediffComp == null) continue;
                if (hediffComp.linkedTarget == target.Thing)
                {
                    hediffComp.Unlink();
                    Messages.Message("MechuPower.Unlinked".Translate(target.Thing.LabelShort), MessageTypeDefOf.NeutralEvent, false);
                }
                else
                {
                    hediffComp.LinkTo(target.Thing);
                    Messages.Message("MechuPower.Linked".Translate(target.Thing.LabelShort), MessageTypeDefOf.PositiveEvent, false);
                }
            }
        }


        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            if (!base.ValidateTarget(target, showMessages)) return false;
            if (target.Thing?.TryGetComp<CompPowerTrader>() is { PowerOutput: < 0f }) return true;
            if (ModsConfig.BiotechActive && target.Thing is Pawn { RaceProps.IsMechanoid: true, needs.energy: { } } p && p.IsMechAlly(pawn)) return true;
            if (target.Thing is Building_Battery || target.Thing?.TryGetComp<CompPowerBattery>() != null) return true;
            if (showMessages) Messages.Message("VPE.MustConsumePower".Translate(), MessageTypeDefOf.RejectInput, false);
            return false;
        }
    }
}