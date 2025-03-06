using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;


namespace TheForce_Psycast.Abilities.Mechu_deru
{
    public class HediffComp_MechuDeru : HediffComp
    {
        // Fields
        public List<Thing> spawnedShields = new List<Thing>(); // Track spawned shields
        private List<HediffDef> studiedImplants = new List<HediffDef>();
        public Thing linkedTarget;
        private HashSet<Building> connectedBuildings = new HashSet<Building>();
        private CompPowerTrader compPower;
        private CompPowerBattery compPowerBattery;
        private Building_MechCharger fakeCharger;
        private Need_MechEnergy needPower;
        private float storedEnergy;
        private float maxStoredEnergy = 1000f;
        private float energyTransferRate = 10f; // Configurable energy transfer rate

        // Properties
        public float StoredEnergy
        {
            get => storedEnergy;
            set
            {
                storedEnergy = Mathf.Clamp(value, 0, maxStoredEnergy);
                if (storedEnergy <= 0)
                {
                    DestroySpawnedShields();
                }
            }
        }

        public float MaxStoredEnergy => maxStoredEnergy;

        public float EnergyTransferRate
        {
            get => energyTransferRate;
            set => energyTransferRate = Mathf.Max(value, 0); // Ensure non-negative
        }

        // Methods
        public void LinkTo(Thing target)
        {
            if (target == null)
            {
                Log.Error("Attempted to link to a null target.");
                return;
            }

            Unlink(); // Ensure clean state before linking
            linkedTarget = target;
            compPower = target.TryGetComp<CompPowerTrader>();
            compPowerBattery = target.TryGetComp<CompPowerBattery>();
            needPower = (target as Pawn)?.needs?.energy;

            if (needPower != null && needPower.currentCharger == null)
            {
                needPower.currentCharger = fakeCharger ??= new Building_MechCharger();
            }
        }

        public void Unlink()
        {
            if (linkedTarget == null) return;

            compPower?.SetUpPowerVars();

            if (needPower != null && needPower.currentCharger == fakeCharger)
            {
                needPower.currentCharger = null;
            }

            linkedTarget = null;
            fakeCharger = null;
            compPower = null;
            compPowerBattery = null;
            needPower = null;
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (needPower != null && needPower.currentCharger == null)
            {
                needPower.currentCharger = fakeCharger ??= new Building_MechCharger();
            }

            maxStoredEnergy = 1000f * (Pawn.mechanitor.TotalBandwidth - Pawn.mechanitor.UsedBandwidth);

            bool batteriesCharging = AreBatteriesCharging();

            float skillFactor = 1.0f; // Base 100%
            if (Pawn.skills != null)
            {
                float intellectualBonus = Pawn.skills.GetSkill(SkillDefOf.Intellectual).Level * 0.01f; // +1% per level
                float craftingBonus = Pawn.skills.GetSkill(SkillDefOf.Crafting).Level * 0.01f; // +1% per level

                skillFactor += intellectualBonus + craftingBonus; // Combine both bonuses
            }

            float energyToGenerate = (maxStoredEnergy / 60000f) * skillFactor; // Passive generation over a day
            StoredEnergy += energyToGenerate;

            if (batteriesCharging)
            {
                StoredEnergy += energyToGenerate * 0.01f;
            }
            else
            {
                StoredEnergy += energyToGenerate;
            }

            if (linkedTarget != null)
            {
                bool isEnergyAvailable = StoredEnergy > 0;

                if (compPower != null)
                {
                    if (isEnergyAvailable)
                    {
                        float powerDraw = compPower.Props.PowerConsumption / 100f;
                        float energyUsed = Mathf.Min(StoredEnergy, powerDraw);
                        StoredEnergy -= energyUsed / 2f;
                    }

                    compPower.PowerOn = StoredEnergy > compPower.Props.PowerConsumption || compPower.PowerNet?.CanPowerNow(compPower) == true;
                }

                if (compPowerBattery != null && isEnergyAvailable)
                {
                    ChargeBattery(compPowerBattery);
                    var powerNet = compPowerBattery.PowerNet;
                    if (powerNet != null)
                    {
                        foreach (var battery in powerNet.batteryComps)
                        {
                            if (battery != compPowerBattery && battery.StoredEnergy < battery.Props.storedEnergyMax)
                            {
                                ChargeBattery(battery);
                            }
                        }
                    }
                }

                Need_MechEnergy mechEnergyNeed = Pawn.needs?.TryGetNeed<Need_MechEnergy>();
                if (mechEnergyNeed != null)
                {
                    if (mechEnergyNeed.CurLevel < mechEnergyNeed.MaxLevel && StoredEnergy > 0)
                    {
                        float energyToTransfer = Mathf.Min(EnergyTransferRate, StoredEnergy);
                        float energyConversionRate = 1.0f;
                        float needEnergyGain = energyToTransfer * energyConversionRate;
                        mechEnergyNeed.CurLevel += needEnergyGain;
                        StoredEnergy -= energyToTransfer;
                    }
                }
            }
        }

