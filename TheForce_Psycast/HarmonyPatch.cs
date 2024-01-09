using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TheForce_Psycast;
using UnityEngine;
using Verse;
using VFECore;
using RimWorld;
using System.Runtime;
using Verse.Sound;
using VFECore.Abilities;
using AbilityDef = VFECore.Abilities.AbilityDef;
using VanillaPsycastsExpanded;


namespace TheForce_Psycast
{

    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Harmony harmony = new("Psycast_ForceThe");
            harmony.PatchAll();
        }
    }
}
