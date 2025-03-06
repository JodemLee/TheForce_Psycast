using RimWorld.Planet;
using RimWorld;
using System.Collections.Generic;
using TheForce_Psycast.Abilities.Mechu_deru;
using TheForce_Psycast.Abilities;
using Verse;

namespace TheForce_Psycast.Abilities.Mechu_deru
{
    internal class Ability_Mechu_Reconste : Ability_WriteCombatLog
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            if (targets == null || targets.Length == 0 || pawn == null || pawn.Map == null)
            {
                return;
            }
            var targetBuilding = targets[0].Thing as Building;
            if (targetBuilding == null || !targetBuilding.def.building.isPowerConduit)
            {
                Messages.Message("Target must be a Power Conduit.", MessageTypeDefOf.RejectInput, false);
                return;
            }
            CompPower powerComp = targetBuilding.GetComp<CompPower>();
            HashSet<Building> connectedBuildings = PowerNetUtility.GetConnectedBuildings(powerComp);
            foreach (var building in connectedBuildings)
            {
                if (building.HitPoints < building.MaxHitPoints)
                {
                    building.HitPoints = building.MaxHitPoints; // Fully repair the building
                    MoteMaker.MakeStaticMote(building.Position.ToVector3().ToIntVec3(), CasterPawn.Map, ThingDefOf.Mote_MechCharging, 3f); // Create a visual effect
                }
            }
        }

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = false)
        {
            base.ValidateTarget(target, showMessages);

            // Ensure the target is a valid power conduit
            var targetBuilding = target.Thing as Building;
            if (targetBuilding == null || !targetBuilding.def.building.isPowerConduit)
            {
                Messages.Message("Target must be a Power Conduit.", MessageTypeDefOf.RejectInput, false);
                return false;
            }

            return true;
        }
    }
}
