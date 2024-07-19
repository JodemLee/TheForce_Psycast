using HarmonyLib;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Verse;

namespace TheForce_Psycast.Harmony_Patches
{
    public class MeditationBuilding_Alignment : DefModExtension
    {
        public HediffDef hedifftoIncrease;
        public HediffDef hedifftoDecrease;
    }
}

