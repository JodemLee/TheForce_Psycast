using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;
using static HarmonyLib.Code;
using TheForce_Psycast.NightSisterMagick;

namespace TheForce_Psycast
{
    internal class AbilityExtensionForce : AbilityExtension_AbilityMod
    {
        public List<HediffDef> Attunement;
        public float severity = -1f;

        public override void Cast(GlobalTargetInfo[] targets, Ability ability)
        {
            base.Cast(targets, ability);

            foreach (GlobalTargetInfo target in targets)
            {
                if (ability.pawn != null && Attunement != null)
                {
                    foreach (var def in Attunement)
                    {
                        var hediff = HediffMaker.MakeHediff(def, ability.pawn);
                        if (severity >= 0f)
                        {
                            hediff.Severity = severity;
                        }
                        ability.pawn.health.AddHediff(hediff);
                    }
                }
            }
        }
    }


    public class AbilityExtension_IchorCost : AbilityExtension_AbilityMod
    {
        public float IchorCost;
        public override void Cast(GlobalTargetInfo[] targets, Ability ability)
        {
            base.Cast(targets, ability);
            var Ichor = ability.pawn.genes.GetFirstGeneOfType<Force_GeneMagick>();
            Ichor.Resource.Value -= IchorCost;
        }

        public override string GetDescription(Ability ability)
        {
            return (("AbilityIchorCost".Translate() + ": ") + Mathf.RoundToInt(IchorCost * 100f)).ToString().Colorize(Color.green);
        }
    }
}
 


