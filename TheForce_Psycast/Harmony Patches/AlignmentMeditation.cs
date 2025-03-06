using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System;
using Verse;
using RimWorld.Planet;
using UnityEngine;

namespace TheForce_Psycast.Harmony_Patches
{
    public class MeditationBuilding_Alignment : DefModExtension
    {
        public HediffDef hedifftoIncrease;
        public HediffDef hedifftoDecrease;
    }
}

