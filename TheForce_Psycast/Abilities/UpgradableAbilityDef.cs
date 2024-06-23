using System.Collections.Generic;
using Verse;
using VFECore.Abilities;

namespace TheForce_Psycast.Abilities
{
    public class UpgradableAbilityDef : AbilityDef
    {
        // Define properties for upgrades
        public List<AbilityUpgrade> upgrades = new List<AbilityUpgrade>();

        // Method to get the current level of the ability
        public int GetCurrentLevel(Pawn pawn)
        {
            var comp = pawn.GetComp<CompAbilities>();
            if (comp != null && comp.HasAbility(this))
            {
                var ability = comp.GetAbility(this);
                return ability?.Level ?? 0;
            }
            return 0;
        }

        // Method to apply upgrades to ability properties
        public void ApplyUpgrades(Pawn pawn)
        {
            int level = GetCurrentLevel(pawn);
            AbilityUpgrade upgrade = upgrades.Find(upg => upg.level == level);

            if (upgrade != null)
            {
                this.range = upgrade.range;
                this.power = upgrade.power;
                this.cooldownTime = upgrade.cooldownTime;
                // Apply other upgrades as needed
            }
        }

        // Method to increase ability level or give the ability if not already possessed
        public void CheckAndUpgradeAbility(Pawn pawn)
        {
            var comp = pawn.GetComp<CompAbilities>();
            if (comp == null) return;

            var ability = comp.GetAbility(this);
            if (ability != null)
            {
                ability.Level++;
            }
            else
            {
                comp.GiveAbility(this);
                ability = comp.GetAbility(this);
                if (ability != null)
                {
                    ability.Level = 1;
                }
            }

            ApplyUpgrades(pawn);
        }

        // Example method to be called in your game logic
        public static void OnAbilityUsed(Pawn pawn, UpgradableAbilityDef abilityDef)
        {
            if (pawn == null || abilityDef == null) return;
            abilityDef.CheckAndUpgradeAbility(pawn);
        }
    }

    public class AbilityUpgrade
    {
        public int level;
        public float range;
        public float power;
        public int cooldownTime;
        // Add other properties as needed

        public AbilityUpgrade(int level, float range, float power, int cooldownTime)
        {
            this.level = level;
            this.range = range;
            this.power = power;
            this.cooldownTime = cooldownTime;
        }
    }

    public static class PawnExtensions
    {
        public static int GetAbilityLevel(this Pawn pawn, AbilityDef abilityDef)
        {
            var comp = pawn.GetComp<CompAbilities>();
            if (comp != null)
            {
                var ability = comp.GetAbility(abilityDef);
                if (ability != null)
                {
                    return ability.Level;
                }
            }
            return 0;
        }
    }
}