        private void DestroySpawnedShields()
        {
            for (int i = spawnedShields.Count - 1; i >= 0; i--)
            {
                Thing shield = spawnedShields[i];
                if (shield != null && !shield.Destroyed)
                {
                    shield.Destroy();
                }
                spawnedShields.RemoveAt(i);
            }
        }

        public void RegisterShield(Thing shield)
        {
            if (shield != null && !spawnedShields.Contains(shield))
            {
                spawnedShields.Add(shield);
            }
        }

        public void UnregisterShield(Thing shield)
        {
            if (shield != null && spawnedShields.Contains(shield))
            {
                spawnedShields.Remove(shield);
            }
        }

        private bool AreBatteriesCharging()
        {
            if (linkedTarget == null || compPowerBattery == null) return false;

            var powerNet = compPowerBattery.PowerNet;
            if (powerNet == null) return false;

            foreach (var battery in powerNet.batteryComps)
            {
                if (battery.StoredEnergy < battery.Props.storedEnergyMax)
                {
                    return true;
                }
            }

            return false;
        }

        private void ChargeBattery(CompPowerBattery battery)
        {
            if (StoredEnergy <= 0.999f || battery.StoredEnergy >= battery.Props.storedEnergyMax) return;

            float availableCapacity = battery.Props.storedEnergyMax - battery.StoredEnergy;
            float maxChargeableEnergy = Mathf.Min(EnergyTransferRate, StoredEnergy / battery.Props.efficiency);
            float chargeAmount = Mathf.Min(availableCapacity, maxChargeableEnergy);

            if (StoredEnergy >= chargeAmount * battery.Props.efficiency)
            {
                battery.AddEnergy(chargeAmount);
                StoredEnergy -= chargeAmount * battery.Props.efficiency;
            }
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            Unlink();
        }

        public void StudyImplant(HediffDef implant)
        {
            if (implant != null && !studiedImplants.Contains(implant))
            {
                studiedImplants.Add(implant);
            }
        }

        public bool HasStudied(HediffDef implant)
        {
            return implant != null && studiedImplants.Contains(implant);
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Collections.Look(ref studiedImplants, "studiedImplants", LookMode.Def);
            Scribe_Collections.Look(ref connectedBuildings, "connectedBuildings", LookMode.Reference);
            Scribe_References.Look(ref linkedTarget, nameof(linkedTarget));
            Scribe_References.Look(ref fakeCharger, nameof(fakeCharger));
            Scribe_Values.Look(ref storedEnergy, nameof(storedEnergy));
            Scribe_Values.Look(ref maxStoredEnergy, nameof(maxStoredEnergy));
            Scribe_Values.Look(ref energyTransferRate, nameof(energyTransferRate));

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                studiedImplants ??= new List<HediffDef>();
                connectedBuildings ??= new HashSet<Building>();

                if (linkedTarget != null)
                {
                    compPower = linkedTarget.TryGetComp<CompPowerTrader>();
                    compPowerBattery = linkedTarget.TryGetComp<CompPowerBattery>();
                    needPower = (linkedTarget as Pawn)?.needs?.energy;

                    if (needPower != null && needPower.currentCharger == null)
                    {
                        needPower.currentCharger = fakeCharger ??= new Building_MechCharger();
                    }
                }
            }
        }

        public override string CompTipStringExtra
        {
            get
            {
                string tip = $"Energy Stored: {StoredEnergy:F2} / {maxStoredEnergy:F2} W\n";


                if (linkedTarget != null)
                {
                    tip += $"Linked Target: {linkedTarget.Label}\n";
                    if (compPower != null)
                    {
                        tip += $"Power Consumption: {compPower.Props.PowerConsumption} W\n";
                    }
                    if (compPowerBattery != null)
                    {
                        tip += $"Battery Stored: {compPowerBattery.StoredEnergy:F2} / {compPowerBattery.Props.storedEnergyMax:F2} W\n";
                    }
                }

                return tip;
            }
        }

    }

    public class HediffCompProperties_MechuDeru : HediffCompProperties
    {
        public HediffCompProperties_MechuDeru()
        {
            compClass = typeof(HediffComp_MechuDeru);
        }
    }
}