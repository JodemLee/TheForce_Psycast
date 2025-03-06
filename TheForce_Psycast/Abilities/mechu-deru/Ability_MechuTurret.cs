using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static RimWorld.EffecterMaintainer;

namespace TheForce_Psycast.Abilities.Mechu_deru
{
    internal class Ability_MechuTurret : Ability_WriteCombatLog
    {
        // Define the ThingDef for the shield you want to spawn
        private static readonly ThingDef ShieldDef = ThingDef.Named("Force_NanoShieldMechuDeru"); // Replace with your shield ThingDef name

        // Define the energy cost for spawning a shield
        private const float ShieldEnergyCost = 150f; // Adjust this value as needed

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            if (targets == null || targets.Length == 0 || pawn == null || pawn.Map == null)
            {
                return;
            }

            HediffComp_MechuDeru mechuDeruComp = pawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_MechuLinkImplant)?.TryGetComp<HediffComp_MechuDeru>();
            if (mechuDeruComp == null)
            {
                Messages.Message("Pawn does not have the Mechu-Deru ability.", MessageTypeDefOf.RejectInput, false);
                return;
            }

            if (mechuDeruComp.StoredEnergy < ShieldEnergyCost)
            {
                Messages.Message("Not enough stored energy to spawn a shield.", MessageTypeDefOf.RejectInput, false);
                return;
            }

            var targetBuilding = targets[0].Thing as Building;
            if (targetBuilding == null || !targetBuilding.def.building.isPowerConduit)
            {
                Messages.Message("Target must be a Power Conduit.", MessageTypeDefOf.RejectInput, false);
                return;
            }

            CompPower powerComp = targetBuilding.GetComp<CompPower>();
            if (powerComp == null)
            {
                return;
            }

            HashSet<Building> connectedBuildings = PowerNetUtility.GetConnectedBuildings(powerComp);
            foreach (var building in connectedBuildings)
            {
                if (building.def.building?.turretGunDef != null)
                {
                    SpawnShieldAround(building);
                    mechuDeruComp.StoredEnergy -= ShieldEnergyCost;
                }
            }
        }

        public override void Tick()
        {
            base.Tick();

            HediffComp_MechuDeru mechuDeruComp = pawn.health.hediffSet
                .GetFirstHediffOfDef(ForceDefOf.Force_MechuLinkImplant)?
                .TryGetComp<HediffComp_MechuDeru>();

            if (mechuDeruComp == null) { return; }

            foreach (var shield in mechuDeruComp.spawnedShields.ToList())
            {
                mechuDeruComp.StoredEnergy -= ShieldEnergyCost * 0.001f;
            }
        }


        private void SpawnShieldAround(Building turret)
        {
            if (ShieldDef == null)
            {
                Log.Error("ShieldDef is not defined. Please assign a valid ThingDef for the shield.");
                return;
            }

            IntVec3 shieldPosition = turret.Position;

            if (!shieldPosition.InBounds(turret.Map))
            {
                return;
            }

            Thing shield = GenSpawn.Spawn(ShieldDef, shieldPosition, turret.Map);
            MoteMaker.ThrowText(turret.Position.ToVector3(), turret.Map, "Shield Spawned", Color.white, 3f);

            // Register the shield with the Mechu-Deru component
            HediffComp_MechuDeru mechuDeruComp = pawn.health.hediffSet.GetFirstHediffOfDef(ForceDefOf.Force_MechuLinkImplant)?.TryGetComp<HediffComp_MechuDeru>();
            mechuDeruComp?.RegisterShield(shield);
        }

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = false)
        {
            base.ValidateTarget(target, showMessages);
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

