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
    internal class Ability_Mechu_FluxInfusis : Ability_WriteCombatLog
    {
        private const float PsyfocusToPowerRatio = 0.01f;
        private const float PowerToPsyfocusRatio = 10f;
        private const float MaxPowerRestore = 2500f;

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);

            if (targets == null || targets.Length == 0 || pawn == null || pawn.Map == null)
            {
                return;
            }

            var targetBuilding = targets[0].Thing as Building;
            if (targetBuilding == null || !ValidateTarget(new LocalTargetInfo(targetBuilding), true))
            {
                return;
            }

            List<FloatMenuOption> primaryOptions = new List<FloatMenuOption>
        {
            new FloatMenuOption("Infuse power", () => ShowInfuseOptions(targetBuilding)),
            new FloatMenuOption("Absorb power", () => ShowAbsorbOptions(targetBuilding))
        };

            Find.WindowStack.Add(new FloatMenu(primaryOptions));
        }

        private void ShowInfuseOptions(Building targetBuilding)
        {
            List<FloatMenuOption> infuseOptions = new List<FloatMenuOption>();
            foreach (float percentage in new float[] { 0.25f, 0.5f, 0.75f, 1f })
            {
                infuseOptions.Add(new FloatMenuOption($"Infuse {percentage * 100:F0}% psyfocus", () =>
                {
                    InfusePower(targetBuilding, percentage, false);
                }));
            }

            infuseOptions.Add(new FloatMenuOption("Overload power grid (guaranteed explosion)", () =>
            {
                InfusePower(targetBuilding, 1f, true);
            }));

            Find.WindowStack.Add(new FloatMenu(infuseOptions));
        }

        private void ShowAbsorbOptions(Building targetBuilding)
        {
            float missingPsyfocus = 1f - CasterPawn.psychicEntropy.CurrentPsyfocus;
            List<FloatMenuOption> absorbOptions = new List<FloatMenuOption>();
            foreach (float percentage in new float[] { 0.25f, 0.5f, 0.75f, missingPsyfocus })
            {
                absorbOptions.Add(new FloatMenuOption($"Absorb {percentage * 100:F0}% psyfocus", () =>
                {
                    AbsorbPower(targetBuilding, percentage);
                }));
            }

            Find.WindowStack.Add(new FloatMenu(absorbOptions));
        }

        private void InfusePower(Building targetBuilding, float percentage, bool causeExplosion)
        {
            CompPower powerComp = targetBuilding.GetComp<CompPower>();
            if (powerComp == null || powerComp.PowerNet == null)
            {
                Messages.Message("Target is not connected to a valid power network.", MessageTypeDefOf.RejectInput, false);
                return;
            }

            float availablePsyfocus = CasterPawn.psychicEntropy.CurrentPsyfocus;
            float psyfocusUsed = Math.Min(availablePsyfocus, MaxPowerRestore * PsyfocusToPowerRatio * percentage);
            float powerRestored = psyfocusUsed / PsyfocusToPowerRatio;

            if (psyfocusUsed > 0)
            {
                CasterPawn.psychicEntropy.OffsetPsyfocusDirectly(-psyfocusUsed);

                float remainingPowerToAdd = powerRestored;
                foreach (var battery in powerComp.PowerNet.batteryComps)
                {
                    if (remainingPowerToAdd <= 0)
                        break;

                    float freeCapacity = battery.Props.storedEnergyMax - battery.StoredEnergy;
                    float powerToAdd = Math.Min(remainingPowerToAdd, freeCapacity);
                    battery.AddEnergy(powerToAdd);
                    remainingPowerToAdd -= powerToAdd;
                }

                if (causeExplosion)
                {
                    ShortCircuitUtility.DoShortCircuit(targetBuilding);
                    Messages.Message("The power grid was overloaded and caused a short circuit!", MessageTypeDefOf.NegativeEvent, false);
                    GenExplosion.DoExplosion(targetBuilding.Position, pawn.Map, 3.9f, DamageDefOf.Flame, pawn, -1, -1f, null, null, null, null, ThingDefOf.Spark);
                }

                Messages.Message($"Caster sacrificed {psyfocusUsed:P1} psyfocus to restore {powerRestored:N0}W of power.", MessageTypeDefOf.PositiveEvent, false);
            }
            else
            {
                Messages.Message("The caster doesn't have enough psyfocus to perform this action.", MessageTypeDefOf.RejectInput, false);
            }
        }

        private void AbsorbPower(Building targetBuilding, float psyfocusPercentage)
        {
            CompPower powerComp = targetBuilding.GetComp<CompPower>();
            if (powerComp == null || powerComp.PowerNet == null)
            {
                Messages.Message("Target is not connected to a valid power network.", MessageTypeDefOf.RejectInput, false);
                return;
            }

            float totalPowerAvailable = powerComp.PowerNet.CurrentEnergyGainRate() * 60000f; // Convert from W/tick to W/day.
            float powerDrained = Math.Min(totalPowerAvailable, psyfocusPercentage * PowerToPsyfocusRatio * 1000f);
            float psyfocusRestored = powerDrained / (PowerToPsyfocusRatio * 1000f);

            if (psyfocusRestored > 0)
            {
                powerComp.PowerNet.batteryComps.ForEach(battery =>
                {
                    if (powerDrained <= 0)
                        return;

                    float batteryEnergy = battery.StoredEnergy;
                    float energyDrainedFromBattery = Math.Min(powerDrained, batteryEnergy);
                    battery.DrawPower(energyDrainedFromBattery);
                    powerDrained -= energyDrainedFromBattery;
                });

                CasterPawn.psychicEntropy.OffsetPsyfocusDirectly(psyfocusRestored);
                Messages.Message($"Caster has restored {psyfocusRestored:P1} psyfocus by draining power.", MessageTypeDefOf.PositiveEvent, false);
            }
            else
            {
                Messages.Message("There is not enough power to drain.", MessageTypeDefOf.RejectInput, false);
            }
        }

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = false)
        {
            base.ValidateTarget(target, showMessages);
            Thing targetThing = target.Thing;
            var targetBuilding = targetThing as Building;
            if (targetBuilding == null || !targetBuilding.def.building.isPowerConduit)
            {
                if (showMessages)
                {
                    Messages.Message("Target must be a Power Conduit.", MessageTypeDefOf.RejectInput, false);
                }
                return false;
            }

            CompPower powerComp = targetBuilding.GetComp<CompPower>();
            if (powerComp == null || powerComp.PowerNet == null)
            {
                if (showMessages)
                {
                    Messages.Message("Target is not connected to a valid power network.", MessageTypeDefOf.RejectInput, false);
                }
                return false;
            }

            return true;
        }
    }
}